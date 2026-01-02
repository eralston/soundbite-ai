using System;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockMediaStreamingService : IMediaStreamingService
    {
        // Mock implementation of IMediaStreamingService
        public Task<string> ReadLevelPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, string level)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadLevelPlaylist(string clipRoute, string level)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadVariantPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadVariantPlaylist(string clipRoute)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateManifest(string sourceUrl, string token)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateManifestQualityStream(string playbackUrl, string token)
        {
            throw new NotImplementedException();
        }
    }
}
