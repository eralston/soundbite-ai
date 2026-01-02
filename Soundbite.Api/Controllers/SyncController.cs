using Masticore;
using Masticore.DirectorySync;
using Masticore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{

    /// <summary>
    /// Controller with endpoints for configuration and execution of sync
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class SyncController : ControllerBase
    {
        #region Members

        private IOrgSyncService SyncService { get; }
        private IOrgSyncRunner SyncRunner { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionsController"/> class.
        /// </summary>
        /// <param name="syncService"><see cref="IOrgSyncService"/> object for the current request</param>
        public SyncController(IOrgSyncService syncService, IOrgSyncRunner syncRunner)
        {
            SyncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            SyncRunner = syncRunner ?? throw new ArgumentNullException(nameof(syncRunner));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Returns a complete analysis of either the current <see cref="OrgSyncConfig"/> or the given one
        /// </summary>
        /// <param name="orgRoute">Target organization</param>
        /// <param name="orgConfig">Null is always valid for saving as a config, but passing it here causes an error</param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/validate")]
        [HttpPost]
        public async Task<SyncValidation> ValidateAsync(string orgRoute, [FromBody][CodeGenField(IsNullable = true)] OrgSyncConfig orgConfig = null)
        {
            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            string valMsg = await SyncService.ValidationMessageAsync(orgRoute, orgConfig);

            return new SyncValidation
            {
                ValidationMessage = valMsg,
                OrgRoute = orgRoute,
                HasAccess = valMsg == null && await SyncService.HasAccessAsync(orgRoute, orgConfig),
                SanitizedConfig = await SyncService.SanitizeConfigAsync(orgRoute, orgConfig)
            };
        }

        /// <summary>
        /// Async gets the current sync settings for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/config")]
        [HttpGet]
        public async Task<SyncValidation> ReadConfigAsync(string orgRoute)
        {
            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            return new SyncValidation
            {
                ValidationMessage = await SyncService.ValidationMessageAsync(orgRoute),
                OrgRoute = orgRoute,
                HasAccess = await SyncService.HasAccessAsync(orgRoute),
                SanitizedConfig = await SyncService.ReadSanitizedConfigAsync(orgRoute)
            };
        }

        /// <summary>
        /// Sets the config for the given org
        /// Keep in mind that the orgConfig coming up will have been stripped of sensitive data
        /// it will likely NOT contains any secrets or continue context, so it must be restored from the DB where appropriate
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="orgConfig"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/config")]
        [HttpPost]
        public async Task<SyncValidation> UpdateConfigAsync(string orgRoute, [FromBody][CodeGenField(IsNullable = true)] OrgSyncConfig orgConfig = null)
        {
            await SyncService.UpdateConfigAsync(orgRoute, orgConfig);
            return await ReadConfigAsync(orgRoute);
        }

        /// <summary>
        /// Executes the sync for this org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync")]
        [HttpPost]
        public async Task<OrgSyncResult> SyncAsync(string orgRoute)
        {
            return await SyncRunner.SyncAsync(orgRoute);
        }

        /// <summary>
        /// Gets the sync run results for the given org, optionally starting on the given page
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/results")]
        [HttpGet]
        public async Task<IndexPageResponse<OrgSyncResult>> ReadResults(string orgRoute, [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page = null)
        {
            IndexPageResponse<OrgSyncResult> ret = await SyncService.ReadAllResultsAsync(orgRoute, page);
            return ret;
        }

        /// <summary>
        /// Gets a page of sync-able users for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/users/page")]
        [HttpGet]
        public async Task<TokenPageResponse<SyncTarget>> ReadUsersAsync(string orgRoute, [FromQuery][CodeGenField(IsNullable = true)] TokenPageRequest page)
        {
            return await SyncService.ReadUsersAsync(orgRoute, page);
        }

        /// <summary>
        /// Reads one specific <see cref="SyncTarget"/> record for the given userId, returning 404 if not found or the sync integration is not yet configured
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/users/{userId}")]
        [HttpGet]
        public async Task<SyncTarget> ReadUserAsync(string orgRoute, string userId)
        {
            return await SyncService.ReadUserAsync(orgRoute, userId);
        }

        /// <summary>
        /// Gets a page of sync-able groups in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/groups/page")]
        [HttpGet]
        public async Task<TokenPageResponse<SyncTarget>> ReadGroupsAsync(string orgRoute, [FromQuery][CodeGenField(IsNullable = true)] TokenPageRequest page)
        {
            return await SyncService.ReadGroupsAsync(orgRoute, page);
        }

        /// <summary>
        /// Gets one specific <see cref="SyncTarget"/> for the given group ID, returning 404 if not found or the sync integration is not yet configured
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sync/groups/{groupId}")]
        [HttpGet]
        public async Task<SyncTarget> ReadGroupAsync(string orgRoute, string groupId)
        {
            return await SyncService.ReadGroupAsync(orgRoute, groupId);
        }

        #endregion
    }
}