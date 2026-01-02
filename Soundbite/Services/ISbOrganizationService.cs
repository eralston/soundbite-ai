using Masticore.Services;
using Soundbite.Models;
using Soundbite.Settings;
using System;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the service operations required to manage organizations with feeds.
    /// </summary>
    /// <seealso cref="IService" />
    public interface ISbOrganizationService : IService
    {
        /// <summary>
        /// Async read the org data plus its relevant <see cref="OrgPermissions"/> object
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<OrganizationWithSettings> ReadAsync(string orgRoute);

        /// <summary>
        /// Async read the <see cref="OrgNotificationSettings"/> object for the given org
        /// </summary>
        /// <param name="orgRoute">Route of the org whose settings are being sought.</param>
        /// <returns>a <see cref="OrgNotificationSettings"/> populated with settings for the organization.</returns>
        Task<OrgNotificationSettings> ReadNotificationSettings(string orgRoute);

        /// <summary>
        /// Async read the <see cref="OrgSessionSettings"/> for the given organization.
        /// </summary>
        /// <param name="orgRoute">Route of the org whose settings are being sought.</param>
        /// <returns>a <see cref="OrgSessionSettings"/> populated with settings for the organization.</returns>
        Task<OrgSessionSettings> ReadSessionSettings(string orgRoute);

        /// <summary>
        /// Async set the <see cref="OrgNotificationSettings"/> object for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task<OrgNotificationSettings> UpdateNotificationSettings(string orgRoute, OrgNotificationSettings settings);

        /// <summary>
        /// Async set the <see cref="OrgSessionSettings"/> for the given organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to update.</param>
        /// <param name="settings">Settings to apply to the organization.</param>
        /// <returns>a <see cref="OrgSessionSettings"/> instance with current settings.</returns>
        Task<OrgSessionSettings> UpdateSessionSettings(string orgRoute, OrgSessionSettings settings);

        /// <summary>
        /// Async read the <see cref="OrgAzureSettings"/> object for the given org
        /// </summary>
        /// <param name="orgRoute">Route of the org whose settings are being sought.</param>
        /// <returns>a <see cref="OrgAzureSettings"/> populated with settings for the organization.</returns>
        Task<OrgAzureSettings> ReadAzureSettings(string orgRoute);

        /// <summary>
        /// Reads the Azure Media Services (AMS) streaming URL prefix for the organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose AMS streaming URL prefix is being sought.</param>
        /// <returns>a string containing the AMS streaming ULR prefix or <c>null</c> if the value is not set.</returns>
        Task<string> ReadAmsStreamingUrl(string orgRoute);

        /// <summary>
        /// Async set the <see cref="OrgAzureSettings"/> object for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task<OrgAzureSettings> UpdateAzureSettings(string orgRoute, OrgAzureSettings settings);

        /// <summary>
        /// Async read the <see cref="OrgAzureSettings"/> object for the given org
        /// </summary>
        /// <param name="orgRoute">Route of the org whose settings are being sought.</param>
        /// <param name="ignoreSecurity"></param>
        /// <returns>a <see cref="OrgPermissions"/> populated with settings for the organization.</returns>
        Task<OrgPermissions> ReadPermissions(string orgRoute, bool ignoreSecurity = false);

        /// <summary>
        /// Async set the <see cref="OrgPermissions"/> object for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task<OrgPermissions> UpdatePermissions(string orgRoute, OrgPermissions settings);

        /// <summary>
        /// TODO: REMOVE WHEN ABLE
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete("Deprecated for perf reasons; use ReadAsync")]
        Task<OrganizationWithPermissions_Obsolete> ReadAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Updates the theme object for the given org, then returns the finalized version which may be augmented or modified to its final ready state
        /// </summary>
        /// <remarks>Sending a null theme means it's being reset to the system default for the org</remarks>
        /// <param name="orgRoute"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        Task<Theme> UpdateTheme(string orgRoute, Theme theme);

        /// <summary>
        /// Sets the security token value used for authentication purposes.
        /// </summary>
        /// <param name="orgRoute">Route of the target organization.</param>
        /// <param name="token">New value of the token.</param>
        /// <returns>an OrgAzureSettings instance updated with the appropriate media service settings.</returns>
        Task SetSecurityTokenValue(string orgRoute, string token);

        /// <summary>
        /// Ensures that the AMS content key policy for the organization is in place.
        /// </summary>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <returns>a task indicating success or failure of the operation</returns>
        Task<string> SyncOrgTokenWithAmsContentKeyPolicy(string orgRoute);

        /// <summary>
        /// Ensures that the AMS content key policy for the organization is in place.
        /// </summary>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <param name="tokenValue">Token value for the content key policy (optional) - this should be looked up if not provided.</param>
        /// <returns>a task indicating success or failure of the operation</returns>
        Task<string> EnsureAmsContentKeyPolicy(string orgRoute, string tokenValue);
    }
}
