using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class SeriesSessionBuilderTests : ServiceTestBase
    {
        #region Supporting Methods

        private static NewSession CreateNewSession(PersonEntity person, DateTime dateTime)
        {
            NewParticipantGroup leadersGroup = new NewParticipantGroup
            {
                GroupRoute = IdentityDbSeed.GroupOrgAdminOwned.Route,
                ParticipantRole = ParticipantRole.Host
            };
            NewParticipant participant = new NewParticipant
            {
                PersonRoute = person.Route,
                ParticipantRole = ParticipantRole.Host
            };
            NewSession newSession = new NewSession
            {
                Recurrence = Recurrence.Daily,
                RecurrenceData = null,

                Name = "Joint Chiefs Ping Pong Tournament",
                Limit = 120,
                SessionType = SessionType.Announcement,
                SessionSecurity = SessionSecurityType.Protected,
                Reminder = dateTime,
                Publish = dateTime.AddHours(8),
                ReminderSent = null,
                PublishSent = null,

                Route = null,
                FirstPrompt = "What are you doing to draw on your paddle?",

                Groups = new NewParticipantGroup[] { leadersGroup },
                Participants = new NewParticipant[] { participant },
            };
            return newSession;
        }

        private static void Snap(DbSnapshot snap)
        {
            snap.Snap<ParticipantEntity>().Snap<ParticipantGroupEntity>().Snap<SessionEntity>().Snap<PromptEntity>();
        }

        private static async Task<SeriesEntity> CreateSeriesAsync(SbDb db, UserEntity currentUser, OrganizationEntity org, DbSnapshot snap, NewSession newSession)
        {
            SessionEntityBuilder sessionBuilder = new SessionEntityBuilder(db, currentUser);
            SessionEntity templateSession = await sessionBuilder.BuildAsync(org, newSession);
            templateSession.IsTemplate = true;

            SeriesEntity series = db.Series.CreateResource(currentUser);
            // Fields
            series.Name = newSession.Name;
            series.Recurrence = newSession.Recurrence;
            series.RecurrenceData = newSession.RecurrenceData;
            // Relationships
            series.Template = templateSession;
            series.Organization = org;

            await db.SaveChangesAsync();

            snap.AssertAdd<ParticipantEntity>(1);
            snap.AssertAdd<ParticipantGroupEntity>(1);
            snap.AssertAdd<SessionEntity>(1);
            snap.AssertAdd<PromptEntity>(1);

            Snap(snap);

            return series;
        }

        #endregion

        [Fact]
        public async Task BuildAsync_Daily()
        {
            //TODO: currentUser and firstPerson point at the same "person" but it does not look
            //      like that was the explicit intent based on the code.  If it was not the
            //      intent then this code should be updated so they point at different "people"
            //      and if it is the intent then the code should be updated to make it look like
            //      it was actually trying to get the same person.  It passes for now so I am
            //      going to leave it :)

            // ARRANGE 
            SbDb db = Builder.DbContext.Value;
            UserEntity currentUser = db.Users.First();
            OrganizationEntity org = await db.OrgWhereRoute(SbDbSeed.OrgPrimary.Route);
            PersonEntity firstPerson = await db.PersonWhereRoute(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.PersonOrgAdmin.Route);

            DbSnapshot snap = new DbSnapshot(db);
            Snap(snap);

            DateTime start = Time.UtcNow;
            NewSession newSession = CreateNewSession(firstPerson, start);

            SeriesEntity series = await CreateSeriesAsync(db, currentUser, org, snap, newSession);

            // ACT
            SeriesSessionBuilder builder = new SeriesSessionBuilder(MockMapper.Instance, db, currentUser);
            SessionEntity[] seriesSessions = await builder.BuildAsync(series);
            await db.SaveChangesAsync();

            // ASSERT
            Assert.Equal(14, seriesSessions.Length);
            snap.AssertAdd<ParticipantEntity>(14);
            snap.AssertAdd<ParticipantGroupEntity>(14);
            snap.AssertAdd<SessionEntity>(14);
            snap.AssertAdd<PromptEntity>(14);

            Assert.Equal(newSession.Publish.Value.AddDays(14), seriesSessions.Last().Publish.Value);
        }

        [Fact]
        public async Task BuildAsync_WeekDaily()
        {
            // ARRANGE 
            SbDb db = Builder.DbContext.Value;
            UserEntity currentUser = db.Users.First();
            OrganizationEntity org = await db.OrgWhereRoute(IdentityDbSeed.OrgPrimary.Route);
            PersonEntity firstPerson = await db.PersonWhereRoute(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.PersonOrgAdmin.Route);

            DbSnapshot snap = new DbSnapshot(db);
            Snap(snap);

            DateTime start = Time.UtcNow;
            NewSession newSession = CreateNewSession(firstPerson, start);
            newSession.Recurrence = Recurrence.Weekday;
            SeriesEntity series = await CreateSeriesAsync(db, currentUser, org, snap, newSession);

            // ACT
            SeriesSessionBuilder builder = new SeriesSessionBuilder(Builder.Mapper.Value, db, currentUser);
            SessionEntity[] seriesSessions = await builder.BuildAsync(series);
            await db.SaveChangesAsync();

            // ASSERT
            int numSessions = 10;
            Assert.Equal(numSessions, seriesSessions.Length);
            snap.AssertAdd<ParticipantEntity>(numSessions);
            snap.AssertAdd<ParticipantGroupEntity>(numSessions);
            snap.AssertAdd<SessionEntity>(numSessions);
            snap.AssertAdd<PromptEntity>(numSessions);

            DateTime lastPublish = newSession.Publish.Value.AddWeekdays(10);
            Assert.Equal(lastPublish, seriesSessions.Last().Publish.Value);
        }

        [Fact]
        public async Task BuildAsync_Weekly()
        {
            // ARRANGE 
            SbDb db = Builder.DbContext.Value;
            UserEntity currentUser = db.Users.First();
            OrganizationEntity org = await db.OrgWhereRoute(IdentityDbSeed.OrgPrimary.Route);
            PersonEntity firstPerson = await db.PersonWhereRoute(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.PersonOrgAdmin.Route);

            DbSnapshot snap = new DbSnapshot(db);
            Snap(snap);

            DateTime start = Time.UtcNow;
            NewSession newSession = CreateNewSession(firstPerson, start);
            newSession.Recurrence = Recurrence.Weekly;
            SeriesEntity series = await CreateSeriesAsync(db, currentUser, org, snap, newSession);

            // ACT
            SeriesSessionBuilder builder = new SeriesSessionBuilder(MockMapper.Instance, db, currentUser);
            SessionEntity[] seriesSessions = await builder.BuildAsync(series);
            await db.SaveChangesAsync();

            // ASSERT
            int numSessions = 4;
            Assert.Equal(numSessions, seriesSessions.Length);
            snap.AssertAdd<ParticipantEntity>(numSessions);
            snap.AssertAdd<ParticipantGroupEntity>(numSessions);
            snap.AssertAdd<SessionEntity>(numSessions);
            snap.AssertAdd<PromptEntity>(numSessions);

            Assert.Equal(newSession.Publish.Value.AddDays(4 * 7), seriesSessions.Last().Publish.Value);
        }

        [Fact]
        public async Task BuildAsync_Monthly()
        {
            // ARRANGE 
            SbDb db = Builder.DbContext.Value;
            UserEntity currentUser = db.Users.First();
            OrganizationEntity org = await db.OrgWhereRoute(SbDbSeed.OrgPrimary.Route);
            PersonEntity firstPerson = await db.PersonWhereRoute(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.PersonOrgAdmin.Route);

            DbSnapshot snap = new DbSnapshot(db);
            Snap(snap);

            DateTime start = Time.UtcNow;
            NewSession newSession = CreateNewSession(firstPerson, start);
            newSession.Recurrence = Recurrence.Monthly;
            SeriesEntity series = await CreateSeriesAsync(db, currentUser, org, snap, newSession);

            // ACT
            SeriesSessionBuilder builder = new SeriesSessionBuilder(MockMapper.Instance, db, currentUser);
            SessionEntity[] seriesSessions = await builder.BuildAsync(series);
            await db.SaveChangesAsync();

            // ASSERT
            int numSessions = 2;
            Assert.Equal(numSessions, seriesSessions.Length);
            snap.AssertAdd<ParticipantEntity>(numSessions);
            snap.AssertAdd<ParticipantGroupEntity>(numSessions);
            snap.AssertAdd<SessionEntity>(numSessions);
            snap.AssertAdd<PromptEntity>(numSessions);

            Assert.Equal(newSession.Publish.Value.AddMonths(2), seriesSessions.Last().Publish.Value);
        }

        [Fact]
        public async Task BuildAsync_MonthFebProblem()
        {
            // ARRANGE 
            SbDb db = Builder.DbContext.Value;
            UserEntity currentUser = db.Users.First();
            OrganizationEntity org = await db.OrgWhereRoute(SbDbSeed.OrgPrimary.Route);
            PersonEntity firstPerson = await db.PersonWhereRoute(SbDbSeed.OrgPrimary.Route, IdentityDbSeed.PersonOrgAdmin.Route);

            DbSnapshot snap = new DbSnapshot(db);
            Snap(snap);

            // Start on Jan 1 2020, which is a leap year
            DateTime start = new DateTime(2020, 1, 31, 2, 3, 5, 123);
            Time.FixedDateTime = start;
            NewSession newSession = CreateNewSession(firstPerson, start);
            newSession.Recurrence = Recurrence.Monthly;
            SeriesEntity series = await CreateSeriesAsync(db, currentUser, org, snap, newSession);

            // ACT
            SeriesSessionBuilder builder = new SeriesSessionBuilder(MockMapper.Instance, db, currentUser);
            SessionEntity[] seriesSessions = await builder.BuildAsync(series);
            await db.SaveChangesAsync();

            // ASSERT
            int numSessions = 2;
            Assert.Equal(numSessions, seriesSessions.Length);
            snap.AssertAdd<ParticipantEntity>(numSessions);
            snap.AssertAdd<ParticipantGroupEntity>(numSessions);
            snap.AssertAdd<SessionEntity>(numSessions);
            snap.AssertAdd<PromptEntity>(numSessions);

            SessionEntity febSessions = seriesSessions.First();
            SessionEntity marSessions = seriesSessions.Last();
            Assert.Equal(29, febSessions.Reminder.Value.Day);
            Assert.Equal(31, marSessions.Reminder.Value.Day);
            Assert.Equal(start.AddMonths(1), febSessions.Reminder.Value);
            Assert.Equal(start.AddMonths(2), marSessions.Reminder.Value);
            Time.Reset();
        }
    }
}
