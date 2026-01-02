using Masticore.Media;
using Masticore.Services;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the contract for service that handles transcription operations.
    /// </summary>
    public interface ISbMediaService : IService
    {
        /// <summary>
        /// Responsible for managing the media effect processing. 
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="mediaEffects"></param>
        /// <returns><c>true</c> if all media effects were processed synchronously or <c>false</c> indicating that media effects will be processed asynchronously.</returns>
        Task<bool> ProcessMediaEffects(string orgRoute, string sessionRoute, string clipRoute, IMediaEffect[] mediaEffects);

        /// <summary>
        /// Retrieves clip duration information from the media service and saves it to the clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the clip.</param>
        /// <param name="clipRoute">Route of the clip whose duration is being sought.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task SetClipDurationInfo(string orgRoute, string sessionRoute, string clipRoute);

        /// <summary>
        /// Responsible for setting up asset streaming and cleanup of the encoding process.
        /// </summary>
        /// <param name="orgRoute">Route of the org in which the session containing the clip resides.</param>
        /// <param name="sessionRoute">Route of the session containing the clip.</param>
        /// <param name="clipRoute">Route of the clip to stream.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task StreamClip(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Responsible for starting the process transcription process.
        /// </summary>
        /// <param name="orgRoute">Route of the org in which the session containing the clip resides.</param>
        /// <param name="sessionRoute">Route of the session containing the clip.</param>
        /// <param name="clipRoute">Route of the clip to transcribe.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task RequestVideoTranscription(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Handles the cleanup and deployment of video transcription files afer AMS processing.
        /// </summary>
        /// <param name="orgRoute">Route of the org in which the session containing the clip resides.</param>
        /// <param name="sessionRoute">Route of the session containing the clip.</param>
        /// <param name="clipRoute">Route of the clip that was transcribed.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task OnTranscriptionComplete(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);
    }
}