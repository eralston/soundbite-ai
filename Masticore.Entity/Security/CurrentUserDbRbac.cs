using Masticore.Resources;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Security;
using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Implements <see cref="IRbac"/> over an <see cref="IIdentityDb"/> checking based on the current <see cref="ISecurityContext"/>
    /// </summary>
    public class CurrentUserDbRbac : IRbac
    {
        #region Members

        protected ILogger<CurrentUserDbRbac> Logger { get; }
        protected IIdentityInfrastructure Infrastructure { get; }
        protected ISecurityContext Security { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="infrastructure"></param>
        /// <param name="security"></param>
        public CurrentUserDbRbac(
            ILogger<CurrentUserDbRbac> logger,
            IIdentityInfrastructure infrastructure,
            // TODO: Swap to something lighter weight than using the whole ISecurityContext object since this is meant to supersede it one day
            ISecurityContext security
            )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Security = security ?? throw new ArgumentNullException(nameof(security));
        }

        #endregion

        /// <inheritdoc />
        public async Task AssertCurrentUser()
        {
            await AssertCurrentUserInRole(UserRole.User);
        }

        /// <inheritdoc />
        public async Task AssertCurrentUserInRole(UserRole minimumRole)
        {
            try
            {
                // TODO: Caching? Theoretically EF does it for us
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                UserEntity currentUser = await db.UserWhereUid(Security.UniversalId, minimumRole);
                currentUser.AssertFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"RBAC Error: User '{Security.CurrentUser?.Route ?? "UNKNOWN"}' does not have user role {minimumRole}");
                throw new SecurityException("User does not have required permission.", ex);
            }
        }

        /// <inheritdoc />
        public async Task AssertCurrentUserInRole(string orgRoute, PersonRole minimumRole)
        {
            try
            {
                // TODO: Caching? Theoretically EF does it for us
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                UserEntity currentUser = await db.UserWhereUid(orgRoute, Security.UniversalId, minimumRole);
                currentUser.AssertFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"RBAC Error: User '{Security.CurrentUser?.Route ?? "UNKNOWN"}' does not have person role {minimumRole}");
                throw new SecurityException("User does not meet person role security requirements.", ex);
            }
        }

        /// <inheritdoc />
        public async Task AssertCurrentUserInRole(string orgRoute, string groupRoute, MemberRole minimumRole)
        {
            try
            {
                // TODO: Caching? Theoretically EF does it for us
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                UserEntity currentUser = await db.UserWhereUid(orgRoute, groupRoute, Security.UniversalId, minimumRole);
                currentUser.AssertFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"RBAC Error: User '{Security.CurrentUser?.Route ?? "UNKNOWN"}' does not have member role {minimumRole}");
                throw new SecurityException("User not in member role.");
            }
        }

        /// <inheritdoc />
        public async Task<UserRole> CurrentUserRole()
        {
            // TODO: Caching? Theoretically EF does it for us
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity currentUser = await db.QueryUser(Security.UniversalId).ReadOnlyOne();
            currentUser.AssertFound();
            return currentUser.UserRole;
        }

        /// <inheritdoc />
        public async Task<PersonRole> CurrentPersonRole(string orgRoute)
        {
            // TODO: Caching? Theoretically EF does it for us
            UserRole userRole = await CurrentUserRole();
            if (userRole >= UserRole.God)
            {
                return PersonRole.Admin;
            }

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            PersonEntity currentPerson = await db.QueryPerson(orgRoute, Security.UniversalId).ReadOnlyOne();
            if (currentPerson == null)
            {
                return PersonRole.Unknown;
            }
            else
            {
                return currentPerson.PersonRole;
            }
        }

        /// <inheritdoc />
        public async Task<MemberRole> CurrentMemberRole(string orgRoute, string groupRoute)
        {
            // TODO: Caching? Theoretically EF does it for us
            PersonRole personRole = await CurrentPersonRole(orgRoute);
            if (personRole >= PersonRole.Admin)
            {
                return MemberRole.Owner;
            }

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            MemberEntity currentMember = await db.QueryMemberByUserUniversalId(orgRoute, groupRoute, Security.UniversalId).ReadOnlyOne();
            if (currentMember == null)
            {
                return MemberRole.Unknown;
            }
            else
            {
                return currentMember.MemberRole;
            }
        }

        /// <inheritdoc />
        public string UniversalIdForCurrentUser => Security.UniversalId;
    }
}
