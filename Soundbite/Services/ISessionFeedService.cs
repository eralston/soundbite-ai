using Masticore.Models;
using Soundbite.Models;
using System.Threading.Tasks;

namespace Soundbite
{
    /// <summary>
    /// Handles the collections of <see cref="Session"/> objects, especially the different varieties of user contextual lists of sessions (EG, Feeds)
    /// </summary>
    public interface ISessionFeedService
    {
        /// <summary>
        /// Gets a collection of <see cref="SessionPreview"/> objects that are scheduled, but not yet published for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<IndexPageResponse<SessionPreview>> ReadPendingAsync(string orgRoute, IndexPageRequest page = null);

        /// <summary>
        /// Gets a collection of <see cref="SessionPreview"/> objects that are scheduled, but not yet published for the given group in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        Task<IndexPageResponse<SessionPreview>> ReadPendingAsync(string orgRoute, string groupRoute, IndexPageRequest page = null);

        /// <summary>
        /// Gets a collection of <see cref="SessionPreview"/> objects were at some point successfully published in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<IndexPageResponse<SessionPreview>> ReadPastAsync(string orgRoute, IndexPageRequest page = null);

        /// <summary>
        /// Gets a collection of <see cref="SessionPreview"/> objects that were published at some point in the given group of the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        Task<IndexPageResponse<SessionPreview>> ReadPublishedAsync(string orgRoute, string groupRoute, IndexPageRequest page = null);
    }
}
