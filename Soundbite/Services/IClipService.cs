using Masticore.Services;
using Soundbite.Models;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the service operations required to manage clips.
    /// </summary>
    public interface IClipService : IService
    {
        /// <summary>
        /// Uploads a clip for a session.
        /// </summary>
        /// <param name="orgRoute">Route identifying the organization containing the session.</param>
        /// <param name="sessionRoute">Route identifying the session containing the clip.</param>
        /// <param name="promptRoute">Route identifying the prompt in response to which the clip was uploaded.</param>
        /// <param name="newClip">Clip to upload.</param>
        /// <param name="mediaOpRequest">Request containing media operations that need to be processed for the clip.</param>
        /// <returns>the URL to the location were the clip has been persisted along with client operations that need to run on the client.</returns>
        Task<ClipDetails> CreateAsync(string orgRoute, string sessionRoute, string promptRoute, NewClip newClip);

        /// <summary>
        /// Evaluates the current status of a clip, indicating its current <see cref="ClipState"/> after evaluating a maximally available processing of the clip
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<ClipState> StateAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Uploads a clip for a broadcast session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to which the clip belongs.</param>
        /// <param name="newClip">Clip to upload.</param>
        /// <param name="mediaOpRequest">Request containing media operations that need to be processed for the clip.</param>
        /// <returns>the URL to the location were the clip has been persisted along with client operations that need to run on the client.</returns>
        Task<ClipDetails> CreateAnnouncementAsync(string orgRoute, string sessionRoute, NewClip newClip);
    }
}
