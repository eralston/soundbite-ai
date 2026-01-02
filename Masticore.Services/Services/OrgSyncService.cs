using Masticore.DirectorySync;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Implementation of <see cref="IOrgSyncService"/>
    /// </summary>
    public class OrgSyncService : IOrgSyncService
    {
        #region Fields & Properties

        private IOrgSyncStrategy _syncStrategy;

        /// <summary>
        /// Gets the <see cref="IOrganizationService"/> for this request
        /// </summary>
        protected IOrganizationService OrgService { get; }

        /// <summary>
        /// Gets the sync <see cref="ISyncStrategyFactory"/> for this request
        /// </summary>
        protected ISyncStrategyFactory SyncFactory { get; }

        /// <summary>
        /// Gets the <see cref="ILogger"/> instance for this request
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// The object that will persist changes to Soundbite
        /// </summary>
        protected IOrgSyncStore OrgSyncStore { get; }

        /// <summary>
        /// The object providing resources to read/write sync run data
        /// </summary>
        protected ISyncInfrastucture SyncInfrastucture { get; }

        /// <summary>
        /// Gets the RBAC component for this service, enforcing roles
        /// </summary>
        protected IRbac Rbac { get; }

        #endregion

        #region Methods

        /// <summary>
        /// /// <summary>
        /// Initializes a new instance of the <see cref="OrgSyncService"/> class.
        /// </summary>
        /// <param name="orgService"><see cref="IOrganizationService"/> object for the current request</param>
        /// <param name="syncFactory"><see cref="ISyncStrategyFactory"/> object for the current request</param>
        /// /// <param name="logger"><see cref="ILogger"/> object for the current request</param>
        /// <param name="logger"><see cref="ILogger"/> object for the current request</param>
        /// <param name="syncFactory"><see cref="ISyncStrategyFactory"/> object for the current request</param>
        /// <param name="orgService"><see cref="IOrganizationService"/> object for the current request</param>
        public OrgSyncService(
            ILogger<OrgSyncService> logger,
            ISyncStrategyFactory syncFactory,
            ISyncInfrastucture syncInfrastructure,
            IOrganizationService orgService,
            IOrgSyncStore orgSyncStore,
            IRbac rbac
            )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SyncFactory = syncFactory ?? throw new ArgumentNullException(nameof(syncFactory));
            SyncInfrastucture = syncInfrastructure ?? throw new ArgumentNullException(nameof(syncInfrastructure));
            OrgService = orgService ?? throw new ArgumentNullException(nameof(orgService));
            OrgSyncStore = orgSyncStore ?? throw new ArgumentNullException(nameof(orgSyncStore));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
        }

        /// <summary>
        /// Generates an <see cref="IOrgSyncStrategy"/> for the given <see cref="OrgSyncConfig"/>
        /// </summary>
        /// <param name="currentOrgConfig"></param>
        /// <param name="noCache"></param>
        /// <returns></returns>
        private IOrgSyncStrategy SyncStrategy(OrgSyncConfig currentOrgConfig, OrgSyncConfig oldOrgConfig = null, bool noCache = true)
        {
            if (currentOrgConfig is null)
            {
                throw new ArgumentNullException(nameof(currentOrgConfig));
            }

            if (_syncStrategy == null || noCache)
            {
                IOrgSyncStrategy provider = SyncFactory.GetOrgStrategy(currentOrgConfig, Logger, OrgSyncStore, oldOrgConfig);
                if (provider is null)
                {
                    throw new ArgumentException($"Cannot find sync provider for type {currentOrgConfig.SyncType}");
                }
                if (noCache)
                {
                    return provider;
                }

                _syncStrategy = provider;
            }

            return _syncStrategy;
        }

        /// <summary>
        /// Ensures the given provider is ready for sync
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="orgConfig"></param>
        /// <param name="syncProvider"></param>
        /// <returns></returns>
        private static async Task ValidateSyncReady(string orgRoute, OrgSyncConfig orgConfig, IOrgSyncStrategy syncProvider)
        {
            // Ensure provider found
            if (syncProvider is null)
            {
                string msg = $"Provider for type '{orgConfig.SyncType}' was not found for organization '{orgConfig.OrgRoute}'.";
                throw new InvalidOperationException(msg);
            }

            // Check access
            bool hasAccess = await syncProvider.HasAccessAsync();

            if (!hasAccess)
            {
                throw new UserSafeException($"Cannot sync: No connection for org '{orgRoute}'");
            }
        }

        /// <summary>
        /// without ever throwing an exception, saves a new <see cref="SyncRunEntity"/> into the <see cref="ISyncDb"/>
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="orgConfig"></param>
        /// <param name="orgResult"></param>
        /// <param name="syncProvider"></param>
        /// <returns></returns>
        private async Task<OrgSyncResult> TrySaveRunAsync(string orgRoute, OrgSyncConfig orgConfig, OrgSyncResult orgResult, IOrgSyncStrategy syncProvider)
        {
            try
            {
                // Reset the infrastructure before proceeding since this should be treated like a new transaction
                SyncInfrastucture.Dispose();
                ISyncDb db = await SyncInfrastucture.SyncDbAsync();

                // We may be resuming an existing run, but most likely a whole new one
                SyncRunEntity run = db.SyncRuns.Where(r => r.UniversalId == orgResult.UniversalId).FirstOrDefault();

                if (run == null)
                {
                    run = db.SyncRuns.CreateResource();

                    // Relationships
                    OrganizationEntity org = await db.OrgWhereRoute(orgRoute, true);
                    org.AssertFound();
                    run.Organization = org;
                }

                run.Timestamp();
                run.Scope = SyncScope.Organization;

                orgResult.State = OrgSyncState.Done;
                run.ApplyResult(orgResult);

                // Save sanitized config into run and result
                if (syncProvider != null)
                {
                    orgConfig = await syncProvider.SanitizeConfig(orgConfig);
                }
                run.SyncType = orgResult.SyncType = orgConfig.SyncType;
                run.SyncConfigJson = orgResult.SyncConfigJson = orgConfig.SyncConfigJson;

                // Save to DB
                await db.SaveChangesAsync();
                orgResult.IsSaved = true;
            }
            catch (Exception ex)
            {
                string msg = $"Error saving {nameof(SyncRunEntity)} during {nameof(OrgSyncService)}.{nameof(SyncAsync)}: {ex.Message} Stack: {ex.StackTrace}";
                Logger.LogError(ex, msg);
            }

            return orgResult;
        }

        #endregion

        #region IOrgSyncService

        public async Task<bool> HasAccessAsync(string orgRoute)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            OrgSyncConfig existingConfig = await OrgService.ReadSyncConfigAsync(orgRoute);

            // An empty org config is always acceptable
            if (existingConfig is null)
            {
                return false;
            }

            IOrgSyncStrategy syncProvider = SyncStrategy(existingConfig);
            return await syncProvider.HasAccessAsync();
        }

        public async Task<string> ValidationMessageAsync(string orgRoute)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            OrgSyncConfig orgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);

            // An empty org config is always acceptable
            if (orgConfig is null)
            {
                return null;
            }

            IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig);
            return await syncProvider.ValidationMessagesAsync(orgConfig);
        }

        public async Task<string> ValidationMessageAsync(string orgRoute, OrgSyncConfig newOrgConfig)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            // An empty org config is always acceptable
            if (newOrgConfig is null)
            {
                return null;
            }

            // A config without a type is NOT valid
            if (newOrgConfig.SyncType == null)
            {
                return null;
            }

            try
            {
                newOrgConfig.OrgRoute = orgRoute;
                OrgSyncConfig oldOrgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
                IOrgSyncStrategy syncProvider = SyncStrategy(newOrgConfig, oldOrgConfig, true);
                return await syncProvider.ValidationMessagesAsync(newOrgConfig);
            }
            catch (Exception ex)
            {
                string msg = $"Could not validate sync config for organization {newOrgConfig.OrgRoute} because {ex.Message} with stack {ex.StackTrace}";
                Logger.LogWarning(msg);
                return ex.Message;
            }
        }

        public async Task<OrgSyncConfig> SanitizeConfigAsync(string orgRoute, OrgSyncConfig orgConfig)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            if (orgConfig == null || orgConfig.SyncType == null)
            {
                return null;
            }

            orgConfig.OrgRoute = orgRoute;
            OrgSyncConfig oldOrgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
            IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig, oldOrgConfig, true);
            orgConfig = await syncProvider.SanitizeConfig(orgConfig);
            return orgConfig;
        }

        public async Task<bool> HasAccessAsync(string orgRoute, OrgSyncConfig orgConfig)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            // If it's empty, then just return false right away
            if (orgConfig == null || orgConfig.SyncType == null)
            {
                return false;
            }

            OrgSyncConfig oldOrgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
            IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig, oldOrgConfig, true);
            return await syncProvider.HasAccessAsync();
        }

        public async Task<OrgSyncConfig> ReadSanitizedConfigAsync(string orgRoute)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            // Pull config from the db
            OrgSyncConfig orgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);

            // If it's empty, then just return empty right away
            if (orgConfig == null || orgConfig.SyncType == null)
            {
                return null;
            }

            // Strip the config of all sensitive data
            IOrgSyncStrategy syncStrategy = SyncStrategy(orgConfig);
            // If we have a provider, then give it the chance to sanitize the config
            if (syncStrategy != null)
            {
                orgConfig = await syncStrategy.SanitizeConfig(orgConfig);
            }

            return orgConfig;
        }

        public async Task<OrgSyncResult> SyncAsync(string orgRoute, OrgSyncResult orgResult = null)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            OrgSyncConfig currentOrgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
            orgResult ??= new OrgSyncResult(currentOrgConfig.OrgRoute);
            if (currentOrgConfig is null)
            {
                // Nothing to do
                return orgResult;
            }

            IOrgSyncStrategy syncStrategy = null;
            try
            {
                syncStrategy = SyncStrategy(currentOrgConfig);

                await ValidateSyncReady(orgRoute, currentOrgConfig, syncStrategy);

                await syncStrategy.Sync(orgResult);
            }
            catch (Exception ex)
            {
                string msg = $"Error running SyncAsync: {ex.Message} Stack: {ex.StackTrace}";
                Logger.LogError(ex, msg);
                orgResult.AddError(ex, "Error Running Sync");
            }
            finally
            {
                orgResult.End();
                orgResult = await TrySaveRunAsync(orgRoute, currentOrgConfig, orgResult, syncStrategy);
            }

            return orgResult;
        }

        public async Task UpdateConfigAsync(string orgRoute, OrgSyncConfig orgConfig)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            if (orgConfig != null && orgConfig.SyncType != null)
            {
                // Setup the resources for this request
                OrgSyncConfig oldOrgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
                IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig, oldOrgConfig, true);

                await ValidateSyncReady(orgRoute, orgConfig, syncProvider);

                // Carry over from existing config in the DB

                orgConfig = await syncProvider.TransferConfigAsync(oldOrgConfig, orgConfig);
            }

            // Update the config for the given org
            await OrgService.UpdateSyncConfigAsync(orgRoute, orgConfig);
        }

        [Obsolete]
        public async Task<IEnumerable<OrgSyncResult>> ReadAllResultsAsync_Obsolete(string orgRoute)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            ISyncDb db = await SyncInfrastucture.SyncDbAsync();

            // Check for runs connected to the given organization
            IQueryable<SyncRunEntity> query = from run in db.SyncRuns
                                              where
                                                  run.DeletedUtc == null &&
                                                  run.Organization.DeletedUtc == null &&
                                                  run.Organization.Tenant.DeletedUtc == null &&
                                                  run.Organization.Route == orgRoute
                                              select run;

            IQueryable<OrgSyncResult> results = query.Include(r => r.Organization).Select(run => OrgSyncResult.FromRun(run));
            OrgSyncResult[] ret = await results.ToArrayAsync();
            return ret;
        }

        public async Task<IndexPageResponse<OrgSyncResult>> ReadAllResultsAsync(string orgRoute, IndexPageRequest page = null)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            ISyncDb db = await SyncInfrastucture.SyncDbAsync();

            // Check for runs connected to the given organization
            IQueryable<SyncRunEntity> query = from run in db.SyncRuns
                                              where
                                                  run.DeletedUtc == null &&
                                                  run.Organization.DeletedUtc == null &&
                                                  run.Organization.Tenant.DeletedUtc == null &&
                                                  run.Organization.Route == orgRoute
                                              orderby run.CreatedUtc descending
                                              select run;

            query = query.Include(r => r.Organization);

            IndexPageQuery pagedQuery = new IndexPageQuery(page).SetMaxResults(int.MaxValue);
            IndexPageResponse<SyncRunEntity> result = await query.ReadPageAsync(pagedQuery);
            IndexPageResponse<OrgSyncResult> ret = result.Map((r) => OrgSyncResult.FromRun(r));
            ret.BasedOn(pagedQuery);
            return ret;
        }


        public async Task<TokenPageResponse<SyncTarget>> ReadUsersAsync(string orgRoute, TokenPageRequest page = null)
        {
            OrgSyncConfig orgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
            if (orgConfig == null)
            {
                TokenPageResponse<SyncTarget> ret = new TokenPageResponse<SyncTarget>(page, Enumerable.Empty<SyncTarget>());
                return ret;
            }
            else
            {
                // Setup the resources for this request
                IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig);
                await ValidateSyncReady(orgRoute, orgConfig, syncProvider);
                TokenPageResponse<SyncTarget> ret = await syncProvider.UsersAsync(page);
                return ret;
            }
        }

        public async Task<SyncTarget> ReadUserAsync(string orgRoute, string userId)
        {
            OrgSyncConfig orgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
            if (orgConfig == null)
            {
                throw new NotFoundException($"Could not find user '{userId}' in org '{orgRoute}'");
            }
            else
            {
                // Setup the resources for this request
                IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig);
                await ValidateSyncReady(orgRoute, orgConfig, syncProvider);
                SyncTarget ret = await syncProvider.UserAsync(userId);
                return ret;
            }
        }

        public async Task<TokenPageResponse<SyncTarget>> ReadGroupsAsync(string orgRoute, TokenPageRequest page = null)
        {
            OrgSyncConfig orgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
            if (orgConfig == null)
            {
                TokenPageResponse<SyncTarget> ret = new TokenPageResponse<SyncTarget>(page, Enumerable.Empty<SyncTarget>());
                return ret;
            }
            else
            {
                // Setup the resources for this request
                IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig);
                await ValidateSyncReady(orgRoute, orgConfig, syncProvider);
                TokenPageResponse<SyncTarget> ret = await syncProvider.GroupsAsync(page);
                return ret;
            }
        }

        public async Task<SyncTarget> ReadGroupAsync(string orgRoute, string groupId)
        {
            OrgSyncConfig orgConfig = await OrgService.ReadSyncConfigAsync(orgRoute);
            if (orgConfig == null)
            {
                throw new NotFoundException($"Could not find group '{groupId}' in org '{orgRoute}'");
            }
            else
            {
                // Setup the resources for this request
                IOrgSyncStrategy syncProvider = SyncStrategy(orgConfig);
                await ValidateSyncReady(orgRoute, orgConfig, syncProvider);
                SyncTarget ret = await syncProvider.GroupAsync(groupId);
                return ret;
            }
        }

        #endregion
    }
}
