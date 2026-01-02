using Masticore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    public class TeamsChannelProcessor : IChannelProcessor
    {
        ////////////////////////////////////////////////////////////////////////////////////////////
        // NOTE: All data access and data updates within this service must occur within a lock
        //       or else there is distinct potential of issues from wonkiness to disaster...
        ////////////////////////////////////////////////////////////////////////////////////////////

        #region Constants

        public const int MaxWorkers = 10;
        public const int MaxNotificationsPerBatch = 2000;
        public const int MaxRequestsPerSecondPerClientTenant = 5;
        public const int MaxRequestsPerSecondPerSbTenant = 50;
        public const int MaxTokenDurationBeforeRefreshInMs = 1000 * 60 * 45; // 45 minutes
        public const int MinApiCallGap = 1000;  // 1 second
        public const int MaxTeamsNotificationRetries = 3;

        #endregion

        #region Fields

        INotificationProcessingService _notificationProcessingService;

        #endregion

        #region Properties - DI

        private ILogger Logger { get; set; }
        private ISbInfrastructure SbInfrastructure { get; set; }
        private TeamsAppAzureSettings TeamsAppAzureSettings { get; set; }
        private ITeamsGraphService TeamsGraphService { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Static flag used to ensure that two TeamsChannelProcessor instances are not running
        /// at the same time in a single application space.
        /// </summary>
        private static bool IsProcessing { get; set; } = false;

        /// <summary>
        /// Gets a LockService that uses the current SbDb instance as a locking mechanism to help
        /// syncronize database calls between various tasks that are all operating simultaenously
        /// on the database.  All database retrieval / modification should happen within the context
        /// of a lock to avoid problems.
        /// </summary>
        private LockService DbLock { get; }

        /// <summary>
        /// Gets a refrence to the current TeamsChannelProcessor state.
        /// </summary>
        private ChannelProccessorState State { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="TeamsChannelProcessor"/> instance.
        /// </summary>
        /// <param name="logger">DI reference to a <see cref="ILogger"/> instance.</param>
        /// <param name="sbInfrastructure">DI reference to <see cref="ISbInfrastructure"/> instance.</param>
        /// <param name="teamsAppAzureSettings">DI reference to a <see cref="TeamsAppAzureSettings"/> instance.</param>
        /// <param name="teamsGraphService">DI reference to a <see cref="ITeamsGraphService"/> instance.</param>
        public TeamsChannelProcessor(ILogger<TeamsChannelProcessor> logger, ISbInfrastructure sbInfrastructure, TeamsAppAzureSettings teamsAppAzureSettings, ITeamsGraphService teamsGraphService)
        {
            Logger = logger;
            SbInfrastructure = sbInfrastructure;
            DbLock = new LockService(async () => await SbInfrastructure.DbAsync());
            TeamsAppAzureSettings = teamsAppAzureSettings;
            TeamsGraphService = teamsGraphService;
            State = new ChannelProccessorState() { Channel = NotificationChannel.Teams };
        }

        #endregion

        #region IChannelProcessor Implementation

        /// <inheritdoc />
        public INotificationProcessingService NotificationProcessingService
        {
            get
            {
                if (_notificationProcessingService == null)
                {
                    throw new Exception("NotificationProcessingService property on the TeamsChannelProcessor is null.  This value is normally set when created via the factory but if created manually the value must be set manually.");
                }
                return _notificationProcessingService;
            }
            set => _notificationProcessingService = value;
        }

        /// <inheritdoc />
        public async Task ProcessMessages()
        {
            // Only allow one process notifications to be running at any given time within an application space.            
            bool isAlreadyRunning = DbLock.Lock(() =>
            {
                if (IsProcessing)
                {
                    return true;
                }
                else
                {
                    IsProcessing = true;
                    return false;
                }
            });

            // Do not continue if the process is already running
            if (isAlreadyRunning)
            {
                Logger.LogWarning("[TeamsChannelProcessor] - exiting because it is already running.");
                return;
            }

            Logger.LogInformation("[TeamsChannelProcessor] - processing messages.");

            // Determine if there are any messages to handle... (only dealing with Teams for now)
            if (await NotificationProcessingService.HasMessagesToSend(NotificationChannel.Teams))
            {
                // Establish a new NotificationProcessingState context
                State.MaxWorkers = await GetMaxWorkers();

                while (!State.Complete)
                {
                    await EnsureMaxWorkers(State);
                    Task[] allTasks = State.RunningWorkers;
                    if (State.NoMoreOrgIdsToProcess && allTasks.Length == 0)
                    {
                        // Nothing left to process so exit loop
                        State.Complete = true;
                    }
                    else
                    {
                        // Wait for something to complete and then continue
                        Task.WaitAny(allTasks);
                        await InspectWorkers(State);
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an integer value specifying the maximum number of concurrent "workers" allowed to run.
        /// </summary>
        /// <returns>an integer value specifying the maximum number of concurrent workers allowed to run.</returns>
        private Task<int> GetMaxWorkers()
        {
            int result = Convert.ToInt32(Math.Floor(Convert.ToDecimal(MaxRequestsPerSecondPerSbTenant / MaxRequestsPerSecondPerClientTenant)));
            if (result <= 0)
            {
                throw new Exception("TeamsChannelProcessor failed because GetMaxWorker returned a count of zero (or less).");
            }
            Logger.LogInformation($"[TeamsChannelProcessor] - determined maximum of {result} workers.");
            return Task.FromResult(result);
        }

        /// <summary>
        /// There is a calculated maximum number of workers that are allowed to run at a time. This
        /// method is responsibele creating additional workers until that capacity is reached or all
        /// available work is divied out appropriately.
        /// <param name="state"></param>
        /// <returns></returns>
        private async Task EnsureMaxWorkers(ChannelProccessorState state)
        {
            // Determine if the maximum number of processes has been reached
            if (state.MaxWorkers > state.Workers.Count)
            {
                // Add more processes
                IEnumerable<int> orgIds = await NotificationProcessingService.UniqueOrgIdsWithMessages(state.Channel, state.MaxWorkers, state.RunningOrgIds);
                state.NoMoreOrgIdsToProcess = orgIds.Count() == 0;
                foreach (int orgId in orgIds)
                {
                    // NOTE: we could check if the organization supports teams messages at this
                    //   point in the code. However, we do not add a SessionNotification entry for
                    //   teams unless teams is enabled. In the future if we want a double-check then
                    //   this is where it should be looked up, and any messages that are processed
                    //   should be marked as complete but with some not about how Teams messages are
                    //   turned off at the organization level (which would occurs elsewhere in code)

                    Logger.LogInformation($"[TeamsChannelProcessor] - adding worker for Org (OrgID:{orgId})");

                    WorkerState worker = new WorkerState()
                    {
                        OrgId = orgId,
                        RequestRate = MaxRequestsPerSecondPerClientTenant
                    };

                    worker.OrgRoute = DbLock.LockAsync(async () =>
                    {
                        SbDb db = await SbInfrastructure.DbAsync();
                        string orgRoute;
                        try
                        {
                            orgRoute = db.Organizations
                                .Where(i => i.Id == orgId)
                                .Select(i => i.Route)
                                .First();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("TeamsChannelProcessor failed to lookup organization route.", ex);
                        }
                        return orgRoute;
                    });

                    state.Workers.TryAdd(orgId, worker);
                    await GetMessagesAndProcessWorkload(state, worker);
                }
            }
        }

        /// <summary>
        /// Retrieves a new "batch" of messages for the worker to process and starts the task the
        /// is responsible for processing the messages.
        /// </summary>
        /// <param name="state">Reference to the parent Channel Processor state information.</param>
        /// <param name="worker">Reference to the worker state to populate with additional work.</param>
        /// <returns></returns>
        private async Task GetMessagesAndProcessWorkload(ChannelProccessorState state, WorkerState worker)
        {
            Logger.LogInformation($"[TeamsChannelProcessor] - getting new workload for worker (OrgID:{worker.OrgId})");
            worker.MessagesToProcess = (await NotificationProcessingService.GetMessagesForProcessing(worker.OrgId, state.Channel, MaxNotificationsPerBatch)).ToList();
            int msgCount = worker.MessagesToProcess.Count();
            if (msgCount == 0)
            {
                OnWorkerCompleted(state, worker);
            }
            else
            {
                worker.MaxedOutMessageRetrieval = msgCount == MaxNotificationsPerBatch;
                worker.ProcessRef = ProcessWorkload(worker);
            }
        }

        /// <summary>
        /// Processes the work assigned to the worker.
        /// </summary>
        /// <param name="worker">Worker state information containing work to process.</param>
        /// <returns>a task representing the operation.</returns>
        private async Task ProcessWorkload(WorkerState worker)
        {
            Logger.LogInformation($"[TeamsChannelProcessor] - worker processing {worker.MessagesToProcess.Count} notifications (OrgRoute:{worker.OrgRoute})");
            worker.Status = "Processing";

            // Setup a variable to help track the requests per second.  Ideally the requests per second 
            // will be equal to the MaxRequestsPerSecondPerClientTenant but it can be reduced if any
            // throttling is detected.
            bool complete = false;

            int lastSessionId = 0;

            // Stores any SessionNotificationEntity instances that do not have cooresponding session information
            List<SessionNotificationToProcess> toMarkFailed = new List<SessionNotificationToProcess>();
            List<SessionNotificationToProcess> toMarkProcessing = new List<SessionNotificationToProcess>();

            // NOTE: incomming notifications should already be ordered by SessionID
            while (!complete)
            {
                // Break off a chunk of items to process
                int nextRequestItemCount = worker.MessagesToProcess.Count > worker.RequestRate
                    ? worker.RequestRate
                    : worker.MessagesToProcess.Count;
                List<SessionNotificationToProcess> nextRequestItems = worker.MessagesToProcess.GetRange(0, nextRequestItemCount);
                List<TeamsAppBatchNotificationItem> requests = new List<TeamsAppBatchNotificationItem>();
                TeamsAppBatchNotificationItem currentRequest = null;
                bool itemFailed;

                Logger.LogInformation($"[TeamsChannelProcessor] - worker batching {nextRequestItemCount} notifications (OrgRoute:{worker.OrgRoute})");

                // Process each individual item for the current request
                foreach (SessionNotificationToProcess itemToProcess in nextRequestItems)
                {
                    // Reset failure flag
                    itemFailed = false;
                    bool hasAltTeamsUpn = false;

                    // Make sure the session info is current for the current item
                    EnsureSessionInfo(requests, itemToProcess, ref lastSessionId, ref currentRequest);

                    // Acquire the user information to determine if they have an alternative UPN for teams
                    UserSettings userSettings = null;
                    if (!string.IsNullOrEmpty(itemToProcess.UserConfigJson))
                    {
                        try
                        {
                            userSettings = JsonUtils.FromLowerCamelJson<UserSettings>(itemToProcess.UserConfigJson);
                            hasAltTeamsUpn = !string.IsNullOrEmpty(userSettings?.MsTeams?.AltId);
                        }
                        catch (Exception ex)
                        {
                            itemFailed = true;
                            SetFailureInfo(itemToProcess, new Exception("Failed to parse user configuration settings from JSON.", ex), false, false, false);
                            toMarkFailed.Add(itemToProcess);
                        }
                    }

                    if (!itemFailed)
                    {
                        currentRequest.Recipients.Add(new TeamsAppBatchNotificationRecipient()
                        {
                            SessionNotificationId = itemToProcess.SessionNotificationId,
                            Upn = hasAltTeamsUpn ? userSettings.MsTeams.AltId : itemToProcess.Email
                        });
                        toMarkProcessing.Add(itemToProcess);
                    }
                }

                Logger.LogInformation($"[TeamsChannelProcessor] - worker updating statuses to 'procssing' (OrgRoute:{worker.OrgRoute})");
                UpdateSessionNotificationStatus(toMarkProcessing, toMarkFailed);

                // Ensure we have a valid graph API token for the organization
                if (string.IsNullOrEmpty(worker.OrgGraphToken) || IsTokenRefreshRequired(worker.OrgGraphTokenRecievedAt))
                {
                    Logger.LogInformation($"[TeamsChannelProcessor] - worker acquiring graph token for org (OrgRoute:{worker.OrgRoute})");
                    worker.OrgGraphToken = await TeamsGraphService.GetTeamsAppToken(worker.OrgRoute);
                }

                // We are simulating the call to azure here and doing all the work that azure
                // would do but it is occuring in-process instead of externally. Running
                // this code in an azure function should scale better than running it all here.
                Logger.LogInformation($"[TeamsChannelProcessor] - worker sending Notifications via 'simulated' Azure Function call (OrgRoute:{worker.OrgRoute})");

                await EnsureApiCallDelay(worker);
                DateTime? delayUntil = await SimulateAzureFunctionWork(worker.OrgRoute, worker.OrgGraphToken, requests);

                // When the delayUntil value is set it means throttling has occured
                if (delayUntil != null)
                {
                    Logger.LogWarning($"[TeamsChannelProcessor] - worker throttled by Graph API (OrgRoute:{worker.OrgRoute})");

                    // Turn down request limit
                    ReduceRequestRate(worker);

                    // Wait the specified amount of time
                    TimeSpan delay = delayUntil.Value - DateTime.UtcNow;
                    if (delay.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delay);
                    }
                }

                // Remove the items from list of items to process
                worker.MessagesToProcess.RemoveRange(0, nextRequestItemCount);

                // Determine if the process is complete
                complete = worker.MessagesToProcess.Count == 0;
            }
        }

        /// <summary>
        /// Ensures that the minimum duration of time has passed since the worker's last API call.
        /// This is for general application operation. Throttling from Graph is handled elsewhere.
        /// </summary>
        /// <param name="worker">Worker whose API calls are being monitored.</param>
        /// <returns>a task on which the caller can wait to ensure the appropriate delay has been observed.</returns>
        private async Task EnsureApiCallDelay(WorkerState worker)
        {
            // Determine the gap between now and the last API call
            TimeSpan apiCallGap = DateTime.UtcNow - worker.LastApiCall;

            // Determine if the gap is big enough
            if (apiCallGap.TotalMilliseconds < MinApiCallGap)
            {
                // Wait the difference between the actual and required gap
                await Task.Delay(Convert.ToInt32(MinApiCallGap - apiCallGap.TotalMilliseconds));
            }
        }

        /// <summary>
        /// In theory the SessionNotificationEntity entries are sorted by session ID so when 
        /// processing messages there is a boundray when the session changes from one session to
        /// another session.  This detects that boundary by checking the session ID to see if the
        /// previous ID matches the current ID.  If so, it keeps the current info.  If not, it
        /// updates the information for the next set of messages.
        /// </summary>
        /// <param name="itemToProcess">The current item being processed.</param>
        /// <param name="lastSessionId">the ID of the session information that is currently loaded.</param>
        /// <param name="currentSessionInfo">Current session information that needs to be updated if the <paramref name="lastSessionId"/> changed.</param>
        private void EnsureSessionInfo(List<TeamsAppBatchNotificationItem> requests, SessionNotificationToProcess itemToProcess, ref int lastSessionId, ref TeamsAppBatchNotificationItem currentSessionInfo)
        {
            if (currentSessionInfo == null || lastSessionId != itemToProcess.SessionId)
            {
                // Determine whether the session was found
                currentSessionInfo = new TeamsAppBatchNotificationItem();
                requests.Add(currentSessionInfo);
                currentSessionInfo.Title = itemToProcess.Title;
                currentSessionInfo.Author = string.IsNullOrEmpty(itemToProcess.Author)
                    ? itemToProcess.OrgName
                    : itemToProcess.Author;
                currentSessionInfo.Url = TeamsAppAzureSettings.DeepLinkForPlay(itemToProcess.SessionRoute);

                // Make sure to update the last session ID
                lastSessionId = itemToProcess.SessionId;
            }
        }

        /// <summary>
        /// Responsible for marking items as processing / failed before they are sent off for processing.
        /// </summary>
        /// <param name="toMarkProcessing">Items whose status is set to processing.</param>
        /// <param name="toMarkFailed">Items whose status are set to retry/fail depending on failure rate.</param>
        private void UpdateSessionNotificationStatus(List<SessionNotificationToProcess> toMarkProcessing, List<SessionNotificationToProcess> toMarkFailed)
        {
            int[] idListProcessing = toMarkProcessing.Select(i => i.SessionNotificationId).ToArray();
            int[] idListFailed = toMarkFailed.Select(i => i.SessionNotificationId).ToArray();

            DbLock.LockAsync(async () =>
            {
                SbDb db = await SbInfrastructure.DbAsync();
                IEnumerable<SessionNotificationEntity> toMarkProcessingEntities = (idListProcessing?.Length >= 0 == true)
                    ? await db.SessionNotifications.Where(i => idListProcessing.Contains(i.Id)).ToListAsync()
                    : Enumerable.Empty<SessionNotificationEntity>();
                IEnumerable<SessionNotificationEntity> toMarkFailedEntities = (idListFailed?.Length >= 0 == true)
                    ? await db.SessionNotifications.Where(i => idListFailed.Contains(i.Id)).ToListAsync()
                    : Enumerable.Empty<SessionNotificationEntity>();

                // Update items for processing
                foreach (SessionNotificationToProcess item in toMarkProcessing)
                {
                    try
                    {
                        SessionNotificationEntity entity = toMarkProcessingEntities.First(i => i.Id == item.SessionNotificationId);
                        entity.Status = NotificationStatus.Processing;
                        entity.UpdatedUtc = DateTime.UtcNow;
                        db.SessionNotifications.Update(entity);
                    }
                    catch
                    {
                        // Do nothing.  In theory the list that retrieves the entities should have
                        // been created shortly before this is called leaving very little time for
                        // anything to happen to the record in the interim. If something goes wrong
                        // and it is not found, then it *may* get updated to complete and have no
                        // issue if it was something wonky. If there is truely an issue with it not
                        // being found it is unlikely to be picked up a second time to create a
                        // second message, but if multiple messages are going through then this
                        // may be something to look at.
                    }
                }

                // Update items that failed
                foreach (SessionNotificationToProcess item in toMarkFailed)
                {
                    try
                    {
                        SessionNotificationEntity entity = toMarkProcessingEntities.First(i => i.Id == item.SessionNotificationId);
                        IList<SessionNotificationStatus> statusList = GetStatusList(entity.StatusJson);
                        entity.Status = GetFailureStatus(statusList);
                        entity.StatusJson = item.StatusJson;
                        entity.UpdatedUtc = DateTime.UtcNow;
                        db.SessionNotifications.Update(entity);
                    }
                    catch
                    {
                        // Do nothing. See note above. The only issue here is that it may keep
                        // failing over and over again without hitting retry max to finally get
                        // marked as actually failed.
                    }
                }

                await db.SaveChangesAsync();
            });
        }

        /// <summary>
        /// Inspects the each worker to determine if the worker has completed its workload.
        /// </summary>
        /// <param name="state">State containing list of workers whose status needs to be checked.</param>
        /// <returns>a Task representing the operation.</returns>
        private async Task InspectWorkers(ChannelProccessorState state)
        {
            // Iterate over each worker
            foreach (WorkerState worker in state.Workers.Select(i => i.Value))
            {
                // Determine if they have run out of work
                if (worker.MessagesToProcess.Count() == 0)
                {
                    // Determine if they maxed out their batch retrieval.  If so it implies there
                    // is likely more work for them to do.  If not, it means all the work that was
                    // assigned has been completed.  If additional work has appeared in the interim
                    // it will be picked up during the next execution cycle.
                    if (worker.MaxedOutMessageRetrieval)
                    {
                        // Assume there is more work
                        await GetMessagesAndProcessWorkload(state, worker);
                    }
                    else
                    {
                        /// Assume all the work is complete
                        OnWorkerCompleted(state, worker);
                    }
                }

                // Check for abnormal exit
                InspectWorkerTask(state, worker);
            }
        }

        /// <summary>
        /// Responsible for inspecting the task associated with a worker for abnormal termination.
        /// </summary>
        /// <param name="state">State containing list of active workers.</param>
        /// <param name="worker">Reference to the worker to inspect.</param>
        private void InspectWorkerTask(ChannelProccessorState state, WorkerState worker)
        {
            if (worker.ProcessRef != null)
            {
                bool abnormalExit = worker.ProcessRef.IsFaulted
                    || worker.ProcessRef.IsCanceled
                    || worker.ProcessRef.IsCompleted
                    || worker.ProcessRef.IsCompletedSuccessfully;

                // If the worker is not complete then verify that it has not stopped or failed
                if (abnormalExit && worker.Status != "Complete")
                {
                    Logger.LogWarning($"[TeamsChannelProcessor] - worker aborted abnormally - task status = '{worker.ProcessRef.Status}' (OrgRoute:{worker.OrgRoute})");
                    OnWorkerFailed(state, worker);
                }
            }
        }

        /// <summary>
        /// Marks the specified worker as complete and removes it from the list of active workers.
        /// </summary>
        /// <param name="state">State containing list of active workers.</param>
        /// <param name="worker">Worker that ran to completion.</param>
        private void OnWorkerCompleted(ChannelProccessorState state, WorkerState worker)
        {
            Logger.LogInformation($"[TeamsChannelProcessor] - worker completed all workloads (OrgRoute:{worker.OrgRoute})");
            worker.Status = "Complete";
            worker.ProcessRef = null;
            state.Workers.TryRemove(worker.OrgId, out _);
        }

        /// <summary>
        /// Marks the specified worker as failed and removes it from the list of active workers.
        /// </summary>
        /// <param name="state">State containing list of active workers.</param>
        /// <param name="worker">Worker that ran to completion.</param>
        private void OnWorkerFailed(ChannelProccessorState state, WorkerState worker)
        {
            Logger.LogInformation($"[TeamsChannelProcessor] - worker failed (OrgRoute:{worker.OrgRoute})");
            worker.Status = "Failed";
            worker.ProcessRef = null;
            state.Workers.TryRemove(worker.OrgId, out _);
        }

        /// <summary>
        /// Responsible for setting failure state information for a Session Notification.
        /// </summary>
        /// <param name="item">Item that failed</param>
        /// <param name="ex">Exception associated with the failure.</param>
        /// <param name="throttled">Flag indicating the failure was due to throttling.</param>
        /// <param name="hasTeamsUpn">Flag indicating that an alterante Teams UPN was in use.</param>
        /// <param name="userNotFound">Flag indicating the failure was due to the UPN not being found.</param>
        private void SetFailureInfo(SessionNotificationToProcess item, Exception ex, bool throttled, bool hasTeamsUpn, bool userNotFound)
        {
            item.Status.LastException = new ExceptionInfo(ex);
            item.Status.Throttled = throttled;
            item.Status.UserNotFound = userNotFound;
            item.Status.UsedAltUpn = hasTeamsUpn;
        }

        /// <summary>
        /// Determines whether the organization graph API token needs to be refreshed.
        /// </summary>
        /// <param name="tokenReceivedAt">DateTime at which the original token was acquired.</param>
        /// <returns><c>true</c> if the token needs to be refreshed, otherwise <c>false</c></returns>
        private bool IsTokenRefreshRequired(DateTime tokenReceivedAt)
        {
            DateTime tokenRefreshAt = tokenReceivedAt.AddMilliseconds(MaxTokenDurationBeforeRefreshInMs);
            bool result = DateTime.UtcNow > tokenRefreshAt;
            return result;
        }

        /// <summary>
        /// Responsible for reducing the request rate down by an amount when throttling occurs.
        /// </summary>
        /// <param name="worker">State whose request rate should be reduced</param>
        private void ReduceRequestRate(WorkerState worker)
        {
            // Decreate the rate by 10% but always ensure at least two calls are going through at a time...
            worker.RequestRate = Math.Max(2, Convert.ToInt32(Math.Floor(worker.RequestRate * 0.9)));
        }

        #endregion

        #region Methods - Simulate Azure Function

        //NOTE: These methods will in theory need to move to the azure function when implemented.

        /// <summary>
        /// Simulates what should ultimately be occuring within an azure function. There
        /// was not enough time to figure out authentication, etc.
        /// </summary>
        /// <returns>a Task representing the operation.</returns>
        private async Task<DateTime?> SimulateAzureFunctionWork(string orgRoute, string orgGraphToken, IEnumerable<TeamsAppBatchNotificationItem> requests)
        {

            DateTime? delayUntil = null;

            // Call the Graph API
            try
            {
                IDictionary<string, HttpResponseMessage> responses = await TeamsGraphService.SendTeamsAppBatchNotification(orgRoute, requests, orgGraphToken);
                delayUntil = UpdateFromResponses(requests, responses);
            }
            catch (Exception ex)
            {
                // There was a failure calling the graph service.
                MarkAllAsFailedDueToApiError(requests, ex);
            }

            return delayUntil;
        }

        private DateTime? UpdateFromResponses(IEnumerable<TeamsAppBatchNotificationItem> requests, IDictionary<string, HttpResponseMessage> responses)
        {
            DateTime? delayUntil = null;

            // Build a dictionary of recipients for easy location
            IDictionary<string, TeamsAppBatchNotificationRecipient> recipientDict =
                requests.SelectMany(req => req.Recipients)
                    .ToDictionary((i) => i.SessionNotificationId.ToString());

            // Build out an array containing all of the session notifications that need to be updated...
            int[] sessionNotificationIds = recipientDict.Select(i => i.Value.SessionNotificationId).ToArray();

            // Acquire all of the session notification records associated with the requests
            DbLock.LockAsync(async () =>
            {
                SbDb db = await SbInfrastructure.DbAsync();
                IDictionary<int, SessionNotificationEntity> sessionNotifications = await db.SessionNotifications
                    .Where(i => sessionNotificationIds.Contains(i.Id))
                    .ToDictionaryAsync(i => i.Id);

                // Iterate over all of the repsonses
                foreach (KeyValuePair<string, HttpResponseMessage> response in responses)
                {
                    // Match up the response to the request / recipient
                    TeamsAppBatchNotificationRecipient recipient = recipientDict[response.Key];
                    if (recipient != null)
                    {
                        // Locate the session notification associated with the recipient
                        SessionNotificationEntity sessionNotification = sessionNotifications[recipient.SessionNotificationId];
                        if (sessionNotification != null)
                        {
                            TimeSpan? delay = await UpdateSessionNotificationForResponse(sessionNotification, response.Value);
                            if (delay != null)
                            {
                                DateTime delayDate = DateTime.UtcNow + delay.Value;
                                if (delayUntil == null || delayUntil < delayDate)
                                {
                                    delayUntil = delayDate;
                                }
                            }
                            db.SessionNotifications.Update(sessionNotification);
                        }
                    }
                }

                // Save Updates
                await db.SaveChangesAsync();
            });

            return delayUntil;
        }

        private void MarkAllAsFailedDueToApiError(IEnumerable<TeamsAppBatchNotificationItem> requests, Exception ex)
        {
            // Build out an array containing all of the session notifications that need to be updated...
            int[] sessionNotificationIds = requests
                    .SelectMany(req => req.Recipients)
                    .Select(i => i.SessionNotificationId)
                    .ToArray();

            // Acquire all of the session notification records associated with the requests
            DbLock.LockAsync(async () =>
            {
                SbDb db = await SbInfrastructure.DbAsync();

                IList<SessionNotificationEntity> sessionNotifications = await db.SessionNotifications
                    .Where(i => sessionNotificationIds.Contains(i.Id))
                    .ToListAsync();

                // Iterate over all of the session notifications
                foreach (SessionNotificationEntity item in sessionNotifications)
                {
                    IList<SessionNotificationStatus> statusList = GetStatusList(item.StatusJson);
                    SessionNotificationStatus statusInfo = new SessionNotificationStatus();
                    statusList.Add(statusInfo);
                    statusInfo.Info = ExceptionInfo.SerializeExOrExInfo(ex);
                    item.StatusJson = statusList.ToLowerCamelJson();
                    item.Status = GetFailureStatus(statusList);
                }

                // Save Updates
                await db.SaveChangesAsync();
            });
        }

        /// <summary>
        /// Updates the session notification based on the HTTP response from Graph API.
        /// </summary>
        /// <param name="sessionNotification"></param>
        /// <param name="response"></param>
        private async Task<TimeSpan?> UpdateSessionNotificationForResponse(SessionNotificationEntity sessionNotification, HttpResponseMessage response)
        {
            TimeSpan? delay = null;

            // A NoContent (204) response indicates the notification was recieved without incident.
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                sessionNotification.Status = NotificationStatus.Success;
                IList<SessionNotificationStatus> statusList = GetStatusList(sessionNotification.StatusJson);
                SessionNotificationStatus statusInfo = new SessionNotificationStatus();
                statusList.Add(statusInfo);
                statusInfo.Info = "HTTP 204";
                sessionNotification.StatusJson = statusList.ToLowerCamelJson();
            }
            else
            {
                IList<SessionNotificationStatus> statusList = GetStatusList(sessionNotification.StatusJson);
                SessionNotificationStatus statusInfo = new SessionNotificationStatus();
                statusList.Add(statusInfo);
                statusInfo.Info = $"(HttpStatus: {(int)response.StatusCode}) " + await response.Content.ReadAsStringAsync();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        // Occurs when the specifies UPN is not found.  We have been using email as
                        // the main user identifier but the UPN is sometimes different for users. 
                        // When this is true, we need to acquire the UPN and set it in the AltId for
                        // teams settings in the user configuration.  If the AltId is already set
                        // then this error will require further investigation.
                        statusInfo.UserNotFound = true;
                        Logger.LogWarning("[Teams Notifications] - User Not Found");
                        break;
                    case HttpStatusCode.TooManyRequests:
                        // Graph has detected too many requests and has requested the API to backoff.
                        Logger.LogWarning(response.Headers.RetryAfter.Delta == null
                            ? "[Teams Notifications] - Graph API sent throttling warning (HTTP 429) - No Retry-After header present"
                            : $"[Teams Notifications] - Graph API sent throttling warning (HTTP 429) - Retry-After {response.Headers.RetryAfter.Delta.Value.TotalMilliseconds} ms");
                        delay = response.Headers.RetryAfter.Delta ?? new TimeSpan(0, 0, 2);
                        statusInfo.Throttled = true;
                        break;
                }

                // Determine if the status is retry or fail
                sessionNotification.Status = GetFailureStatus(statusList);

                // Make sure status information is persisted back to JSON for DB update
                sessionNotification.StatusJson = statusList.ToLowerCamelJson();

            }

            // Return the throttling delay (it may or may not have been set)
            return delay;
        }

        /// <summary>
        /// Responsible for retrieving the status list from the specified JSON or creating a new
        /// status list if the JSON is empty.
        /// </summary>
        /// <param name="json">JSON containing the serialized status list.</param>
        /// <returns>a list of SessionNotificationStatus entries (if any) ready for use.</returns>
        private IList<SessionNotificationStatus> GetStatusList(string statusListJson)
        {
            IList<SessionNotificationStatus> result = null;
            if (!string.IsNullOrEmpty(statusListJson))
            {
                try
                {
                    result = JsonUtils.FromLowerCamelJson<IList<SessionNotificationStatus>>(statusListJson);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "TeamsChannelProcessor GetStatusList JSON parsing failed.");
                    // WARNING - the assumption here is that something not crazy happened and
                    //   that the JSON was malformed because of a change in schema that is a
                    //   one time thing and not a recurring thing.  If it turns out it is a
                    //   recurring thing, then this will technically put the process into an
                    //   infinite loop because we use the count of the list that is constructed
                    //   here to fail after a number (currently 3) attempts. I do not expect
                    //   that to happen, but if you are seeing that happen this may be issue.
                }
            }

            // Build a list if one does not already exist
            result = result ?? new List<SessionNotificationStatus>();
            return result;
        }

        /// <summary>
        /// Determines whether a notification should be retried or failed.
        /// </summary>
        /// <param name="statusInfo">Notification status information.</param>
        /// <returns>an appropriate NotificationStatus value for the item in question.</returns>
        private NotificationStatus GetFailureStatus(IList<SessionNotificationStatus> statusList)
        {
            NotificationStatus result = statusList.Count >= MaxTeamsNotificationRetries
                            ? NotificationStatus.Failed
                            : NotificationStatus.Retry;
            return result;
        }

        #endregion
    }
}