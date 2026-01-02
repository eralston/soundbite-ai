using Masticore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Sync action itself, which may have more varied implementation
    /// </summary>
    public interface IOrgSyncRunner
    {
        /// <summary>
        /// Implements the sync process for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="result">Result object enabling passthrough of partial sync process</param>
        /// <returns></returns>
        Task<OrgSyncResult> SyncAsync(string orgRoute, OrgSyncResult result = null);
    }

    /// <summary>
    /// Describes an object that supports not only <see cref="IOrgSyncRunner"/>, but all supporting methods as well
    /// </summary>
    /// <remarks>This includes providing <see cref="SyncTarget"/> to build up and <see cref="OrgSyncResult"/> to report on sync runs </remarks>
    public interface IOrgSyncService : IOrgSyncRunner
    {
        /// <summary>
        /// Returns true if current org has access to the provider
        /// If false, then the org must grant it via manual or self-service setup
        /// </summary>
        /// <param name="orgRoute">Route for the current org; current user must be an admin or better</param>
        /// <returns></returns>
        Task<bool> HasAccessAsync(string orgRoute);

        /// <summary>
        /// Returns true if the given org and <see cref="OrgSyncConfig"/> has access to their provider given current settings
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="orgConfig"></param>
        /// <returns></returns>
        Task<bool> HasAccessAsync(string orgRoute, OrgSyncConfig orgConfig);

        /// <summary>
        /// Returns the validation message for the current org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<string> ValidationMessageAsync(string orgRoute);

        /// <summary>
        /// Returns the validation message for the current <see cref="OrgSyncConfig"/> for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="orgConfig"></param>
        /// <returns></returns>
        Task<string> ValidationMessageAsync(string orgRoute, OrgSyncConfig orgConfig);

        /// <summary>
        /// Returns the sanitized config based on its provider type
        /// If the provider type is missing, this returns null
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="orgConfig"></param>
        /// <returns></returns>
        Task<OrgSyncConfig> SanitizeConfigAsync(string orgRoute, OrgSyncConfig orgConfig);

        /// <summary>
        /// Reads the config for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<OrgSyncConfig> ReadSanitizedConfigAsync(string orgRoute);

        /// <summary>
        /// Updates the given org to have the given config
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        Task UpdateConfigAsync(string orgRoute, OrgSyncConfig config);

        /// <summary>
        /// Reads all stored <see cref="OrgSyncResult"/> objects for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete]
        Task<IEnumerable<OrgSyncResult>> ReadAllResultsAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Paging query for the given set of run results
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<IndexPageResponse<OrgSyncResult>> ReadAllResultsAsync(string orgRoute, IndexPageRequest page = null);

        /// <summary>
        /// Gets all active users in the given org with paging
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<TokenPageResponse<SyncTarget>> ReadUsersAsync(string orgRoute, TokenPageRequest page = null);

        /// <summary>
        /// Reads a single user from the Sync system, returning its complete data; useful for pre-loading items
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<SyncTarget> ReadUserAsync(string orgRoute, string userId);

        /// <summary>
        /// Gets all active groups in the given org by page with paging
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<TokenPageResponse<SyncTarget>> ReadGroupsAsync(string orgRoute, TokenPageRequest page = null);

        /// <summary>
        /// Reads a single group from the Sync system, returning its complete data; useful for pre-loading items
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<SyncTarget> ReadGroupAsync(string orgRoute, string groupId);
    }
}