using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Providers;
using Masticore.Resources;
using Masticore.Services.Tests;
using Masticore.Tests;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using Soundbite.Services;
using Soundbite.Services.Lifecycle;
using Soundbite.Services.Tests;
using Soundbite.Services.Tests.Mock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.AzFunc.Scheduler.Tests
{
    /// <summary>
    /// A testing class for anything about time travel and running tests since these manipulate the time static variable
    /// If you want to break up these tests, you can theoretically use the XUnit test annotations
    /// </summary>
    public class SchedulerTests : ServiceTestBase
    {
        #region Constructor

        public SchedulerTests()
        {
            Builder.NotificationService.Use(new MockNotifications());
        }

        #endregion

        #region Methods - Utility

        private Stream ValidMp3File
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "Soundbite.AzFunc.Scheduler.Tests.Content.Broke.mp3";

                return assembly.GetManifestResourceStream(resourceName);
            }
        }

        private async Task RunSeriesSchedulerAsync(MockLifecycleFactory lifecycleFactory)
        {

            SeriesScheduler scheduler = new SeriesScheduler(MockMapper.Instance, lifecycleFactory, Builder.Infrastructure.Value, new ThrowIfErrorLogger<SessionScheduler>())
            {
                ReThrowExceptions = true
            };
            await scheduler.ProcessAsync();
        }

        private async Task RunSeriesSchedulerAsync(MockLifecycleFactory lifecycleFactory, DbSnapshot snap, int expectedNumberOfSessions)
        {
            Reset(snap);
            await RunSeriesSchedulerAsync(lifecycleFactory);
            snap.AssertAdd<SeriesEntity>(0);
            snap.AssertAdd<SessionEntity>(expectedNumberOfSessions);
        }

        private async Task<SessionDetails> RunSessionSchedulerAsync(MockSbNotifications notifications, MockLifecycleFactory lifecycleFactory, SessionService sessions, SessionDetails sessionDetails)
        {
            await RunSessionSchedulerAsync(notifications, lifecycleFactory);
            SessionDetails latestSession = await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, sessionDetails.Route);
            return latestSession;
        }

        private async Task RunSessionSchedulerAsync(MockSbNotifications notifications, MockLifecycleFactory lifecycleFactory)
        {
            notifications.Reset();
            SessionScheduler scheduler = new SessionScheduler(lifecycleFactory, Builder.Infrastructure.Value, MockMapper.Instance, new ThrowIfErrorLogger<SessionScheduler>());
            await scheduler.ProcessAsync();
        }

        private async Task RunSessionSchedulerAsync(MockSbNotifications notifications, MockLifecycleFactory lifecycleFactory, SessionService sessions, int expectedFeedCount)
        {
            await RunSessionSchedulerAsync(notifications, lifecycleFactory);
            IEnumerable<Session> feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(expectedFeedCount, feed.Count());
        }

        private static IEnumerable<NewParticipant> ToNewParticipants(IEnumerable<PersonEntity> audience, ParticipantRole role)
        {
            return audience.Select((p) =>
            {
                return ToNewParticipant(role, p);
            });
        }

        private static NewParticipant ToNewParticipant(ParticipantRole role, PersonEntity p)
        {
            return new NewParticipant { ParticipantRole = role, PersonRoute = p.Route };
        }

        private static IEnumerable<NewParticipantGroup> ToNewGroups(IEnumerable<GroupEntity> groups, ParticipantRole role)
        {
            return groups.Select((grp) =>
            {
                return new NewParticipantGroup { ParticipantRole = role, GroupRoute = grp.Route };
            });
        }

        private static void Reset(DbSnapshot snap, MockSbNotifications notifications = null)
        {
            snap
                .Snap<ParticipantEntity>()
                .Snap<ParticipantGroupEntity>()
                .Snap<SessionEntity>()
                .Snap<ClipEntity>()
                .Snap<SeriesEntity>()
                .Snap<PromptEntity>();

            notifications?.Reset();
        }

        private static async Task<IEnumerable<Session>> AssertFeedAsync(SessionService sessions, ParticipantState audienceRole, ParticipantState hostState)
        {
            IEnumerable<SessionPreview> feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            SessionPreview feedSession = feed.OrderByDescending(s => s.Publish).First();
            // Check participants
            Assert.NotEmpty(feedSession.MyParticipants);
            Assert.Equal(2, feedSession.MyParticipants.Count());
            // Check audience
            Participant myAudience = feedSession.MyParticipants.Where(p => p.ParticipantRole == ParticipantRole.Audience).Single();
            Assert.Equal(ParticipantRole.Audience, myAudience.ParticipantRole);
            Assert.Equal(audienceRole, myAudience.ParticipantState);
            // Check host
            Participant myHost = feedSession.MyParticipants.Where(p => p.ParticipantRole == ParticipantRole.Host).Single();
            Assert.Equal(ParticipantRole.Host, myHost.ParticipantRole);
            Assert.Equal(hostState, myHost.ParticipantState);
            return feed;
        }

        private static void AssertParticipantState(SessionDetails session, ParticipantRole role, ParticipantState state)
        {
            bool allInState = session.Participants.Where(p => p.ParticipantRole == role).All(p => p.ParticipantState == state);
            Assert.True(allInState);
        }

        #endregion

        [Fact]
        public async Task Announcement_HappyWithFeed()
        {
            Time.Reset();

            // Setup services
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockSbNotifications notifications = new MockSbNotifications();
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            AnnouncementLifecycleStrategy announcements =
                new AnnouncementLifecycleStrategy(
                    notifications,
                    Builder.Mapper.Value,
                    new ThrowIfErrorLogger<AnnouncementLifecycleStrategy>(),
                    Builder.SbMediaService.Value,
                    Builder.SbTranscriptionService.Value);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory(announcements);

            // Services
            SessionService sessions = new SessionService(
                Builder.Rbac.Value,
                series,
                Builder.ImageService.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                providerFactory,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<SessionService>());

            ClipService clips = new ClipService(
                Builder.SecurityContext.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                Builder.SbMediaService.Value,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<ClipService>(),
                Builder.Rbac.Value);

            // Reset
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db)
                    .Snap<ClipEntity>()
                    .Snap<ParticipantEntity>()
                    .Snap<ParticipantGroupEntity>()
                    .Snap<SessionEntity>();

            // Fire previous sessions
            await RunSessionSchedulerAsync(notifications, lifecycleFactory);
            Assert.Equal(23, notifications.Count);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ParticipantEntity>(11);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<SessionEntity>(0);
            IEnumerable<SessionPreview> feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed.Count());

            // Make Things
            PersonEntity me = await db.PersonWhereUid(IdentityDbSeed.OrgPrimary.Route, Builder.SecurityContext.Value.UniversalId);
            IEnumerable<PersonEntity> peopleInOrg = await db.PeopleAsync(IdentityDbSeed.OrgPrimary.Route);
            IEnumerable<NewParticipant> hosts = new NewParticipant[] { ToNewParticipant(ParticipantRole.Host, me) };
            IEnumerable<NewParticipant> newAudience = ToNewParticipants(peopleInOrg, ParticipantRole.Audience);
            IEnumerable<NewParticipant> allParticipants = newAudience.Concat(hosts);

            DateTime now = Time.UtcNow;
            Time.FixedDateTime = now;
            DateTime reminder = now.AddHours(1);
            DateTime betweenReminderAndDeadline = reminder.AddHours(1);
            DateTime deadline = betweenReminderAndDeadline.AddHours(1);
            DateTime afterDeadline = deadline.AddHours(1);

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = reminder,
                ReminderSent = null,
                Publish = deadline,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = allParticipants,
            };

            // CREATE

            // Create the session, which should come back completely fresh
            notifications.Reset();
            SessionDetails sessionDetails = (await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);

            Assert.Equal(0, Builder.GetMockNotificationService().Count);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ParticipantEntity>(23);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<SessionEntity>(1);

            // Still only the first sessions
            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);

            // DRY FIRE BEFORE REMINDER

            // Fire the reminder, which should NOT do anything right now
            Time.FixedDateTime = now;
            Reset(snap, notifications);
            SessionDetails latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(0, Builder.GetMockNotificationService().Count);
            Assert.Null(latestSession.ReminderSent);
            Assert.Null(latestSession.PublishSent);

            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);

            // REMINDER

            // Move forward to after the reminder period, which should shoot out all the reminders to the hosts
            Time.FixedDateTime = betweenReminderAndDeadline;
            Reset(snap, notifications);
            latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(1, notifications.Count);
            Assert.NotNull(latestSession.ReminderSent);
            Assert.Null(latestSession.PublishSent);

            await AssertFeedAsync(sessions, ParticipantState.Pending, ParticipantState.ContributionRequested);

            // UPLOAD

            // Upload one clip from one host
            notifications.Reset();
            snap.Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>().Snap<SessionEntity>().Snap<ClipEntity>();
            NewClip newClip = new NewClip
            {
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                Stream = ValidMp3File,
                ParticipantRole = ParticipantRole.Host
            };
            string promptRoute = sessionDetails.Prompts.First().Route;
            await clips.CreateAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route, promptRoute, newClip);

            Assert.Equal(1, clipFiles.UploadCount);
            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(1);

            // Having uploaded the clip, the session will disappear from the feed
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);

            // DEADLINE

            // Move forward to after the deadline
            Time.FixedDateTime = afterDeadline;
            Reset(snap, notifications);
            latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(allParticipants.Count() - 1 + 1, notifications.Count); // There is one repeat person in the participants (current user), but one extra for the host
            Assert.NotNull(latestSession.ReminderSent);
            Assert.NotNull(latestSession.PublishSent);

            await AssertFeedAsync(sessions, ParticipantState.ConsumptionRequested, ParticipantState.ConsumptionRequested);

            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);
        }

        [Fact]
        public async Task Announcement_Happy()
        {
            Time.Reset();

            // Setup services
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockSbNotifications notifications = new MockSbNotifications();
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            AnnouncementLifecycleStrategy announcements =
                new AnnouncementLifecycleStrategy(
                    notifications,
                    MockMapper.Instance,
                    new ThrowIfErrorLogger<AnnouncementLifecycleStrategy>(),
                    Builder.SbMediaService.Value,
                    Builder.SbTranscriptionService.Value);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory(announcements);

            // Services
            SessionService sessions = new SessionService(
                Builder.Rbac.Value,
                series,
                Builder.ImageService.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                providerFactory,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<SessionService>());

            ClipService clips = new ClipService(
                Builder.SecurityContext.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                Builder.SbMediaService.Value,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<ClipService>(),
                Builder.Rbac.Value);

            // Reset
            SbDb db = Builder.DbContext.Value;
            db.Sessions.ForEach(s => s.DeletedUtc = DateTime.UtcNow);
            db.SaveChanges();

            // Make Things
            DbSnapshot snap = new DbSnapshot(db).Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>().Snap<SessionEntity>().Snap<ClipEntity>();
            IEnumerable<PersonEntity> peopleInOrg = await db.PeopleAsync(SbDbSeed.OrgPrimary.Route);
            int numberOfHosts = 3;
            IEnumerable<NewParticipant> newHosts = ToNewParticipants(peopleInOrg.Take(numberOfHosts), ParticipantRole.Host);
            IEnumerable<PersonEntity> audience = peopleInOrg.Skip(numberOfHosts).Take(peopleInOrg.Count() - numberOfHosts);
            IEnumerable<NewParticipant> newAudience = ToNewParticipants(audience, ParticipantRole.Audience);
            IEnumerable<NewParticipant> allParticipants = newAudience.Concat(newHosts);

            DateTime now = Time.UtcNow;
            Time.FixedDateTime = now;
            DateTime reminder = now.AddHours(1);
            DateTime betweenReminderAndDeadline = reminder.AddHours(1);
            DateTime deadline = betweenReminderAndDeadline.AddHours(1);
            DateTime afterDeadline = deadline.AddHours(1);

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = reminder,
                ReminderSent = null,
                Publish = deadline,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = allParticipants,
            };

            // Create the session, which should come back completely fresh
            SessionDetails sessionDetails = (await sessions.CreateAsync(SbDbSeed.OrgPrimary.Route, newSession));
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);

            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(1);
            snap.AssertAdd<ParticipantEntity>(allParticipants.Count());
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(0);

            // Fire the reminder, which should NOT do anything right now
            Time.FixedDateTime = now;
            SessionDetails latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(0, notifications.Count);
            Assert.Null(latestSession.ReminderSent);
            Assert.Null(latestSession.PublishSent);

            // Move forward to after the reminder period, which should shoot out all the reminders to the hosts
            Time.FixedDateTime = betweenReminderAndDeadline;
            latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(3, notifications.Count);
            Assert.NotNull(latestSession.ReminderSent);
            Assert.Null(latestSession.PublishSent);

            // Upload one clip from one host
            notifications.Reset();
            snap.Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>().Snap<SessionEntity>().Snap<ClipEntity>();
            NewClip newClip = new NewClip
            {
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                Stream = ValidMp3File,
                ParticipantRole = ParticipantRole.Host
            };
            string promptRoute = sessionDetails.Prompts.First().Route;
            await clips.CreateAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route, promptRoute, newClip);

            Assert.Equal(1, clipFiles.UploadCount);
            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(1);

            // Move forward to after the deadline
            Time.FixedDateTime = afterDeadline;
            latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(allParticipants.Count() + newHosts.Count(), notifications.Count);
            Assert.NotNull(latestSession.ReminderSent);
            Assert.NotNull(latestSession.PublishSent);
        }

        [Fact]
        public async Task Announcement_NoReminder()
        {
            Time.Reset();

            // Setup services
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockSbNotifications notifications = new MockSbNotifications();
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            AnnouncementLifecycleStrategy announcements =
                new AnnouncementLifecycleStrategy(
                    notifications,
                    Builder.Mapper.Value,
                    new ThrowIfErrorLogger<AnnouncementLifecycleStrategy>(),
                    Builder.SbMediaService.Value,
                    Builder.SbTranscriptionService.Value);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory(announcements);

            // Services
            SessionService sessions = new SessionService(
                Builder.Rbac.Value,
                series,
                Builder.ImageService.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                providerFactory,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<SessionService>());

            ClipService clips = new ClipService(
                Builder.SecurityContext.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                Builder.SbMediaService.Value,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<ClipService>(),
                Builder.Rbac.Value);

            // Reset
            SbDb db = Builder.DbContext.Value;
            db.Sessions.ForEach(s => s.DeletedUtc = DateTime.UtcNow);
            db.SaveChanges();

            // Make Things
            DbSnapshot snap = new DbSnapshot(db).Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>().Snap<SessionEntity>().Snap<ClipEntity>();
            IEnumerable<PersonEntity> peopleInOrg = await db.PeopleAsync(SbDbSeed.OrgPrimary.Route);
            int numberOfHosts = 3;
            IEnumerable<NewParticipant> newHosts = ToNewParticipants(peopleInOrg.Take(numberOfHosts), ParticipantRole.Host);
            IEnumerable<PersonEntity> audience = peopleInOrg.Skip(numberOfHosts).Take(peopleInOrg.Count() - numberOfHosts);
            IEnumerable<NewParticipant> newAudience = ToNewParticipants(audience, ParticipantRole.Audience);
            IEnumerable<NewParticipant> allParticipants = newAudience.Concat(newHosts);

            DateTime now = Time.UtcNow;
            Time.FixedDateTime = now;
            DateTime deadline = now.AddHours(1);
            DateTime afterDeadline = deadline.AddHours(1);

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = null,
                ReminderSent = null,
                Publish = deadline,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = allParticipants,
            };

            // Create the session, which should come back completely fresh
            SessionDetails sessionDetails = (await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);

            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(1);
            snap.AssertAdd<ParticipantEntity>(allParticipants.Count());
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(0);

            // Fire the reminder, which should NOT do anything right now
            Time.FixedDateTime = now;
            SessionDetails latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(0, notifications.Count);
            Assert.Null(latestSession.ReminderSent);
            Assert.Null(latestSession.PublishSent);

            // Upload one clip from one host
            notifications.Reset();
            snap.Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>().Snap<SessionEntity>().Snap<ClipEntity>();
            NewClip newClip = new NewClip
            {
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                Stream = ValidMp3File,
                ParticipantRole = ParticipantRole.Host
            };
            string promptRoute = sessionDetails.Prompts.First().Route;
            await clips.CreateAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route, promptRoute, newClip);

            Assert.Equal(1, clipFiles.UploadCount);
            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(1);

            // Move forward to after the deadline
            Time.FixedDateTime = afterDeadline;
            latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(allParticipants.Count() + newHosts.Count(), notifications.Count);
            Assert.NotNull(latestSession.ReminderSent);
            Assert.NotNull(latestSession.PublishSent);
        }

        [Fact]
        public async Task Announcement_ExpandGroup()
        {
            Time.Reset();

            // Setup services
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockSbNotifications notifications = new MockSbNotifications();
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            AnnouncementLifecycleStrategy announcements =
                new AnnouncementLifecycleStrategy(
                    notifications,
                    MockMapper.Instance,
                    new ThrowIfErrorLogger<AnnouncementLifecycleStrategy>(),
                    Builder.SbMediaService.Value,
                    Builder.SbTranscriptionService.Value);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory(announcements);

            // Services
            SessionService sessions = new SessionService(
                Builder.Rbac.Value,
                series,
                Builder.ImageService.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                providerFactory,
                MockMapper.Instance,
                new ThrowIfErrorLogger<SessionService>());

            ClipService clips = new ClipService(
                Builder.SecurityContext.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                Builder.SbMediaService.Value,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<ClipService>(),
                Builder.Rbac.Value);

            // Reset

            // Fire previous sessions
            await RunSessionSchedulerAsync(notifications, lifecycleFactory);
            Assert.Equal(23, notifications.Count);
            IEnumerable<SessionPreview> feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);

            // Make Things
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>().Snap<SessionEntity>().Snap<ClipEntity>();
            GroupEntity memberGrp = await db.GroupWhereRoute(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            PersonEntity currentPerson = await db.PersonWhereUid(SbDbSeed.OrgPrimary.Route, Builder.SecurityContext.Value.UniversalId);

            DateTime now = Time.UtcNow;
            Time.FixedDateTime = now;
            DateTime reminder = now.AddHours(1);
            DateTime betweenReminderAndDeadline = reminder.AddHours(1);
            DateTime publish = betweenReminderAndDeadline.AddHours(1);
            DateTime afterPublish = publish.AddHours(1);

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = reminder,
                ReminderSent = null,
                Publish = publish,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = ToNewParticipants(new PersonEntity[] { currentPerson }, ParticipantRole.Host),
                Groups = ToNewGroups(new GroupEntity[] { memberGrp }, ParticipantRole.Audience),
            };

            // Create the session, which should come back completely fresh
            notifications.Reset();
            SessionDetails sessionDetails = (await sessions.CreateAsync(SbDbSeed.OrgPrimary.Route, newSession));
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);

            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(1);
            snap.AssertAdd<ParticipantEntity>(1);
            snap.AssertAdd<ParticipantGroupEntity>(1);
            snap.AssertAdd<ClipEntity>(0);

            // Fire the reminder, which should NOT do anything right now
            Time.FixedDateTime = now;
            Reset(snap, notifications);
            SessionDetails latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(0, notifications.Count);
            Assert.Null(latestSession.ReminderSent);
            Assert.Null(latestSession.PublishSent);
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(2, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);

            // Move forward to after the reminder period, which should shoot out all the reminders to the hosts
            Time.FixedDateTime = betweenReminderAndDeadline;
            Reset(snap, notifications);
            latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(1, notifications.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            Assert.NotNull(latestSession.ReminderSent);
            Assert.Null(latestSession.PublishSent);
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Equal(3, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(3, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);

            // Upload one clip from one host
            Reset(snap, notifications);
            NewClip newClip = new NewClip
            {
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                Stream = ValidMp3File,
                ParticipantRole = ParticipantRole.Host
            };
            string promptRoute = sessionDetails.Prompts.First().Route;
            await clips.CreateAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route, promptRoute, newClip);

            Assert.Equal(1, clipFiles.UploadCount);
            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(1);

            // Move forward to after the deadline
            Time.FixedDateTime = afterPublish;
            Reset(snap, notifications);
            latestSession = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(5);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            Assert.Equal(6, notifications.Count);
            Assert.NotNull(latestSession.ReminderSent);
            Assert.NotNull(latestSession.PublishSent);

            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Equal(3, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(3, feed.Count());
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            Assert.Empty(feed);
        }

        [Fact]
        public async Task Announcement_FromThePast()
        {
            // ARRANGE
            Time.Reset();

            // Setup services
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockSbNotifications notifications = new MockSbNotifications();
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            AnnouncementLifecycleStrategy announcements =
                new AnnouncementLifecycleStrategy(
                    notifications,
                    MockMapper.Instance,
                    new ThrowIfErrorLogger<AnnouncementLifecycleStrategy>(),
                    Builder.SbMediaService.Value,
                    Builder.SbTranscriptionService.Value);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory(announcements);

            // Services
            // Services
            SessionService sessions = new SessionService(
                Builder.Rbac.Value,
                series,
                Builder.ImageService.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                providerFactory,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<SessionService>());

            ClipService clips = new ClipService(
                Builder.SecurityContext.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                Builder.SbMediaService.Value,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<ClipService>(),
                Builder.Rbac.Value);

            // Reset
            SbDb db = Builder.DbContext.Value;
            db.Sessions.ForEach(s => s.DeletedUtc = DateTime.UtcNow);
            db.SaveChanges();

            // Make Things
            DbSnapshot snap = new DbSnapshot(db);
            Reset(snap, notifications);
            GroupEntity heirs = await db.GroupWhereRoute(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            PersonEntity currentPerson = await db.PersonWhereUid(SbDbSeed.OrgPrimary.Route, Builder.SecurityContext.Value.UniversalId);

            // Session was created 2 days day ago, scheduled to reminder 1 day ago, publish right now, and be listened to an hour after publish
            DateTime now = Time.UtcNow;
            DateTime created = now.AddDays(-2);
            Time.FixedDateTime = created;
            DateTime reminder = now.AddDays(-1);
            DateTime publish = now;
            DateTime afterPublish = publish.AddHours(1);

            // CREATE & UPLOAD

            // Create the session
            IEnumerable<NewParticipant> hosts = ToNewParticipants(new PersonEntity[] { currentPerson }, ParticipantRole.Host);
            IEnumerable<NewParticipant> participants = ToNewParticipants(new PersonEntity[] { currentPerson }, ParticipantRole.Audience);

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = reminder,
                ReminderSent = null,
                Publish = publish,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = hosts.Concat(participants)
            };
            SessionDetails sessionDetails = (await sessions.CreateAsync(SbDbSeed.OrgPrimary.Route, newSession));
            Assert.Equal(reminder, sessionDetails.Reminder);
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Equal(publish, sessionDetails.Publish);
            Assert.Null(sessionDetails.PublishSent);
            Assert.Equal(0, notifications.Count);
            AssertParticipantState(sessionDetails, ParticipantRole.Host, ParticipantState.Pending);
            AssertParticipantState(sessionDetails, ParticipantRole.Audience, ParticipantState.Pending);
            snap.AssertAdd<SessionEntity>(1);
            snap.AssertAdd<ParticipantEntity>(2);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<PromptEntity>(1);
            snap.AssertAdd<ClipEntity>(0);
            Reset(snap, notifications);

            // Immediately upload the clip
            NewClip newClip = new NewClip
            {
                Stream = ValidMp3File,
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                ParticipantRole = ParticipantRole.Host
            };
            PromptWithClips promp = sessionDetails.Prompts.First();
            ClipDetails response = (await clips.CreateAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route, promp.Route, newClip));
            Assert.NotNull(response);
            Assert.NotNull(response.OrgRoute);
            Assert.NotNull(response.SessionRoute);
            Assert.NotNull(response.PromptRoute);
            Assert.NotNull(response.ClipRoute);
            Assert.NotNull(response.DownloadUrl);
            Assert.Null(response.UploadUrl);

            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(1);
            Reset(snap, notifications);

            // Check the session is not notified yet and that the host registers as contributed
            sessionDetails = await sessions.ReadAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route);
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);
            IEnumerable<ClipWithContributor> promptClips = sessionDetails.Prompts.First().Clips;
            Assert.Single(promptClips);
            AssertParticipantState(sessionDetails, ParticipantRole.Host, ParticipantState.Contributed);
            AssertParticipantState(sessionDetails, ParticipantRole.Audience, ParticipantState.Pending);
            IEnumerable<SessionPreview> feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Empty(feed);

            // Fire the reminder, which should NOT do anything right now
            Reset(snap, notifications);
            sessionDetails = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(0, notifications.Count);
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);
            AssertParticipantState(sessionDetails, ParticipantRole.Host, ParticipantState.Contributed);
            AssertParticipantState(sessionDetails, ParticipantRole.Audience, ParticipantState.Pending);
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Empty(feed);

            // REMINDER

            // Move forward to after the reminder period, which should shoot out all the reminders to the hosts
            Time.FixedDateTime = reminder;
            Reset(snap, notifications);
            sessionDetails = await sessions.ReadAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route);
            sessionDetails = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            Assert.Equal(0, notifications.Count);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            Assert.Equal(reminder, sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);

            // Check the session is not notified yet and that the host registers as contributed
            sessionDetails = await sessions.ReadAsync(SbDbSeed.OrgPrimary.Route, sessionDetails.Route);
            Assert.Equal(reminder, sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);
            AssertParticipantState(sessionDetails, ParticipantRole.Host, ParticipantState.Contributed);
            AssertParticipantState(sessionDetails, ParticipantRole.Audience, ParticipantState.Pending);
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Empty(feed);

            // PUBLISH

            // Move forward to after the deadline
            Time.FixedDateTime = publish;
            Reset(snap, notifications);
            sessionDetails = await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, sessionDetails);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            Assert.Equal(2, notifications.Count);
            Assert.Equal(reminder, sessionDetails.ReminderSent);
            Assert.Equal(publish, sessionDetails.PublishSent);
            AssertParticipantState(sessionDetails, ParticipantRole.Host, ParticipantState.ConsumptionRequested);
            AssertParticipantState(sessionDetails, ParticipantRole.Audience, ParticipantState.ConsumptionRequested);
            feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Single(feed);
        }

        [Fact]
        public async Task SeriesScheduler_HappyPath()
        {
            Time.Reset();

            // Setup services
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockSbNotifications notifications = new MockSbNotifications();
            MockClipFileService clipFiles = new MockClipFileService();
            ProviderFactory providerFactory = new ProviderFactory();

            AnnouncementLifecycleStrategy announcements =
                new AnnouncementLifecycleStrategy(
                    notifications,
                    MockMapper.Instance,
                    new ThrowIfErrorLogger<AnnouncementLifecycleStrategy>(),
                    Builder.SbMediaService.Value,
                    Builder.SbTranscriptionService.Value);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory(announcements);

            // Services
            SeriesService series = new SeriesService(
                Builder.SecurityContext.Value,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                MockMapper.Instance,
                new ThrowIfErrorLogger<SeriesService>());
            SessionService sessions = new SessionService(
                Builder.Rbac.Value,
                series,
                Builder.ImageService.Value,
                clipFiles,
                lifecycleFactory,
                Builder.Infrastructure.Value,
                providerFactory,
                Builder.Mapper.Value,
                new ThrowIfErrorLogger<SessionService>());

            // Reset
            SbDb db = Builder.DbContext.Value;
            db.Series.ForEach(s => s.SoftDelete());
            db.Sessions.ForEach(s => s.SoftDelete());
            db.SaveChanges();

            await RunSessionSchedulerAsync(notifications, lifecycleFactory);
            Assert.Equal(0, notifications.Count);
            IEnumerable<Session> feed = await sessions.ReadFeedAsync_Obsolete(SbDbSeed.OrgPrimary.Route);
            Assert.Empty(feed);

            // Make Things
            DbSnapshot snap = new DbSnapshot(db);
            Reset(snap);
            PersonEntity me = await db.PersonWhereUid(SbDbSeed.OrgPrimary.Route, Builder.SecurityContext.Value.UniversalId);
            IEnumerable<PersonEntity> peopleInOrg = await db.PeopleAsync(SbDbSeed.OrgPrimary.Route);
            IEnumerable<NewParticipant> hosts = new NewParticipant[] { ToNewParticipant(ParticipantRole.Host, me) };
            IEnumerable<NewParticipant> newAudience = ToNewParticipants(peopleInOrg, ParticipantRole.Audience);
            IEnumerable<NewParticipant> allParticipants = newAudience.Concat(hosts);

            DateTime start = DateTime.UtcNow;
            Time.FixedDateTime = start;
            DateTime reminder = start;
            DateTime publish = reminder.AddHours(1);

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.Daily,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = reminder,
                ReminderSent = null,
                Publish = publish,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = allParticipants,
            };

            // CREATE & FIRST POPULATE

            // Create the session, which should come back completely fresh
            notifications.Reset();
            Reset(snap);
            SessionDetails sessionDetails = (await sessions.CreateAsync(SbDbSeed.OrgPrimary.Route, newSession));
            Assert.Null(sessionDetails.ReminderSent);
            Assert.Null(sessionDetails.PublishSent);
            snap.AssertAdd<SeriesEntity>(1);
            snap.AssertAdd<SessionEntity>(16); // 1 First Session + 1 Template + 14 Future Sessions
            SessionEntity[] firstSessions = AssertTimespan(snap, start, MinRecurrence.Daily + 1); // first session + 14 from the series builder
            DateTime lastReminderOfFirstGroup = firstSessions.Last().Reminder.Value;

            // Run session scheduler and check feed
            Reset(snap);
            await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, 1);

            // Check running again will not make more sessions
            await RunSeriesSchedulerAsync(lifecycleFactory, snap, 0);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ParticipantEntity>(0);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<SeriesEntity>(0);

            // MOVE AHEAD & POPULATE

            DateTime oneWeekLater = Time.UtcNow.AddDays(7);
            Time.FixedDateTime = oneWeekLater;

            Reset(snap);
            await RunSeriesSchedulerAsync(lifecycleFactory, snap, 7);
            AssertTimespan(snap, lastReminderOfFirstGroup.AddDays(1), 7); // 7 days later means 7 more sessions

            // 7 more sessions added, but only 2 in the feed at a time
            await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, 2);
            snap.AssertAdd<SessionEntity>(7);
            snap.AssertAdd<PromptEntity>(7);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ParticipantEntity>(84);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<SeriesEntity>(0);

            // MOVE PAST CURRENT SESSIONS AND POPULATE

            DateTime oneMonthLater = Time.UtcNow.AddDays(30);
            Time.FixedDateTime = oneMonthLater;

            Reset(snap);
            await RunSeriesSchedulerAsync(lifecycleFactory, snap, MinRecurrence.Daily);
            AssertTimespan(snap, oneMonthLater, MinRecurrence.Daily);

            // Should be 15 days after the start
            // count = 1 First Session + 14 starter in series + 7 on second run + 1 from now
            await RunSessionSchedulerAsync(notifications, lifecycleFactory, sessions, 2);
            snap.AssertAdd<SessionEntity>(14);
            snap.AssertAdd<PromptEntity>(14);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ParticipantEntity>(168);
            snap.AssertAdd<ParticipantGroupEntity>(0);
            snap.AssertAdd<SeriesEntity>(0);
        }

        private static SessionEntity[] AssertTimespan(DbSnapshot snap, DateTime firstReminder, int days)
        {
            days--; // Technically we're counting from zero
            SessionEntity[] newSessions = snap.NewEntities<SessionEntity>().OrderBy(s => s.Reminder).ToArray();
            Assert.Equal(firstReminder, newSessions.First().Reminder);
            Assert.Equal(firstReminder.AddDays(days), newSessions.Last().Reminder);
            return newSessions;
        }
    }
}
