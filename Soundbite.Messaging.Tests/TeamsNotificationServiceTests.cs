using Masticore.Azure.MediaServices;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Providers;
using Masticore.Services;
using Masticore.Services.Tests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Soundbite.Entity;
using Soundbite.Services;
using Soundbite.Services.Tests;
using Soundbite.Services.Tests.Mock;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Messaging.Tests
{
    public class TeamsNotificationServiceTests : ServiceTestBase
    {

        private static Mock<ITeamsGraphService> GetTeamsGraphMock()
        {
            Mock<ITeamsGraphService> teamsGraphMock = new Mock<ITeamsGraphService>();
            teamsGraphMock.Setup(t => t.SendTeamsAppNotification(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                )).Returns(Task.FromResult("Example"));
            return teamsGraphMock;
        }

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
            MockImageService images = new MockImageService();
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
                config, //TODO: make part of builder
                new AzureMediaService() //TODO: make part of builder
            );
            return service;
        }

        [Fact]
        public async Task NoopMethods()
        {
            // ARRANGE
            User receiver = await UserWhereRoute();
            Organization org = await OrgAsync();
            SessionEntity session = await SessionEntityAsync();
            TeamsAppAzureSettings azureSettings = new TeamsAppAzureSettings
            {
                BaseUrl = "ABC.com"
            };

            Mock<ITeamsGraphService> teamsGraphMock = GetTeamsGraphMock();
            TeamsNotificationService teamsNotifications =
                new TeamsNotificationService(
                    azureSettings,
                    Builder.Infrastructure.Value,
                    new NullLogger<TeamsNotificationService>());

            // ACT
            // These methods should do nothing for now
            await teamsNotifications.SessionHostPublishAsync(receiver, org, session);
            await teamsNotifications.SessionReminderAsync(receiver, org, session);

            // ASSERT
            teamsGraphMock.Verify(x => x.SendTeamsAppNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SessionPublish_NoTeamsSettingsForOrg()
        {
            // ARRANGE
            User receiver = await UserWhereRoute();
            Organization org = await OrgAsync();
            SessionEntity session = await SessionEntityAsync();
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // By default, there are no org settings to enable teams notifications
            TeamsAppAzureSettings azureSettings = new TeamsAppAzureSettings
            {
                BaseUrl = "ABC.com"
            };
            Mock<ITeamsGraphService> teamsGraphMock = GetTeamsGraphMock();
            TeamsNotificationService teamsNotifications =
                new TeamsNotificationService(
                    azureSettings,
                    Builder.Infrastructure.Value,
                    new NullLogger<TeamsNotificationService>());

            // ACT
            // These methods should do nothing for now
            await teamsNotifications.SessionPublishAsync(receiver, org, session);

            // ASSERT
            snap.AssertAdd<SessionNotificationEntity>(0);
            teamsGraphMock.Verify(x => x.SendTeamsAppNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SessionPublish_WithSettings()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            User receiver = await UserWhereRoute();
            Organization org = await OrgAsync();
            SessionEntity session = await SessionEntityAsync();
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // Toggle notifications on
            SbOrganizationService orgs = CreateSbOrgService();
            OrgAzureSettings settings = await orgs.ReadAzureSettings(org.Route);
            settings.EnableTeamsNotifications = true;
            settings.TenantId = "123";
            await orgs.UpdateAzureSettings(org.Route, settings);

            TeamsAppAzureSettings azureSettings = new TeamsAppAzureSettings
            {
                BaseUrl = "ABC.com"
            };

            TeamsNotificationService teamsNotifications =
                new TeamsNotificationService(
                    azureSettings,
                    Builder.Infrastructure.Value,
                    Builder.Logger<TeamsNotificationService>());

            // ACT
            await teamsNotifications.SessionPublishAsync(receiver, org, session);

            // ASSERT
            snap.AssertAdd<SessionNotificationEntity>(1);
        }

        [Fact]
        public async Task SessionPublish_WithOrgDisabledSettings()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            User receiver = await UserWhereRoute();
            Organization org = await OrgAsync();
            SessionEntity session = await SessionEntityAsync();
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // Toggle notifications explicity off
            SbOrganizationService orgs = CreateSbOrgService();
            OrgAzureSettings settings = await orgs.ReadAzureSettings(org.Route);
            settings.EnableTeamsNotifications = false;
            settings.TenantId = "123";
            await orgs.UpdateAzureSettings(org.Route, settings);

            TeamsAppAzureSettings azureSettings = new TeamsAppAzureSettings
            {
                BaseUrl = "ABC.com"
            };
            Mock<ITeamsGraphService> teamsGraphMock = GetTeamsGraphMock();
            TeamsNotificationService teamsNotifications =
                new TeamsNotificationService(
                    azureSettings,
                    Builder.Infrastructure.Value,
                    Builder.Logger<TeamsNotificationService>());

            // ACT
            // These methods should do nothing for now
            await teamsNotifications.SessionPublishAsync(receiver, org, session);

            // ASSERT
            snap.AssertAdd<SessionNotificationEntity>(0);
            teamsGraphMock.Verify(x => x.SendTeamsAppNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
