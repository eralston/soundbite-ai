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
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace Masticore.DirectorySync.Aad.Tests
{
    public class AadOrgSyncServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        private ISecurityContext SecurityContext(IIdentityDb db, string userRoute)
        {
            UserEntity user = db.Users.Where(u => u.Route == userRoute).Single();
            ISecurityContext security = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            return security;
        }

        private ILogger<AadOrgSyncStrategy> StrategyLogger { get; } = NullLogger<AadOrgSyncStrategy>.Instance;
        protected ILogger<OrgSyncService> OrgSyncServiceLogger { get; } = NullLogger<OrgSyncService>.Instance;

        public AadOrgSyncServiceTests()
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
        public async Task Sync_Users()
        {
            // ARRANGE
            MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();
            Utils.ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult _, out MockGraphClient graphClient);
            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy strategy = new AadOrgSyncStrategy(StrategyLogger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);
            ISyncStrategyFactory factory = new MockSyncStrategyFactory(strategy);
            MockOrgService orgService = new MockOrgService
            {
                OrgSyncConfig = orgConfig
            };
            ISecurityContext security = SecurityContext(syncInfra.Db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), syncInfra, security);

            // ACT
            IOrgSyncService OrgSyncService = new OrgSyncService(OrgSyncServiceLogger, factory, syncInfra, orgService, orgStore, dbRbac);
            OrgSyncResult result = await OrgSyncService.SyncAsync(IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.False(result.IsFailed);
            Utils.AssertAllUsers(orgStore);
        }

        [Fact]
        public async Task ReadConfig()
        {
            // ARRANGE
            Utils.ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult _, out MockGraphClient graphClient);
            using MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();
            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(StrategyLogger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);
            ISyncStrategyFactory factory = new MockSyncStrategyFactory(provider);
            MockOrgService orgService = new MockOrgService
            {
                OrgSyncConfig = orgConfig
            };
            ISecurityContext security = SecurityContext(syncInfra.Db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), syncInfra, security);

            // ACT
            IOrgSyncService OrgSyncService = new OrgSyncService(OrgSyncServiceLogger, factory, syncInfra, orgService, orgStore, dbRbac);
            OrgSyncConfig readConfig = await OrgSyncService.ReadSanitizedConfigAsync(IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(readConfig);
            Assert.NotNull(readConfig.SyncType);
            Assert.NotNull(readConfig.SyncConfigJson);
            AadOrgConfig aadConfig = readConfig.GetSyncConfig<AadOrgConfig>();
            Assert.NotNull(aadConfig);
            Assert.NotNull(aadConfig.TenantId);
        }

        [Fact]
        public async Task UpdateConfig_AllUserAllGroups_SameTenant()
        {
            // ARRANGE
            string oldTenantId = nameof(oldTenantId);
            string oldUserDelta = nameof(oldUserDelta);
            string oldUserNext = nameof(oldUserNext);
            string oldGroupDelta = nameof(oldGroupDelta);
            string oldGroupNext = nameof(oldGroupNext);
            string oldAppId = nameof(oldAppId);
            string oldSecretKey = nameof(oldSecretKey);

            AadOrgConfig oldAadConfig = new AadOrgConfig
            {
                // Sync
                GroupsMode = CollectionSyncMode.All,
                UsersMode = CollectionSyncMode.All,

                // Context
                DeltaLinkUsers = oldUserDelta,
                NextLinkUsers = oldUserNext,
                DeltaLinkGroups = oldGroupDelta,
                NextLinkGroups = oldGroupNext,

                // Security
                TenantId = oldTenantId,
                AppId = oldAppId,
                SecretKey = oldSecretKey
            };
            OrgSyncConfig oldOrgConfig = new OrgSyncConfig
            {
                SyncType = AadOrgSyncStrategyBase.Name,
                SyncConfigJson = oldAadConfig.ToLowerCamelJson(false)
                // Route should only be used during runtime
            };

            // ARRANGE

            // Trying to overwrite deltas and nexts should not carry over
            string newUserDelta = nameof(newUserDelta);
            string newUserNext = nameof(newUserNext);
            string newGroupDelta = nameof(newGroupDelta);
            string newGroupNext = nameof(newGroupNext);

            AadOrgConfig newAadConfig = new AadOrgConfig
            {
                // Sync Settings
                GroupsMode = CollectionSyncMode.All,
                UsersMode = CollectionSyncMode.All,

                // Sync Context
                DeltaLinkUsers = newUserDelta,
                NextLinkUsers = newUserNext,
                DeltaLinkGroups = newGroupDelta,
                NextLinkGroups = newGroupNext,

                // Security
                TenantId = oldTenantId,
                AppId = oldAppId,
                SecretKey = null
            };

            OrgSyncConfig newOrgConfig = new OrgSyncConfig
            {
                SyncType = AadOrgSyncStrategyBase.Name,
                SyncConfigJson = newAadConfig.ToLowerCamelJson(false)
                // Route should only be used during runtime
            };

            MockOrgStore orgStore = new MockOrgStore();
            MockGraphClient graphClient = new MockGraphClient();
            using MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();
            IOrgSyncStrategy strategy = new AadOrgSyncStrategy(StrategyLogger, oldOrgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);
            ISyncStrategyFactory factory = new MockSyncStrategyFactory(strategy);
            MockOrgService orgService = new MockOrgService
            {
                OrgSyncConfig = oldOrgConfig
            };
            ISecurityContext security = SecurityContext(syncInfra.Db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), syncInfra, security);

            // ACT
            IOrgSyncService OrgSyncService = new OrgSyncService(OrgSyncServiceLogger, factory, syncInfra, orgService, orgStore, dbRbac);
            await OrgSyncService.UpdateConfigAsync(IdentityDbSeed.OrgPrimary.Route, newOrgConfig);

            // ASSERT

            // Database config
            OrgSyncConfig dbConfig = orgService.OrgSyncConfig;
            AssertDbConfig(dbConfig, newAadConfig, oldAadConfig, AadOrgSyncStrategyBase.Name);

            // API Available config
            OrgSyncConfig readConfig = await OrgSyncService.ReadSanitizedConfigAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertReadConfig(AadOrgSyncStrategyBase.Name, newAadConfig, readConfig);
        }

        [Fact]
        public async Task UpdateConfig_AllUserAllGroups_DifferentTenant()
        {
            // ARRANGE
            string oldTenantId = nameof(oldTenantId);
            string oldUserDelta = nameof(oldUserDelta);
            string oldUserNext = nameof(oldUserNext);
            string oldGroupDelta = nameof(oldGroupDelta);
            string oldGroupNext = nameof(oldGroupNext);
            string oldAppId = nameof(oldAppId);
            string oldSecretKey = nameof(oldSecretKey);

            AadOrgConfig oldAadConfig = new AadOrgConfig
            {
                // Sync
                GroupsMode = CollectionSyncMode.All,
                UsersMode = CollectionSyncMode.All,

                // Context
                DeltaLinkUsers = oldUserDelta,
                NextLinkUsers = oldUserNext,
                DeltaLinkGroups = oldGroupDelta,
                NextLinkGroups = oldGroupNext,

                // Security
                TenantId = oldTenantId,
                AppId = oldAppId,
                SecretKey = oldSecretKey
            };
            OrgSyncConfig oldOrgConfig = new OrgSyncConfig
            {
                SyncType = AadOrgSyncStrategyBase.Name,
                SyncConfigJson = oldAadConfig.ToLowerCamelJson(false)
                // Route should only be used during runtime
            };

            // ARRANGE

            // Trying to overwrite deltas and nexts should not carry over
            string newTenantId = nameof(newTenantId);
            string newAppId = nameof(newAppId);
            string newSecretKey = nameof(newSecretKey);
            string newUserDelta = nameof(newUserDelta);
            string newUserNext = nameof(newUserNext);
            string newGroupDelta = nameof(newGroupDelta);
            string newGroupNext = nameof(newGroupNext);

            AadOrgConfig newAadConfig = new AadOrgConfig
            {
                // Sync Settings
                GroupsMode = CollectionSyncMode.All,
                UsersMode = CollectionSyncMode.All,

                // Sync Context - These should still be ignored
                DeltaLinkUsers = newUserDelta,
                NextLinkUsers = newUserNext,
                DeltaLinkGroups = newGroupDelta,
                NextLinkGroups = newGroupNext,

                // Security
                TenantId = newTenantId,
                AppId = newAppId,
                SecretKey = newSecretKey
            };

            OrgSyncConfig newOrgConfig = new OrgSyncConfig
            {
                SyncType = AadOrgSyncStrategyBase.Name,
                SyncConfigJson = newAadConfig.ToLowerCamelJson(false)
            };

            MockOrgStore orgStore = new MockOrgStore();
            using MockSyncInfrastructure syncInfra = new MockSyncInfrastructure();
            MockGraphClient graphClient = new MockGraphClient()
            {
                Users = new GraphUser[] { }
            };
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(StrategyLogger, oldOrgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);
            ISyncStrategyFactory factory = new MockSyncStrategyFactory(provider);
            MockOrgService orgService = new MockOrgService
            {
                OrgSyncConfig = oldOrgConfig
            };
            ISecurityContext security = SecurityContext(syncInfra.Db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), syncInfra, security);

            // ACT
            IOrgSyncService OrgSyncService = new OrgSyncService(OrgSyncServiceLogger, factory, syncInfra, orgService, orgStore, dbRbac);
            await OrgSyncService.UpdateConfigAsync(IdentityDbSeed.OrgPrimary.Route, newOrgConfig);

            // ASSERT

            // Database config
            OrgSyncConfig dbConfig = orgService.OrgSyncConfig;
            oldAadConfig.SecretKey = newAadConfig.SecretKey; // Expect saved config to have the new secret
            oldAadConfig.DeltaLinkUsers = null;
            oldAadConfig.NextLinkUsers = null;
            oldAadConfig.DeltaLinkGroups = null;
            oldAadConfig.NextLinkGroups = null;
            AssertDbConfig(dbConfig, newAadConfig, oldAadConfig, AadOrgSyncStrategyBase.Name);

            // API Available config
            OrgSyncConfig readConfig = await OrgSyncService.ReadSanitizedConfigAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertReadConfig(AadOrgSyncStrategyBase.Name, newAadConfig, readConfig);
        }

        /// <summary>
        /// Examines the database version of the OrgSyncConfig
        /// </summary>
        /// <param name="actualConfig"></param>
        /// <param name="expectedNewAad"></param>
        /// <param name="expectedOldAad"></param>
        /// <param name="expectedProviderType"></param>
        private static void AssertDbConfig(OrgSyncConfig actualConfig, AadOrgConfig expectedNewAad, AadOrgConfig expectedOldAad, string expectedProviderType)
        {
            Assert.NotNull(actualConfig);
            Assert.NotNull(actualConfig.SyncType);
            Assert.Equal(expectedProviderType, actualConfig.SyncType);
            Assert.NotNull(actualConfig.SyncConfigJson);

            // AAD config
            AadOrgConfig actualAad = actualConfig.GetSyncConfig<AadOrgConfig>();
            Assert.NotNull(actualAad);
            Assert.NotNull(actualAad.TenantId);
            // Security
            Assert.Equal(expectedNewAad.TenantId, actualAad.TenantId);
            Assert.Equal(expectedNewAad.AppId, actualAad.AppId);
            Assert.Equal(expectedOldAad.SecretKey, actualAad.SecretKey); // Secret key should never come back
            // Settings
            Assert.Equal(expectedNewAad.ImportAllGroups, actualAad.ImportAllGroups);
            Assert.Equal(expectedNewAad.ImportAllUsers, actualAad.ImportAllUsers);
            // Context
            Assert.Equal(expectedOldAad.DeltaLinkUsers, actualAad.DeltaLinkUsers);
            Assert.Equal(expectedOldAad.NextLinkUsers, actualAad.NextLinkUsers);
            Assert.Equal(expectedOldAad.DeltaLinkGroups, actualAad.DeltaLinkGroups);
            Assert.Equal(expectedOldAad.NextLinkGroups, actualAad.NextLinkGroups);
        }

        private static void AssertReadConfig(string expectedProviderType, AadOrgConfig expectedAad, OrgSyncConfig actualConfig)
        {
            // Org
            Assert.NotNull(actualConfig);
            Assert.NotNull(actualConfig.SyncType);
            Assert.Equal(actualConfig.SyncType, expectedProviderType);
            Assert.NotNull(actualConfig.SyncConfigJson);

            // AAD
            AadOrgConfig aadConfig = actualConfig.GetSyncConfig<AadOrgConfig>();
            Assert.NotNull(aadConfig);
            Assert.NotNull(aadConfig.TenantId);

            // Security
            Assert.Equal(aadConfig.TenantId, expectedAad.TenantId);
            Assert.Equal(aadConfig.AppId, expectedAad.AppId);
            Assert.Null(aadConfig.SecretKey); // Secret key should never come back
            // Settings
            Assert.Equal(aadConfig.ImportAllGroups, expectedAad.ImportAllGroups);
            Assert.Equal(aadConfig.ImportAllUsers, expectedAad.ImportAllUsers);
            // Context
            Assert.Null(aadConfig.DeltaLinkUsers);
            Assert.Null(aadConfig.NextLinkUsers);
            Assert.Null(aadConfig.DeltaLinkGroups);
            Assert.Null(aadConfig.NextLinkGroups);
        }

        [Fact]
        public async Task HasAccess_Granted()
        {
            // ARRANGE
            Utils.ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult _, out MockGraphClient graphClient);

            // ARRANGE
            TestBuilder<MockSyncInfrastructure, MockSyncDb> builder = new TestBuilder<MockSyncInfrastructure, MockSyncDb>(TestUserContext.OrgAdmin);

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(StrategyLogger, orgConfig, new MockGraphFactory(graphClient), orgStore, builder.Infrastructure.Value);
            ISyncStrategyFactory factory = new MockSyncStrategyFactory(provider);
            MockOrgService orgService = new MockOrgService
            {
                OrgSyncConfig = orgConfig
            };

            // ACT
            IOrgSyncService OrgSyncService = new OrgSyncService(
                OrgSyncServiceLogger,
                factory,
                builder.Infrastructure.Value,
                orgService,
                orgStore,
                builder.Rbac.Value);

            bool hasAccess = await OrgSyncService.HasAccessAsync(IdentityDbSeed.OrgPrimary.Route, orgConfig);

            // ASSERT
            Assert.True(hasAccess);
        }
    }
}
