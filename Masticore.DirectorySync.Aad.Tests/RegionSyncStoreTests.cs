using Masticore.DirectorySync.Tests.Mocks;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Aad.Tests
{
    public class RegionSyncStoreTests
    {
        protected ILogger<RegionSyncStore> Logger { get; } = NullLogger<RegionSyncStore>.Instance;

        [Fact]
        public async Task SingleOrg_NoConfig()
        {
            // ARRANGE
            using MockSyncInfrastructure db = new MockSyncInfrastructure();

            // ACT
            RegionSyncStore regionStore = new RegionSyncStore(Logger, db);
            IEnumerable<OrgSyncConfig> orgs = await regionStore.GetOrgsToSync();

            // ASSERT
            Assert.Equal(2, db.Db.Organizations.Count());
            Assert.NotNull(orgs);
            Assert.Empty(orgs);
        }

        [Fact]
        public async Task SingleOrg_UserConfig()
        {
            // ARRANGE
            AadOrgConfig aadConfig = Utils.SetupAadConfig(false, false, null, null);
            OrgSyncConfig orgConfig = Utils.SetupAadOrgConfig(aadConfig);
            using MockSyncInfrastructure db = new MockSyncInfrastructure();
            OrganizationEntity syncOrg = db.Db.MockOrg();
            syncOrg.SyncType = orgConfig.SyncType;
            syncOrg.SyncConfigJson = orgConfig.SyncConfigJson;
            db.Db.SaveChanges();

            // ACT
            RegionSyncStore regionStore = new RegionSyncStore(Logger, db);
            IEnumerable<OrgSyncConfig> orgs = await regionStore.GetOrgsToSync();

            // ASSERT
            Assert.Equal(3, db.Db.Organizations.Count());
            Assert.NotNull(orgs);
            Assert.Single(orgs);
        }
    }
}
