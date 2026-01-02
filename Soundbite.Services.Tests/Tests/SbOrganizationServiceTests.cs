using Masticore;
using Masticore.Azure.MediaServices;
using Masticore.Entity.Tests;
using Masticore.Providers;
using Masticore.Services;
using Soundbite.Models;
using Soundbite.Services.Tests.Mock;
using Soundbite.Settings;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class SbOrganizationServiceTests : ServiceTestBase
    {
        #region Supporting Methods

        private SbOrganizationService CreateSbOrgService()
        {
            // ARRANGE

            // Orgs
            IOrganizationService organizationService = new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);

            // Sessions
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            SessionService sessions = new SessionService(
                Builder.Rbac.Value,
                series,
                Builder.ImageService.Value,
                clipFileService,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                providerFactory,
                Builder.Mapper.Value,
                Builder.Logger<SessionService>());

            AzureMediaServiceConfig config = new AzureMediaServiceConfig(); //TODO: make part of builder
            SbOrganizationService service = new SbOrganizationService(
                organizationService,
                sessions,
                Builder.Infrastructure.Value,
                Builder.Logger<SbOrganizationService>(),
                Builder.Rbac.Value,
                new MockEmailTemplates(),
                Builder.Mapper.Value,
                Builder.TokenDataService.Value,
                config, // TODO: make part of builder
                new AzureMediaService()); // TODO: make part of builder

            return service;
        }

        private static void AssertInitial(OrgNotificationSettings settings)
        {
            Assert.NotNull(settings);
            // Default is for everything to start false since the seed org has no settings
            Assert.True(settings.IsWelcomeEnabled);
            Assert.True(settings.IsPersonInviteEnabled);
            Assert.True(settings.IsMemberInvitedEnabled);
            Assert.True(settings.IsSessionReminderEnabled);
            Assert.True(settings.IsSessionPublishEnabled);
            Assert.True(settings.IsSessionHostPublishEnabled);
            Assert.True(settings.Channels.IsEmailEnabled);
            Assert.True(settings.Channels.IsSmsEnabled);
        }

        private static void AssertMatching(OrgNotificationSettings expected, OrgNotificationSettings actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.IsWelcomeEnabled, actual.IsWelcomeEnabled);
            Assert.Equal(expected.IsPersonInviteEnabled, actual.IsPersonInviteEnabled);
            Assert.Equal(expected.IsMemberInvitedEnabled, actual.IsMemberInvitedEnabled);
            Assert.Equal(expected.IsSessionReminderEnabled, actual.IsSessionReminderEnabled);
            Assert.Equal(expected.IsSessionPublishEnabled, actual.IsSessionPublishEnabled);
            Assert.Equal(expected.IsSessionHostPublishEnabled, actual.IsSessionHostPublishEnabled);
        }

        private static void AssertInitial(OrgAzureSettings settings)
        {
            Assert.NotNull(settings);
            // Default is for everything to start false since the seed org has no settings
            Assert.False(settings.EnableTeamsNotifications);
            Assert.Null(settings.TenantId);
        }

        private static void AssertMatching(OrgAzureSettings expected, OrgAzureSettings actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.EnableTeamsNotifications, actual.EnableTeamsNotifications);
            Assert.Equal(expected.TenantId, actual.TenantId);
        }

        private static void AssertInitial(OrgPermissions settings)
        {
            Assert.NotNull(settings);
            // Default is for everything to start false since the seed org has no settings
            Assert.Equal(PersonRole.Admin, settings.MinRoleToCreatePublic);
            Assert.Equal(MemberRole.Owner, settings.MinRoleToCreateTeamPublic);
            Assert.Equal(PersonRole.Person, settings.MinRoleToCreateTeam);
            Assert.Equal(PersonRole.Person, settings.MinRoleToCreateSession);
            Assert.Equal(MemberRole.Member, settings.MinRoleToCreateTeamSession);
            Assert.Equal(PersonRole.Person, settings.MinRoleForOrgInvite);
            Assert.Equal(MemberRole.Member, settings.MinRoleForTeamInvite);
            Assert.Equal(PersonRole.Person, settings.MinRoleForAudience);
        }

        private static void AssertMatching(OrgPermissions expected, OrgPermissions actual)
        {
            Assert.NotNull(actual);
            // Default is for everything to start false since the seed org has no settings
            Assert.Equal(expected.MinRoleToCreatePublic, actual.MinRoleToCreatePublic);
            Assert.Equal(expected.MinRoleToCreateTeam, actual.MinRoleToCreateTeam);
            Assert.Equal(expected.MinRoleToCreateSession, actual.MinRoleToCreateSession);
            Assert.Equal(expected.MinRoleToCreateTeamSession, actual.MinRoleToCreateTeamSession);
            Assert.Equal(expected.MinRoleForOrgInvite, actual.MinRoleForOrgInvite);
            Assert.Equal(expected.MinRoleForTeamInvite, actual.MinRoleForTeamInvite);
            Assert.Equal(expected.MinRoleForAudience, actual.MinRoleForAudience);
            Assert.Equal(expected.MinRoleToCreateTeamPublic, actual.MinRoleToCreateTeamPublic);
        }

        #endregion

        #region Tests

        [Fact]
        public async Task ReadAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbOrganizationService service = CreateSbOrgService();

            // ACT
            OrganizationWithSettings org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(org);
            Assert.NotNull(org.Settings);
            Assert.NotNull(org.Details);
            Assert.Equal(IdentityDbSeed.OrgPrimary.Route, org.Details.Route);
        }

        [Fact]
        public async Task ReadAndUpdateNotificationSettings()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbOrganizationService service = CreateSbOrgService();

            // ACT
            // Verify initial settings match expectation
            OrgNotificationSettings currentSettings = await service.ReadNotificationSettings(IdentityDbSeed.OrgPrimary.Route);
            AssertInitial(currentSettings);
            OrganizationWithSettings org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertInitial(org.Settings.Notifications);

            // Change to new settings
            OrgNotificationSettings newSettings = new OrgNotificationSettings
            {
                IsWelcomeEnabled = true,
                IsPersonInviteEnabled = false,
                IsMemberInvitedEnabled = true,
                IsSessionReminderEnabled = true,
                IsSessionHostPublishEnabled = false,
                IsSessionPublishEnabled = true,
            };
            currentSettings = await service.UpdateNotificationSettings(IdentityDbSeed.OrgPrimary.Route, newSettings);

            // ASSERT
            AssertMatching(newSettings, currentSettings);
            // Re-read directly and double-check, maybe it's different?
            currentSettings = await service.ReadNotificationSettings(IdentityDbSeed.OrgPrimary.Route);
            AssertMatching(newSettings, currentSettings);
            org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertMatching(newSettings, org.Settings.Notifications);
        }

        [Fact]
        public async Task ReadAndUpdateAzureSettings()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbOrganizationService service = CreateSbOrgService();

            // ACT
            // Verify initial settings match expectation
            OrgAzureSettings currentSettings = await service.ReadAzureSettings(IdentityDbSeed.OrgPrimary.Route);
            AssertInitial(currentSettings);
            OrganizationWithSettings org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertInitial(org.Settings.Azure);

            // Change to new settings
            OrgAzureSettings newSettings = new OrgAzureSettings
            {
                EnableTeamsNotifications = true,
                TenantId = "123-ABC-XYZ",
            };
            currentSettings = await service.UpdateAzureSettings(IdentityDbSeed.OrgPrimary.Route, newSettings);

            // ASSERT
            AssertMatching(newSettings, currentSettings);
            // Re-read directly and double-check, maybe it's different?
            currentSettings = await service.ReadAzureSettings(IdentityDbSeed.OrgPrimary.Route);
            AssertMatching(newSettings, currentSettings);
            org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertMatching(newSettings, org.Settings.Azure);
        }

        [Fact]
        public async Task ReadAndUpdatePermissions()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbOrganizationService service = CreateSbOrgService();

            // ACT
            // Verify initial settings match expectation
            OrgPermissions currentSettings = await service.ReadPermissions(IdentityDbSeed.OrgPrimary.Route);
            AssertInitial(currentSettings);
            OrganizationWithSettings org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertInitial(org.Settings.Permissions);

            // Change to new settings
            OrgPermissions newSettings = new OrgPermissions
            {
                MinRoleToCreatePublic = PersonRole.Person,
                MinRoleToCreateTeam = PersonRole.Admin,
                MinRoleToCreateSession = PersonRole.Admin,
                MinRoleToCreateTeamSession = MemberRole.Owner,
                MinRoleForOrgInvite = PersonRole.Admin,
                MinRoleForTeamInvite = MemberRole.Owner,
                MinRoleForAudience = PersonRole.Admin,
            };
            currentSettings = await service.UpdatePermissions(IdentityDbSeed.OrgPrimary.Route, newSettings);

            // ASSERT
            AssertMatching(newSettings, currentSettings);
            // Re-read directly and double-check, maybe it's different?
            currentSettings = await service.ReadPermissions(IdentityDbSeed.OrgPrimary.Route);
            AssertMatching(newSettings, currentSettings);
            org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            AssertMatching(newSettings, org.Settings.Permissions);
        }

        [Fact]
        public async Task ReadAndUpdateTheme()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbOrganizationService service = CreateSbOrgService();

            // ACT
            // Verify it starts empty
            OrganizationWithSettings org = await service.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            Assert.NotNull(org);
            Assert.NotNull(org.Settings);
            Assert.Null(org.Settings.Theme);

            // Verify it can be set to a value and published
            const string ExampleName = "Example";
            Theme newTheme = new Theme
            {
                Name = ExampleName
            };
            Theme finalTheme = await service.UpdateTheme(IdentityDbSeed.OrgPrimary.Route, newTheme);
            Assert.NotNull(finalTheme);
            Assert.Equal(ExampleName, finalTheme.Name);
            Assert.NotNull(finalTheme.PublishedUtc);

            // Verify we can clear it to null
            Theme nullTheme = await service.UpdateTheme(IdentityDbSeed.OrgPrimary.Route, null);
            Assert.Null(nullTheme);
        }

        #endregion
    }
}