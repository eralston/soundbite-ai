using Masticore.Models;
using Masticore.Services;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of <see cref="IOrganizationService"/>
    /// </summary>
    public class MockOrgService : IOrganizationService
    {
        public static string ValidOrgRoute => nameof(ValidOrgRoute);

        public Task<OrganizationExtended> CreateAsync(Organization organization)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string orgRoute)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRouteFromUniversalId(string universalId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserInOrg(string userRoute, string orgRoute)
        {
            throw new NotImplementedException();
        }

        public Task<OrganizationExtended> PatchAsync(string orgRoute, JsonPatchDocument updates)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrganizationExtended>> ReadAllArchivedAsync_Obsolete()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public Task<IEnumerable<OrganizationExtended>> ReadAllAsync_Obsolete(bool includeAll)
        {
            throw new NotImplementedException();
        }

        public Task<OrganizationDetails> ReadAsync(string orgRoute)
        {
            throw new NotImplementedException();
        }

        public OrgSyncConfig OrgSyncConfig { get; set; }

        public Task<OrgSyncConfig> ReadSyncConfigAsync(string orgRoute)
        {
            return Task.FromResult(OrgSyncConfig);
        }

        public Task<OrganizationExtended> RestoreAsync(string orgRoute)
        {
            throw new NotImplementedException();
        }

        public Task<OrganizationExtended> SaveAsync(Organization organization)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSyncConfigAsync(string orgRoute, OrgSyncConfig orgConfig)
        {
            OrgSyncConfig = orgConfig;
            return Task.CompletedTask;
        }

        public Task<string> UploadImage(string orgRoute, Stream image)
        {
            throw new NotImplementedException();
        }

        public Task AssertCurrentUserInOrg(string orgRoute, PersonRole personRole)
        {
            // Nothing to do
            return Task.CompletedTask;
        }

        [Obsolete]
        public Task<OrganizationDetails_Obsolete> ReadAsync_Obsolete(string orgRoute)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the organization settings for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization with which the settings are associated.</param>
        /// <param name="orgSettingsJson">JSON string to save.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        public Task SaveOrgSettingsAsync(string orgRoute, string orgSettingsJson)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Retrieves the organization configuration settings.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose settings are being sought.</param>
        /// <returns>a JSON string containing configuration settings (value may also be <c>null</c>).</returns>
        public Task<string> ReadOrgSettingsAsync(string orgRoute)
        {
            throw new NotImplementedException();
        }
    }
}
