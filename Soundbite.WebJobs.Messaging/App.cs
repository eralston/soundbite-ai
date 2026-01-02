using Masticore;
using Soundbite.Messaging;

namespace Soundbite.WebJobs.Messaging
{
    internal class App
    {
        #region Properties (DI)

        private INotificationProcessingService NotificationProcessingService { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="App"/> instance.
        /// </summary>
        /// <param name="notificationProcessingService">DI reference to a <see cref="INotificationProcessingService"/> instance.</param>
        public App(INotificationProcessingService notificationProcessingService)
        {
            Validator.ArgNotNull(nameof(notificationProcessingService), notificationProcessingService);
            NotificationProcessingService = notificationProcessingService;
        }

        #endregion

        public void Run()
        {
            NotificationProcessingService.ProcessNotifications();
        }
    }
}