using Masticore;
using Masticore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with sessions.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class SessionCommentsController : ControllerBase
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="ISessionCommentService"/> object for this controller; specialized to CRUD actions on <see cref="SessionComment"/> and similar objects
        /// </summary>
        protected ISessionCommentService SessionService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionCommentsController"/> class.
        /// </summary>
        /// <param name="sessionCommentService"> DI reference.</param>
        public SessionCommentsController(
            ISessionCommentService sessionCommentService)
        {
            SessionService = sessionCommentService ?? throw new ArgumentNullException(nameof(sessionCommentService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Retrieves the comments for the given session in the given org
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to retrieve.</param>
        /// <param name="page">Optional paging token</param>
        ///<returns>A page of comments for the given session</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/comments")]
        [HttpGet]
        public async Task<IndexPageResponse<SessionComment>> ReadAllSessionComments(string orgRoute, string sessionRoute, [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page)
        {
            return await SessionService.ReadAllAsync(orgRoute, sessionRoute, page);
        }

        /// <summary>
        /// Retrieves the comments for the given session in the given org
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to retrieve.</param>
        /// <param name="newComment">Content for the newly created comment</param>
        ///<returns>A page of comments for the given session</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/comments")]
        [HttpPost]
        public async Task<SessionComment> CreateSessionComment(string orgRoute, string sessionRoute, [FromBody] NewSessionComment newComment)
        {
            return await SessionService.CreateAsync(orgRoute, sessionRoute, newComment);
        }

        /// <summary>
        /// Updates the given comment on the given session in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="commentRoute"></param>
        /// <param name="newComment"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/comments/{commentRoute}")]
        [HttpPut]
        public async Task UpdateSessionComment(string orgRoute, string sessionRoute, string commentRoute, [FromBody] NewSessionComment newComment)
        {
            await SessionService.UpdateAsync(orgRoute, sessionRoute, commentRoute, newComment);
        }

        /// <summary>
        /// Deletes the given comment on the given session within the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="commentRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/comments/{commentRoute}")]
        [HttpDelete]
        public async Task DeleteSessionComment(string orgRoute, string sessionRoute, string commentRoute)
        {
            await SessionService.DeleteAsync(orgRoute, sessionRoute, commentRoute);
        }

        #endregion        
    }
}