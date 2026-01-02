using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Exceptions;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using Soundbite.Services.Tests.Mock;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class SeriesServiceTests : ServiceTestBase
    {


        #region Supporting Methods

        private static NewSession CreateNewSession(PersonEntity person)
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
                Reminder = Time.UtcNow.AddDays(1),
                Publish = Time.UtcNow.AddDays(1).AddHours(8),
                ReminderSent = null,
                PublishSent = null,

                Route = null,
                FirstPrompt = "What are you doing to draw on your paddle?",

                Groups = new NewParticipantGroup[] { leadersGroup },
                Participants = new NewParticipant[] { participant },
            };
            return newSession;
        }

        #endregion

        // Create
        [Fact]
        public async Task CreateAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();
            PersonEntity person = db.People.Where(p => p.Organization == org).First();
            NewSession newSession = CreateNewSession(person);

            // ACT
            SeriesService seriesService = new SeriesService(Builder.SecurityContext.Value, lifecycleFactory, Builder.Infrastructure.Value, Builder.Mapper.Value, Builder.Logger<SeriesService>());
            SeriesDetails series = (await seriesService.CreateAsync(IdentityDbSeed.OrgPrimary.Route, newSession));

            // ASSERT
            Assert.NotNull(series);
            Assert.NotNull(series.Template);
            Assert.Equal(14, series.Sessions.Count());
            Assert.Equal(1, lifecycleFactory.MockStrategy.PreSeriesCount);
            Assert.Equal(1, lifecycleFactory.MockStrategy.PostSeriesCount);
        }

        [Fact]
        public async Task ReadAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            SbDb db = Builder.DbContext.Value;
            SeriesEntity s = db.Series.First();

            // ACT
            SeriesService seriesService = new SeriesService(Builder.SecurityContext.Value, lifecycleFactory, Builder.Infrastructure.Value, Builder.Mapper.Value, Builder.Logger<SeriesService>());
            SeriesDetails series = await seriesService.ReadAsync(IdentityDbSeed.OrgPrimary.Route, s.Route);

            // ASSERT
            Assert.NotNull(series);
            Assert.NotNull(series.Template);
            Assert.Empty(series.Sessions);
        }

        [Fact]
        public async Task ReadAllAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            // ACT
            SeriesService seriesService = new SeriesService(Builder.SecurityContext.Value, lifecycleFactory, Builder.Infrastructure.Value, Builder.Mapper.Value, Builder.Logger<SeriesService>());

            IEnumerable<SeriesPreview> series = await seriesService.ReadAllAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(series);
            Assert.NotEmpty(series);
            Assert.Equal(SbDbSeed.SeriesRoute, series.Last().Route);
            foreach (SeriesPreview s in series)
            {
                Assert.NotNull(s.Template);
                Assert.NotEmpty(s.Template.MyParticipants);
            }
        }

        [Fact]
        public async Task ReadAllAsync_Group()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            // ACT
            SeriesService seriesService = new SeriesService(Builder.SecurityContext.Value, lifecycleFactory, Builder.Infrastructure.Value, Builder.Mapper.Value, Builder.Logger<SeriesService>());
            IEnumerable<SeriesPreview> series = await seriesService.ReadAllAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);

            // ASSERT
            Assert.NotNull(series);
            Assert.NotEmpty(series);
            foreach (SeriesPreview s in series)
            {
                Assert.NotNull(s.Template);
                Assert.NotEmpty(s.Template.MyParticipants);
            }
        }

        [Fact]
        public async Task ReadAllAsync_Group_Empty()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            // ACT
            SeriesService seriesService = new SeriesService(Builder.SecurityContext.Value, lifecycleFactory, Builder.Infrastructure.Value, Builder.Mapper.Value, Builder.Logger<SeriesService>());
            IEnumerable<SeriesPreview> series = await seriesService.ReadAllAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOutside.Route);

            // ASSERT
            Assert.NotNull(series);
            Assert.Empty(series);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            NewSession newSession = new NewSession
            {
                Name = "New Name",
                Recurrence = Recurrence.Monthly,
                RecurrenceData = "Hello"
            };

            // ACT
            SeriesService seriesService = new SeriesService(Builder.SecurityContext.Value, lifecycleFactory, Builder.Infrastructure.Value, Builder.Mapper.Value, Builder.Logger<SeriesService>());
            SeriesDetails updatedSeries = await seriesService.UpdateAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SeriesRoute, newSession);

            // ASSERT
            Assert.NotNull(updatedSeries);
            Assert.NotNull(updatedSeries.Template);
            Assert.Equal(2, updatedSeries.Sessions.Count());
            Assert.Equal(newSession.Name, updatedSeries.Name);
            Assert.Equal(newSession.Recurrence, updatedSeries.Recurrence);
            Assert.Equal(newSession.RecurrenceData, updatedSeries.RecurrenceData);
        }

        [Fact]
        public async Task DeleteAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ILogger<SeriesService> logger = new NullLogger<SeriesService>();
            ISecurityContext securityContext = new MockSecurityContext();
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            SbDb db = Builder.DbContext.Value;
            SeriesEntity s = db.Series.First();
            Assert.Null(s.DeletedUtc);

            // ACT
            SeriesService seriesService = new SeriesService(Builder.SecurityContext.Value, lifecycleFactory, Builder.Infrastructure.Value, Builder.Mapper.Value, Builder.Logger<SeriesService>());
            await seriesService.DeleteAsync(IdentityDbSeed.OrgPrimary.Route, s.Route);

            // ASSERT
            Assert.NotNull(s.DeletedUtc);
            await Assert.ThrowsAsync<NotFoundException>(async () => await seriesService.ReadAsync(IdentityDbSeed.OrgPrimary.Route, s.Route));
        }
    }
}
