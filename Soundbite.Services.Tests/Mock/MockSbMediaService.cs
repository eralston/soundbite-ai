using Masticore.Media;
using Soundbite.Models.JobRequests;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    /// <summary>
    /// Fake <see cref="ISessionService"/>
    /// </summary>
    public class MockSbMediaService : ISbMediaService
    {
        public Task MoveEncodedAssetsToClipContainer(AzureTransformRequest encodingRequest)
        {
            throw new System.NotImplementedException();
        }

        public Task OnTranscriptionComplete(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ProcessMediaEffects(string orgRoute, string sessionRoute, string clipRoute, IMediaEffect[] mediaEffects)
        {
            throw new System.NotImplementedException();
        }

        public Task ProcessMediaOperationRequest(MediaOperationRequest request)
        {
            return Task.CompletedTask;
        }

        public Task QueueMediaOperationJob(MediaOperationRequest request)
        {
            return Task.CompletedTask;
        }

        public Task RequestVideoTranscription(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task SetClipDurationInfo(string orgRoute, string sessionRoute, string clipRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task StreamClip(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Responsible for submitting an EncodeClipRequest for processing.
        /// </summary>
        /// <param name="request">Request to submit.</param>
        /// <returns>a boolean value indicating sucess or failure of the operation.</returns>
        public Task<bool> SubmitEncodeClipRequest(EncodeClipRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
