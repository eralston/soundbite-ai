using Masticore.Jobs;
using Masticore.Queue;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Jobs
{
    /// <summary>
    /// Base class for an <see cref="IJob"/>
    /// </summary>
    public class SyncJob : JobBase
    {
        public const string QueueName = "q-syncjob";

        /// <summary>
        /// Get or set the org for which this sync has been requested
        /// </summary>
        public string OrgRoute { set; get; }

        /// <summary>
        /// Gets or set the underlying sync service
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IOrgSyncService OrgSyncService { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public ISyncInfrastucture Infrastucture { get; set; }

        public override async Task Process()
        {
            if (OrgSyncService == null)
            {
                throw new Exception($"Cannot process {nameof(SyncJob)}; no {nameof(IOrgSyncService)} set to support syncing org '{OrgRoute}'");
            }

            if (Infrastucture == null)
            {
                throw new Exception($"Cannot process {nameof(SyncJob)}; no {nameof(ISyncInfrastucture)} set to support syncing org '{OrgRoute}'");
            }

            OrgSyncResult result = await GetResultToResume();
            await OrgSyncService.SyncAsync(OrgRoute, result);
        }

        /// <summary>
        /// Async retrieve the <see cref="OrgSyncResult"/> to resume based on this queue item; if not found, returns null
        /// </summary>
        /// <returns></returns>
        private async Task<OrgSyncResult> GetResultToResume()
        {
            ISyncDb db = await Infrastucture.SyncDbAsync();
            SyncRunEntity resumingRun = db.SyncRuns.Where(r => r.UniversalId == Id).FirstOrDefault();
            if (resumingRun != null)
            {
                OrgSyncResult result = resumingRun.GetOrgSyncResult();
                if (result != null)
                {
                    result.State = OrgSyncState.Processing;
                    resumingRun.ApplyResult(result);
                    await db.SaveChangesAsync();
                }
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
