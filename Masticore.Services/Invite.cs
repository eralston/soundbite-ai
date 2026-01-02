using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    //TODO: Determine if this should become a service.  If so, we can stop exposing Mapper from the service base class and just expose Map<>

    /// <summary>
    /// A command object for loading the database with record for an invite
    /// NOTE: This does NOT do RBAC
    /// </summary>
    internal class Invite
    {
        // Configurable by client

        /// <summary>
        /// Gets or sets the application-level role of the user.
        /// </summary>
        public UserRole UserRole { get; set; } = UserRole.User;

        /// <summary>
        /// Gets or sets the organization-level role of the user.
        /// </summary>
        public PersonRole PersonRole { get; set; } = PersonRole.Person;

        /// <summary>
        /// Gets or sets the group-level role of the user.
        /// </summary>
        public MemberRole MemberRole { get; set; } = MemberRole.Member;

        // Set in Constructor

        private IIdentityDb Db { get; }

        private UserEntity CurrentUserEntity { get; }
        private User CurrentUser { get; }

        public OrganizationEntity OrgEntity { get; private set; }
        public GroupEntity GroupEntity { get; private set; }
        private IMapper Mapper { get; }

        // Loaded by the class

        public bool IsNewUser { get; set; } = false;

        public Invite(IIdentityDb db, IMapper mapper, UserEntity currentUser, OrganizationEntity org = null, GroupEntity grp = null)
        {
            Validator.ArgNotNull(nameof(db), db);
            Validator.ArgNotNull(nameof(mapper), mapper);
            Validator.ArgNotNull(nameof(currentUser), currentUser);

            Db = db;
            CurrentUserEntity = currentUser;
            CurrentUser = mapper.MapSafe<User>(CurrentUserEntity);
            OrgEntity = org;
            GroupEntity = grp;
            Mapper = mapper;
        }

        /// <summary>
        /// Sends an invite to the given user based on the current state of this invite and the db
        /// </summary>
        /// <param name="notifications"></param>
        /// <param name="userToInvite"></param>
        /// <returns></returns>
        private async Task<InviteResult> SendInvite(INotificationService notifications, User userToInvite)
        {
            InviteResult result = new InviteResult();
            if (userToInvite.IsDeleted())  // Deleted users cannot be brought back via invite - we assume it's a ban of some kind
            {
                return result;
            }

            result.UserRoute = userToInvite.Route;

            // User
            if (OrgEntity == null)
            {
                await notifications.UserInviteAsync(userToInvite, CurrentUser);
                return result;
            }

            Organization org = Mapper.MapSafe<Organization>(OrgEntity);
            Group group = Mapper.MapSafe<Group>(GroupEntity);

            // Person
            PersonEntity person = await EnsurePersonWhereUid(userToInvite);
            result.PersonRoute = person.Route;
            if (GroupEntity == null)
            {
                await notifications.PersonInviteAsync(userToInvite, CurrentUser, org, IsNewUser);
                return result;
            }

            // Member
            MemberEntity member = await EnsureMemberAsync(userToInvite, person);
            result.MemberRoute = member.Route;
            await notifications.MemberInviteAsync(userToInvite, CurrentUser, org, group, IsNewUser);
            return result;
        }

        /// <summary>
        /// Invites the given user by email or User.Route
        /// NOTE: This will always send a notification, even if the user has already accepted
        /// </summary>
        /// <param name="token">E-Mail address or User.Route</param>
        /// <param name="notifications"></param>
        /// <returns></returns>
        public async Task<InviteResult> InviteUserWhereUid(string token, INotificationService notifications)
        {
            UserEntity userEntity = await EnsureUserByRouteOrEmail(token);
            User user = Mapper.MapSafe<User>(userEntity);
            return await SendInvite(notifications, user);
        }

        /// <summary>
        /// Invites the given person by email or Person.Route
        /// </summary>
        /// <param name="token"></param>
        /// <param name="notifications"></param>
        /// <returns></returns>
        public async Task<InviteResult> InvitePersonWhereUid(string token, INotificationService notifications)
        {
            if (token.IsEmail())
            {
                return await InviteUserWhereUid(token, notifications);
            }

            UserEntity userEntity = await UserByPersonRoute(token);
            User user = Mapper.MapSafe<User>(userEntity);
            return await SendInvite(notifications, user);
        }

        private async Task<UserEntity> UserByPersonRoute(string token)
        {
            // Pull out the person with the user, regardless if the person or user was deleted
            UserEntity user = await Db.Users.Where(u => u.People.Any(p => p.Route == token)).FirstOrDefaultAsync();
            user.AssertFound($"Could not find person '{token}'");
            return user;
        }

        private async Task<UserEntity> EnsureUserByRouteOrEmail(string routeOrEmail)
        {
            if (routeOrEmail.IsEmail())
            {
                return await EnsureUserByEmail(routeOrEmail);
            }
            else
            {
                UserEntity user = await Db.UserWhereRoute(routeOrEmail);
                user.AssertFound($"Cannot find user for invite token '{routeOrEmail}'");
                IsNewUser = user.IsAccepted();
                return user;
            }
        }

        private async Task<UserEntity> EnsureUserByEmail(string email)
        {
            UserEntity user = await Db.UserWhereEmail(email);
            if (user == null)
            {
                user = Db.Users.CreateResource(CurrentUserEntity);
                user.UserRole = UserRole;
                user.AllowNews = true;
                user.AllowMarketing = true;
                user.AllowEmail = true;
                user.AllowSms = true;
                user.Email = email;
                await Db.SaveChangesAsync();
                user.Invite();
                // No UniversalId right now, it should be populated on first login
            }

            // NOTE: This does NOT restore the user if they're deleted - Assume it's a ban

            IsNewUser = user.IsAccepted();
            return user;
        }

        private async Task<PersonEntity> EnsurePersonWhereUid(User user)
        {
            PersonEntity person = await Db.People.Where(p => p.User.Route == user.Route
                                            && p.User.DeletedUtc == null
                                            && p.Organization.DeletedUtc == null
                                            && p.Organization.Route == OrgEntity.Route
                                            && p.Organization.Tenant.DeletedUtc == null
                                           ).FirstOrDefaultAsync();

            if (person == null)
            {
                UserEntity userEntity = await Db.UserWhereRoute(user.Route);
                Validator.NotNull(userEntity, "Failed to locate user by route '{0}'", user.Route);
                person = Db.People.CreateResource(CurrentUserEntity);
                person.PersonRole = PersonRole;
                person.User = userEntity;
                person.Organization = OrgEntity;
                person.Invite();
            }
            else
            {
                person.Restore();
                if (person.Restore())
                {
                    person.PersonRole = PersonRole.Person;
                }
            }

            return person;
        }

        protected async Task<MemberEntity> EnsureMemberAsync(User user, PersonEntity person)
        {
            MemberEntity member = await Db.Members.Where(m => m.Person.User.Route == user.Route
                                      && m.Group.Route == GroupEntity.Route
                                      && m.Person.Organization.DeletedUtc == null
                                      && m.Person.Organization.Route == OrgEntity.Route
                                      && m.Person.Organization.Tenant.DeletedUtc == null).FirstOrDefaultAsync();
            if (member == null)
            {
                member = Db.Members.CreateResource(CurrentUserEntity);
                member.MemberRole = MemberRole;
                member.Person = person;
                member.Group = GroupEntity;
                member.Invite();
            }
            else
            {
                if (member.Restore())
                {
                    member.MemberRole = MemberRole.Member;
                }
            }

            return member;
        }
    }
}
