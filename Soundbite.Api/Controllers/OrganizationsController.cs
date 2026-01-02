using Masticore;
using Masticore.Models;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Models;
using Soundbite.Services;
using Soundbite.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.App.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with Organizations.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        #region Properties

        private IOrganizationService Organizations { get; }
        private ISbOrganizationService SbOrganizations { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationsController"/> class.
        /// </summary>
        /// <param name="orgService">IOrganizationService DI reference.</param>
        /// <param name="sbOrgService">IOrganizationWithFeedService DI reference.</param>
        public OrganizationsController(
            IOrganizationService orgService,
            ISbOrganizationService sbOrgService)
        {
            Organizations = orgService ?? throw new ArgumentNullException(nameof(orgService));
            SbOrganizations = sbOrgService ?? throw new ArgumentNullException(nameof(sbOrgService));
        }

        #endregion

        #region EndPoints

        /// <summary>
        /// Gets a list of all the active organizations to which the current user has access.
        /// </summary>
        /// <returns>a list of all the active organizations to which the current user has access.</returns>
        [Route("/organizations")]
        [HttpGet]
        public async Task<IEnumerable<OrganizationExtended>> ReadAllAsync()
        {
            IEnumerable<OrganizationExtended> result = await Organizations.ReadAllAsync_Obsolete(false);
            return result;
        }

        /// <summary>
        /// Creates a new organization
        /// </summary>
        /// <param name="organization">Organization to create.</param>
        /// <returns>a reference to the new organization populated with any server-side values.</returns>
        [Route("/organizations/")]
        [HttpPost]
        public async Task<OrganizationExtended> CreateAsync([FromBody] Organization organization)
        {
            OrganizationExtended result = await Organizations.CreateAsync(organization);
            return result;
        }

        /// <summary>
        /// Gets an array containing all of the active organizations in the system.
        /// </summary>
        /// <returns>an array containing all of the active organizations in the system.</returns>
        [Route("/organizations/all")]
        [HttpGet]
        public async Task<IEnumerable<OrganizationExtended>> ReadAllAsGodAsync()
        {
            IEnumerable<OrganizationExtended> result = await Organizations.ReadAllAsync_Obsolete(true);
            return result;
        }

        /// <summary>
        /// Gets a list of all the archived organizations to which the current user has access.
        /// </summary>
        /// <returns>a list of all the archived organizations to which the current user has access.</returns>
        [Route("/organizations/archived")]
        [HttpGet]
        public async Task<IEnumerable<OrganizationExtended>> ReadAllArchivedAsync()
        {
            IEnumerable<OrganizationExtended> result = await Organizations.ReadAllArchivedAsync_Obsolete();
            return result;
        }

        /// <summary>
        /// Gets an organization by route.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to retrieve.</param>
        /// <returns>a organization instance with feed information if found, otherwise throws an HTTP 404 not found exception.</returns>
        [Route("/organizations/{orgRoute}")]
        [HttpGet]
        public async Task<OrganizationWithSettings> ReadAsync(string orgRoute)
        {
            return await SbOrganizations.ReadAsync(orgRoute);
        }

        /// <summary>
        /// Performs a "soft" delete of the organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to "soft" delete.</param>
        [Route("/organizations/{orgRoute}")]
        [HttpDelete]
        public async Task DeleteAsync(string orgRoute)
        {
            await Organizations.DeleteAsync(orgRoute);
        }

        /// <summary>
        /// Read the <see cref="OrgNotificationSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/settings/notifications")]
        [HttpGet]
        public async Task<OrgNotificationSettings> ReadNotificationSettingsAsync(string orgRoute)
        {
            return await SbOrganizations.ReadNotificationSettings(orgRoute);
        }

        /// <summary>
        /// Read the <see cref="OrgSessionSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/settings/sessions")]
        [HttpGet]
        public async Task<OrgSessionSettings> ReadSessionSettingsAsync(string orgRoute)
        {
            OrgSessionSettings result = await SbOrganizations.ReadSessionSettings(orgRoute);
            return result;
        }

        /// <summary>
        /// Update the <see cref="OrgNotificationSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/settings/notifications")]
        [HttpPut]
        public async Task<OrgNotificationSettings> UpdateNotificationSettingsAsync(string orgRoute, [FromBody] OrgNotificationSettings settings)
        {
            return await SbOrganizations.UpdateNotificationSettings(orgRoute, settings);
        }

        /// <summary>
        /// Update the <see cref="OrgSessionSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute">Route of the organization to update.</param>
        /// <param name="settings">Session settings to apply to the organization.</param>
        /// <returns>a <see cref="OrgSessionSettings"/> instance with current settings.</returns>
        [Route("/organizations/{orgRoute}/settings/sessions")]
        [HttpPut]
        public async Task<OrgSessionSettings> UpdateSessionSettingsAsync(string orgRoute, [FromBody] OrgSessionSettings settings)
        {
            OrgSessionSettings result = await SbOrganizations.UpdateSessionSettings(orgRoute, settings);
            return result;
        }

        /// <summary>
        /// Read the <see cref="OrgAzureSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose Azure settings are being requested.</param>
        /// <returns>the requested azure settings or <c>null</c> if the settings are not available.</returns>
        [Route("/organizations/{orgRoute}/settings/azure")]
        [HttpGet]
        public async Task<OrgAzureSettings> ReadAzureSettingsAsync(string orgRoute)
        {
            OrgAzureSettings settings = await SbOrganizations.ReadAzureSettings(orgRoute);
            return settings;
        }

        /// <summary>
        /// Update the <see cref="OrgAzureSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/settings/azure")]
        [HttpPut]
        public async Task<OrgAzureSettings> UpdateAzureSettingsAsync(string orgRoute, [FromBody] OrgAzureSettings settings)
        {
            return await SbOrganizations.UpdateAzureSettings(orgRoute, settings);
        }

        /// <summary>
        /// Read the <see cref="OrgAzureSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/settings/permissions")]
        [HttpGet]
        public async Task<OrgPermissions> ReadPermissionsAsync(string orgRoute)
        {
            return await SbOrganizations.ReadPermissions(orgRoute);
        }

        /// <summary>
        /// Update the <see cref="OrgAzureSettings"/> for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/settings/permissions")]
        [HttpPut]
        public async Task<OrgPermissions> UpdatePermissionsAsync(string orgRoute, [FromBody] OrgPermissions settings)
        {
            return await SbOrganizations.UpdatePermissions(orgRoute, settings);
        }

        /// <summary>
        /// Restores an organization that was "soft" deleted.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to restore.</param>
        /// <returns>a organization instance if found, otherwise throws an HTTP 404 not found exception.</returns>
        [Route("/organizations/{orgRoute}/restore")]
        [HttpPost]
        public async Task<OrganizationExtended> RestoreAsync(string orgRoute)
        {
            OrganizationExtended result = await Organizations.RestoreAsync(orgRoute);
            return result;
        }

        /// <summary>
        /// Patches the specified organization according to the patch instructions.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to patch.</param>
        /// <param name="changes">Patch instructions identifying what changes to apply to the organization.</param>
        /// <returns>an organization instance containing updated information (if found), otherwise throws an HTTP 404 not found exception.</returns>
        [Route("/organizations/{orgRoute}")]
        [HttpPatch]
        public async Task<OrganizationExtended> PatchAsync(string orgRoute, [FromBody] JsonPatchDocument changes)
        {
            OrganizationExtended result = await Organizations.PatchAsync(orgRoute, changes);
            return result;
        }

        //TODO: may need additional details in this one

        /// <summary>
        /// Uploads an "avatar" image to associate with the organization.  The image sent to this
        /// endpoint must be included in the body of the request a FormData file.  Any file name 
        /// associated with the image file in the FormData is discarded.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the image is associated.</param>
        /// <returns>a string containing the URL at which the image may be retrieved. This value contains a short lived access token.</returns>
        [Route("/organizations/{orgRoute}/image")]
        [HttpPost]
        [CodeGenMethod(Scenario = CodeGenScenarioType.FileUploadSingleFromFormData)]
        public async Task<string> UploadImageAsync(string orgRoute)
        {
            if (Request?.Form?.Files?.Count > 0)
            {
                using System.IO.Stream stream = Request.Form.Files[0].OpenReadStream();
                string url = await Organizations.UploadImage(orgRoute, stream);
                return url;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Updates the theme for the given org; null theme indicating we want to clear the theme back to default
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/theme")]
        [HttpPost]
        public async Task<Theme> UpdateThemeAsync(string orgRoute, [CodeGenField(IsNullable = true)][FromBody] Theme theme)
        {
            Theme result = await SbOrganizations.UpdateTheme(orgRoute, theme);
            return result;
        }

        /// <summary>
        /// Ensures that there is an Azure Media Services account setup and associated with the org.
        /// </summary>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/setOrgToken")]
        [HttpPost]
        public async Task SetOrgToken(string orgRoute, [FromBody] string newToken)
        {
            await SbOrganizations.SetSecurityTokenValue(orgRoute, newToken);
        }

        /// <summary>
        /// Ensures that there is an Azure Media Services account setup and associated with the org.
        /// </summary>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/syncOrgTokenWithAmsContentKeyPolicy")]
        [HttpPut]
        public async Task SyncOrgTokenWithAmsContentKeyPolicy(string orgRoute)
        {
            await SbOrganizations.SyncOrgTokenWithAmsContentKeyPolicy(orgRoute);
        }

        /// <summary>
        /// Ensures that there is an Azure Media Services account setup and associated with the org.
        /// </summary>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/EnsureAmsContentKeyPolicy")]
        [HttpPost]
        public async Task<string> EnsureAmsContentKeyPolicy(string orgRoute)
        {
            return await SbOrganizations.EnsureAmsContentKeyPolicy(orgRoute, null);
        }

        /// <summary>
        /// Retrieves the Azure Media Services (AMS) streaming URL prefix for the organization.
        /// This value is required for the public media player.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose AMS streaming URL prefix is being sought.</param>
        /// <returns>a string containing the AMS prefix URL or <c>null</c> if the value is not configured.</returns>
        [Route("/organizations/{orgRoute}/readAmsStreamingUrl")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<string> ReadAmsStreamingUrl(string orgRoute)
        {
            string result = await SbOrganizations.ReadAmsStreamingUrl(orgRoute);
            return result;
        }

        #endregion
    }
}
