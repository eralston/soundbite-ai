using Masticore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{

    /// <summary>
    /// <see cref="IRegionSyncStore"/> over Entity Framework
    /// </summary>
    public class RegionSyncStore : IRegionSyncStore
    {
        #region Fields

        private ILogger Logger { get; }
        private ISyncInfrastucture SyncInfrastructure { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="syncInfrastructure"></param>
        public RegionSyncStore(ILogger<RegionSyncStore> logger, ISyncInfrastucture syncInfrastructure)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SyncInfrastructure = syncInfrastructure ?? throw new ArgumentNullException(nameof(syncInfrastructure));
        }

        /// <summary>
        /// Gets a list of all directory sync configurations representing all organizations that have indicated a sync type, but without validating if that type is active
        /// </summary>
        /// <returns>a list of all directory sync configurations.</returns>
        public async Task<IEnumerable<OrgSyncConfig>> GetOrgsToSync()
        {
            try
            {
                ISyncDb db = await SyncInfrastructure.SyncDbAsync();
                IQueryable<OrgSyncConfig> orgsToSync =
                    from
                        org in db.Organizations
                    where
                        org.DeletedUtc == null &&
                        org.Tenant.DeletedUtc == null &&
                        org.SyncType != null
                    select
                        new OrgSyncConfig
                        {
                            OrgRoute = org.Route,
                            SyncType = org.SyncType,
                            SyncConfigJson = org.SyncConfigJson
                        };
                return await orgsToSync.ToArrayAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error querying orgs in {nameof(RegionSyncStore)}.{nameof(GetOrgsToSync)} with error '{ex.Message}'  with trace {ex.StackTrace}");
                throw;
            }
        }

        #endregion
    }
}
