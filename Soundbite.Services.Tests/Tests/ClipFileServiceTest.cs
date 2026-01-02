using Masticore;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Services;
using Masticore.Storage;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class ClipFileServiceTests : ServiceTestBase
    {
        #region Constructor

        public ClipFileServiceTests()
        {
            Builder.ClipFileService.Use(i => new ClipFileService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.Logger<ClipFileService>()));

            Builder.OrganizationService.Use(i => new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value));
        }

        #endregion

        #region Methods - Utility

        private async Task<OrganizationDetails> OrgAsync()
        {
            IOrganizationService organizationService = Builder.OrganizationService.Value;
            OrganizationDetails org = await organizationService.ReadAsync(IdentityDbSeed.OrgPrimary.Route);
            return org;
        }

        #endregion

        [Fact]
        public async Task UploadAsync()
        {
            // ARRANGE

            // Data
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbDb db = Builder.DbContext.Value;
            IOrganizationService organizationService = Builder.OrganizationService.Value;
            OrganizationDetails org = await organizationService.ReadAsync(IdentityDbSeed.OrgPrimary.Route);

            Stream fakeFileStream = "Face".ToStream();
            DbSnapshot snap = new DbSnapshot(db)
                .Snap<SessionEntity>()
                .Snap<PromptEntity>()
                .Snap<ClipEntity>()
                .Snap<ClipEventEntity>();

            // ACT
            IClipFileService clips = Builder.ClipFileService.Value;
            string url = await clips.UploadAsync(
                org,
                SbDbSeed.SessionRoute,
                SbDbSeed.PromptRoute,
                SbDbSeed.LastClipRoute,
                FileType.Mp3,
                fakeFileStream);
            // Upload does NOT persist on its own
            // TODO: Implement a real "Unit of Work" pattern for services
            await db.SaveChangesAsync();

            // ASSERT
            Assert.NotNull(url);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ClipEventEntity>(1);

            ClipEventEntity clipEvent = snap.NewEntities<ClipEventEntity>().Single();
            Assert.True(clipEvent.TargetId > 0);
            Assert.Equal(SbDbSeed.DefaultDisplaySeconds, clipEvent.Duration);
            Assert.Null(clipEvent.Position);
            Assert.Equal(ClipEventType.ServerUpload, clipEvent.ClipEventType);

            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.DownloadCount);
            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.DownloadUrlCount);
            Assert.Equal(1, Builder.Infrastructure.Value.Blobs.UploadCount);
            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.UploadUrlCount);
        }

        [Fact]
        public async Task DownloadUrlAsync()
        {
            // ARRANGE

            // Data
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbDb db = Builder.DbContext.Value;
            OrganizationDetails org = await OrgAsync();

            DbSnapshot snap = new DbSnapshot(db)
                .Snap<SessionEntity>()
                .Snap<PromptEntity>()
                .Snap<ClipEntity>()
                .Snap<ClipEventEntity>();

            // ACT
            IClipFileService clips = Builder.ClipFileService.Value;
            string url = await clips.DownloadUrlAsync(
                org,
                SbDbSeed.SessionRoute,
                SbDbSeed.PromptRoute,
                SbDbSeed.LastClipRoute,
                FileType.Mp3);
            // Upload does NOT persist on its own
            // TODO: Implement a real "Unit of Work" pattern for services
            await db.SaveChangesAsync();

            // ASSERT
            Assert.NotNull(url);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ClipEventEntity>(2);
            foreach (ClipEventEntity clipEvent in snap.NewEntities<ClipEventEntity>())
            {
                Assert.True(clipEvent.TargetId > 0);
                Assert.Equal(SbDbSeed.DefaultDisplaySeconds, clipEvent.Duration);
                Assert.Null(clipEvent.Position);
            }

            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.DownloadCount);
            Assert.Equal(1, Builder.Infrastructure.Value.Blobs.DownloadUrlCount);
            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.UploadCount);
            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.UploadUrlCount);
        }

        [Fact]
        public async Task UploadUrlAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            // Data
            SbDb db = Builder.DbContext.Value;
            OrganizationDetails org = await OrgAsync();
            DbSnapshot snap = new DbSnapshot(db)
                .Snap<SessionEntity>()
                .Snap<PromptEntity>()
                .Snap<ClipEntity>()
                .Snap<ClipEventEntity>();

            // ACT
            IClipFileService clips = Builder.ClipFileService.Value;
            string url = await clips.UploadUrlAsync(
                org,
                SbDbSeed.SessionRoute,
                SbDbSeed.PromptRoute,
                SbDbSeed.LastClipRoute,
                FileType.Mp3);
            // Upload does NOT persist on its own
            // TODO: Implement a real "Unit of Work" pattern for services
            await db.SaveChangesAsync();

            // ASSERT
            Assert.NotNull(url);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ClipEventEntity>(1);

            ClipEventEntity clipEvent = snap.NewEntities<ClipEventEntity>().Single();
            Assert.True(clipEvent.TargetId > 0);
            Assert.Equal(SbDbSeed.DefaultDisplaySeconds, clipEvent.Duration);
            Assert.Null(clipEvent.Position);
            Assert.Equal(ClipEventType.ServerUnlockForWrite, clipEvent.ClipEventType);

            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.DownloadCount);
            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.UploadCount);
            Assert.Equal(0, Builder.Infrastructure.Value.Blobs.DownloadUrlCount);
            Assert.Equal(1, Builder.Infrastructure.Value.Blobs.UploadUrlCount);
        }
    }
}
