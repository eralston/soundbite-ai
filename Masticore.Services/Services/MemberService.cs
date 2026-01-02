using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// EF Implementation of <see cref="IMemberService"/>
    /// </summary>
    public class MemberService : IdentityServiceBase, IMemberService
    {
        #region Constants

        /// <summary>
        /// The maximum total people returned by <see cref="ReadAllAsync(string, string, IndexPageRequest)"/>
        /// </summary>
        public const int MaxResultsReadAllAsync = int.MaxValue;

        #endregion

        #region Support Members

        protected INotificationService Notifications { get; }
        protected IImageService Images { get; }

        public MemberService(
            // TODO: Convert to IRbac
            ISecurityContext securityContext,
            INotificationService notifications,
            IImageService images,
            IIdentityInfrastructure infrastructure,
            IMapper mapper,
            ILogger<MemberService> logger)
            : base(infrastructure, logger, mapper, securityContext)
        {
            Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
            Images = images ?? throw new ArgumentNullException(nameof(images));
        }

        /// <summary>
        /// Async load the <see cref="User.ImageSrc"/> property on the <see cref="Member.Person"/> object
        /// </summary>
        /// <remarks>
        /// If the members list is greater than 32 in length, then this will abort and do nothing to prevent performance issues
        /// </remarks>
        /// <param name="members"></param>
        /// <returns></returns>
        private async Task LoadUserImagesAsync(IEnumerable<Member> members)
        {
            // TODO: Consider what to do for large batches
            // For now, assume masses of users do not need their images
            // Ideally, we will just implement paging that prevents loading more than some limit at a time
            if (members.Count() > 32)
            {
                return;
            }

            Dictionary<string, Task<string>> tasks = new Dictionary<string, Task<string>>();
            foreach (Member member in members)
            {
                Task<string> task = Images.UserImageAsync(member.Person.User);
                tasks.Add(member.Person.User.Route, task);
            }

            await Task.WhenAll(tasks.Values);

            foreach (Member member in members)
            {
                string imageSrc = tasks[member.Person.User.Route].Result;
                member.Person.User.ImageSrc = imageSrc;
            }
        }

        private async Task<MemberEntity> MemberEntityAsync(IIdentityDb db, string orgRoute, string groupRoute, string memberRoute)
        {
            IQueryable<MemberEntity> query = db.Members
                .Where(m => m.Route == memberRoute
                    && m.DeletedUtc == null
                    && m.Person.DeletedUtc == null
                    && m.Person.Organization.DeletedUtc == null
                    && m.Person.Organization.Route == orgRoute
                    && m.Person.Organization.Tenant.DeletedUtc == null
                    && m.Group.DeletedUtc == null
                    && m.Group.Route == groupRoute)
                .Include(i => i.Person)
                .Include(i => i.Person.User);

            MemberEntity member = await query.FirstOrDefaultAsync();
            member.AssertFound($"Could not find member with ID '{memberRoute}' in group '{groupRoute}' in org '{orgRoute}'");
            return member;
        }


        /// <summary>
        /// Extensibly described role for successfully sending <see cref="MemberService.InviteAsync(string, string, Models.Invite[])"/>
        /// </summary>
        /// <returns></returns>
        protected virtual Task<MemberRole> MinRoleForInviteAsync(string orgRoute, string groupRoute)
        {
            return Task.FromResult(MemberRole.Owner);
        }

        #endregion

        #region IMemberService

        /// <summary>
        /// Reads the IMember for the given member ID, but only if you're an admin in the org OR owner on the team
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="memberRoute"></param>
        /// <returns></returns>
        public virtual async Task<Member> ReadAsync(string orgRoute, string groupRoute, string memberRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            UserEntity currentUser = await db.UserWhereUid(orgRoute, groupRoute, SecurityContext.UniversalId, MemberRole.Member);
            currentUser.AssertFound("Could not find user for claims");

            IQueryable<Member> query = from m in db.Members
                                       where
                                           m.DeletedUtc == null
                                        && m.Route == memberRoute
                                        && m.Group.DeletedUtc == null
                                        && m.Group.Route == groupRoute
                                        && m.Person.DeletedUtc == null
                                        && m.Person.Organization.DeletedUtc == null
                                        && m.Person.Organization.Route == orgRoute
                                        && m.Person.Organization.Tenant.DeletedUtc == null
                                       select Mapper.MapSafeWith<Member>(m, m.Person, m.Person.User);

            Member member = await query.FirstOrDefaultAsync();
            member.AssertFound($"Could not find member with ID {memberRoute} in group '{groupRoute}' in org '{orgRoute}'");

            // TODO: Hydrate ImageSrc

            return member;
        }

        /// <inheritdoc />
        public async Task<IndexPageResponse<Member>> ReadAllAsync(string orgRoute, string groupRoute, IndexPageRequest page = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            IQueryable<MemberEntity> entities = db.QueryMembers(orgRoute, groupRoute, true);

            // People allows for a different max result, which means we use a PageQuery
            IndexPageQuery query = new IndexPageQuery(page).SetMaxResults(MaxResultsReadAllAsync);
            IndexPageResponse<MemberEntity> entityPage = await entities.ReadPageAsync(query, (i) =>
                i.Person.User.FamilyName.Contains(page.Filter) ||
                i.Person.User.GivenName.Contains(page.Filter) ||
                i.Person.User.Email.Contains(page.Filter));
            IndexPageResponse<Member> ret = entityPage.Map((i) => Mapper.MapSafeWith<Member>(i, i.Person, i.Person.User));

            ret.BasedOn(query);

            await LoadUserImagesAsync(ret.Result);

            return ret;
        }

        /// <summary>
        /// Async get all <see cref="Member"/> objects associated with the given group in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task<IEnumerable<Member>> ReadAllAsync_Obsolete(string orgRoute, string groupRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            UserEntity currentUser = await db.UserWhereUid(orgRoute, groupRoute, SecurityContext.UniversalId, MemberRole.Member);
            currentUser.AssertFound("Could not find user for claims");

            IEnumerable<Member> members =
               await db.QueryMembers(orgRoute, groupRoute, true)
               .MapReadOnlyAsync(e => Mapper.MapSafeWith<Member>(e, e.Person, e.Person.User));

            await LoadUserImagesAsync(members);

            return members;
        }

        /// <summary>
        /// Asynchronously gets a member record based on org/group/user routes.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the group belongs.</param>
        /// <param name="groupRoute">Route of the group to which the member belongs.</param>
        /// <param name="userRoute">Route of the user whose membership record is being sought.</param>
        /// <returns>the requested member record or <c>null</c> if the member record cannot be found.</returns>
        public async Task<Member> ReadByUserRouteAsync(string orgRoute, string groupRoute, string userRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);
            Validator.ArgNotNull(nameof(userRoute), userRoute);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            UserEntity currentUser = await db.UserWhereUid(orgRoute, groupRoute, SecurityContext.UniversalId, MemberRole.Member);
            currentUser.AssertFound("Could not find user for claims");

            IQueryable<MemberEntity> query = db.QueryMemberByRoute(orgRoute, groupRoute, userRoute, true);

            MemberEntity member = await query.FirstOrDefaultAsync();
            member.AssertFound($"Could not find user '{userRoute}' who is a member in group '{groupRoute}' in org '{orgRoute}'");

            Member ret = Mapper.MapSafeWith<Member>(member, member.Person, member.Person.User);
            // TODO: Hydrate ImageSrc

            return ret;
        }

        /// <summary>
        /// Deletes the given member ID, as long as the claims are admin OR team owner
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="memberRoute"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(string orgRoute, string groupRoute, string memberRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity currentUser = await db.UserWhereUid(orgRoute, groupRoute, SecurityContext.UniversalId, MemberRole.Owner);
            currentUser.AssertFound();

            MemberEntity member = await MemberEntityAsync(db, orgRoute, groupRoute, memberRoute);
            member.AssertFound();

            member.SoftDelete();
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the Member record associated with the given IMember
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="memberFields"></param>
        /// <returns></returns>
        public virtual async Task<Member> UpdateAsync(string orgRoute, string groupRoute, string memberRoute, Member memberFields)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);
            Validator.ArgNotNull(nameof(memberFields), memberFields);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity user = await db.UserWhereUid(orgRoute, groupRoute, SecurityContext.UniversalId, MemberRole.Owner);
            user.AssertFound("Could not find user for claims");

            // Update the Member record
            MemberEntity member = await MemberEntityAsync(db, orgRoute, groupRoute, memberRoute);
            member.MemberRole = memberFields.MemberRole;
            member.Timestamp();
            await db.SaveChangesAsync();

            //Query back out an IMember and send it back
            return await ReadAsync(orgRoute, groupRoute, memberRoute);
        }

        /// <summary>
        /// Sets up inviting a user with the given email to the platform, but only if the Claims are an admin or team owner
        /// NOTE: this will potentially create or restore a User, Person, and Member entity for this invite, but
        /// it will NEVER restore an Organization or Team
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public virtual async Task<InviteResult[]> InviteAsync(string orgRoute, string groupRoute, Models.Invite[] invites)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);
            Validator.ArgNotNull(nameof(invites), invites);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            // TODO: Make person invite role requirement configurable
            MemberRole requiredRole = await MinRoleForInviteAsync(orgRoute, groupRoute);
            UserEntity user = await db.UserWhereUid(orgRoute, groupRoute, SecurityContext.UniversalId, requiredRole);
            user.AssertFound("Could not find user for claims");

            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            GroupEntity grp = await db.GroupWhereRoute(orgRoute, groupRoute);
            grp.AssertFound($"Could not find group '{groupRoute}' in org '{orgRoute}'");

            Invite inviter = new Invite(db, Mapper, user, org, grp);
            List<InviteResult> ret = new List<InviteResult>();
            foreach (Models.Invite invite in invites)
            {
                InviteResult result = await inviter.InvitePersonWhereUid(invite.Token, Notifications);
                ret.Add(result);
            }
            await db.SaveChangesAsync();
            return ret.ToArray();
        }

        public async Task<MemberRole> ReadRoleAsync(string orgRoute, string groupRoute, string userRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);
            Validator.ArgNotNull(nameof(userRoute), userRoute);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            // Current user needs to have regular access to the org to even ask
            // TODO: Replace with IRbac call
            (await db.UserWhereUid(orgRoute, SecurityContext.UniversalId, PersonRole.Person)).AssertFound();

            // If the user is a god, then they count as an owner
            UserEntity godUser = await db.UserWhereRoute(userRoute);
            if (godUser != null && godUser.UserRole >= UserRole.God)
            {
                return MemberRole.Owner;
            }

            // If the user is an admin on the org, they count as an owner
            PersonEntity adminPerson = await db.PersonWhereUserRoute(orgRoute, userRoute);
            if (adminPerson != null && adminPerson.PersonRole >= PersonRole.Admin)
            {
                return MemberRole.Owner;
            }

            // If all ese 
            MemberEntity member = await db.QueryMemberByRoute(orgRoute, groupRoute, userRoute).ReadOnlyOne();
            if (member != null)
            {
                return member.MemberRole;
            }
            else
            {
                return MemberRole.Unknown;
            }
        }

        #endregion
    }
}
