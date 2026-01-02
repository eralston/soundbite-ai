using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// EF Implementation of <see cref="IGroupService"/>
    /// </summary>
    public class GroupService : IdentityServiceBase, IGroupService
    {
        #region Fields

        private INotificationService Notifications { get; }
        private IRbac Rbac { get; }

        #endregion

        #region Methods

        public GroupService(
            IRbac rbac,
            INotificationService notifications,
            IIdentityInfrastructure infrastructure,
            IMapper mapper,
            ILogger<GroupService> logger)
            : base(infrastructure, logger, mapper)
        {
            if (infrastructure is null)
            {
                throw new ArgumentNullException(nameof(infrastructure));
            }

            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
            Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        }

        /// <summary>
        /// Async get a complete <see cref="GroupDetails"/> object for the given group in the given org, based on the current user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        private async Task<GroupDetails> GroupDetailsAsync(IIdentityDb db, string orgRoute, string groupRoute)
        {
            GroupDetails group = await db.GroupDetailsAsync(orgRoute, groupRoute);
            group.MemberRole = await Rbac.CurrentMemberRole(orgRoute, groupRoute);
            return group;
        }

        protected async Task<GroupEntity> CreateEntitiesAsync(IIdentityDb db, string orgRoute, NewGroup newGroup)
        {
            UserEntity currentUser = (await db.UserWhereUid(Rbac.UniversalIdForCurrentUser)).AssertFound();
            OrganizationEntity org = (await db.OrgWhereRoute(orgRoute)).AssertFound($"Could not find org {orgRoute}");

            // Team
            GroupEntity group = CreateGroup(newGroup, db, currentUser, org);

            // Flatten members
            Dictionary<string, MemberRole> tokensWithHighestRole = BuildTokensAndCheckForOwner(newGroup);
            Invite inviter = new Invite(db, Mapper, currentUser, org, group);

            // Create members, potentially inviting people
            foreach (KeyValuePair<string, MemberRole> tokenRole in tokensWithHighestRole)
            {
                inviter.PersonRole = PersonRole.Person;
                inviter.MemberRole = tokenRole.Value;
                await inviter.InvitePersonWhereUid(tokenRole.Key, Notifications);
            }

            // Save and return team
            await db.SaveChangesAsync();
            return group;
        }

        private static GroupEntity CreateGroup(NewGroup newGroup, IIdentityDb db, UserEntity currentUser, OrganizationEntity org)
        {
            GroupEntity group = db.Groups.CreateResource(currentUser);
            group.Name = newGroup.Name;
            group.Description = newGroup.Description;
            group.Organization = org;
            return group;
        }

        private static Dictionary<string, MemberRole> BuildTokensAndCheckForOwner(NewGroup newGroup)
        {
            bool hasOwner = false;
            Dictionary<string, MemberRole> tokenAndHighestRole = new Dictionary<string, MemberRole>();
            foreach (NewMember newMember in newGroup.Members)
            {
                tokenAndHighestRole.TryGetValue(newMember.PersonToken, out MemberRole role);
                if (newMember.MemberRole >= role)
                {
                    tokenAndHighestRole[newMember.PersonToken] = newMember.MemberRole;
                    if (newMember.MemberRole >= MemberRole.Owner && !newMember.PersonToken.IsEmail())
                    {
                        hasOwner = true;
                    }
                }
            }

            if (!hasOwner)
            {
                throw new InvalidOperationException("A new group must have at least one known owner");
            }

            return tokenAndHighestRole;
        }

        private async Task ApplyUpdates(IIdentityDb db, string orgRoute, string groupRoute, JsonPatchDocument updates)
        {
            GroupEntity group = await db.Groups.Where(i => i.Route == groupRoute && i.Organization.Route == orgRoute).FirstOrDefaultAsync();
            Validator.EnsureFound(group, "Could not find group '{0}'", groupRoute);
            if (group.DeletedUtc != null)
            {
                throw new InvalidOperationException("Cannot update an archived group");
            }

            updates.ApplyTo(group);
            group.Timestamp();
            await db.SaveChangesAsync();
        }

        #endregion

        #region IGroupService Implementation

        public async Task<GroupDetails> CreateAsync(string orgRoute, NewGroup newGroup)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(newGroup), newGroup);
            Validator.ArgValidate(nameof(newGroup), newGroup, t => t?.Members?.Count() > 0, "Group must have at least one new member");

            // TODO: Make role for creating teams configurable
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            GroupEntity group = await CreateEntitiesAsync(db, orgRoute, newGroup);
            GroupDetails ret = await GroupDetailsAsync(db, orgRoute, group.Route);
            return ret;
        }

        [Obsolete]
        public async Task<IEnumerable<Group>> ReadAllAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            IEnumerable<Group> ret = await db.QueryGroups(orgRoute)
                .MapReadOnlyAsync((g) => Mapper.MapSafe<Group>(g));
            return ret;
        }

        public async Task<IEnumerable<Group>> ReadMyAsync(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Guest);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            IEnumerable<Group> ret = await db.QueryMyGroups(orgRoute, Rbac.UniversalIdForCurrentUser)
                .MapReadOnlyAsync((g) => Mapper.MapSafe<Group>(g));
            return ret;
        }

        public async Task<IEnumerable<Group>> ReadMyTargetsAsync(string orgRoute, Func<Task<MemberRole>> minRoleAction = null)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            // Org role determines potential scope
            PersonRole role = await Rbac.CurrentPersonRole(orgRoute);

            // Unassigned is locked out; Admins see everything; everyone else just see their own groups
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            IQueryable<GroupEntity> query;
            if (role < PersonRole.Guest)
            {
                throw new SecurityException($"User does not have required permission for org '{orgRoute}'");
            }
            else if (role >= PersonRole.Admin)
            {
                query = db.QueryGroups(orgRoute);
            }
            else
            {
                // Minimum role for the org determines minimum role to post into a group for non-admins
                MemberRole memberRole = minRoleAction == null ? MemberRole.Member : await minRoleAction();
                query = db.QueryMyGroups(orgRoute, Rbac.UniversalIdForCurrentUser, memberRole);
            }

            Group[] ret = await query.MapReadOnlyAsync((g) => Mapper.MapSafe<Group>(g));
            return ret;
        }

        public async Task<GroupDetails> ReadAsync(string orgRoute, string groupRoute)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, groupRoute, MemberRole.Member);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            GroupDetails ret = await GroupDetailsAsync(db, orgRoute, groupRoute);
            return ret;
        }

        [Obsolete("Deprecated for performance reasons; use ReadAsync")]
        public async Task<GroupDetails_Obsolete> ReadAsync_Obsolete(string orgRoute, string groupRoute)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, groupRoute, MemberRole.Member);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            IQueryable<GroupDetails_Obsolete> query = from t in db.Groups
                                                      let members = from m in db.Members
                                                                    where m.GroupId == t.Id
                                                                       && m.DeletedUtc == null
                                                                       && m.Person.DeletedUtc == null
                                                                    orderby m.Person.User.GivenName
                                                                    select Mapper.MapSafeWith<Member>(m, m.Person, m.Person.User)
                                                      where
                                                           // Active team by route
                                                           t.Route == groupRoute
                                                           && t.DeletedUtc == null
                                                           && t.Organization.Route == orgRoute
                                                           && t.Organization.DeletedUtc == null
                                                           && t.Organization.Tenant.DeletedUtc == null
                                                      select new GroupDetails_Obsolete
                                                      {
                                                          Route = t.Route,
                                                          CreatedUtc = t.CreatedUtc,
                                                          UpdatedUtc = t.UpdatedUtc,
                                                          Description = t.Description,
                                                          Name = t.Name,
                                                          Members = members,
                                                      };

            GroupDetails_Obsolete ret = await query.FirstOrDefaultAsync();
            Validator.EnsureFound(ret, "Could not find team '{0}' in org '{1}'", groupRoute, orgRoute);

            return ret;
        }

        public async Task DeleteAsync(string orgRoute, string groupRoute)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, groupRoute, MemberRole.Owner);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            GroupEntity group = await db.GroupWhereRoute(orgRoute, groupRoute);
            group.AssertFound($"Could not find group '{groupRoute}'");

            group.SoftDelete();
            await db.SaveChangesAsync();
        }

        public async Task<GroupDetails> PatchAsync(string orgRoute, string groupRoute, JsonPatchDocument updates)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), orgRoute);
            Validator.ArgNotNull(nameof(updates), orgRoute);
            Validator.ArgValidate(nameof(updates), updates, i => i.Operations?.Count > 0, "One or more operations should be present in the patch document.");

            await Rbac.AssertCurrentUserInRole(orgRoute, groupRoute, MemberRole.Owner);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            await ApplyUpdates(db, orgRoute, groupRoute, updates);
            GroupDetails ret = await GroupDetailsAsync(db, orgRoute, groupRoute);
            return ret;
        }

        #endregion
    }
}