using Masticore.DirectorySync;
using Masticore.DirectorySync.Jobs;
using Masticore.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Soundbite.Api;
using Soundbite.AzFun;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.DirectorySync
{
    /// <summary>
    /// Acquire functions pertaining to the directory sync process.
    /// </summary>
    public class DirectorySyncFunctions
    {
        #region Fields

#if DEBUG
        // If we're debug, then it will fire immediately on start of the functiona app
        // This means that locally you just hit the play button
        protected const bool RunOnStartup = false;
#else
        protected const bool RunOnStartup = false;
#endif

        protected IRegionSyncStore RegionSyncStore { get; }
        protected IOrgSyncStore OrgSyncStore { get; }
        protected ISyncStrategyFactory SyncFactory { get; }
        protected ISyncInfrastucture SyncInfrastructure { get; }
        protected IOrgSyncService OrgSyncService { get; }
        protected ILogger Logger { get; }
        protected ISyncJobWorker Worker { get; }

        #endregion

        #region Constructor and Members

        public DirectorySyncFunctions(
            ILogger<DirectorySyncFunctions> logger,
            IRegionSyncStore regionSyncService,
            IOrgSyncStore orgSyncStore,
            ISyncStrategyFactory syncFactory,
            ISyncInfrastucture syncInfrastructure,
            IOrgSyncService orgSyncService,
            ISyncJobWorker syncWorker
            )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RegionSyncStore = regionSyncService ?? throw new ArgumentNullException(nameof(regionSyncService));
            OrgSyncStore = orgSyncStore ?? throw new ArgumentNullException(nameof(orgSyncStore));
            SyncFactory = syncFactory ?? throw new ArgumentNullException(nameof(syncFactory));
            SyncInfrastructure = syncInfrastructure ?? throw new ArgumentNullException(nameof(syncInfrastructure));
            OrgSyncService = orgSyncService ?? throw new ArgumentNullException(nameof(orgSyncService));
            Worker = syncWorker ?? throw new ArgumentNullException(nameof(syncWorker));
        }

        private Task<bool> HasConnection()
        {
            return Task.FromResult(AzInfrastructure.HasConnection);
        }

        private void LoadInfrastructure(string dbStr, string storStr)
        {
            AzInfrastructure.Configure(dbStr, storStr);
        }

        private async Task RunAllOrgsStart(IDurableOrchestrationClient starter)
        {
            try
            {
                string instanceId = await starter.StartNewAsync(nameof(SyncAllOrgs), null);
                Logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            }
            catch (Exception ex)
            {
                string msg = $"Error running GetOrgsToSyncTimerStart: {ex.Message} Stack: {ex.StackTrace}";
                Logger.LogError(ex, msg);
                throw;
            }
        }

        private async Task<OrgSyncResult> RunSyncOrg(IDurableActivityContext activityContext)
        {
            OrgSyncConfig orgConfig = activityContext.GetInput<OrgSyncConfig>();
            OrgSyncResult orgResult = await OrgSyncService.SyncAsync(orgConfig.OrgRoute);
            return orgResult;
        }

        private async Task RunSyncJob(string message, string messageId, ILogger _)
        {
            await Worker.Process(message, messageId);
        }

        private async Task<IEnumerable<OrgSyncConfig>> RunGetOrgsToSync()
        {
            try
            {
                IEnumerable<OrgSyncConfig> orgSyncConfigs = await RegionSyncStore.GetOrgsToSync();
                return orgSyncConfigs;
            }
            catch (Exception ex)
            {
                string msg = $"Error running GetOrgsToSync: {ex.Message} Stack: {ex.StackTrace}";
                Logger.LogError(ex, msg);
                throw;
            }
        }

        #endregion

        #region Azure Functions

#if DEBUG
        [FunctionName(nameof(SyncAllOrgsHttpStart))]
        public async Task<HttpResponseMessage> SyncAllOrgsHttpStart(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
           [DurableClient] IDurableOrchestrationClient starter)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                 nameof(SyncAllOrgsHttpStart),
                 Logger,
                 HasConnection,
                 LoadInfrastructure,
                 async () => { return await StartNewAsync(req, starter); });
        }

        private static async Task<HttpResponseMessage> StartNewAsync(HttpRequestMessage req, IDurableOrchestrationClient starter)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(nameof(SyncAllOrgs), null);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
#endif

        /// <summary>
        /// Responsible for starting the SyncAllOrgs durable function on a timer.
        /// </summary>
        /// <param name="myTimer">Timer information.</param>
        /// <param name="starter">Durable orchestration client.</param>
        /// <param name="log">Logger reference.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        [FunctionName(nameof(SyncAllOrgsTimerStart))]
        public async Task SyncAllOrgsTimerStart(
            [TimerTrigger("0 0 0 * * *", RunOnStartup = RunOnStartup)] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            await AzFunUtils.EnsureSecretsAndRun(
              nameof(SyncAllOrgsTimerStart),
              Logger,
              HasConnection,
              LoadInfrastructure,
              async () => { await RunAllOrgsStart(starter); });
        }

        /// <summary>
        /// Azure orchestration function that is responsible for orchestrating the activities 
        /// required to syncronize users and groups across all organization.
        /// </summary>
        /// <param name="context">Durable synchronization context</param>
        /// <returns>a <see cref="SyncAllOrgResults"/> instance populated with information about the directory sync process.</returns>
        [FunctionName(nameof(SyncAllOrgs))]
        public async Task<RegionSyncResult> SyncAllOrgs(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                     nameof(SyncAllOrgs),
                     Logger,
                     HasConnection,
                     LoadInfrastructure,
                     async () => { return await RunSyncAllOrgs(context); });
        }

        private async Task<RegionSyncResult> RunSyncAllOrgs(IDurableOrchestrationContext context)
        {
            RegionSyncResult regionResult = new RegionSyncResult();
            List<Task<OrgSyncResult>> syncTasks = new List<Task<OrgSyncResult>>();
            try
            {
                IList<OrgSyncConfig> orgsToSync = await context.CallActivityAsync<IList<OrgSyncConfig>>(nameof(GetOrgsToSync), null);

                foreach (OrgSyncConfig orgConfig in orgsToSync)
                {
                    syncTasks.Add(context.CallActivityAsync<OrgSyncResult>(nameof(SyncOrg), orgConfig));
                }

                // Wait for the sub orechestration tasks to all run to completion
                await Task.WhenAll(syncTasks);

                // Gather the results of the sub orechestrations into the organizational results
                syncTasks.ForEach(i =>
                {
                    regionResult.OrgResults.Add(i.Result);
                });
            }
            catch (Exception ex)
            {
                string msg = $"Error running SyncAllOrgs: {ex.Message} Stack: {ex.StackTrace}";
                Logger.LogError(ex, msg);
                regionResult.AddError(ex, msg);
            }

            regionResult.End();
            return regionResult;
        }

        /// <summary>
        /// Activity responsible for retrieving all organizations that can/need to be syncronized.
        /// </summary>
        /// <param name="nullValue">Pass <c>null</c> to this parameter.  Activity functions must have at least one parameter because the ActivityTrigger attribute can only be applied to a method parameter.</param>
        /// <returns>a list of organization synchronization configuration settings that can be used for directory synchronization.</returns>
        [FunctionName(nameof(GetOrgsToSync))]
        public async Task<IEnumerable<OrgSyncConfig>> GetOrgsToSync([ActivityTrigger] object nullValue)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                     nameof(SyncAllOrgs),
                     Logger,
                     HasConnection,
                     LoadInfrastructure,
                     async () => { return await RunGetOrgsToSync(); });
        }

        /// <summary>
        /// Azure orchestration function that is responsible for orchestrating the activities 
        /// required to syncronize users and groups within a single organization.
        /// </summary>
        /// <param name="context">Durable synchronization context</param>
        /// <returns>a <see cref="OrgSyncResult"/> instance populated with information about the directory sync process.</returns>
        [FunctionName(nameof(SyncOrg))]
        public async Task<OrgSyncResult> SyncOrg([ActivityTrigger] IDurableActivityContext activityContext)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                    nameof(SyncAllOrgs),
                    Logger,
                    HasConnection,
                    LoadInfrastructure,
                    async () => { return await RunSyncOrg(activityContext); });
        }

        [FunctionName(nameof(ProcessSyncJob))]
        public async Task ProcessSyncJob(
            [QueueTrigger(SyncJob.QueueName, Connection = "SbStorage")] string message,
            string id,
            ILogger log)
        {
            await AzFunUtils.EnsureSecretsAndRun(
                  nameof(ProcessSyncJob),
                  log,
                  HasConnection,
                  LoadInfrastructure,
                  async () => { await RunSyncJob(message, id, log); });
        }

        #endregion
    }
}