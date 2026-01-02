using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Soundbite.Entity;
using Soundbite.Services;
using Soundbite.Services.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Messaging.Tests
{
    public class NotificationProcessingServiceTests : ServiceTestBase
    {
        #region Methods - Utility

        private IChannelProcessorFactory GetChannelProcessorFactory()
        {
            // Settings are for the Test version of the Teams application...
            TeamsAppAzureSettings teamsAppAzureSettings = new TeamsAppAzureSettings()
            {
                BaseUrl = "[TEAMS APP BASE URL]",
                ClientId = "[TEAMS APP API KEY]",
                SecretKey = "[TEAMS APP SECRET]"
            };

            Mock<IOrgSettingsService> orgSettingsService = new Mock<IOrgSettingsService>();
            orgSettingsService.Setup(i => i.ReadOrgSettingsAsync<OrgSettings>(It.IsAny<string>()))
                .Returns(Task.FromResult(new OrgSettings()
                {
                    Azure = new OrgAzureSettings()
                    {
                        TenantId = "[AMS TENANT ID]",
                        EnableTeamsNotifications = true
                    }
                }));

            Mock<IUserSettingsService> userSettignsService = new Mock<IUserSettingsService>();

            TeamsGraphService teamsGraphService = new TeamsGraphService(
                new NullLogger<TeamsGraphService>(),
                Masticore.Tests.Extensions.CreateHttpClientFactory(),
                teamsAppAzureSettings,
                orgSettingsService.Object,
                 userSettignsService.Object);

            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(i => i.GetService(It.IsAny<Type>()))
                .Returns(new TeamsChannelProcessor(new NullLogger<TeamsChannelProcessor>(), Builder.Infrastructure.Value, teamsAppAzureSettings, teamsGraphService));
            ChannelProcessorFactory factory = new ChannelProcessorFactory(serviceProvider.Object);
            return factory;
        }

        #endregion

        [Fact]
        public async Task HasMessagesToSend_NoMessages()
        {
            // Arrange
            NotificationProcessingService notificationProcessingService = new NotificationProcessingService(Builder.Infrastructure.Value, GetChannelProcessorFactory());

            // Act
            bool result = await notificationProcessingService.HasMessagesToSend(NotificationChannel.Teams);

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task HasMessagesToSend_HasMessages()
        {
            // Arrange
            ISbInfrastructure sbInfrastructure = Builder.Infrastructure.Value;
            NotificationProcessingService notificationProcessingService = new NotificationProcessingService(Builder.Infrastructure.Value, GetChannelProcessorFactory());
            SbDb sbDb = await sbInfrastructure.DbAsync();
            Masticore.Entity.UserEntity user = await sbDb.Users.FirstAsync();
            SessionEntity session = await sbDb.Sessions.FirstAsync();
            await sbDb.CreateSessionNotification(user.Route, session.Route, SessionNotificationType.Publish, NotificationChannel.Teams, null, NotificationStatus.Pending);
            await sbDb.SaveChangesAsync();

            // Act
            bool result = await notificationProcessingService.HasMessagesToSend(NotificationChannel.Teams);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasMessagesToSend_UniqueOrgIdsWithMessages()
        {
            // Arrange
            ISbInfrastructure sbInfrastructure = Builder.Infrastructure.Value;
            NotificationProcessingService notificationProcessingService = new NotificationProcessingService(Builder.Infrastructure.Value, GetChannelProcessorFactory());
            SbDb sbDb = await sbInfrastructure.DbAsync();
            Masticore.Entity.UserEntity user = await sbDb.Users.FirstAsync();
            SessionEntity session = await sbDb.Sessions.FirstAsync();
            SessionEntity session2 = await sbDb.Sessions.FirstAsync(i => i.OrganizationId != session.OrganizationId);
            await sbDb.CreateSessionNotification(user.Route, session.Route, SessionNotificationType.Publish, NotificationChannel.Teams, null, NotificationStatus.Pending);
            await sbDb.CreateSessionNotification(user.Route, session2.Route, SessionNotificationType.Publish, NotificationChannel.Teams, null, NotificationStatus.Pending);
            await sbDb.SaveChangesAsync();

            // Act
            int maxCount = 10;
            int testMaxCount = 1;
            IEnumerable<int> result = await notificationProcessingService.UniqueOrgIdsWithMessages(NotificationChannel.Teams, maxCount);
            IEnumerable<int> resultTestMaxCount = await notificationProcessingService.UniqueOrgIdsWithMessages(NotificationChannel.Teams, testMaxCount);
            IEnumerable<int> resultExclude = await notificationProcessingService.UniqueOrgIdsWithMessages(NotificationChannel.Teams, maxCount, result.Take(1));

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(resultTestMaxCount);
            Assert.NotNull(resultExclude);

            Assert.True(result.Count() == 2);
            Assert.True(resultTestMaxCount.Count() == 1);
            Assert.True(resultExclude.Count() == 1);
        }

        [Fact]
        public async Task GetMessagesForProcessing()
        {
            // Arrange
            ISbInfrastructure sbInfrastructure = Builder.Infrastructure.Value;
            NotificationProcessingService notificationProcessingService = new NotificationProcessingService(Builder.Infrastructure.Value, GetChannelProcessorFactory());
            SbDb sbDb = await sbInfrastructure.DbAsync();
            Masticore.Entity.UserEntity user = await sbDb.Users.FirstAsync();
            SessionEntity session = await sbDb.Sessions.FirstAsync();
            SessionEntity session2 = await sbDb.Sessions.FirstAsync(i => i.OrganizationId != session.OrganizationId);

            // Create a bunch of session notifications for the first organization
            for (int i = 0; i < 100; i++)
            {
                await sbDb.CreateSessionNotification(user.Route, session.Route, SessionNotificationType.Publish, NotificationChannel.Teams, null, NotificationStatus.Pending);
            }

            // Create a bunch of session notifications for the second organization
            for (int i = 0; i < 100; i++)
            {
                await sbDb.CreateSessionNotification(user.Route, session2.Route, SessionNotificationType.Publish, NotificationChannel.Teams, null, NotificationStatus.Pending);
            }

            await sbDb.SaveChangesAsync();

            // Act
            IEnumerable<SessionNotificationToProcess> resultOrg1 = await notificationProcessingService.GetMessagesForProcessing(session.OrganizationId, NotificationChannel.Teams, 100);
            IEnumerable<SessionNotificationToProcess> resultOrg2 = await notificationProcessingService.GetMessagesForProcessing(session2.OrganizationId, NotificationChannel.Teams, 50);

            // Assert
            Assert.NotNull(resultOrg1);
            Assert.NotNull(resultOrg2);
            Assert.Equal(100, resultOrg1.Count());
            Assert.Equal(50, resultOrg2.Count());

            // Make sure nothing from first org appears in second org
            foreach (SessionNotificationToProcess item in resultOrg1)
            {
                bool hasMatch = resultOrg2.Any(i => i.SessionNotificationId == item.SessionNotificationId);
                Assert.False(hasMatch);
            }

            // Make sure nothing from first second org appears in first org
            foreach (SessionNotificationToProcess item in resultOrg2)
            {
                bool hasMatch = resultOrg1.Any(i => i.SessionNotificationId == item.SessionNotificationId);
                Assert.False(hasMatch);
            }
        }
    }
}
