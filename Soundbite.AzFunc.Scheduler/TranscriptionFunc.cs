using Masticore;
using Masticore.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Azure function responsible for transcribing a clip.
    /// </summary>
    public class TranscriptionFunc : SbAzureQueueFunc
    {
        #region Properties

        private TranscriptionJob Job { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="VideoEncodingFunc"/> instance.
        /// </summary>
        /// <param name="logger">DI reference to <see cref="Ilogger"/>.</param>
        /// <param name="queuedJobStatusService">DI reference to <see cref="IQueuedJobStatusService"/>.</param>
        /// <param name="job">DI reference (with DI values injected) to the transcription job.</param>
        public TranscriptionFunc(ILogger<TranscriptionFunc> logger, IQueuedJobStatusService queuedJobStatusService, TranscriptionJob job)
            : base(logger, queuedJobStatusService)
        {
            QueuedJobStatusService = queuedJobStatusService;
            Job = job;
        }

        #endregion

        #region EntryPoint

        /// <summary>
        /// Entry point for the azure function.
        /// </summary>
        /// <param name="message">Contents of the queue message (a.k.a the data)</param>
        /// <param name="id">ID of the queue message.</param>
        /// <param name="log"></param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        [FunctionName(nameof(TranscriptionFunc))]
        public async Task EntryPoint([QueueTrigger(TranscriptionJob.QueueName, Connection = "SbStorage")] string message, string id)
        {
            await SetupAndRunProcess(message, id);
        }

        #endregion

        #region Methods

        /// <inheritdoc>
        protected override async Task Process(string message, string messageId)
        {
            try
            {
                TranscriptionJob jobData = JsonUtils.FromLowerCamelJson<TranscriptionJob>(message);
                await QueuedJobStatusService.SetQueueInfo(jobData.JobStatusRoute, messageId, message);
                Job.Populate(jobData);
                await Job.Process();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"TranscriptionFunc failed to deserialize TranscriptionJob from message (Message ID:{messageId}).");
            }
        }

        #endregion        
    }
}