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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// EF implementation of IOrganizationService
    /// </summary>
    public class OrganizationService : IdentityServiceBase, IOrganizationService
    {
        #region Properties
        private IImageService Images { get; }
        private IRbac Rbac { get; }

        #endregion

        #region Methods

        public OrganizationService(
            ISecurityContext securityContext,
            IIdentityInfrastructure infrastructure,
            IImageService imageService,
            IMapper mapper,
            ILogger<OrganizationService> logger,
            IRbac rbac
            )
            : base(infrastructure, logger, mapper, securityContext)
        {
            Images = imageService ?? throw new ArgumentNullException(nameof(imageService));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
        }

        /// <summary>
        /// Queries the <see cref="OrganizationDetails"/> for the given org.
        /// WARNING: This does NOT enforce RBAC
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        protected virtual async Task<OrganizationDetails> QueryOrganizationDetails(IIdentityDb db, string orgRoute)
        {
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            OrganizationDetails details = await db.OrganizationDetails(orgRoute);
            details.AssertFound($"Could not find details for org '{orgRoute}'");

            details.ImageSrc = await Images.OrgImageAsync(details);

            if (Rbac.UniversalIdForCurrentUser != null)
            {
                details.Me = await db.PersonWhereUid(Mapper, SecurityContext.UniversalId, orgRoute);
            }

            return details;
        }

        /// <summary>
        /// TODO: REMOVE WHEN ABLE
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete("Deprecated for performance reasons; use QueryOrganizationDetails")]
        protected virtual async Task<OrganizationDetails_Obsolete> QueryOrganizationDetails_Obsolete(IIdentityDb db, string orgRoute)
        {
            Validator.SecurityContextUniversalIdRequired(SecurityContext);

            OrganizationDetails_Obsolete details = await db.OrganizationDetails_Obsolete(orgRoute);
            details.AssertFound($"Could not find details for org '{orgRoute}'");

            details.ImageSrc = await Images.OrgImageAsync(details);

            details.Me = await db.PersonWhereUid(Mapper, SecurityContext.UniversalId, orgRoute);

            return details;
        }

        /// <summary>
        /// Responsible for creating a new organization.
        /// </summary>
        /// <param name="organization">Contains the property values for the new organization.</param>
        /// <param name="checkExisting">Flag indicating whether to check for an existing item.</param>
        /// <returns>a reference to the item that was created.</returns>
        private async Task<OrganizationExtended> CreateAsync(Organization organization, bool checkExisting)
        {
            // Validate
            Validator.ArgNotNull(nameof(organization), organization);
            Validator.ArgNotNull("organization.Name", organization.Name);
            Validator.ArgValidate("organization.Route", organization.Route, i => string.IsNullOrEmpty(i), "Route property on organization must be null or empty.");
            Validator.Validate(await SecurityContext.CurrentTenant(), (i) => i != null, "Tenant is not associated with the user.");

            await Rbac.AssertCurrentUserInRole(UserRole.God);

            organization.NewRoute();
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity existing = checkExisting ? await db.Organizations.Where(i => i.Route == organization.Route).FirstOrDefaultAsync() : null;
            if (existing != null)
            {
                throw new InvalidOperationException("New organization was assigned the same route as an existing organization.  Please try recreating the organization.");
            }

            Tenant tenantInfo = await SecurityContext.CurrentTenant();
            TenantEntity tenant = await db.Tenants.Where(i => i.Route == tenantInfo.Route).FirstOrDefaultAsync();
            UserEntity currentUser = await db.UserWhereRoute(SecurityContext.CurrentUser.Route);

            OrganizationEntity org = EntityBase.Create<OrganizationEntity>(SecurityContext.CurrentUserId);
            Map<Organization, OrganizationEntity>(organization, org);
            org.SetCreatedFields(currentUser);
            org.Tenant = tenant;
            org.NewUniversalId();

            // Setup the user who created the organization as the default owner
            PersonEntity owner = new PersonEntity()
            {
                CreatedById = SecurityContext.CurrentUserId,
                CreatedUtc = Time.UtcNow,
                InviteAcceptUtc = null,
                InviteUtc = null,
                Organization = org,
                PersonRole = PersonRole.Admin,
                UserId = SecurityContext.CurrentUserId
            };

            owner.NewRoute();

            org.People = new Collection<PersonEntity> { owner };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();

            OrganizationExtended result = Mapper.MapSafe<OrganizationExtended>(org);
            return result;
        }

        /// <summary>
        /// Calls out to the image service to acquire the image URLs for the organizations.
        /// </summary>
        /// <param name="orgs">organizations whose image sources should be populated.</param>
        private async Task PopulateOrgImageSrc(IEnumerable<OrganizationExtended> orgs)
        {
            // TODO: we should have a "HasImage" flag to help avoid unnecessarily calling out for image sources when they don't exist
            IEnumerable<Task<string>> tasks = orgs.Select(o => Images.OrgImageAsync(o));
            string[] results = await Task.WhenAll(tasks);
            orgs.ForEach((o, index) => o.ImageSrc = results[index]);
        }

        #endregion

        #region IOrganizationService

        public async Task<OrganizationDetails> ReadAsync(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(UserRole.User);

            // TODO: Caching?

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationDetails details = await QueryOrganizationDetails(db, orgRoute);

            return details;
        }

        [Obsolete("Deprecated for performance reasons; use ReadAsync")]
        public async Task<OrganizationDetails_Obsolete> ReadAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(UserRole.User);

            // TODO: Caching?

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationDetails_Obsolete details = await QueryOrganizationDetails_Obsolete(db, orgRoute);
            details.AssertFound($"Could not find details for org '{orgRoute}'");

            details.Groups = await db.QueryGroups(orgRoute)
                .MapReadOnlyAsync((g) => Mapper.MapSafe<Group>(g));

            details.MyGroups =
                await db.QueryMyGroups(orgRoute, SecurityContext.UniversalId)
                .MapReadOnlyAsync((g) => Mapper.MapSafe<Group>(g));

            return details;
        }

        public async Task<OrgSyncConfig> ReadSyncConfigAsync(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = (await db.OrgWhereRoute(orgRoute)).AssertFound($"Org '{orgRoute}' not found");
            OrgSyncConfig orgConfig = org.GetOrgSyncConfig();

            return orgConfig;
        }

        public async Task UpdateSyncConfigAsync(string orgRoute, OrgSyncConfig orgConfig)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = (await db.OrgWhereRoute(orgRoute)).AssertFound($"Org '{orgRoute}' not found");

            org.SyncConfigJson = orgConfig?.SyncConfigJson;
            org.SyncType = orgConfig?.SyncType; // null here disables the whole system

            await db.SaveChangesAsync();
        }

        [Obsolete]
        public virtual async Task<IEnumerable<OrganizationExtended>> ReadAllAsync_Obsolete(bool includeAll)
        {
            UserRole userRole = includeAll ? UserRole.God : UserRole.User;
            await Rbac.AssertCurrentUserInRole(userRole);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            Validator.SecurityContextUniversalIdRequired(SecurityContext);

            OrganizationExtended[] orgs;

            if (includeAll)
            {
                orgs = await db.ActiveOrganizations();
            }
            else
            {
                orgs = await db.MyOrganizations(SecurityContext);
            }

            await PopulateOrgImageSrc(orgs);

            return orgs;
        }

        public async Task<IEnumerable<OrganizationExtended>> ReadAllArchivedAsync_Obsolete()
        {
            Validator.SecurityContextUniversalIdRequired(SecurityContext);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationExtended[] orgs = await db.ArchivedOrganizations(SecurityContext);
            await PopulateOrgImageSrc(orgs);

            return orgs;
        }

        /// <summary>
        /// Responsible for creating a new organization.
        /// </summary>
        /// <param name="organization">Contains the property values for the new organization.</param>
        /// <param name="checkExisting">Flag indicating whether to check for an existing item.</param>
        /// <returns>a reference to the item that was created.</returns>
        public async Task<OrganizationExtended> CreateAsync(Organization organization)
        {
            return await CreateAsync(organization, true);
        }

        public async Task DeleteAsync(string orgRoute)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(UserRole.God);

            // Perform Operation
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find organization '{orgRoute}'");
            org.SoftDelete();
            await db.SaveChangesAsync();
        }

        public async Task<OrganizationExtended> RestoreAsync(string orgRoute)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            await Rbac.AssertCurrentUserInRole(UserRole.God);

            // Perform Operation
            OrganizationExtended result = null;
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.Organizations.FirstOrDefaultAsync(i => i.Route == orgRoute
                && i.DeletedUtc != null
                && i.Tenant.DeletedUtc == null);
            if (org != null)
            {
                org.DeletedUtc = null;
                db.Organizations.Update(org);
                await db.SaveChangesAsync();
                result = Mapper.MapSafe<OrganizationExtended>(org);
                result.ImageSrc = await Images.OrgImageAsync(result);
                return result;
            }

            return result;
        }

        /// <summary>
        /// Gets the route of an organization based on the organization's universal ID.
        /// </summary>
        /// <param name="universalId">Universal ID of the organization whose route is being sought.</param>
        /// <returns>The route associated with the organization with the specified universal ID or <c>null</c> if the organization is not found.</returns>
        public async Task<string> GetRouteFromUniversalId(string universalId)
        {
            //Validate parameters
            Validator.ArgNotNull(nameof(universalId), universalId);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            string result = await db.Organizations
                    .Where(i => i.UniversalId == universalId
                        && i.DeletedUtc == null
                        && i.Tenant.DeletedUtc == null)
                    .Select(i => i.Route)
                    .FirstOrDefaultAsync();

            return result;
        }

        public async Task<OrganizationExtended> PatchAsync(string orgRoute, JsonPatchDocument updates)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(updates), orgRoute);
            Validator.ArgValidate(nameof(updates), updates, i => i.Operations?.Count > 0, "One or more operations should be present in the patch document.");

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            // Validate Business Rules
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find organization '{orgRoute}'");
            if (org.DeletedUtc != null)
            {
                throw new InvalidOperationException("Cannot update an archived organization.");
            }

            updates.ApplyTo(org);
            org.SetUpdatedFields(SecurityContext);
            await db.SaveChangesAsync();

            //TODO: this is going to be missing the ImageSrc + IsAccepting properties.  Need to figure out if those ar worth repopulating, what ramifications this has
            OrganizationExtended result = Mapper.MapSafe<OrganizationExtended>(org);
            result.ImageSrc = await Images.OrgImageAsync(result);
            return result;
        }

        public async Task<OrganizationExtended> SaveAsync(Organization organization)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(organization), organization);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity existing = string.IsNullOrEmpty(organization.Route) ? null : await db.Organizations.Where(i => i.Route == organization.Route).FirstOrDefaultAsync();

            // Determine whether this is an insert or update
            if (existing == null)
            {
                await Rbac.AssertCurrentUserInRole(UserRole.God);
                return await CreateAsync(organization, false);
            }
            else
            {
                // Validate User
                await Rbac.AssertCurrentUserInRole(organization.Route, PersonRole.Admin);

                // Validate Business Rules
                if (existing.DeletedUtc != null)
                {
                    throw new InvalidOperationException("Cannot update an archived organization.");
                }

                // Perform operation
                Map(organization, existing);
                existing.SetUpdatedFields(SecurityContext);
                db.Update(existing);
                await db.SaveChangesAsync();

                OrganizationExtended result = Mapper.MapSafe<OrganizationExtended>(existing);
                return result;
            }
        }

        /// <summary>
        /// Persists an organization image to a data store.
        /// </summary>
        /// <param name="orgRoute">Unique but non-identifying arbitrary key associated with the organization (for now use orgRoute)</param>
        /// <param name="stream">Stream containing image data.</param>
        /// <returns>a URL pointing to the image that was uploaded.</returns>
        public async Task<string> UploadImage(string orgRoute, Stream image)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(image), image);

            // Validate User
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            // Pull out the org
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity orgEntity = await db.OrgWhereRoute(orgRoute);
            Organization org = Mapper.MapSafe<Organization>(orgEntity);

            // Upload to it
            string result = await Images.UploadOrgImageAsync(org, image);

            return result;
        }

        /// <summary>
        /// Determines whether the specified user is in the specified organization.
        /// </summary>
        /// <param name="userRoute">Route of the user.</param>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <returns>a task result containing <c>true</c> if the user is part of the organization; otherwise <c>false</c>.</returns>
        public async Task<bool> IsUserInOrg(string userRoute, string orgRoute)
        {
            // Validate parameters
            Validator.ArgNotNull(nameof(userRoute), userRoute);
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            bool result = await db.People
                    .Where(i =>
                        i.User.Route == userRoute
                        && i.User.DeletedUtc == null
                        && i.Organization.Route == orgRoute
                        && i.Organization.DeletedUtc == null
                        && i.Organization.Tenant.DeletedUtc == null)
                    .AnyAsync();

            return result;
        }

        /// <summary>
        /// Throw an exception if the current user is NOT in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="personRole"></param>
        /// <returns></returns>
        public async Task AssertCurrentUserInOrg(string orgRoute, PersonRole personRole = PersonRole.Guest)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            PersonEntity myPerson = await db.PersonWhereUid(orgRoute, SecurityContext.UniversalId, personRole);
            myPerson.AssertFound();
        }

        /// <summary>
        /// Saves the organization settings for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization with which the settings are associated.</param>
        /// <param name="orgSettingsJson">JSON string to save.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        public async Task SaveOrgSettingsAsync(string orgRoute, string orgSettingsJson)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.Organizations
                .Where(i => i.DeletedUtc == null
                    && i.Tenant.DeletedUtc == null
                    && i.Route == orgRoute)
                .FirstOrDefaultAsync();
            org.AssertFound();
            org.SetUpdatedFields(SecurityContext);
            org.ConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(orgSettingsJson);
            db.Organizations.Update(org);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the organization configuration settings.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose settings are being sought.</param>
        /// <returns>a JSON string containing configuration settings (value may also be <c>null</c>).</returns>
        public async Task<string> ReadOrgSettingsAsync(string orgRoute)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            string configJson = await db.Organizations
                .Where(i => i.DeletedUtc == null
                    && i.Tenant.DeletedUtc == null
                    && i.Route == orgRoute)
                .Select(i => i.ConfigJson).FirstOrDefaultAsync();
            return configJson;
        }

        #endregion
    }
}
