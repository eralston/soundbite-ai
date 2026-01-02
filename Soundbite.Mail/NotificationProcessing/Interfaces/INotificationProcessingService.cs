using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    public interface INotificationProcessingService
    {
        /// <summary>
        /// Determines whether there are any messages that need processing.
        /// </summary>
        /// <param name="channel">Specifies the message channel filter.</param>
        /// <returns><c>true</c> if there are messages to process, otherwise <c>false</c>.</returns>
        Task<bool> HasMessagesToSend(NotificationChannel channel);

        /// <summary>
        /// Acquires an enumeration of all the IDs of organizations that have pending messages.
        /// </summary>
        /// <param name="channel">Specifies the message channel filter.</param>
        /// /// <param name="maxCount">Maximum number of values to retrieve.</param>
        /// <param name="exclude">List identifying items to exclude from the retrieval list.</param>
        /// <returns>An enumeration of all organization IDs that have pending messages.</returns>
        Task<IEnumerable<int>> UniqueOrgIdsWithMessages(NotificationChannel channel, int maxCount, IEnumerable<int> exclude = null);

        /// <summary>
        /// Responsible for processing all pending messages.
        /// </summary>
        /// <returns>a Task representing the operation.</returns>
        Task ProcessNotifications();

        /// <summary>
        /// Retrieves pending messages for a given organization and channel.
        /// </summary>
        /// <param name="orgId">ID of the organization associated with the messages.</param>
        /// <param name="maxCount">Maximum number of items to retrieve at once.</param>
        /// <returns>an enumeration of messages within the organization and channel to be processed.</returns>
        Task<IEnumerable<SessionNotificationToProcess>> GetMessagesForProcessing(int orgId, NotificationChannel channel, int maxCount);

        /// <summary>
        /// Updates the status of items that have been abandoned by a failed procsses.
        /// </summary>
        /// <param name="channel">Specifies a specific channel within which to search for abandoned processes.</param>
        /// <param name="allChannels">Flag indicating whether all channels should be updated. When <c>true</c> the <paramref name="channel"/> has no impact on the operation.</param>
        /// <returns>a boolean value indicating whether any items were updated.</returns>
        Task<bool> ResetAbandonedItems(NotificationChannel channel, bool allChannels = false);
    }
}