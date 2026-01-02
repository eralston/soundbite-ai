using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soundbite
{
    /// <summary>
    /// Responsible for managing organization-level tokens for Azure/Graph calls.
    /// </summary>
    public interface ITeamsGraphService
    {
        #region Methods

        /// <summary>
        /// Responsible for retrieving the Teams app access token.
        /// </summary>
        /// <param name="orgRoute">Route of the org on whose behalf the access token is being requested.</param>
        /// <returns>a string containing the access token or <c>null</c> if the access token cannot be acquired.</returns>
        Task<string> GetTeamsAppToken(string orgRoute);

        /// <summary>
        /// Sends a MS Graph API call that generates a MS Teams Notification inside the Teams application.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the user receiving the notification belongs.</param>
        /// <param name="userRoute">Route of the user to whom the message is being sent.</param>
        /// <param name="emailOrUserId">Email or ID of the user to which the notification is sent (used to build the Graph API call)</param>
        /// <param name="soundbiteTitle">Title to display in the Teams notification.</param>
        /// <param name="soundbiteUrl">URL to which the user is redirected.</param>
        /// <param name="soundbiteAuthor">Author of the Soundbite for which the notification is being sent.</param>
        /// <returns>A loggable description of the result of the notification; if cancelled then this returns null</returns>
        Task<string> SendTeamsAppNotification(string orgRoute, string userRoute, string emailOrUserId, string soundbiteTitle, string soundbiteUrl, string soundbiteAuthor);

        /// <summary>
        /// Sends a MS Graph Batch API call that sends multiple MS Teams Notifications inside the
        /// Teams application. The response from this call is a dictionary containing HTTP response
        /// information keyed to the SessionNotificationId associated with the call.
        /// </summary>
        /// <param name="orgRoute">Route of the organization associated with the session for which notifications are being sent.</param>
        /// <param name="notificationRequests">Requests to fulfill.</param>
        /// <returns>a dictionary with keys matched to the SessionNotificationId in the <see cref="TeamsAppBatchNotificationItem.Recipients"/> items of the <paramref name="notificationRequests"/>.</returns>
        Task<IDictionary<string, HttpResponseMessage>> SendTeamsAppBatchNotification(string orgRoute, IEnumerable<TeamsAppBatchNotificationItem> notificationRequests, string orgGraphToken = null);

        #endregion
    }
}
