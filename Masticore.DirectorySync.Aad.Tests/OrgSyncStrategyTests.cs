using Masticore.Ad;
using Masticore.DirectorySync.Tests.Mocks;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Services.Tests;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Aad.Tests
{
    public class OrgSyncStrategyTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        public OrgSyncStrategyTests()
        {
            // Pre-setup for all tests
            AdAppSettings.Init(
                new AdAppSettings
                {
                    AppId = "123",
                    AppSecret = "SUPER_SECRET"
                }, false);
        }

        [Fact]
        public async Task NoSync()
        {
            // ARRANGE

            // Mocks
            MockGraphClient mockGraph = new MockGraphClient();
            using MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();

            GroupSyncConfig[] groups = null;
            bool allGroups = false;
            bool allUsers = false;
            UserSyncConfig[] users = null;
            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Test targets
            OrgSyncStore orgStore = new OrgSyncStore(NullLogger<OrgSyncStore>.Instance, syncInfra);
            AadOrgSyncStrategy aadSync = new AadOrgSyncStrategy(NullLogger<AadOrgSyncStrategy>.Instance, orgConfig, new MockGraphFactory(mockGraph), orgStore, Builder.Infrastructure.Value);

            // ACT
            await aadSync.Sync(orgResult);

            // ASSERT
            Assert.Equal(0, orgResult.UserNetworkRequests);
            Assert.Equal(0, orgResult.GroupNetworkRequests);
            syncInfra.AssertAdd();
        }

        [Fact]
        public async Task Sync_AllUsers()
        {
            // ARRANGE

            // Mocks
            using MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();
            Utils.ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult orgResult, out MockGraphClient mockGraph);

            // Test targets
            OrgSyncStore orgStore = new OrgSyncStore(NullLogger<OrgSyncStore>.Instance, syncInfra);
            AadOrgSyncStrategy aadSync = new AadOrgSyncStrategy(NullLogger<AadOrgSyncStrategy>.Instance, orgConfig, new MockGraphFactory(mockGraph), orgStore, Builder.Infrastructure.Value);

            // ACT
            await aadSync.Sync(orgResult);

            // ASSERT
            Assert.Equal(1, orgResult.UserNetworkRequests);
            Assert.Equal(0, orgResult.GroupNetworkRequests);
            syncInfra.AssertAdd(3, 3);
            Assert.Equal(3, orgResult.UsersAdded);
            Assert.Equal(3, orgResult.UsersUpdated);
            Assert.Equal(3, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            Assert.Equal(0, orgResult.GroupsAdded);
            Assert.Equal(0, orgResult.GroupsUpdated);
            Assert.Equal(0, orgResult.GroupsRemoved);
            Assert.Equal(0, orgResult.MembersAdded);
            Assert.Equal(0, orgResult.MembersRemoved);
        }

        [Fact]
        public async Task Sync_AllGroups()
        {
            // ARRANGE

            // Mocks
            using MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();

            string firstGroupId = nameof(firstGroupId);
            string secondGroupId = nameof(secondGroupId);

            string firstUserId = nameof(firstUserId);
            string secondUserId = nameof(secondUserId);
            string thirdUserId = nameof(thirdUserId);
            GraphUser userInFirstGroup = new GraphUser { Email = $"{firstUserId}@geocities.com", UniversalId = firstUserId, Name = $"Name:{firstUserId}" };
            GraphUser userInSecondGroup = new GraphUser { Email = $"{secondUserId}@geocities.com", UniversalId = secondUserId, Name = $"Name:{secondUserId}" };
            GraphUser userInBoth = new GraphUser { Email = $"{thirdUserId}@geocities.com", UniversalId = thirdUserId, Name = $"Name:{thirdUserId}" };

            GroupSyncConfig[] groups = null;
            bool allGroups = true;
            bool allUsers = false;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient mockGraph = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    userInFirstGroup,
                    userInSecondGroup,
                    userInBoth
                },
                Groups = new GraphGroup[]
                {
                    Utils.CreateGroup(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateGroup(secondGroupId, userInSecondGroup, userInBoth)
                },
                Members = Utils.CreateMembers(
                    Utils.CreateMember(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateMember(secondGroupId, userInSecondGroup, userInBoth)
                    )
            };
            // Test targets
            OrgSyncStore orgStore = new OrgSyncStore(NullLogger<OrgSyncStore>.Instance, syncInfra);
            AadOrgSyncStrategy aadSync = new AadOrgSyncStrategy(NullLogger<AadOrgSyncStrategy>.Instance, orgConfig, new MockGraphFactory(mockGraph), orgStore, Builder.Infrastructure.Value);

            // ACT
            await aadSync.Sync(orgResult);

            // ASSERT
            Assert.Equal(0, orgResult.UserNetworkRequests);
            Assert.Equal(3, orgResult.GroupNetworkRequests);
            syncInfra.AssertAdd(3, 3, 2, 4);
            Assert.Equal(3, orgResult.UsersAdded);
            Assert.Equal(3, orgResult.UsersUpdated);
            Assert.Equal(3, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            Assert.Equal(2, orgResult.GroupsAdded);
            Assert.Equal(2, orgResult.GroupsUpdated);
            Assert.Equal(0, orgResult.GroupsRemoved);
            Assert.Equal(4, orgResult.MembersAdded);
            Assert.Equal(0, orgResult.MembersRemoved);
        }
    }
}
