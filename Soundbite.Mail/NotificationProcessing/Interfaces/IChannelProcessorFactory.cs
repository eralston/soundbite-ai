namespace Soundbite.Messaging
{
    public interface IChannelProcessorFactory
    {
        /// <summary>
        /// Repsonsible for returning an appropriate <see cref="IChannelProcessor"/> for the specified <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">Type of chanel for which a <see cref="IChannelProcessor"/> is being sought.</param>
        /// <param name="notificationProcessingService">Reference to the <see cref="INotificationProcessingService"/> to pass to the channel processor (cannot be in consturctor due to circular dependency issues)</param>
        /// <returns>a <see cref="IChannelProcessor"/> instance ready for use.</returns>        
        IChannelProcessor GetChannelProcessor(NotificationChannel channel, INotificationProcessingService notificationProcessingService);
    }
}