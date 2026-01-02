using Masticore.Entity;
using Masticore.Jobs;
using Masticore.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Jobs
{
    /// <summary>
    /// <see cref="IOrgSyncRunner"/> over a queue
    /// </summary>
    public class QueuedOrgSyncRunner : IOrgSyncRunner
    {
        protected ILogger<QueuedOrgSyncRunner> Logger { get; }
        protected IJobQueue Queue { get; }
        protected ISyncInfrastucture Infrastucture { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="queue"></param>
        /// <param name="infrastructure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public QueuedOrgSyncRunner(
            ILogger<QueuedOrgSyncRunner> logger,
            IJobQueue queue,
            ISyncInfrastucture infrastructure
            )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            Infrastucture = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
        }

        /// <summary>
        /// Try to save the given <see cref="OrgSyncResult"/> for the given org
        /// </summary>
        /// <remarks>Even if this fails, it does NOT throw exceptions; it just logs an error and moves on</remarks>
        /// <param name="orgRoute"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task<OrgSyncResult> TrySaveRunAsync(string orgRoute, OrgSyncResult result)
        {
            try
            {
                // Creating a new record with partial results
                ISyncDb db = await Infrastucture.SyncDbAsync();
                SyncRunEntity run = db.SyncRuns.CreateResource();

                // Relationships
                OrganizationEntity org = await db.OrgWhereRoute(orgRoute, true);
                org.AssertFound();
                run.Organization = org;

                // Fields
                run.UniversalId = result.UniversalId;
                run.Scope = SyncScope.Organization;
                run.ApplyResult(result);

                // Save to DB
                await db.SaveChangesAsync();
                result.IsSaved = true;
                Logger.LogDebug($"Saved queued sync run for org '{orgRoute}' with id '{result.UniversalId}'");
            }
            catch (Exception ex)
            {
                string msg = $"Error saving {nameof(SyncRunEntity)} during {nameof(QueuedOrgSyncRunner)}.{nameof(SyncAsync)}: {ex.Message} Stack: {ex.StackTrace}";
                Logger.LogError(ex, msg);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<OrgSyncResult> SyncAsync(string orgRoute, OrgSyncResult result = null)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);

            Logger.LogDebug($"Queueing sync for org '{orgRoute}'");

            SyncJob job = new SyncJob { OrgRoute = orgRoute };
            string id = await Queue.Add(job);

            // Prepare partial result
            result ??= new OrgSyncResult();

            result.State = OrgSyncState.Pending;
            result.OrgRoute = orgRoute;
            result.UniversalId = id;

            result = await TrySaveRunAsync(orgRoute, result);

            return result;
        }
    }
}
