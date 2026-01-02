using Masticore.DirectorySync.Tests.Mocks;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Tests
{
    public class OrgSyncStore_GroupStoreTests
    {
        private readonly ILogger<OrgSyncStore> StoreLogger = NullLogger<OrgSyncStore>.Instance;

        [Fact]
        public async Task AddGroup_AddOrUpdate()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            const string newGrpName = nameof(newGrpName);
            string newGrpDesc = $"Description for {nameof(newGrpName)}";
            MockSyncGroup newGrp = new MockSyncGroup
            {
                Name = newGrpName,
                Description = newGrpDesc
            };
            await orgStore.AddGroup(newGrp, SyncOpType.AddOrUpdate);

            await orgStore.EndSync();

            // ASSERT - A new group was added with valid values
            Assert.Equal(1, orgResult.GroupsAdded);
            Assert.Equal(1, orgResult.GroupsUpdated);
            dbProvider.AssertAdd(0, 0, 1);
            dbProvider.Db.AssertGroupActive(IdentityDbSeed.OrgPrimary.Route, newGrp);
        }

        [Fact]
        public async Task AddGroup_UpdateOnly_New()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            const string newGrpName = nameof(newGrpName);
            string newGrpDesc = $"Description for {nameof(newGrpName)}";
            MockSyncGroup newGrp = new MockSyncGroup
            {
                Name = newGrpName,
                Description = newGrpDesc
            };
            await orgStore.AddGroup(newGrp, SyncOpType.UpdateOnly);

            await orgStore.EndSync();

            // ASSERT - Nothing was added, certainly not the one above
            Assert.Equal(0, orgResult.GroupsAdded);
            Assert.Equal(0, orgResult.GroupsUpdated);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertGroupMissing(newGrp.UniversalId);
        }

        [Fact]
        public async Task AddGroup_UpdateOnly_Existing()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Get valid existing group
            GroupEntity existingGrp = dbProvider.Db.Groups.Where(g => g.Organization.Route == IdentityDbSeed.OrgPrimary.Route).FirstOrDefault();
            Assert.NotNull(existingGrp);
            Assert.NotNull(existingGrp.UniversalId);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            const string newGrpName = nameof(newGrpName);
            string newGrpDesc = $"Description for {nameof(newGrpName)}";
            MockSyncGroup updateGrp = new MockSyncGroup
            {
                UniversalId = existingGrp.UniversalId,
                Name = newGrpName,
                Description = newGrpDesc
            };
            await orgStore.AddGroup(updateGrp, SyncOpType.UpdateOnly);

            await orgStore.EndSync();

            // ASSERT - Nothing was added, but it was updated
            Assert.Equal(0, orgResult.GroupsAdded);
            Assert.Equal(1, orgResult.GroupsUpdated);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertGroupActive(IdentityDbSeed.OrgPrimary.Route, updateGrp);
        }

        [Fact]
        public async Task RemoveGroup_ExistingGroup()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Get valid existing group
            GroupEntity existingGrp = dbProvider.Db.Groups.Where(g => g.Organization.Route == IdentityDbSeed.OrgPrimary.Route).FirstOrDefault();
            Assert.NotNull(existingGrp);
            Assert.NotNull(existingGrp.UniversalId);
            Assert.Null(existingGrp.DeletedUtc);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.RemoveGroup(existingGrp.UniversalId);

            await orgStore.EndSync();

            // ASSERT - Nothing was added, but it was updated
            Assert.Equal(1, orgResult.GroupsRemoved);
            dbProvider.AssertAdd();
            GroupEntity deletedGrp = await dbProvider.Db.GroupWhereUid(IdentityDbSeed.OrgPrimary.Route, existingGrp.UniversalId, true);
            Assert.NotNull(deletedGrp);
            Assert.NotNull(deletedGrp.DeletedUtc);
        }

        [Fact]
        public async Task RemoveGroup_NoGroup()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Get valid existing group
            string invalidGrpUid = "INVALID GROUP ROUTE";
            GroupEntity existingGrp = dbProvider.Db.Groups.Where(g => g.UniversalId == invalidGrpUid).FirstOrDefault();
            Assert.Null(existingGrp);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.RemoveGroup(invalidGrpUid);

            await orgStore.EndSync();

            // ASSERT - Nothing was added and the non-existent group is still non-existent
            Assert.Equal(0, orgResult.GroupsRemoved);
            dbProvider.AssertAdd();
            GroupEntity deletedGrp = await dbProvider.Db.GroupWhereUid(IdentityDbSeed.OrgPrimary.Route, invalidGrpUid);
            Assert.Null(deletedGrp);
        }
    }
}
