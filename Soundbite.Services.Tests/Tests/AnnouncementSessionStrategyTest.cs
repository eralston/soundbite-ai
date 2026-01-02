using Masticore;
using Masticore.Azure.MediaServices;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Providers;
using Masticore.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Soundbite.Entity;
using Soundbite.Messaging;
using Soundbite.Models;
using Soundbite.Services.Lifecycle;
using Soundbite.Services.Tests.Content;
using Soundbite.Services.Tests.Mock;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class AnnouncementSessionStrategyTest : ServiceTestBase
    {
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
                MockMapper.Instance,
                Builder.TokenDataService.Value,
                config, //TODO: make part of builder
                new AzureMediaService()); //TODO: Make part of builder

            return service;
        }
        [Fact]
        public async Task HappyPathAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            // Notifications
            GlobalNotificationSettings.SetAllFlags();
            MockEmailTemplates emailTemplates = new MockEmailTemplates();
            MockMailbox mailbox = new MockMailbox();
            SbOrganizationService orgs = CreateSbOrgService();
            EmailNotificationService emails = new EmailNotificationService(Builder.Mapper.Value, emailTemplates, mailbox, Builder.Infrastructure.Value, Builder.Logger<EmailNotificationService>(), new TeamsAppAzureSettings { BaseUrl = "https://google.com" }, Builder.ImageService.Value);
            MockSmsGateway smsGateway = new MockSmsGateway();
            SmsNotificationService sms = new SmsNotificationService(smsGateway, Builder.Infrastructure.Value, Builder.Logger<SmsNotificationService>(), new TeamsAppAzureSettings { BaseUrl = "https://google.com" });
            //TODO: REVISIT THE NULL REFERENCES IN THIS NEXT LINE...
            Mock<ICombinedNotificationService> teamsMock = new Mock<ICombinedNotificationService>();
            NotificationComposite notifications = new NotificationComposite(NullLogger<NotificationComposite>.Instance, emails, sms, teamsMock.Object);

            // Lifecycle and factory
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();
            AnnouncementLifecycleStrategy announcements =
                new AnnouncementLifecycleStrategy(
                    notifications,
                    Builder.Mapper.Value,
                    new NullLogger<AnnouncementLifecycleStrategy>(),
                    Builder.SbMediaService.Value,
                    Builder.SbTranscriptionService.Value);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory(announcements);

            // Supporting data access services
            SessionService sessions = new SessionService(Builder.Rbac.Value, series, Builder.ImageService.Value, clipFiles, lifecycleFactory, Builder.Infrastructure.Value, providerFactory, Builder.Mapper.Value, Builder.Logger<SessionService>());
            ClipService clips = new ClipService(
                Builder.SecurityContext.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                Builder.SbMediaService.Value,
                Builder.Mapper.Value,
                new NullLogger<ClipService>(),
                Builder.Rbac.Value);

            // We should start with one pre-existing Session from the seed data
            IEnumerable<SessionPreview> feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed.Count());
            string originalFeedSessionRoute = feed.First().Route;

            // The seed database session does NOT currently start published
            IEnumerable<SessionPreview> recentlyPublished = await sessions.ReadRecentlyPublishedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Single(recentlyPublished);

            // Create session
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db);

            IEnumerable<PersonEntity> peopleInOrg = await db.PeopleAsync(IdentityDbSeed.OrgPrimary.Route);
            int numberOfHosts = 3;
            IEnumerable<NewParticipant> newHosts = ToNewParticipants(peopleInOrg.Take(numberOfHosts), ParticipantRole.Host);
            IEnumerable<PersonEntity> audience = peopleInOrg.Skip(numberOfHosts).Take(peopleInOrg.Count() - numberOfHosts);
            IEnumerable<NewParticipant> newAudience = ToNewParticipants(audience, ParticipantRole.Audience);
            IEnumerable<NewParticipant> allParticipants = newAudience.Concat(newHosts);

            SessionDetails sessionDetails = await CreateSessionAndAssert(mailbox, smsGateway, sessions, originalFeedSessionRoute, snap, allParticipants);

            await UploadClipAndAssert(mailbox, smsGateway, clipFiles, sessions, clips, originalFeedSessionRoute, snap, newHosts, allParticipants, sessionDetails);

            await ConsumeSessionAndAssert(sessions, originalFeedSessionRoute, sessionDetails);

            await RunReportAndAssert(sessions, sessionDetails);
        }

        private static async Task<SessionDetails> CreateSessionAndAssert(MockMailbox mailbox, MockSmsGateway smsGateway, SessionService sessions, string originalFeedSessionRoute, DbSnapshot snap, IEnumerable<NewParticipant> allParticipants)
        {
            snap
            .Snap<ParticipantEntity>()
            .Snap<ParticipantGroupEntity>()
            .Snap<SessionEntity>()
            .Snap<SessionNotificationEntity>()
            .Snap<ClipEntity>();

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                // We are setting a publish date in the future, but null reminder assumes we are uploading a clip immediately
                Reminder = null,
                ReminderSent = null,
                Publish = Time.UtcNow,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = allParticipants,
            };
            SessionDetails sessionDetails = (await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));
            // We sent in a publish time, but no reminder. This means it's scheduled in its lifecycle since this does NOT take into account missing content
            Assert.Equal(SessionLifecycleStage.WaitingToPublish, sessionDetails.LifecycleStage());

            Assert.Equal(0, mailbox.Count);
            Assert.Equal(0, smsGateway.Count);
            snap.AssertAdd<SessionEntity>(1);
            snap.AssertAdd<ParticipantEntity>(allParticipants.Count());
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<SessionNotificationEntity>(0);

            // One already published session
            IEnumerable<SessionPreview> feed1 = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed1.Count());
            Assert.Equal(originalFeedSessionRoute, feed1.First().Route);

            // There should still only be one thing from the past
            IEnumerable<SessionPreview> recentlyPublished1 = await sessions.ReadRecentlyPublishedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Single(recentlyPublished1);
            return sessionDetails;
        }

        private static async Task UploadClipAndAssert(MockMailbox mailbox, MockSmsGateway smsGateway, MockClipFileService clipFiles, SessionService sessions, ClipService clips, string originalFeedSessionRoute, DbSnapshot snap, IEnumerable<NewParticipant> newHosts, IEnumerable<NewParticipant> allParticipants, SessionDetails sessionDetails)
        {
            // ACT

            // Upload clip
            snap
                .Snap<ParticipantEntity>()
                .Snap<ParticipantGroupEntity>()
                .Snap<SessionEntity>()
                .Snap<SessionNotificationEntity>()
                .Snap<ClipEntity>();
            NewClip newClip = new NewClip
            {
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                Stream = MediaExamples.Valid_ThreeSecondCbrMp3,
                ParticipantRole = ParticipantRole.Host
            };
            string promptRoute = sessionDetails.Prompts.First().Route;
            // Creating clip fires the lifecycle
            await clips.CreateAsync(IdentityDbSeed.OrgPrimary.Route, sessionDetails.Route, promptRoute, newClip);

            // ASSERT

            // Check we uploaded and generated notifications, but did not create records for more than just the clip
            Assert.Equal(1, clipFiles.UploadCount);
            // Each unique participant and each unique host receives a notification
            Assert.Equal(allParticipants.Count() + newHosts.Count(), mailbox.Count);
            Assert.Equal(allParticipants.Count() + newHosts.Count(), smsGateway.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(1);
            snap.AssertAdd<SessionNotificationEntity>(28);

            // Confirm deadline was sent
            SessionDetails latestSession = await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, sessionDetails.Route);
            Assert.NotNull(latestSession.PublishSent);
            Assert.Equal(SessionLifecycleStage.Published, latestSession.LifecycleStage());

            // Reading the feed should now contain the published session
            IEnumerable<SessionPreview> feed1 = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(3, feed1.Count());
            Assert.True(feed1.Where(s => s.Route == originalFeedSessionRoute).Any()); // contains original
            Assert.True(feed1.Where(s => s.Route == sessionDetails.Route).Any()); // contains new session
            Assert.Equal(2, feed1.Where(s => s.PublishSent != null).Count()); // contains only a single published session

            IEnumerable<SessionPreview> recentlyPublished1 = await sessions.ReadRecentlyPublishedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, recentlyPublished1.Count());
        }

        private static async Task ConsumeSessionAndAssert(SessionService sessions, string originalFeedSessionRoute, SessionDetails sessionDetails)
        {
            // Update session to mark as consumed
            await sessions.UpdateStateAsync(IdentityDbSeed.OrgPrimary.Route, sessionDetails.Route, ParticipantState.Consumed);

            // Consumed session should NOT appear in feed anymore, just the old ones
            IEnumerable<SessionPreview> feed1 = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed1.Count());
            Assert.Equal(originalFeedSessionRoute, feed1.First().Route);

            IEnumerable<SessionPreview> recentlyPublished1 = await sessions.ReadRecentlyPublishedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, recentlyPublished1.Count());
        }

        private async Task RunReportAndAssert(SessionService sessions, SessionDetails sessionDetails)
        {
            // Reporting
            // ACT
            IReportService reports = new ReportService(Builder.Infrastructure.Value, Builder.Logger<ReportService>(), Builder.Rbac.Value, sessions, Builder.Mapper.Value);
            SessionContentDetailsReport report = await reports.SessionContentDetailsReportAsync(
                IdentityDbSeed.OrgPrimary.Route,
                sessionDetails.Route);

            // ASSERT - Depends on SbDbSeed and ARRANGE above
            Assert.NotNull(report);
            Assert.Equal(0, int.Parse(report.ConsumeCount.Value));
            Assert.Empty(report.Plays);
            Assert.Empty(report.ConsumeCountOverTime.Items);
            Assert.Equal(0, int.Parse(report.ConsumerCount.Value));
            Assert.Equal(1, int.Parse(report.AcknowledgeCount.Value));
            Assert.Single(report.Acknowledgers);
            Assert.Equal(11, int.Parse(report.AudienceSize.Value));
            Assert.Empty(report.Highlights);
            Assert.Equal(11, report.Audience.Length);
            Assert.NotNull(report.Audience[0].Person);
            Assert.NotNull(report.Audience[0].Person.User);
            Assert.Empty(report.AudienceGroups);
            Assert.Equal(28, report.Notifications.Length);
        }

        private static IEnumerable<NewParticipant> ToNewParticipants(IEnumerable<PersonEntity> audience, ParticipantRole role)
        {
            return audience.Select((p) =>
            {
                return new NewParticipant { ParticipantRole = role, PersonRoute = p.Route };
            });
        }
    }
}