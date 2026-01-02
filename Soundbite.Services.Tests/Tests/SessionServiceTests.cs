using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Providers;
using Masticore.Security;
using Masticore.Services;
using Masticore.Services.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using Soundbite.Services.Tests.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class SessionServiceTests : ServiceTestBase
    {
        #region Utility Methods

        public SessionServiceTests()
        {
            Builder.UseNullLogger = true;
            Builder.SessionFeedService.Use(i => new SessionFeedService(
                Builder.Rbac.Value,
                Builder.Infrastructure.Value,
                Builder.Logger<SessionFeedService>(),
                Builder.Mapper.Value));

            Builder.SessionService.Use(i => new SessionService(
                Builder.Rbac.Value,
                Builder.SeriesService.Value,
                Builder.ImageService.Value,
                Builder.ClipFileService.Value,
                Builder.LifeCycleFactory.Value,
                Builder.Infrastructure.Value,
                Builder.ProviderFactory.Value,
                Builder.Mapper.Value,
                Builder.Logger<SessionService>()
                ));

            Builder.MemberService.Use(i => new MemberService(
                Builder.SecurityContext.Value,
                Builder.NotificationService.Value,
                Builder.ImageService.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                Builder.Logger<MemberService>()));
        }

        private ISessionService CreateSessionService()
        {
            return Builder.SessionService.Value;
        }

        private async Task InviteUserToGroup()
        {
            UserEntity user = await Builder.UserForRoute(IdentityDbSeed.UserRegular.Route);
            Invite[] invites = new Invite[]
            {
                new Invite { Token = user.Email }
            };

            SbDb db = Builder.DbContext.Value;
            IRbac rbac = Builder.Rbac.Value;
            int[] oldGroupIds = await db.GroupIdsAsync(IdentityDbSeed.OrgPrimary.Route, rbac.UniversalIdForCurrentUser);

            // ACT
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            ISessionService sessions = Builder.SessionService.Value;
            SessionDetails details = await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.NonMemberSessionRoute);
            Assert.NotNull(details);

            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, invites);
            await Builder.SetCurrentUserContext(TestUserContext.SessionParticipantNoGroupAffiliation);

            int[] newGroupIds = await db.GroupIdsAsync(IdentityDbSeed.OrgPrimary.Route, rbac.UniversalIdForCurrentUser);

            Assert.Equal(oldGroupIds.Length + 1, newGroupIds.Length);
        }

        #endregion

        [Fact]
        public async Task AcknowledgePublicSession()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            ISessionService sessions = Builder.SessionService.Value;

            // ACT
            SbDb db = Builder.DbContext.Value;
            var info = await db.People
                .Where(i => i.Organization.Route == SbDbSeed.OrgPrimary.Route)
                .Select(i => new { PersonRoute = i.Route, UserRoute = i.User.Route })
                .FirstOrDefaultAsync();
            string personRoute = info.PersonRoute;
            string userRoute = info.UserRoute;

            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,
                Name = "Happy Path",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Public,
                // We are setting a publish date in the future, but null reminder assumes we are uploading a clip immediately
                Reminder = null,
                ReminderSent = null,
                Publish = Time.UtcNow,
                PublishSent = null,
                FirstPrompt = "Is this working?",
                Participants = new List<NewParticipant>() {
                    new NewParticipant()  { PersonRoute=personRoute, ParticipantRole=ParticipantRole.Host },
                }
            };

            SessionDetails sessionDetails = (await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));
            Assert.Single(sessionDetails.Participants);
            Assert.Empty(sessionDetails.Reactions); // No participants yet

            // Fake whatever happens to move the participant into the pending consumption state
            ParticipantEntity pEntity = db.Participants.Where(i => i.Route == sessionDetails.Participants.First().Route).First();
            pEntity.ParticipantState = ParticipantState.ConsumptionRequested;
            await db.SaveChangesAsync();
            pEntity = db.Participants.Where(i => i.Route == sessionDetails.Participants.First().Route).First();
            Assert.Equal(ParticipantState.ConsumptionRequested, pEntity.ParticipantState);

            await sessions.AcknowledgePublicSession(SbDbSeed.OrgPrimary.Route, sessionDetails.Route, userRoute);
            pEntity = db.Participants.Where(i => i.Route == sessionDetails.Participants.First().Route).First();
            Assert.Equal(ParticipantState.Consumed, pEntity.ParticipantState);
        }


        [Fact]
        public async Task AcknowledgePublicSession_NoUpdateIfNotPublic()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            ISessionService sessions = Builder.SessionService.Value;

            // ACT
            SbDb db = Builder.DbContext.Value;

            var host = await db.People
                .Where(i =>
                    i.Organization.Route == SbDbSeed.OrgPrimary.Route
                    && i.User.Id == Builder.SecurityContext.Value.CurrentUserId)
                .Select(i => new { PersonRoute = i.Route, UserRoute = i.User.Route })
                .FirstOrDefaultAsync();

            // Ensure Host user/person was found in the current organization
            Assert.NotNull(host);
            Assert.True(!string.IsNullOrEmpty(host.UserRoute));
            Assert.True(!string.IsNullOrEmpty(host.PersonRoute));

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
                Participants = new List<NewParticipant>() {
                    new NewParticipant()  { PersonRoute=host.PersonRoute, ParticipantRole=ParticipantRole.Host },
                }
            };

            SessionDetails sessionDetails = (await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));
            Assert.Single(sessionDetails.Participants);
            Assert.Empty(sessionDetails.Reactions); // Created means there are no participants

            // Fake whatever happens to move the participant into the pending consumption state
            ParticipantEntity pEntity = db.Participants.Where(i => i.Route == sessionDetails.Participants.First().Route).First();
            pEntity.ParticipantState = ParticipantState.ConsumptionRequested;
            await db.SaveChangesAsync();
            pEntity = db.Participants.Where(i => i.Route == sessionDetails.Participants.First().Route).First();
            Assert.Equal(ParticipantState.ConsumptionRequested, pEntity.ParticipantState);

            await sessions.AcknowledgePublicSession(SbDbSeed.OrgPrimary.Route, sessionDetails.Route, host.UserRoute);
            pEntity = db.Participants.Where(i => i.Route == sessionDetails.Participants.First().Route).First();
            Assert.Equal(ParticipantState.ConsumptionRequested, pEntity.ParticipantState);
        }

        // Create

        [Fact]
        public async Task CreateAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbDb db = Builder.DbContext.Value;

            // Host will be the current user
            PersonEntity personHost = db.People.Where(p =>
                    p.User.Id == Builder.SecurityContext.Value.CurrentUserId
                    && p.OrganizationId == IdentityDbSeed.OrgPrimary.Id)
                .Include(p => p.User)
                .FirstOrDefault();
            Assert.NotNull(personHost);

            db.SaveChanges();

            NewSession newSession = CreateNewSession(personHost);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            SessionDetails session = (await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));

            // ASSERT
            Assert.NotNull(session);
            Assert.NotEmpty(session.Participants);
            Assert.Empty(session.Reactions); // This isn't published yet, so it doesn't have any reactions
            Assert.NotEmpty(session.Groups);
            Assert.NotEmpty(session.Prompts);
            Assert.True(Builder.GetMockImageService().NoCalls);
        }

        [Fact]
        public async Task CreateAsync_NoParticipants()
        {
            // ARRANGE
            ILogger<SessionService> logger = new NullLogger<SessionService>();
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.People.Where(p => p.Route == IdentityDbSeed.PersonOrgAdmin.Route).Include(p => p.User).FirstOrDefault();
            person.PersonRole = PersonRole.Person;
            db.SaveChanges();
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<Masticore.Models.User>(person.User), person.User.Id);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            NewSession newSession = CreateNewSession(person);
            newSession.Groups = null;
            newSession.Participants = null;
            MockClipFileService clipFileService = new MockClipFileService();
            MockImageService images = new MockImageService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await Assert.ThrowsAsync<ArgumentException>(async () => await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));
        }

        [Fact]
        public async Task CreateAsync_NoHosts()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();

            // Host will be the current user
            PersonEntity personHost = db.People.Where(p => p.User.Id == Builder.SecurityContext.Value.CurrentUserId && p.OrganizationId == IdentityDbSeed.OrgPrimary.Id).Include(p => p.User).FirstOrDefault();
            Assert.NotNull(personHost);
            personHost.PersonRole = PersonRole.Person;
            db.SaveChanges();

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            NewSession newSession = CreateNewSession(personHost);
            newSession.Groups.First().ParticipantRole = ParticipantRole.Participant;
            newSession.Participants.First().ParticipantRole = ParticipantRole.Participant;
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await Assert.ThrowsAsync<ArgumentException>(async () => await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));
        }

        [Fact]
        public async Task CreateAsync_NoAccessToGroup()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.RandomUser);
            ILogger<SessionService> logger = new NullLogger<SessionService>();
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            person.PersonRole = PersonRole.Person;
            db.SaveChanges();
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(person.User), person.User.Id);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            NewSession newSession = CreateNewSession(person); // This will add groups where this new user is not part of the group
            newSession.Groups.First().ParticipantRole = ParticipantRole.Participant;
            newSession.Participants.First().ParticipantRole = ParticipantRole.Participant;
            MockClipFileService clipFileService = new MockClipFileService();
            MockImageService images = new MockImageService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession);
            });
        }

        private static NewSession CreateNewSession(PersonEntity personHost)
        {
            NewParticipantGroup heirsGroup = new NewParticipantGroup
            {
                GroupRoute = IdentityDbSeed.GroupOrgAdminOwned.Route,
                ParticipantRole = ParticipantRole.Participant
            };
            NewParticipant participant = new NewParticipant
            {
                PersonRoute = personHost.Route,
                ParticipantRole = ParticipantRole.Host
            };
            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.NoRepeat,
                RecurrenceData = null,

                Name = "Joint Chiefs Ping Pong Tournament",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = Time.UtcNow.AddDays(1),
                Publish = Time.UtcNow.AddDays(1).AddHours(8),
                ReminderSent = null,
                PublishSent = null,

                Route = null,
                FirstPrompt = "What are you doing to draw on your paddle?",

                Groups = new NewParticipantGroup[] { heirsGroup },
                Participants = new NewParticipant[] { participant },
            };
            return newSession;
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.People.Where(p => p.Route == IdentityDbSeed.PersonOrgAdmin.Route).Include(p => p.User).FirstOrDefault();
            person.PersonRole = PersonRole.Person;
            db.SaveChanges();

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            NewSession newSession = CreateNewSession(person);
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            SessionDetails session = (await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));

            PersonEntity newPerson = db.MockPerson(org, db.MockUser("NewPerson@gmail.com"));
            GroupEntity newGroup = db.MockGroup(org);
            db.SaveChanges();
            DbSnapshot snap = new DbSnapshot(db).Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>();

            newSession.Route = session.Route;

            string newName = "Hello World!";
            newSession.Name = newName;
            string newPrompt = "How much wood would a woodchunk chuck if a woodchunk could chuck wood?";
            newSession.FirstPrompt = newPrompt;
            int newLimit = 77;
            newSession.Limit = newLimit;
            DateTime newReminder = Time.UtcNow.AddMinutes(5);
            newSession.Reminder = newReminder;
            DateTime newDeadline = Time.UtcNow.AddMinutes(10);
            newSession.Publish = newDeadline;
            List<NewParticipant> participants = new List<NewParticipant>(newSession.Participants)
            {
                new NewParticipant { ParticipantRole = ParticipantRole.Audience, PersonRoute = newPerson.Route }
            };
            newSession.Participants = participants.ToArray();
            List<NewParticipantGroup> participantGroups = new List<NewParticipantGroup>(newSession.Groups)
            {
                new NewParticipantGroup { ParticipantRole = ParticipantRole.Audience, GroupRoute = newGroup.Route }
            };
            newSession.Groups = participantGroups.ToArray();

            // ACT
            SessionDetails updatedSession = await sessions.UpdateAsync(IdentityDbSeed.OrgPrimary.Route, newSession);

            // ASSERT
            Assert.NotNull(updatedSession);
            Assert.NotEmpty(updatedSession.Participants);
            Assert.NotEmpty(updatedSession.Reactions);
            Assert.NotEmpty(updatedSession.Groups);
            Assert.NotEmpty(updatedSession.Prompts);
            Assert.Equal(newName, updatedSession.Name);
            Assert.Equal(newLimit, updatedSession.Limit);
            Assert.Equal(newReminder, updatedSession.Reminder);
            Assert.Equal(newDeadline, updatedSession.Publish);
            Assert.Equal(newPrompt, updatedSession.Prompts.First().Text);
            snap.AssertAdd<ParticipantEntity>(1);
            snap.AssertAdd<ParticipantGroupEntity>(1);
        }

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_NewPerson()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            person.PersonRole = PersonRole.Person;
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await sessions.DeleteAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute));
        }

        [Fact]
        public async Task DeleteAsync_God()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            person.User.UserRole = UserRole.God;
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await sessions.DeleteAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute);

            // ASSERT
            await Assert.ThrowsAsync<ResourceGoneException>(async () => await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute));
        }

        [Fact]
        public async Task DeleteAsync_Admin()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            person.PersonRole = PersonRole.Admin;
            person.User.UserRole = UserRole.Unknown;
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await sessions.DeleteAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute);

            // ASSERT
            await Assert.ThrowsAsync<ResourceGoneException>(async () => await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute));
        }

        [Fact]
        public async Task DeleteAsync_Participant()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            ParticipantEntity participant = db.MockParticipant(person);
            participant.ParticipantRole = ParticipantRole.Host;
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await sessions.DeleteAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute);

            // ASSERT
            await Assert.ThrowsAsync<ResourceGoneException>(async () => await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute));
        }

        [Fact]
        public async Task DeleteAsync_ParticipantGroup()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            GroupEntity heirGrp = db.Groups.Where(g => g.Route == IdentityDbSeed.GroupOrgAdminOwned.Route).First();
            heirGrp.MockMember(person);
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<Masticore.Models.User>(person.User), person.User.Id);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockImageService images = new MockImageService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;
            await sessions.DeleteAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute);

            // ASSERT
            await Assert.ThrowsAsync<ResourceGoneException>(async () => await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute));
        }

        #endregion

        [Fact]
        public async Task ReadAsync()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            GroupEntity leaders = db.Groups.Where(g => g.Route == IdentityDbSeed.GroupOrgAdminOwned.Route).First();
            leaders.MockMember(person);
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;

            // Empty is the only possible outcome w/o using the lifecycle classes to make participant records
            SessionDetails sessionDetails = await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute);
            Assert.NotNull(sessionDetails);
            Assert.NotNull(sessionDetails.Name);
            Assert.True(sessionDetails.CreatedUtc != DateTime.MinValue);
            Assert.True(sessionDetails.UpdatedUtc != DateTime.MinValue);
            Assert.Null(sessionDetails.DeletedUtc);
            // The Participants attribute is "direct" people and all people are brought in via groups for this session
            Assert.Empty(sessionDetails.Participants);
            // Reactions are all people (not just direct) so this should have data in it
            Assert.Single(sessionDetails.Reactions);
            Assert.Equal(2, sessionDetails.Groups.Count());
            Assert.Single(sessionDetails.Prompts);
            Assert.Null(sessionDetails.Series);
            Assert.Equal(SessionLifecycleStage.WaitingToRemind, sessionDetails.LifecycleStage());
        }

        [Fact]
        public async Task ReadFeedAsync()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            GroupEntity leaders = db.Groups.Where(g => g.Route == IdentityDbSeed.GroupOrgAdminOwned.Route).First();
            leaders.MockMember(person);
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;

            // Empty is the only possible outcome w/o using the lifecycle classes to make participant records
            IEnumerable<SessionPreview> feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Empty(feed);

            feed = await sessions.ReadFeedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Empty(feed);
        }

        [Fact]
        public async Task ReadRecentlyPublishedAsync_NoContent()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;

            // Empty is the only possible outcome w/o using the lifecycle classes to make participant records
            IEnumerable<SessionPreview> feed = await sessions.ReadRecentlyPublishedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);
            Assert.Single(feed);
        }

        [Fact]
        public async Task ReadRecentlyPublishedAsync_NoAccess()
        {
            // ARRANGE

            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.MockPerson(org);
            db.SaveChanges();

            await Builder.SetCurrentUserContext(person.User);

            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();

            // ACT
            ISessionService sessions = Builder.SessionService.Value;

            // Valid org, but just some random person does NOT have access to the endpoint
            await Assert.ThrowsAsync<NotFoundException>(async () => await sessions.ReadRecentlyPublishedAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route));
        }

        [Fact]
        public async Task UpdateReactionAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();
            MockClipFileService clipFileService = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();
            ISessionService sessions = Builder.SessionService.Value;

            // ACT
            await sessions.UpdateReactionAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, ParticipantReactionType.Like);

            // ASSERT
            IndexPageResponse<Participant> participants = await sessions.ReadParticipants(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute);
            Assert.NotNull(participants);
            Assert.Single(participants.Result);
            Participant currentUserParticipant = participants.Result.Single(p => p.Person.User.UniversalId == Builder.Rbac.Value.UniversalIdForCurrentUser);
            Assert.NotNull(currentUserParticipant);
            Assert.Equal(ParticipantReactionType.Like, currentUserParticipant.ReactionType);

            SessionDetails session = await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute);
            Assert.NotNull(session);
            // Order doesn't matter, but number of categories and items in each catgory does
            Assert.Equal(2, session.Reactions.Count());
            ReactionSummary noneReactions = session.Reactions.Where(r => r.ReactionType == ParticipantReactionType.None).Single();
            Assert.Equal(1, noneReactions.Count);
            ReactionSummary likeReactions = session.Reactions.Where(r => r.ReactionType == ParticipantReactionType.Like).Single();
            Assert.Equal(1, likeReactions.Count);
        }

        [Fact]
        public async Task ReadAsync_AddedToGroupLater()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.SessionParticipantNoGroupAffiliation);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;
            ISessionService sessions = Builder.SessionService.Value;

            // #1 Verify accessing the feed for the group is locked out
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.NonMemberSessionRoute);
            });

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = Builder.DbSnapshot.Value;
            snap.Snap<ParticipantEntity>();
            SessionEntity groupSession = db.Sessions.Where(s => s.Route == SbDbSeed.NonMemberSessionRoute).Single();
            groupSession.PublishSent = Time.UtcNow;
            db.SaveChanges();

            await InviteUserToGroup();

            // ACT
            SessionDetails details = await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.NonMemberSessionRoute);

            // ASSERT
            Assert.NotNull(details);
            Assert.NotNull(details.Name);
            Assert.NotEmpty(details.MyParticipation);
            snap.AssertAdd<ParticipantEntity>(1);
        }
    }
}
