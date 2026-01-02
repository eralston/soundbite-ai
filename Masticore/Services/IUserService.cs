using Masticore.Models;
using Masticore.Security;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// A service for IUser
    /// </summary>
    public interface IUserService : IService
    {
        /// <summary>
        /// Updates the current user with the specified user information.
        /// </summary>
        /// <param name="suggestedUserFields">User information with which to update the current user.</param>
        /// <returns>a reference to the current user with any updated information applied.</returns>
        Task<User> UpdateMeAsync(User suggestedUserFields);

        /// <summary>
        /// Create or update the current user's record to match the given <see cref="User"/> record
        /// </summary>
        /// <param name="userFields"></param>
        /// <returns></returns>
        Task<User> UpsertMeAsync(User userFields);

        /// <summary>
        /// Sets the current user's <see cref="IUserNotifications"/>
        /// </summary>
        /// <param name="myNotificationSettings"></param>
        /// <returns></returns>
        Task UpdateMyNotifications(IUserNotifications myNotificationSettings);

        /// <summary>
        /// Responsible for extracting information from a third-party claims principal and creating
        /// a new soundbite user entry.
        /// </summary>
        /// <param name="claims">Claims from which to extract information.</param>
        /// <returns>an <see cref="User"/> instance representing the new user or <c>null</c> if a user could not be created from claims info.</returns>
        Task<User> UpsertMeAsync(IClaims claims);

        /// <summary>
        /// Create or update the current user's image to match the given <see cref="Stream"/>
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        Task<User> UpsertMyImageAsync(Stream image);

        /// <summary>
        /// Read the current user's <see cref="User"/> record
        /// </summary>
        /// <param name="includeImage">Opotionally indicate if <see cref="User.ImageSrc"/> should be loaded with its value</param>
        /// <returns></returns>
        Task<User> ReadMeAsync(bool includeImage = true);

        /// <summary>
        /// Determines whether the specified refresh token is valid, and updates the security 
        /// context with the user associated with the refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token string.</param>
        /// <returns>a boolean value indicating whether the security context was updated from the refresh token.</returns>
        Task<bool> SetSecurityContextFromRefreshToken(string refreshToken);

        /// <summary>
        /// Gets the route of a user based on the user's universal ID.
        /// </summary>
        /// <param name="universalId">Universal ID of the user whose route is being sought.</param>
        /// <returns>The route associated with the user with the specified universal ID or <c>null</c> if the user is not found.</returns>
        Task<string> ReadRouteFromUniversalId(string universalId);

        /// <summary>
        /// Gets the user associated with the specified organization route and univeral id.  If the
        /// user exists but does not belong to the organization, the user record is NOT retrieved.
        /// </summary>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="universalId">Universal ID of the user to retrieve.</param>
        /// <returns>a reference to the requested user if found in the organization, otherwise <c>null</c>.</returns>
        Task<User> ReadByOrgRouteAndUniversalId(string orgRoute, string universalId);

        /// <summary>
        /// Gets the user associated with the specified organization route and email.  If the user 
        /// exists but does not belong to the organization, the user record is NOT retrieved.
        /// </summary>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="email">Email address of the user to retrieve.</param>
        /// <param name="includeImage">Opotionally indicate if <see cref="User.ImageSrc"/> should be loaded with its value</param>
        /// <returns>a reference to the requested user if found in the organization, otherwise <c>null</c>.</returns>
        Task<User> ReadByOrgRouteAndEmail(string orgRoute, string email, bool includeImage = true);

        /// <summary>
        /// Async get the user associated with the given route; throws not found if not found
        /// </summary>
        /// <remarks>This does NOT implement RBAC since there is no guaranteed relationship possible between caller and target</remarks>
        /// <param name="userRoute"></param>
        /// <param name="includeImage">Opotionally indicate if <see cref="User.ImageSrc"/> should be loaded with its value</param>
        /// <returns></returns>
        Task<User> ReadAsync(string userRoute, bool includeImage = true);

        /// <summary>
        /// Gets the specified user by email address
        /// </summary>
        /// <param name="email">Email address of the user to retrieve.</param>
        /// <param name="includeImage">Opotionally indicate if <see cref="User.ImageSrc"/> should be loaded with its value</param>
        /// <returns>user associated with the email address if found, otherwise <c>null</c>.</returns>
        Task<User> ReadByEmail(string email, bool includeImage = true);

        /// <summary>
        /// Invites the given array of <see cref="Invite"/> to the platform (does NOT add them to an org or team)
        /// </summary>
        /// <param name="invites"></param>
        /// <returns></returns>
        Task<InviteResult[]> InviteAsync(Invite[] invites);
    }
}