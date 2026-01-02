using Masticore.Transcription;
using Soundbite.Models.JobRequests;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    /// <summary>
    /// Fake <see cref="ISessionService"/>
    /// </summary>
    public class MockSbTranscriptionService : ISbTranscriptionService
    {
        public Task DeleteTranscript(string orgRoute, string sessionRoute, string clipRoute)
        {
            return Task.CompletedTask;
        }

        public Task ProcessTranscriptionRequest(TranscriptionRequest request)
        {
            return Task.CompletedTask;
        }

        public Task QueueTranscriptionRequest(TranscriptionRequest request)
        {
            return Task.CompletedTask;
        }

        public Task<TranscriptionResult> ReadTranscript(string orgRoute, string sessionRoute, string clipRoute, bool isPublic)
        {
            throw new System.NotImplementedException();
        }

        Task<string> ISbTranscriptionService.ReadTranscriptFileUrl(string orgRoute, string sessionRoute, string clipRoute, bool isPublic)
        {
            throw new System.NotImplementedException();
        }
    }
}
