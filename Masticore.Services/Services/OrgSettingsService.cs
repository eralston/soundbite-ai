using AutoMapper;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// EF implementation of IOrganizationService
    /// </summary>
    public class OrgSettingsService : IdentityServiceBase, IOrgSettingsService
    {
        #region Properties

        private IRbac Rbac { get; }

        #endregion

        #region Constructor

        public OrgSettingsService(
            ISecurityContext securityContext,
            IIdentityInfrastructure infrastructure,
            IMapper mapper,
            ILogger<OrganizationService> logger,
            IRbac rbac
            )
            : base(infrastructure, logger, mapper, securityContext)
        {
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
        }

        #endregion

        #region IOrgSettingsService Implementation

        /// <summary>
        /// Saves the organization settings for the specified organization.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings object being saved.</typeparam>
        /// <param name="orgRoute">Route of the organization with which the settings are associated.</param>
        /// <param name="orgSettings">Organization settings object to persist.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        public async Task SaveOrgSettingsAsync<T>(string orgRoute, T orgSettings)
            where T : IOrgSettings, new()
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.Organizations
                .Where(i => i.DeletedUtc == null
                    && i.Tenant.DeletedUtc == null
                    && i.Route == orgRoute)
                .FirstOrDefaultAsync();
            org.AssertFound();
            org.SetUpdatedFields(SecurityContext);
            org.ConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(orgSettings);
            db.Organizations.Update(org);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the organization configuration settings.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings to retrieve.</typeparam>
        /// <param name="orgRoute">Route of the organization whose settings are being sought.</param>        
        /// <returns>a settings object ready for use.  A default settings object is created if settings are not present.</returns>
        public async Task<T> ReadOrgSettingsAsync<T>(string orgRoute)
            where T : IOrgSettings, new()
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            string configJson = await db.Organizations
                .Where(i => i.DeletedUtc == null
                    && i.Tenant.DeletedUtc == null
                    && i.Route == orgRoute)
                .Select(i => i.ConfigJson).FirstOrDefaultAsync();
            T result = string.IsNullOrEmpty(configJson) ? default(T)
                : Newtonsoft.Json.JsonConvert.DeserializeObject<T>(configJson);
            return result;
        }

        #endregion
    }
}