using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Defines the contract to implement a handler for processing pending SessionNotification messages in batch.
    /// </summary>
    public interface IChannelProcessor
    {
        /// <summary>
        /// Responsible for processing all pending messages in the channel.
        /// </summary>
        /// <returns></returns>
        Task ProcessMessages();

        /// <summary>
        /// Gets or sets a reference to the NotificationProcessingService used by the <see cref="IChannelProcessor"/>
        /// implementation. This value must be passed outside of the constructor to avoid a  circular dependency 
        /// issues when performing dependency injection.
        /// </summary>
        INotificationProcessingService NotificationProcessingService { get; set; }
    }
}