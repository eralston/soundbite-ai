using Azure.Messaging.EventGrid;
using Masticore;
using Masticore.Azure.MediaServices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Services;
using Soundbite.Services.Lifecycle;
using System;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Azure function that acts as the recipient of Azure Event Grid messages for Azure Media 
    /// Service job status changes.
    /// </summary>
    public class SbAzureMediaJobStatusChangedFunc
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IClipOperationService _clipOperationService;
        private readonly ISbInfrastructure _infrastructure;
        private readonly ILifecycleFactory _lifecycleFactory;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SbAzureMediaEventFunc"/> instance.
        /// </summary>
        public SbAzureMediaJobStatusChangedFunc(ILogger<SbAzureMediaJobStatusChangedFunc> logger,
            ILifecycleFactory lifecycleFactory,
            ISbInfrastructure infrastructure,
            IClipOperationService clipOperationService)
        {
            _logger = logger;
            _lifecycleFactory = lifecycleFactory;
            _infrastructure = infrastructure;
            _clipOperationService = clipOperationService;
        }

        #endregion

        #region Entry Point

        [FunctionName(nameof(SbAzureMediaJobStatusChangedFunc))]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            string externalId = $"{eventGridEvent.Topic}/{eventGridEvent.Subject}";
            ClipOperation clipOp = await _clipOperationService.ReadByExternalId(externalId);

            // Make sure we have a clip operation with which to work
            if (clipOp == null)
            {
                _logger.LogError($"No associated clip operation for external ID \"{externalId}\"");
                return;
            }

            // Make sure that the event contains useful data
            Validator.ArgNotNull(nameof(eventGridEvent), eventGridEvent);
            Validator.NotNullOrEmpty(nameof(eventGridEvent.Topic), eventGridEvent.Topic);
            Validator.NotNullOrEmpty(nameof(eventGridEvent.Subject), eventGridEvent.Subject);
            MediaJobChangedEvent data = JsonConvert.DeserializeObject<MediaJobChangedEvent>(eventGridEvent.Data.ToString());
            Validator.NotNull(data, "EventGridEvent data is null/missing.");

            ////////////////////////////////////////////////////////////////////////////////////////
            // NOTE: possible states per https://learn.microsoft.com/en-us/azure/event-grid/event-schema-media-services?tabs=event-grid-event-schema
            // - Queued (will only appear in PreviousState)
            // - Scheduled      - scheduled for execution but not started
            // - Processing     - procesing started but has not completed
            // - Canceling      - attempting to cancel (maybe error if it fails?)
            // - Canceled       - successful cancellation
            // - Error          - error occured
            // - Finished       - successful completion
            ////////////////////////////////////////////////////////////////////////////////////////

            // Process
            switch (data.State)
            {
                case "Cancelled":
                    // Denotes an error state
                    await SetClipOpStatus(eventGridEvent.Id, clipOp.Route, ClipOperationStateType.Error, "Clip operation was cancelled.");
                    break;
                case "Error":
                    // Denotes an error state
                    await SetClipOpStatus(eventGridEvent.Id, clipOp.Route, ClipOperationStateType.Error, "Clip operation was failed. See Azure/App Insights logs for more details.");
                    break;
                case "Finished":
                    await SetClipOpStatus(eventGridEvent.Id, clipOp.Route, ClipOperationStateType.Complete);
                    await ContinueLifecycle(clipOp.Route);
                    break;

                default:
                    // Unknown state
                    break;
            }
        }

        #endregion

        #region Methods

        private async Task SetClipOpStatus(string eventGridEventId, string clipOpRoute, ClipOperationStateType state, string errorDetails = null)
        {
            try
            {
                errorDetails = (state != ClipOperationStateType.Error) ? null
                    : $"{errorDetails} [Event Grid ID: {eventGridEventId}]";
                await _clipOperationService.SetState(clipOpRoute, state, errorDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update ClipOperation (route:{clipOpRoute}) to state ({state}).");
            }
        }

        private async Task ContinueLifecycle(string clipOpRoute)
        {
            try
            {
                SbDb db = await _infrastructure.DbAsync();
                SessionType sessionType = await db.ReadSessionTypeByClipOpRoute(clipOpRoute);
                ILifecycleStrategy lifeCycle = _lifecycleFactory.StrategyForSession(sessionType);
                await lifeCycle.AfterClipOperationComplete(db, clipOpRoute);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception encountered trying to continue session lifecycle.");
            }
        }

        #endregion
    }
}
