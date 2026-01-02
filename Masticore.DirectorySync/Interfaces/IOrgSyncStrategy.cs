using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Interface defining a type which implements a full synchronization process for a given organization via their chosen LDAP provider
    /// EG, there should be on AAD implementation, one OKTA implementation, ones for third-party white labels, etc
    /// </summary>
    public interface IOrgSyncStrategy
    {
        /// <summary>
        /// Responsible for running the directory synchronization of a single organization.
        /// </summary>
        /// <param name="orgResult"></param>
        /// <returns></returns>
        Task Sync(OrgSyncResult orgResult);

        /// <summary>
        /// Returns true if current org has access to the provider
        /// If false, then the org must grant it via manual or self-service setup
        /// </summary>
        /// <returns></returns>
        Task<bool> HasAccessAsync();

        /// <summary>
        /// Async get all available users with paging
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<TokenPageResponse<SyncTarget>> UsersAsync(TokenPageRequest page = null);

        /// <summary>
        /// Asynchronously acquires the associated SyncTarget information for the specified user 
        /// from the third party directory. This is primarily intended for populating display 
        /// information used in the UI.
        /// </summary>
        /// <param name="userId">Unique ID of the user in the third-party directory.</param>
        /// <returns>a <see cref="SyncTarget"/> instance populated with user information or <c>null</c> if the user is not found.</returns>        
        Task<SyncTarget> UserAsync(string userId);

        /// <summary>
        /// Asynchronously acquires all groups in the third-party directory with paging.
        /// </summary>
        /// <param name="page">Paged query information.</param>
        /// <returns>A <see cref="TokenPageResponse{SyncTarget}"/> containing results for the requested "page" of data.</returns>
        Task<TokenPageResponse<SyncTarget>> GroupsAsync(TokenPageRequest page = null);

        /// <summary>
        /// Asynchronously acquires the associated SyncTarget information for the specified group
        /// from the third party directory. This is primarily intended for populating display
        /// information used in the UI.
        /// </summary>
        /// <param name="groupId">Unique ID of the group in the third-party directory.</param>
        /// <returns>a <see cref="SyncTarget"/> instance populated with group information or <c>null</c> if the group is not found.</returns>        
        Task<SyncTarget> GroupAsync(string groupId);

        /// <summary>
        /// Checks for validation messages on the given orgConfig
        /// Returns null if no issues; otherwise, returns the validation issue
        /// </summary>
        /// <param name="orgConfig">Sync config settings to validate.</param>
        /// <returns>a string containing a validation error message or <c>null</c> if the sync config is valid.</returns>
        Task<string> ValidationMessagesAsync(OrgSyncConfig orgConfig);

        /// <summary>
        /// Removes sensitive data from the sync config so it can be sent to the client.
        /// </summary>
        /// <param name="orgConfig">Sync config settings to strip of sensitive data.</param>
        /// <returns>An <see cref="OrgSyncConfig"/> stripped of sensitive data that can be sent to a client.</returns>
        Task<OrgSyncConfig> SanitizeConfig(OrgSyncConfig orgConfig);

        /// <summary>
        /// Responsible for transferring any sanitized values from the current sync settings to the
        /// incoming sync settings. Sensitive information is stripped from the sync settings by
        /// the <see cref="SanitizeConfig(OrgSyncConfig)"/> method when being sent to the client and
        /// these values "disappear" if not accounted for in this method or set client-side. Please
        /// note that sometimes "sanitized" data is set by client and must be preferred over the
        /// old setting that was sanitized.
        /// </summary>
        /// <param name="oldOrgConfig">Existing sync settings containing sanitized values.</param>
        /// <param name="newOrgConfig">Incoming sync settings with sanitized values removed (or set by the client).</param>
        /// <returns>an <see cref="OrgSyncConfig"/> instance populated with data merged from the existing and incoming sync config settings.</returns>
        Task<OrgSyncConfig> TransferConfigAsync(OrgSyncConfig oldOrgConfig, OrgSyncConfig newOrgConfig);
    }
}
