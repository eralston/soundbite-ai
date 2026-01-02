using Masticore;
using Masticore.Entity.Tests;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using Soundbite.Services.Tests.Content;
using Soundbite.Services.Tests.Mock;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class ClipServiceTests : ServiceTestBase
    {
        #region Constructor
        public ClipServiceTests()
        {
            Builder.UseNullLogger = true;
            Builder.ClipService.Use(i => new ClipService(
                Builder.SecurityContext.Value,
                Builder.ClipFileService.Value,
                Builder.LifeCycleFactory.Value,
                Builder.Infrastructure.Value,
                Builder.SbMediaService.Value,
                Builder.Mapper.Value,
                Builder.Logger<ClipService>(),
                Builder.Rbac.Value));
        }
        #endregion

        #region Properties

        MockClipFileService MockClipFiles => Builder.ClipFileService.Value as MockClipFileService;
        MockLifecycleFactory LifeCycleFactory => Builder.LifeCycleFactory.Value as MockLifecycleFactory;

        #endregion

        // Create
        [Fact]
        public async Task CreateAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Valid_ThreeSecondCbrMp3,
                FileType = FileType.Mp3,
                ClipType = ClipType.Contribution,
                ParticipantRole = ParticipantRole.Host
            };

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db)
                .Snap<SessionEntity>()
                .Snap<PromptEntity>()
                .Snap<ClipEntity>()
                .Snap<ClipEventEntity>();

            // ACT
            IClipService clips = Builder.ClipService.Value;
            ClipDetails clip = (await clips.CreateAsync(SbDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, newClip));

            // ASSERT
            Assert.NotNull(clip);
            Assert.NotNull(clip.OrgRoute);
            Assert.NotNull(clip.PromptRoute);
            Assert.NotNull(clip.ClipRoute);
            Assert.NotNull(clip.DownloadUrl);
            Assert.Null(clip.UploadUrl);

            Assert.Equal(1, MockClipFiles.UploadCount);
            Assert.Equal(1, LifeCycleFactory.MockStrategy.PreClipCount);
            Assert.Equal(1, LifeCycleFactory.MockStrategy.PostClipCount);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(1);
            // Because ClipFileService holds the centralized logic for upload events and it is actually a mock in this scenario
            // there actually should no events in this process and we're just confirming that is so
            snap.AssertAdd<ClipEventEntity>(0);

            // New clips with streams should be ready immediately until we have async AzFun processing
            ClipState currentStep = await clips.StateAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, clip.ClipRoute);
            Assert.Equal(ClipState.Ready, currentStep);
        }

        [Fact]
        public async Task CreateAsync_StorageUpload()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbDb db = Builder.DbContext.Value;
            NewClip newClip = new NewClip
            {
                FileType = FileType.Mp3,
                ClipType = ClipType.Contribution,
                ParticipantRole = ParticipantRole.Host
            };

            MockClipFiles.SizeBytes = int.MaxValue;

            DbSnapshot snap = new DbSnapshot(db)
                .Snap<SessionEntity>()
                .Snap<PromptEntity>()
                .Snap<ClipEntity>()
                .Snap<ClipEventEntity>();

            // ACT & ASSERT
            IClipService clips = Builder.ClipService.Value;

            // START

            ClipDetails response = (await clips.CreateAsync(SbDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, newClip));

            Assert.NotNull(response);
            Assert.NotNull(response.OrgRoute);
            Assert.NotNull(response.PromptRoute);
            Assert.NotNull(response.ClipRoute);
            Assert.Null(response.DownloadUrl);
            Assert.NotNull(response.UploadUrl);
            Assert.Equal(ClipState.Started, response.State);

            Assert.Equal(0, MockClipFiles.UploadCount);
            Assert.Equal(0, MockClipFiles.DownloadUrlCount);
            Assert.Equal(1, MockClipFiles.UploadUrlCount);
            Assert.Equal(0, MockClipFiles.ExistCount);

            Assert.Equal(1, LifeCycleFactory.MockStrategy.PreClipCount);
            Assert.Equal(0, LifeCycleFactory.MockStrategy.PostClipCount); // Not fired because not done

            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(1);
            snap.AssertAdd<ClipEventEntity>(0);

            // CHECK STATUS w/o UPLOAD

            ClipState currentStep = await clips.StateAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, response.ClipRoute);
            Assert.Equal(ClipState.Started, currentStep);

            Assert.Equal(0, MockClipFiles.UploadCount);
            Assert.Equal(0, MockClipFiles.DownloadUrlCount);
            Assert.Equal(1, MockClipFiles.UploadUrlCount);
            Assert.Equal(1, MockClipFiles.ExistCount);

            Assert.Equal(1, LifeCycleFactory.MockStrategy.PreClipCount);
            Assert.Equal(0, LifeCycleFactory.MockStrategy.PostClipCount); // Still not fired

            // FAKE UPLOAD

            MockClipFiles.DoesClipExist = true;

            // CHECK STATUS w/ UPLOAD - Ensure there was activity to verify and capture an event that the upload was successful

            currentStep = await clips.StateAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, response.ClipRoute);

            Assert.Equal(ClipState.Ready, currentStep);

            Assert.Equal(0, MockClipFiles.UploadCount);
            Assert.Equal(0, MockClipFiles.DownloadUrlCount);
            Assert.Equal(1, MockClipFiles.UploadUrlCount);
            Assert.Equal(2, MockClipFiles.ExistCount);

            Assert.Equal(1, LifeCycleFactory.MockStrategy.PreClipCount);
            Assert.Equal(1, LifeCycleFactory.MockStrategy.PostClipCount); // Fired now because it's done

            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(1);
            snap.AssertAdd<ClipEventEntity>(1);
            ClipEventEntity clipEvent = snap.NewEntities<ClipEventEntity>().Single();
            Assert.Equal(ClipEventType.ServerUpload, clipEvent.ClipEventType);

            ClipEntity clip = await db.ClipAsync(SbDbSeed.OrgPrimary.Route, response.ClipRoute);
            Assert.Equal(ClipState.Ready, clip.ClipState);

            // CHECK STATUS AGAIN - Ensure state is valid, but no further work is done aside from reading state

            currentStep = await clips.StateAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, response.ClipRoute);

            // Should be same as last block because we just return ready and do not do any work, most critical no new events
            Assert.Equal(ClipState.Ready, currentStep);
            Assert.Equal(0, MockClipFiles.UploadCount);
            Assert.Equal(0, MockClipFiles.DownloadUrlCount);
            Assert.Equal(1, MockClipFiles.UploadUrlCount);
            Assert.Equal(2, MockClipFiles.ExistCount);

            clip = await db.ClipAsync(SbDbSeed.OrgPrimary.Route, response.ClipRoute);
            Assert.Equal(ClipAnalyzer.ByteCountAsSeconds(MockClipFiles.SizeBytes), clip.BillingSeconds);

            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(1);
            snap.AssertAdd<ClipEventEntity>(1);
        }

        [Fact]
        public async Task CreateAsync_InvalidMp3()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Invalid_Mp3,
                FileType = FileType.Mp3,
                ClipType = ClipType.Contribution,
                ParticipantRole = ParticipantRole.Host
            };
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db)
                .Snap<SessionEntity>()
                .Snap<PromptEntity>()
                .Snap<ClipEntity>()
                .Snap<ClipEventEntity>();

            // ACT + ASSERT
            await Assert.ThrowsAsync<BadClipFileTypeException>(async () =>
            {
                IClipService sessions = Builder.ClipService.Value;
                await sessions.CreateAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, newClip);
            });

            Assert.Equal(0, MockClipFiles.UploadCount);
            Assert.Equal(0, LifeCycleFactory.MockStrategy.PreClipCount);
            Assert.Equal(0, LifeCycleFactory.MockStrategy.PostClipCount);
            snap.AssertAdd<SessionEntity>(0);
            snap.AssertAdd<PromptEntity>(0);
            snap.AssertAdd<ClipEntity>(0);
            snap.AssertAdd<ClipEventEntity>(0);
        }
    }
}
