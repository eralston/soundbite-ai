using Masticore.Models;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// The session comment
    /// </summary>
    public class SessionComment : ResourceBase
    {
        /// <summary>
        /// Gets or sets the text content for a given comment
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets a reference to the person who made this comment
        /// </summary>        
        public Person Person { get; set; }

        // Comments are posted at time of creation, but if updatedutc is distinct then treat it as an "updated at" time
    }

    public class NewSessionComment
    {
        /// <summary>
        /// Gets or sets the text content for a given comment
        /// </summary>
        public string Content { get; set; }

    }

    /// <summary>
    /// Service for commenting on a session; user must have access to the session to enable CRUD actions on its related comments
    /// </summary>
    public interface ISessionCommentService
    {
        /// <summary>
        /// Creates a new comment in the given session in the given org with the given content
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="newComment"></param>
        /// <returns></returns>
        public Task<SessionComment> CreateAsync(string orgRoute, string sessionRoute, NewSessionComment newComment);

        /// <summary>
        /// Reads a list of all comments for the given session in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="pageRequest"></param>
        /// <returns></returns>
        public Task<IndexPageResponse<SessionComment>> ReadAllAsync(string orgRoute, string sessionRoute, IndexPageRequest pageRequest = null);

        /// <summary>
        /// Updates the given comment for the given session in the given org, sending back the changed value
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="commentRoute"></param>
        /// <param name="updatedComment"></param>
        /// <returns></returns>
        public Task<SessionComment> UpdateAsync(string orgRoute, string sessionRoute, string commentRoute, NewSessionComment updatedComment);

        /// <summary>
        /// Deletes the given comment, removing it from the given session; consumer required to read comments again to show consequences
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="commentRoute"></param>
        /// <returns></returns>
        public Task DeleteAsync(string orgRoute, string sessionRoute, string commentRoute);
    }
}
