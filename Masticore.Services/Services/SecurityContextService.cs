using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

//TODO: need to implement caching of sorts so we don't look this up continually

namespace Masticore.Services
{
    /// <summary>
    /// EF implementation of IOrganizationService
    /// </summary>
    public class SecurityContextService : IdentityServiceBase, ISecurityContextService
    {
        #region Internal Classes

        /// <summary>
        /// Stores user and tenant information so it can be retrieved together in a single unit.
        /// </summary>
        private class UserAndTenantInfo
        {
            /// <summary>
            /// Gets or sets the person entity from the database.
            /// </summary>
            public PersonEntity Person { get; set; }

            /// <summary>
            /// Reference to the user entity from the database.
            /// </summary>
            public UserEntity User { get; set; }

            /// <summary>
            /// Reference to the tenant entity from the database.
            /// </summary>
            public TenantEntity Tenant { get; set; }

            /// <summary>
            /// Gets or sets an organization route value.
            /// </summary>
            public string OrgRoute { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="tenant">Reference to the tenant entity.</param>
            /// <param name="person">Reference to the person entity.</param>
            /// <param name="user">Reference to the user entity.</param>
            /// <param name="orgRoute">Organization route value.</param>
            public UserAndTenantInfo(TenantEntity tenant, PersonEntity person, UserEntity user, string orgRoute)
            {
                Tenant = tenant;
                Person = person;
                User = user;
                OrgRoute = orgRoute;

            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="SecurityContextService"/> instance.
        /// </summary>
        /// <param name="dbBuilder">DI reference to identity infrastructure.</param>
        /// <param name="logger">DI reference to a logger.</param>
        /// <param name="mapper">DI reference to a mapper.</param>
        /// <param name="securityContext">DI reference to the security context.</param>
        public SecurityContextService(
            IIdentityInfrastructure dbBuilder,
            ILogger<SecurityContextService> logger,
            IMapper mapper,
            ISecurityContext securityContext)
            : base(dbBuilder, logger, mapper, securityContext)
        {
        }

        #endregion

        #region ISecurityContextService Implementation

        /// <summary>
        /// Populates the current organization info by an organization's univeral ID.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgUniversalId">Universal ID of the organization whose info should be populated into the security context.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        public async Task<bool> SetCurrentOrgByOrgUniversalId(ISecurityContext securityContext, string orgUniversalId)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            string orgRoute = await db.Organizations
                .Where(i => i.UniversalId == orgUniversalId
                    && i.DeletedUtc == null
                    && i.Tenant.DeletedUtc == null)
                .Select(i => i.Route)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            securityContext.OrgRoute = orgRoute;
            return !string.IsNullOrEmpty(orgRoute);
        }

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="email">Email address of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        public async Task<bool> SetUserByOrgRouteAndUserEmail(ISecurityContext securityContext, string orgRoute, string email)
        {
            UserAndTenantInfo info = await GetUserAndTenantByOrgRouteAndUserEmail(orgRoute, email);
            bool result = PopulateSecurityContext(securityContext, info, "email");
            return result;
        }

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="userRoute">Route of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        public async Task<bool> SetUserByOrgRouteAndUserRoute(ISecurityContext securityContext, string orgRoute, string userRoute)
        {
            Validator.ArgNotNull(nameof(SecurityContext), securityContext);
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(userRoute), userRoute);

            UserAndTenantInfo info = await GetUserAndTenantByOrgRouteAndUserRoute(orgRoute, userRoute);
            bool result = PopulateSecurityContext(securityContext, info, "user route");
            return result;
        }

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="univeralId">Universal ID of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        public async Task<bool> SetUserByOrgRouteAndUserUniversalId(ISecurityContext securityContext, string orgRoute, string univeralId)
        {
            UserAndTenantInfo info = await GetUserAndTenantByOrgRouteAndUserUniversalId(orgRoute, univeralId);
            bool result = PopulateSecurityContext(securityContext, info, "universal ID");
            return result;
        }

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="azureTenantId">Azure Tenant ID of the organization with which the user is associated.</param>
        /// <param name="userUniveralId">Universal ID of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        public async Task<bool> SetUserByAzureTenantAndUserUniversalId(ISecurityContext securityContext, string azureTenantId, string userUniveralId)
        {
            UserAndTenantInfo info = await GetUserAndTenantByAzureTenantIdAndUserUniversalId(azureTenantId, userUniveralId);
            bool result = PopulateSecurityContext(securityContext, info, "universal ID (w/org UID)");
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for populating the security context with data.
        /// </summary>
        /// <param name="orgRoute">Route of the organization associated with the security context.</param>
        /// <param name="securityContext">Reference to the security</param>
        /// <param name="info">References to tenant, user, person, and org route information.</param>
        /// <param name="identifierName">Label to include in error messages to help identify the calling method.</param>
        private bool PopulateSecurityContext(ISecurityContext securityContext, UserAndTenantInfo info, string identifierName)
        {
            if (info == null || info.User == null)
            {
                Logger.LogInformation($"Failed to set user in the security context because the specified user could not be found by {identifierName} and organization route.");
                return false;
            }

            User user = Mapper.Map<User>(info.User);
            securityContext.CurrentUser = user;
            securityContext.CurrentUserId = info.User.Id;
            securityContext.UniversalId = info.User.UniversalId;
            securityContext.OrgRoute = info.OrgRoute;

            if (info.Tenant != null)
            {
                Tenant tenant = Mapper.Map<Tenant>(info.Tenant);
                securityContext.CurrentTenant(tenant);
                securityContext.CurrentTenantId = info.Tenant.Id;
            }

            if (info.Person != null)
            {
                Person person = Mapper.Map<Person>(info.Person);
                securityContext.CurrentPerson = person;
                SecurityContext.CurrentPersonId = info.Person.Id;
            }

            return true;
        }

        /// <summary>
        /// Retrieves user/tenant information by email address.
        /// </summary>
        /// <param name="orgRoute">Route of the org with which the user is associated.</param>
        /// <param name="email">Email address of the user being sought.</param>
        /// <returns>User and tenant information if found, otherwise <c>null</c>.</returns>
        private async Task<UserAndTenantInfo> GetUserAndTenantByOrgRouteAndUserEmail(string orgRoute, string email)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            //TODO: Figure out a way to do this in a single SQL call.

            // Attempt to locate the user/org information
            UserAndTenantInfo data = await db.People
                .Where(i => i.User.Email == email
                    && i.Organization.Route == orgRoute
                    && i.DeletedUtc == null
                    && i.User.DeletedUtc == null
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(i.Organization.Tenant, i, i.User, orgRoute))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Determine whether user/org information was located
            if (data == null)
            {
                // Attempt to lookup user information without org information
                data = await db.Users
                .Where(i => i.Email == email
                    && i.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(null, null, i, null))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            }

            return data;
        }

        /// <summary>
        /// Retrieves user/tenant information by universal ID.
        /// </summary>
        /// <param name="azureTenantId">Universal ID of the organization with which the user is associated.</param>
        /// <param name="userUniversalId">Universal ID of the user being sought.</param>
        /// <returns>User and tenant information if found, otherwise <c>null</c>.</returns>
        private async Task<UserAndTenantInfo> GetUserAndTenantByAzureTenantIdAndUserUniversalId(string azureTenantId, string userUniversalId)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            //TODO: Figure out a way to do this in a single SQL call.

            // Attempt to locate the user/org information
            UserAndTenantInfo data = await db.People
                .Where(i => i.User.UniversalId == userUniversalId
                    && i.Organization.AuthProviders.Any(i => i.ProviderType == AuthProviderType.Azure && i.ProviderUid == azureTenantId)
                    && i.DeletedUtc == null
                    && i.User.DeletedUtc == null
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(i.Organization.Tenant, i, i.User, i.Organization.Route))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Determine whether user/org information was located
            if (data == null)
            {
                // Attempt to lookup user information without org information
                data = await db.Users
                .Where(i => i.UniversalId == userUniversalId
                    && i.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(null, null, i, null))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            }

            return data;
        }

        /// <summary>
        /// Retrieves user/tenant information by user route.
        /// </summary>
        /// <param name="orgRoute">Route of the org with which the user is associated.</param>
        /// <param name="email">Email address of the user being sought.</param>
        /// <returns>User and tenant information if found, otherwise <c>null</c>.</returns>
        private async Task<UserAndTenantInfo> GetUserAndTenantByOrgRouteAndUserRoute(string orgRoute, string userRoute)
        {
            //TODO: Figure out a way to do this in a single SQL call.

            // Attempt to locate the user/org information
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserAndTenantInfo data = await db.People
                .Where(i => i.User.Route == userRoute
                    && i.Organization.Route == orgRoute
                    && i.DeletedUtc == null
                    && i.User.DeletedUtc == null
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(i.Organization.Tenant, i, i.User, orgRoute))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Determine whether user/org information was located
            if (data == null)
            {
                // Attempt to lookup user information without org information
                data = await db.Users
                .Where(i => i.Route == userRoute
                    && i.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(null, null, i, null))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            }

            return data;
        }

        /// <summary>
        /// Retrieves user/tenant information by universal ID.
        /// </summary>
        /// <param name="orgRoute">Route of the org with which the user is associated.</param>
        /// <param name="email">Email address of the user being sought.</param>
        /// <returns>User and tenant information if found, otherwise <c>null</c>.</returns>
        private async Task<UserAndTenantInfo> GetUserAndTenantByOrgRouteAndUserUniversalId(string orgRoute, string universalId)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            //TODO: Figure out a way to do this in a single SQL call.

            // Attempt to locate the user/org information
            UserAndTenantInfo data = await db.People
                .Where(i => i.User.UniversalId == universalId
                    && i.Organization.Route == orgRoute
                    && i.DeletedUtc == null
                    && i.User.DeletedUtc == null
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(i.Organization.Tenant, i, i.User, orgRoute))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            // Determine whether user/org information was located
            if (data == null)
            {
                // Attempt to lookup user information without org information
                data = await db.Users
                .Where(i => i.UniversalId == universalId
                    && i.DeletedUtc == null)
                .Select(i => new UserAndTenantInfo(null, null, i, null))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            }

            return data;
        }

        #endregion
    }
}