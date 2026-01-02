using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Masticore.Jobs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.Queue
{
    /// <summary>
    /// Base Azure functionality for an <see cref="IJobQueue"/> clases
    /// </summary>
    public class AzQueue : IJobQueue
    {
        /// <summary>
        /// Forces a given queue to conform to the <see href="https://learn.microsoft.com/en-us/rest/api/storageservices/naming-queues-and-metadata">Azure queue name constraints</see>
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string ToQueueName(string typeName)
        {
            return $"q-{typeName.ToLower().Replace(".", "-")}";
        }

        /// <summary>
        /// Gets or sets the connection credentials for the underlying <see cref="QueueClient"/>
        /// </summary>
        public string ConnectionString { get; set; }

        protected Dictionary<string, QueueClient> _clients = new Dictionary<string, QueueClient>();

        /// <summary>
        /// Instantiate the client for the given queue since we need custom options
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private QueueClient CreateQueueClient(string queueName)
        {
            return new QueueClient(
                ConnectionString,
                queueName,
                new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                });
        }

        /// <summary>
        /// Retrieve a <see cref="QueueClient"/> for the given unique type name
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        protected QueueClient GetClient(string typeName)
        {
            string queueName = ToQueueName(typeName);
            if (!_clients.ContainsKey(queueName))
            {
                _clients.Add(queueName, CreateQueueClient(queueName));
            }

            return _clients[queueName];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public AzQueue(string connectionString)
        {
            ConnectionString = connectionString ?? throw new System.ArgumentNullException(nameof(connectionString));
        }

        #region IJobQueue

        /// <inheritdoc/>
        public async Task<string> Add<TJobType>(TJobType job) where TJobType : IJob
        {
            Validator.NotNull(job, nameof(job));

            // Queue may not exist yet
            string typeName = job.Type;
            QueueClient client = GetClient(typeName);
            await client.CreateIfNotExistsAsync();

            // Delegate to the queue then send back the unique ID
            string message = job.Message;
            Response<SendReceipt> response = await client.SendMessageAsync(message);
            job.Id = response.Value.MessageId;
            return job.Id;
        }

        #endregion

        /// <summary>
        /// Manually processes a job from the queue, returning true if there was a job; otherwise, false 
        /// </summary>
        /// <remarks>Keep in mind that Azure Functions can trigger based on the message itself, so this is used only when work must be processed from the thread "manually"</remarks>
        /// <returns></returns>
        public async Task<IJob> ProcessNext(IJobWorker worker, string typeName, bool deleteWhenDone = true)
        {
            Validator.NotNull(worker, nameof(worker));
            Validator.NotNullOrWhitespace(typeName, nameof(typeName));

            QueueClient client = GetClient(typeName);
            bool exists = await client.ExistsAsync();
            if (!exists)
            {
                throw new System.Exception($"Cannot process next on queue for '{typeName}'; Azure queue '{ToQueueName(typeName)}' does not exist");
            }

            QueueMessage message = await client.ReceiveMessageAsync();
            if (message == null)
            {
                return null;
            }

            IJob ret = null;
            if (message.Body != null)
            {
                ret = await worker.Process(message.Body.ToString(), message.MessageId);
            }

            if (deleteWhenDone)
            {
                await client.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }

            return ret;
        }
    }
}
