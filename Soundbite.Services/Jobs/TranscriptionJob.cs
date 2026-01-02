using Masticore;
using Masticore.Queue;
using Masticore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soundbite.Models.JobRequests;
using System;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    public class TranscriptionJob : JobBase
    {
        public const string QueueName = "q-transcriptionjob";

        #region Properties

        public TranscriptionRequest Request { get; set; }
        public string JobStatusRoute { get; set; }
        private ISbTranscriptionService TranscriptionService { get; set; }
        private IQueuedJobStatusService QueuedJobStatusService { get; set; }
        private ILogger Logger { get; set; }

        #endregion

        #region Constructor

        public TranscriptionJob()
        {
        }

        public TranscriptionJob(TranscriptionRequest request)
        {
            Request = request;
        }

        [ActivatorUtilitiesConstructor]
        public TranscriptionJob(ILogger<TranscriptionJob> logger, IQueuedJobStatusService queuedJobStatusService, ISbTranscriptionService transcriptionService)
        {
            Logger = logger;
            QueuedJobStatusService = queuedJobStatusService;
            TranscriptionService = transcriptionService;
        }

        #endregion

        #region JobBase Implementation

        /// <inheritdoc/>
        public override async Task Process()
        {
            try
            {
                await QueuedJobStatusService.UpdateStatusAsync(JobStatusRoute, JobStatusType.Processing);
                await TranscriptionService.ProcessTranscriptionRequest(Request);
                await QueuedJobStatusService.UpdateStatusAsync(JobStatusRoute, JobStatusType.Complete);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "TranscriptionJob failed to process a request.");
                await QueuedJobStatusService.UpdateStatusAsync(JobStatusRoute, JobStatusType.Failed, JsonUtils.ToLowerCamelJson(ex));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Populates the current instance with data from another instance.
        /// </summary>
        /// <param name="source"></param>
        public void Populate(TranscriptionJob source)
        {
            Request = source.Request;
            Id = source.Id;
            JobStatusRoute = source.JobStatusRoute;
        }

        #endregion

    }
}
