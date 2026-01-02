using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class BillingServiceTests : ServiceTestBase
    {
        protected void ClipFileService(SbDb db, out Organization org, out ClipFileService clips)
        {
            OrganizationEntity orgEnt = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).Single();
            org = new Organization();
            Builder.Mapper.Value.Map(orgEnt, org);
            ILogger<ClipFileService> clipFileLogger = new NullLogger<ClipFileService>();
            clips = new ClipFileService(Builder.SecurityContext.Value, Builder.Infrastructure.Value, clipFileLogger);
        }

        protected static void AssertHighlight(ActivityReport report, string title, string value)
        {
            ActivityHighlight highlight = report.Highlights.Where(h => h.Title == title).Single();
            Assert.NotNull(highlight);
            Assert.Equal(value, highlight.Value);
        }

        [Fact]
        public async Task ActivityReport()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            // Consumption data
            SbDb db = Builder.DbContext.Value;
            // Make an acknowledgement
            ParticipantEntity participant = db.Participants.First();
            participant.ParticipantState = ParticipantState.Consumed;
            // Make some consumption events
            ClipFileService(db, out Organization org, out ClipFileService clips);
            await clips.DownloadUrlAsync(org, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute, FileType.Mp3);
            await clips.DownloadUrlAsync(org, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute, FileType.Mp3);
            await clips.DownloadUrlAsync(org, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute, FileType.Mp3);
            db.SaveChanges();

            // ACT
            BillingService billing = new BillingService(Builder.Infrastructure.Value, Builder.Logger<BillingService>(), Builder.Rbac.Value);
            ActivityReport report = await billing.ActivityAsync(
                IdentityDbSeed.OrgPrimary.Route,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow);

            // ASSERT - Depends on SbDbSeed
            Assert.NotNull(report);
            Assert.Equal(2, report.UnitsConsumed);
            Assert.Equal(1, report.ConsumerCount);
            Assert.Equal(9, report.UnitsProduced);
            Assert.Equal(4, report.ProducerCount);
            Assert.Empty(report.Highlights);
        }
    }
}
