using Masticore.Entity.Tests;
using Soundbite.Entity.Tests;
using Xunit;

namespace Soundbite.Services.Tests
{
    /// <summary>
    /// Unit tests for <see cref="MediaStreamingService"/>
    /// </summary>
    public class MediaStreamingServiceTests : ServiceTestBase
    {

        #region Constructor

        public MediaStreamingServiceTests()
        {
            Builder.MediaStreamingService.Use(svc =>
            {
                return new MediaStreamingService(
                    svc.HttpClientFactory.Value,
                    svc.Rbac.Value,
                    svc.Infrastructure.Value,
                    svc.Mapper.Value,
                    svc.Logger<MediaStreamingService>()
                    );
            });
        }

        #endregion

        #region Unit Tests

        [Fact]
        public async void ReadVariantPlaylist()
        {
            // Arrange
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            IMediaStreamingService mediaStreamingService = Builder.MediaStreamingService.Value;
            Builder.Infrastructure.Value.Blobs.SetStreamFromFile("Example/V1/playlist.m3u8");

            // Act
            string variantPlaylistWithTokens = await mediaStreamingService.ReadVariantPlaylist(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute);

            // Assert
            Assert.NotNull(variantPlaylistWithTokens);
        }

        [Fact]
        public async void ReadLevelPlaylist()
        {
            // Arrange
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            IMediaStreamingService mediaStreamingService = Builder.MediaStreamingService.Value;
            Builder.Infrastructure.Value.Blobs.SetStreamFromFile("Example/V1/1080p/playlist.m3u8");

            // Act
            string levelPlaylistWithTokens = await mediaStreamingService.ReadLevelPlaylist(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.SessionRoute, SbDbSeed.PromptRoute, SbDbSeed.LastClipRoute, "1080p");

            // Assert
            Assert.NotNull(levelPlaylistWithTokens);
        }

        #endregion
    }
}
