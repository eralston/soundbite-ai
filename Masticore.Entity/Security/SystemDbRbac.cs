using Masticore.Resources;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Implements <see cref="IRbac"/> over an <see cref="IIdentityDb"/> that simply double-checks entities exist without doing any real work
    /// </summary>
    public class SystemDbRbac : IRbac
    {
        #region Members

        protected ILogger<SystemDbRbac> Logger { get; }
        protected IIdentityInfrastructure Infrastructure { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="infrastructure"></param>
        /// <param name="security"></param>
        public SystemDbRbac(
            ILogger<SystemDbRbac> logger,
            IIdentityInfrastructure infrastructure
            )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
        }

        #endregion

        /// <inheritdoc />
        public Task AssertCurrentUser()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task AssertCurrentUserInRole(UserRole minimumRole)
        {
            // The system has unlimited access as a user
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task AssertCurrentUserInRole(string orgRoute, PersonRole minimumRole)
        {
            try
            {
                // TODO: Caching? Theoretically EF does it for us
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                OrganizationEntity org = await db.OrgWhereRoute(orgRoute, true);
                org.AssertFound($"RBAC could not find org '{orgRoute}'");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error in RBAC, likely could not find org '{orgRoute}'");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task AssertCurrentUserInRole(string orgRoute, string groupRoute, MemberRole minimumRole)
        {
            try
            {
                // TODO: Caching? Theoretically EF does it for us
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                GroupEntity grp = await db.GroupWhereRoute(orgRoute, groupRoute);
                grp.AssertFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error in RBAC, likely could not find group '{groupRoute}' in org '{orgRoute}'");
                throw;
            }
        }

        /// <summary>
        /// Always returns <see cref="UserRole.God"/>
        /// </summary>
        /// <returns></returns>
        public Task<UserRole> CurrentUserRole()
        {
            return Task.FromResult(UserRole.God);
        }

        /// <summary>
        /// Always returns <see cref="PersonRole.Admin"/>
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public Task<PersonRole> CurrentPersonRole(string orgRoute)
        {
            return Task.FromResult(PersonRole.Admin);
        }

        /// <summary>
        /// Always returns <see cref="MemberRole.Owner"/>
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        public Task<MemberRole> CurrentMemberRole(string orgRoute, string groupRoute)
        {
            return Task.FromResult(MemberRole.Owner);
        }

        /// <summary>
        /// Async get null - there is no user when the system is in effect
        /// </summary>
        /// <returns></returns>
        public string UniversalIdForCurrentUser => null;
    }
}
