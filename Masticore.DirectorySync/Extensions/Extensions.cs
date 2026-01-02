using System;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Static class for <see cref="SyncRunEntity"/> and related extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Applies the given <see cref="RegionSyncResult"/> to the given <see cref="SyncRunEntity"/>
        /// </summary>
        /// <param name="run"></param>
        /// <param name="regionSyncResult"></param>
        public static void ApplyResult(this SyncRunEntity run, RegionSyncResult regionSyncResult)
        {
            if (run is null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (regionSyncResult is null)
            {
                throw new ArgumentNullException(nameof(regionSyncResult));
            }

            if (!regionSyncResult.IsStarted && !regionSyncResult.IsEnded)
            {
                throw new InvalidOperationException($"Cannot save {nameof(RegionSyncResult)} that has not started and stopped");
            }

            // Fixed
            run.Scope = SyncScope.Region;
            // Region
            // Base
            run.ApplyBaseFields(regionSyncResult);
        }

        /// <summary>
        /// Saves the values from the given <see cref="SyncResultBase"/> to the given <see cref="SyncRunEntity"/>
        /// </summary>
        /// <param name="run"></param>
        /// <param name="syncResult"></param>
        private static void ApplyBaseFields(this SyncRunEntity run, SyncResultBase syncResult)
        {
            run.UniversalId = syncResult.UniversalId;
            run.IsFailed = syncResult.IsFailed;
            run.DeltaBytes = syncResult.DeltaBytes;
            run.DeltaTime = syncResult.DeltaTime;
            run.ActionCount = syncResult.ActionCount;
            run.ResultJson = syncResult.ToLowerCamelJson();
        }

        /// <summary>
        /// Applies the given <see cref="OrgSyncResult"/> to the given <see cref="SyncRunEntity"/>
        /// </summary>
        /// <param name="run"></param>
        /// <param name="orgSyncResult"></param>
        public static void ApplyResult(this SyncRunEntity run, OrgSyncResult orgSyncResult)
        {
            if (run is null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (orgSyncResult is null)
            {
                throw new ArgumentNullException(nameof(orgSyncResult));
            }

            // Fixed
            run.Scope = SyncScope.Organization;
            // Base
            run.ApplyBaseFields(orgSyncResult);
        }

        /// <summary>
        /// Gets the <see cref="OrgSyncResult"/> captured into the given <see cref="SyncRunEntity"/>
        /// </summary>
        /// <param name="run"></param>
        /// <returns></returns>
        public static OrgSyncResult GetOrgSyncResult(this SyncRunEntity run)
        {
            if (run is null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (run.Scope != SyncScope.Organization)
            {
                throw new InvalidOperationException($"Scope of {nameof(SyncRunEntity)} does not match {SyncScope.Organization}");
            }

            if (run.ResultJson is null)
            {
                return null;
            }

            return JsonUtils.FromLowerCamelJson<OrgSyncResult>(run.ResultJson);
        }

        /// <summary>
        /// Gets the <see cref="RegionSyncResult"/> captured into the given <see cref="SyncRunEntity"/>
        /// </summary>
        /// <param name="run"></param>
        /// <returns></returns>
        public static RegionSyncResult GetRegionSyncResult(this SyncRunEntity run)
        {
            if (run is null)
            {
                throw new ArgumentNullException(nameof(run));
            }

            if (run.Scope != SyncScope.Region)
            {
                throw new InvalidOperationException($"Scope of {nameof(SyncRunEntity)} does not match {SyncScope.Region}");
            }

            if (run.ResultJson is null)
            {
                return null;
            }

            return JsonUtils.FromLowerCamelJson<RegionSyncResult>(run.ResultJson);
        }
    }
}
