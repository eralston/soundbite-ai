using Masticore.Services;
using Masticore.Transcription;
using Soundbite.Models.JobRequests;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the contract for service that handles transcription operations.
    /// </summary>
    public interface ISbTranscriptionService : IService
    {
        /// <summary>
        /// Deletes the transcript associated with the specified clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization associated with the session.</param>
        /// <param name="sessionRoute">Route of the session associated with the clip.</param>
        /// <param name="clipRoute">Route of the clip whose transcription is being deleted.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task DeleteTranscript(string orgRoute, string sessionRoute, string clipRoute);

        /// <summary>
        /// Processes a request to transcribe a clip.
        /// </summary>
        /// <param name="request">Request containing information identifying the clip to transcribe.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task ProcessTranscriptionRequest(TranscriptionRequest request);

        /// <summary>
        /// Queues a request to the azure function that processes transcription requests.
        /// </summary>
        /// <param name="request">Transcription request to queue for processing.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task QueueTranscriptionRequest(TranscriptionRequest request);

        /// <summary>
        /// Retrieves the full transcript for the specified clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization associated with the session.</param>
        /// <param name="sessionRoute">Route of the session associated with the clip.</param>
        /// <param name="clipRoute">Route of the clip whose transcription is being sought.</param>
        /// <param name="isPublic">Flag indicating whether the clip is public.</param>
        /// <returns>a <see cref="string"/> secure URL to the clip's transcript file</returns>
        Task<string> ReadTranscriptFileUrl(string orgRoute, string sessionRoute, string clipRoute, bool isPublic);

        /// <summary>
        /// Retrieves the full transcript for the specified clip
        /// </summary>
        /// <param name="orgRoute">Route of the organization associated with the session.</param>
        /// <param name="sessionRoute">Route of the session associated with the clip.</param>
        /// <param name="clipRoute">Route of the clip whose transcription is being sought.</param>
        /// <param name="isPublic">Flag indicating whether the clip is public.</param>
        /// <returns>a <see cref="TranscriptionResult"/> containing information about the transcript for the clip.</returns>
        public Task<TranscriptionResult> ReadTranscript(string orgRoute, string sessionRoute, string clipRoute, bool isPublic);
    }
}