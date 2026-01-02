using Masticore;
using Masticore.Models;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with sessions.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        #region Properties

        private IUserService Users { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="userService">IUserService DI reference.</param>
        public UsersController(IUserService userService)
        {
            Users = userService;
        }

        #endregion

        #region EndPoints

        /// <summary>
        /// Updates the current user's information.
        /// </summary>
        /// <param name="userInfo">User information to update.</param>
        /// <returns>an <see cref="User"/> instance populated with updated user information for the current user.</returns>
        [Route("/me")]
        [HttpPost]
        public async Task<User> UpsertMeAsync([FromBody] User userInfo)
        {
            User user = await Users.UpsertMeAsync(userInfo);
            return user;
        }

        /// <summary>
        /// Async update the current user's notification settings
        /// </summary>
        /// <param name="notificationSettings"></param>
        /// <returns></returns>
        [Route("/me/notifications/settings")]
        [HttpPost]
        public async Task UpdateNotificationAsync([FromBody] UserNotifications notificationSettings)
        {
            await Users.UpdateMyNotifications(notificationSettings);
        }

        /// <summary>
        /// Uploads a new "avatar" image for the current user.  The URL returned in the ImageSrc
        /// property contains a short-lived token used to access the image.
        /// </summary>
        /// <returns>an <see cref="User"/> instance with an updated ImageSrc property containing the URL of the uploaded image.</returns>
        [Route("/me/image")]
        [HttpPost]
        [CodeGenMethod(Scenario = CodeGenScenarioType.FileUploadSingleFromBlob)]
        public async Task<User> UpsertMyImageAsync()
        {
            using MemoryStream stream = await Request.ToStream();
            User user = await Users.UpsertMyImageAsync(stream);
            return user;
        }

        /// <summary>
        /// Retrieves user details for the current user.
        /// </summary>
        /// <returns>an <see cref="User"/> instance populated with the current user's information.</returns>
        [Route("/me")]
        [HttpGet]
        public async Task<User> ReadMeAsync()
        {
            User result = await Users.ReadMeAsync();
            return result;
        }

        /// <summary>
        /// Gets the route of a user based on the user's universal ID.
        /// </summary>
        /// <param name="universalId">Universal ID of the user whose route is being sought.</param>
        /// <returns>The route associated with the user with the specified universal ID or <c>null</c> if the user is not found.</returns>
        [Route("/users/routeFromUid/{universalId}")]
        [HttpGet]
        public async Task<string> GetRouteFromUniversalId(string universalId)
        {
            string result = await Users.ReadRouteFromUniversalId(universalId);
            return result;
        }

        /// <summary>
        /// Invites a user to the application.
        /// </summary>
        /// <param name="invites">One or more invitations identifying the people to invite to the application.</param>
        /// <returns>an HTTP response indicating success or failure of the operation.</returns>
        [Route("/users")]
        [HttpPost]
        public async Task InviteAsync([FromBody] Invite[] invites)
        {
            await Users.InviteAsync(invites);
        }

        #endregion
    }
}