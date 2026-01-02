using Microsoft.Extensions.DependencyInjection;
using System;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Factory for creating <see cref="IChannelProcessor"/> instances based on <see cref="NotificationChannel"/> values.
    /// </summary>
    public class ChannelProcessorFactory : IChannelProcessorFactory
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="ChannelProcessorFactory"/> instance.
        /// </summary>
        /// <param name="serviceProvider">DI service provider reference.</param>
        public ChannelProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region IChannelProcessorFactory Implementation

        /// <inheritdoc />
        public IChannelProcessor GetChannelProcessor(NotificationChannel channel, INotificationProcessingService notificationProcessingService)
        {
            IChannelProcessor result;
            switch (channel)
            {
                case NotificationChannel.Teams:
                    result = _serviceProvider.GetRequiredService<TeamsChannelProcessor>();
                    break;
                default:
                    result = null;
                    break;
            }

            // Make sure to assign the INotificationProcessingService to the IChannelProcessor
            if (result != null)
            {
                result.NotificationProcessingService = notificationProcessingService;
            }

            return result;
        }


        #endregion
    }
}