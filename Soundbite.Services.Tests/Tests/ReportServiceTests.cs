using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Providers;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using Soundbite.Services.Tests.Mock;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class ReportServiceTests : ServiceTestBase
    {
        protected void ClipFileService(out Organization org, out ClipFileService clips)
        {
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity orgEnt = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).Single();
            org = new Organization();
            Builder.Mapper.Value.Map(orgEnt, org);
            clips = new ClipFileService(Builder.SecurityContext.Value, Builder.Infrastructure.Value, Builder.Logger<ClipFileService>());
        }

        private async Task TestData(SessionService sessions)
        {
            ClipFileService(out Organization org, out ClipFileService clips);

            // One acknowledgement
            await sessions.UpdateStateAsync(org.Route, SbDbSeed.SessionRoute, ParticipantState.Consumed);

            // Three listens - Clips does not save its own DB changes
            SbDb db = Builder.DbContext.Value;
            await clips.DownloadUrlAsync(org, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute, FileType.Mp3);
            await clips.DownloadUrlAsync(org, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute, FileType.Mp3);
            await clips.DownloadUrlAsync(org, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute, FileType.Mp3);
            db.SaveChanges();
        }

        protected static void AssertHighlight(ActivityReport report, string title, string value)
        {
            ActivityHighlight highlight = report.Highlights.Where(h => h.Title == title).Single();
            Assert.NotNull(highlight);
            Assert.Equal(value, highlight.Value);
        }

        [Fact]
        public async Task OrgContentReport()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            // Services
            SessionService sessions = new SessionService(Builder.Rbac.Value, series, Builder.ImageService.Value, clipFiles, lifecycleFactory, Builder.Infrastructure.Value, providerFactory, Builder.Mapper.Value, Builder.Logger<SessionService>());

            await TestData(sessions);

            // ACT
            IReportService reports = new ReportService(Builder.Infrastructure.Value, Builder.Logger<ReportService>(), Builder.Rbac.Value, sessions, Builder.Mapper.Value);
            OrgContentReport report = await reports.OrgContentReportAsync(
                IdentityDbSeed.OrgPrimary.Route,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow);

            // ASSERT - Depends on SbDbSeed and ARRANGE above
            Assert.NotNull(report);
            Assert.Equal(1, int.Parse(report.SessionsCount.Value));
            Assert.NotEmpty(report.SessionsCountOverTime.Items);
            Assert.Equal(1, int.Parse(report.SeriesCount.Value));
            Assert.Equal(3, int.Parse(report.ConsumeCount.Value));
            Assert.NotEmpty(report.ConsumeCountOverTime.Items);
            Assert.Equal(1, int.Parse(report.AcknowledgeCount.Value));
            Assert.Empty(report.Highlights);
        }

        [Fact]
        public async Task SessionContentReport()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            // Services
            SessionService sessions = new SessionService(Builder.Rbac.Value, series, Builder.ImageService.Value, clipFiles, lifecycleFactory, Builder.Infrastructure.Value, providerFactory, Builder.Mapper.Value, Builder.Logger<SessionService>());

            // Consumption data
            await TestData(sessions);

            // ACT
            IReportService reports = new ReportService(Builder.Infrastructure.Value, Builder.Logger<ReportService>(), Builder.Rbac.Value, sessions, Builder.Mapper.Value);
            SessionContentReport report = await reports.SessionContentReportAsync(
                IdentityDbSeed.OrgPrimary.Route,
                SbDbSeed.SessionRoute);

            // ASSERT - Depends on SbDbSeed and ARRANGE above
            Assert.NotNull(report);
            Assert.Equal(3, int.Parse(report.ConsumeCount.Value));
            Assert.NotEmpty(report.ConsumeCountOverTime.Items);
            Assert.Equal(1, int.Parse(report.ConsumerCount.Value));
            Assert.Equal(1, int.Parse(report.AcknowledgeCount.Value));
            Assert.Equal(5, int.Parse(report.AudienceSize.Value));
            Assert.Empty(report.Highlights);
        }

        [Fact]
        public async Task SessionContentDetailsReport()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockClipFileService clipFiles = new MockClipFileService();
            MockSeriesService series = new MockSeriesService();
            ProviderFactory providerFactory = new ProviderFactory();
            MockLifecycleFactory lifecycleFactory = new MockLifecycleFactory();

            // Services
            SessionService sessions = new SessionService(Builder.Rbac.Value, series, Builder.ImageService.Value, clipFiles, lifecycleFactory, Builder.Infrastructure.Value, providerFactory, Builder.Mapper.Value, Builder.Logger<SessionService>());

            // Consumption data
            await TestData(sessions);

            // ACT
            IReportService reports = new ReportService(Builder.Infrastructure.Value, Builder.Logger<ReportService>(), Builder.Rbac.Value, sessions, Builder.Mapper.Value);
            SessionContentDetailsReport report = await reports.SessionContentDetailsReportAsync(
                IdentityDbSeed.OrgPrimary.Route,
                SbDbSeed.SessionRoute);

            // ASSERT - Depends on SbDbSeed and ARRANGE above
            Assert.NotNull(report);
            Assert.Equal(3, int.Parse(report.ConsumeCount.Value));
            Assert.Equal(3, report.Plays.Length);
            Assert.NotEmpty(report.ConsumeCountOverTime.Items);
            Assert.Equal(1, int.Parse(report.ConsumerCount.Value));
            Assert.Equal(1, int.Parse(report.AcknowledgeCount.Value));
            Assert.Single(report.Acknowledgers);
            Assert.Equal(5, int.Parse(report.AudienceSize.Value));
            Assert.Empty(report.Highlights);
            Assert.Equal(5, report.Audience.Length);
            Assert.NotNull(report.Audience[0].Person);
            Assert.NotNull(report.Audience[0].Person.User);
            Assert.Equal(2, report.AudienceGroups.Length);
            Assert.NotNull(report.AudienceGroups[0].Group);
            Assert.Empty(report.Notifications); // No notifications fired in this unit test, but there should have been a query with a result
        }
    }
}
