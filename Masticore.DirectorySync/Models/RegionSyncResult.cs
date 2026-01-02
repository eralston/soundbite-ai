using Newtonsoft.Json;
using System.Collections.Generic;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Synchronization results for all organization sync results.
    /// </summary>
    public class RegionSyncResult : SyncResultBase
    {
        public RegionSyncResult(bool autoRun = true) : base(autoRun) { }

        /// <summary>
        /// Gets a list containing organization-specific directory sync process results.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IList<OrgSyncResult> OrgResults { get; } = new List<OrgSyncResult>();

        protected override int GetActionCount()
        {
            return OrgResults.Count;
        }
    }
}
