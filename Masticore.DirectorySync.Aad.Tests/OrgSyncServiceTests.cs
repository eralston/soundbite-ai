using Masticore.Ad;
using Masticore.DirectorySync.Tests;
using Masticore.DirectorySync.Tests.Mocks;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Masticore.Services.Tests;
using Masticore.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Aad.Tests
{
    public class OrgSyncServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        private ILogger<AadOrgSyncStrategy> StrategyLogger { get; } = NullLogger<AadOrgSyncStrategy>.Instance;

        private static ISecurityContext SecurityContext(IIdentityDb db, string userRoute)
        {
            UserEntity user = db.Users.Where(u => u.Route == userRoute).Single();
            ISecurityContext security = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            return security;
        }

        public OrgSyncServiceTests()
        {
            // Pre-setup for all tests
            AdAppSettings.Init(
                new AdAppSettings
                {
                    AppId = "123",
                    AppSecret = "SUPER_SECRET"
                }, false);
        }

        #region ReadSyncConfigAsync

        [Fact]
        public async Task ReadSyncConfigAsync()
        {
            // ARRANGE
            using MockSyncInfrastructure infra = new MockSyncInfrastructure();
            ISecurityContext security = SecurityContext(infra.Db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), infra, security);
            ILogger<OrganizationService> logger = new NullLogger<OrganizationService>();

            // Org Sync config
            MockSyncDb db = infra.Db;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).Single();
            Utils.ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult _, out MockGraphClient graphClient);
            AadOrgConfig aadConfig = orgConfig.GetSyncConfig<AadOrgConfig>();
            string veryUniqueTenantId = nameof(veryUniqueTenantId);
            aadConfig.TenantId = veryUniqueTenantId;
            aadConfig.SecretKey = "VERY SECRET";
            org.SyncConfigJson = aadConfig.ToLowerCamelJson(false);
            org.SyncType = AadOrgSyncStrategyBase.Name;
            db.SaveChanges();

            // ACT
            IImageService imageService = new MockImageService();
            IOrganizationService organizationService = new OrganizationService(security, infra, imageService, MockMapper.Instance, logger, dbRbac);
            OrgSyncConfig retOrgConfig = await organizationService.ReadSyncConfigAsync(IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(retOrgConfig);
            Assert.Equal(orgConfig.OrgRoute, retOrgConfig.OrgRoute);
            Assert.Equal(orgConfig.SyncType, retOrgConfig.SyncType);
            AadOrgConfig retAadConfig = retOrgConfig.GetSyncConfig<AadOrgConfig>();
            Assert.NotNull(retAadConfig);
            Assert.Equal(aadConfig.TenantId, retAadConfig.TenantId);
            Assert.Equal(aadConfig.SecretKey, retAadConfig.SecretKey);
        }

        #endregion

        [Fact]
        public async Task SyncAndQueryTests()
        {
            // ARRANGE
            using MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();
            Utils.ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult _, out MockGraphClient graphClient);
            AadOrgConfig syncAadConfig = orgConfig.GetSyncConfig<AadOrgConfig>();
            string veryUniqueTenantId = nameof(veryUniqueTenantId);
            syncAadConfig.TenantId = veryUniqueTenantId;
            syncAadConfig.SecretKey = "VERY SECRET";
            orgConfig.SyncConfigJson = syncAadConfig.ToLowerCamelJson(false);
            MockGraphFactory graphFactory = new MockGraphFactory(graphClient);
            MockOrgService orgService = new MockOrgService
            {
                OrgSyncConfig = orgConfig
            };
            OrgSyncStore orgStore = new OrgSyncStore(NullLogger<OrgSyncStore>.Instance, syncInfra);
            IOrgSyncStrategy syncStrategy = new AadOrgSyncStrategy(StrategyLogger, orgConfig, graphFactory, orgStore, Builder.Infrastructure.Value);
            ISyncStrategyFactory syncFactory = new MockSyncStrategyFactory(syncStrategy);
            ISecurityContext security = SecurityContext(syncInfra.Db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), syncInfra, security);
            IOrgSyncService orgSyncService = new OrgSyncService(
                NullLogger<OrgSyncService>.Instance,
                syncFactory,
                syncInfra,
                orgService,
                orgStore,
                dbRbac
                );

            // ACT & ASSERT SYNC
            OrgSyncResult result = await orgSyncService.SyncAsync(IdentityDbSeed.OrgPrimary.Route);
            Assert.False(result.IsFailed);
            Assert.True(result.IsSaved);
            syncInfra.AssertAdd(3, 3, 0, 0, 1, 0);
            Assert.Equal(1, result.UserNetworkRequests);
            Assert.Equal(0, result.GroupNetworkRequests);
            Assert.Equal(3, result.UsersAdded);
            Assert.Equal(3, result.UsersUpdated);
            Assert.Equal(3, result.PeopleAdded);
            Assert.Equal(0, result.PeopleRemoved);
            Assert.Equal(0, result.GroupsAdded);
            Assert.Equal(0, result.GroupsUpdated);
            Assert.Equal(0, result.GroupsRemoved);
            Assert.Equal(0, result.MembersAdded);
            Assert.Equal(0, result.MembersRemoved);

            // ACT & ASSERT READ RESULTS
            IEnumerable<OrgSyncResult> results = await orgSyncService.ReadAllResultsAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            AssertRun(veryUniqueTenantId, results);

            IndexPageResponse<OrgSyncResult> response = await orgSyncService.ReadAllResultsAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertRun(veryUniqueTenantId, response.Result);
        }

        private static void AssertRun(string veryUniqueTenantId, IEnumerable<OrgSyncResult> results)
        {
            Assert.Single(results);
            OrgSyncResult firstResult = results.Single();
            Assert.Equal(1, firstResult.UserNetworkRequests);
            Assert.Equal(0, firstResult.GroupNetworkRequests);
            Assert.Equal(3, firstResult.UsersAdded);
            Assert.Equal(3, firstResult.UsersUpdated);
            Assert.Equal(3, firstResult.PeopleAdded);
            Assert.Equal(0, firstResult.PeopleRemoved);
            Assert.Equal(0, firstResult.GroupsAdded);
            Assert.Equal(0, firstResult.GroupsUpdated);
            Assert.Equal(0, firstResult.GroupsRemoved);
            Assert.Equal(0, firstResult.MembersAdded);
            Assert.Equal(0, firstResult.MembersRemoved);
            AadOrgConfig aadConfig = firstResult.GetSyncConfig<AadOrgConfig>();
            Assert.Equal(veryUniqueTenantId, aadConfig.TenantId);
            Assert.Null(aadConfig.SecretKey);
            Assert.True(aadConfig.ImportAllUsers);
            Assert.False(aadConfig.ImportAllGroups);
        }
    }
}
