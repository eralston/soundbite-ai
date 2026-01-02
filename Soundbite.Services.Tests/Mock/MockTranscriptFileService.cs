using Masticore.Transcription;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockTranscriptFileService : ITranscriptFileService
    {
        public int DownloadCount { get; set; } = 0;
        public int DownloadUrlCount { get; set; } = 0;
        public int UploadCount { get; set; } = 0;
        public TranscriptionResult Result { get; set; }

        public Task<TranscriptionResult> DownloadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            DownloadCount++;
            return Task.FromResult(Result);
        }

        public Task<string> DownloadUrlAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            DownloadUrlCount++;
            return Task.FromResult("Examples/location/here");
        }

        public Task<string> UploadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, TranscriptionResult result)
        {
            UploadCount++;
            Result = result;
            return Task.FromResult("Examples/location/here");
        }
    }
}