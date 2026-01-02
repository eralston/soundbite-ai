using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Soundbite.AzFun;
using Soundbite.Messaging;
using Soundbite.Messaging.Jobs;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Async processes <see cref="SbNotificationJob"/> to send via <see cref="SbNotificationJobWorker"/>
    /// </summary>
    public class SendSessioionNotificationsFunc : SbAzureFunc
    {
        readonly IConfiguration _config;

        #region Constructor

        public SendSessioionNotificationsFunc(IConfiguration config) : base(null)
        {
            _config = config;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes notifications based on messages from the <see cref="SbNotificationJob.QueueName"/>.
        /// </summary>
        /// <param name="message">Message body of the queue item being processed.</param>
        /// <param name="messageId">ID of the queue item being processed.</param>
        /// <returns>a task representing the operation</returns>
        [FunctionName(nameof(SendSessioionNotificationsFunc))]
        public async Task EntryPoint([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req)
        {
            await EnsureSecretsAndRun(nameof(SendSessioionNotificationsFunc), RunProcess);
        }

        /// <summary>
        /// Performs the main operation of the function.
        /// </summary>
        /// <returns>a task representing the </returns>
        private Task RunProcess()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}