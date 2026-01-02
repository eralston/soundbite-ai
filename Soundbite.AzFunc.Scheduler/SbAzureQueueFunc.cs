using Masticore.Services;
using Microsoft.Extensions.Logging;
using Soundbite.Api;
using Soundbite.AzFun;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Base class for building queue-triggered Azure functions.
    /// </summary>
    public abstract class SbAzureQueueFunc
    {
        #region Properties

        /// <summary>
        /// Gets the dependency injected <see cref="ILogger"/> reference.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the dependency injected <see cref="IQueuedJobStatusService"/> reference.
        /// </summary>
        protected IQueuedJobStatusService QueuedJobStatusService { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SbAzureQueueFunc"/> instance.
        /// </summary>
        /// <param name="logger">DI reference to a logger.</param>
        public SbAzureQueueFunc(ILogger logger, IQueuedJobStatusService queuedJobStatusService)
        {
            Logger = logger;
            QueuedJobStatusService = queuedJobStatusService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for loading Soundbite infrastructure in an Azure Function.
        /// </summary>
        /// <param name="dbStr">Soundbite database connection string.</param>
        /// <param name="storStr">Soundbite storage connection string.</param>
        public void LoadInfrastructure(string dbCnxStr, string storCnxStr)
        {
            AzInfrastructure.Configure(dbCnxStr, storCnxStr);
        }

        /// <summary>
        /// Determines whether a valid connection to Soundbite infrastructure has been established.
        /// </summary>
        /// <returns><c>true</c> if connection is valid otherwise <c>false</c>.</returns>
        public Task<bool> HasConnection()
        {
            return Task.FromResult(AzInfrastructure.HasConnection);
        }


        /// <summary>
        /// Responsible for setting up and verifying Soundbite infrastructure and running <see cref="Process"/>.
        /// </summary>
        /// <param name="message">Contents of the queue message (a.k.a the data)</param>
        /// <param name="messageId">ID of the queue message.</param>        
        /// <returns>a task indicating success or failure of the operation.</returns>
        public async Task SetupAndRunProcess(string message, string messageId)
        {
            await AzFunUtils.EnsureSecretsAndRun(
                GetType().Name,
                Logger,
                HasConnection,
                LoadInfrastructure,
                async () => { await Process(message, messageId); });
        }

        /// <summary>
        /// Responsible for running any processing that the function needs to complete.
        /// </summary>
        /// <param name="message">Contents of the queue message (a.k.a the data)</param>
        /// <param name="messageId">ID of the queue message.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        protected abstract Task Process(string message, string messageId);

        #endregion
    }
}
