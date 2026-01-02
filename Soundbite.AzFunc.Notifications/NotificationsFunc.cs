using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Soundbite.AzFun;
using Soundbite.Messaging;
using Soundbite.Messaging.Jobs;
using System;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Async processes <see cref="SbNotificationJob"/> to send via <see cref="SbNotificationJobWorker"/>
    /// </summary>
    public class NotificationsFunc : SbAzureFunc
    {
        #region Fields

        /// <summary>
        /// Stores a reference to the worker injected into the constructor.  It is injected to allow
        /// the DI system to populate all the re
        /// </summary>
        private SbNotificationJobWorker Worker { get; }

        #endregion

        #region Constructor

        public NotificationsFunc(ILogger<NotificationsFunc> logger, SbNotificationJobWorker worker) : base(logger)
        {
            Worker = worker ?? throw new ArgumentNullException(nameof(worker));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes notifications based on messages from the <see cref="SbNotificationJob.QueueName"/>.
        /// </summary>
        /// <param name="message">Message body of the queue item being processed.</param>
        /// <param name="id">ID of the queue item being processed.</param>
        /// <returns>a task representing the operation</returns>
        [FunctionName(nameof(NotificationsFunc))]
        public async Task ProcessNotification([QueueTrigger(SbNotificationJob.QueueName, Connection = "SbStorage")] string message, string id)
        {
            //NOTE: The message and id parameters CANNOT be renamed -- the QueueTrigger requires these names for binding values to them.
            await EnsureSecretsAndRun(
                  nameof(NotificationsFunc),
                  async () => { await RunProcessNotification(message, id); });
        }


        private async Task RunProcessNotification(string message, string messageId)
        {
            Logger.LogDebug($"Sending notifications for message {messageId}");
            await Worker.Process(message, messageId);
        }

        #endregion
    }
}