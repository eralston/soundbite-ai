using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Okta
{
    /// <summary>
    /// Organization synchronization strategy implementation for OKTA.
    /// </summary>
    public class OktaOrgSyncStrategy : OktaOrgSyncStrategyBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the time after which the sync auto-aborts to avoid an azure function timeout.
        /// </summary>
        private DateTime AbortTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the sync was aborted.
        /// </summary>
        private bool IsAborted { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the sync is being resumed and the resume point is
        /// currently being found.
        /// </summary>
        private bool IsFindingResumePoint { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="OktaOrgSyncStrategy"/> instance.
        /// </summary>
        /// <param name="logger">DI reference to a logger.</param>
        /// <param name="currentOrgConfig">DI reference to the organization sync settings.</param>
        /// <param name="oldOrgConfig">Optional reference to old config</param>
        /// <param name="syncStore">DI reference to a org synchronization store.</param>
        /// <param name="syncConfig">DI reference to the organization configuration defining the sync process for the org.</param>
        public OktaOrgSyncStrategy(
            IHttpClientFactory httpClientFactory,
            OrgSyncConfig currentOrgConfig,
            OrgSyncConfig oldOrgConfig,
            ILogger logger,
            IOrgSyncStore syncStore,
            IIdentityInfrastructure infrastructure)
            : base(httpClientFactory, currentOrgConfig, oldOrgConfig, logger, syncStore, infrastructure)
        {
            OldConfig = oldOrgConfig;
        }

        #endregion

        #region Overridden Methods

        /// <inheritdoc />
        public override async Task Sync(OrgSyncResult syncResults)
        {
            AbortTime = DateTime.Now.AddMilliseconds(Settings.MaxRunLength);
            OrgResult = syncResults;
            Logger.LogDebug($"Syncing organization '{OrgConfig.OrgRoute}' (SyncType: {Settings.SyncType})");

            try
            {
                // Determine whether this is resuming a partial run that failed or aborted.
                if (Settings.IsPartialSync)
                {
                    Logger.LogDebug($"Resuming partial synchronization");
                    IsFindingResumePoint = true;
                }
                else
                {
                    // Fresh sync so clear out values and set defaults
                    IsFindingResumePoint = false;
                    Settings.NextPage = null;
                    Settings.ModifiedDateFilterStart = Settings.MaxModifiedDateFound;
                    Settings.ModifiedDateFilterEnd = DateTime.Now.ToUniversalTime();
                    Settings.MaxModifiedDateFound = null;
                    Settings.MostRecentItems = new Queue<string>();
                }

                await Store.StartSync(OrgConfig, syncResults);
                await SyncUsers();
                await SyncGroups();

                if (IsAborted)
                {
                    Logger.LogDebug($"Sync ABORTED to avoid Azure Function timeout '{OrgConfig.OrgRoute}' (SyncType: {Settings.SyncType})");
                    Settings.IsPartialSync = true;
                }
                else
                {
                    // Use the max modified date as the "end" sync date/time to ensure no issue                    
                    Settings.ModifiedDateFilterEnd = Settings.MaxModifiedDateFound ?? Settings.ModifiedDateFilterEnd;
                    Settings.IsPartialSync = false;
                    Settings.MostRecentItems = null;
                    Settings.UserSyncComplete = false;
                    Settings.NextPage = null;
                }

                //TODO: do we want to make this configurable?
                await Store.SaveStrategyConfig(Settings);
                await Store.SetCommandTimeout(500);
                await Store.EndSync();
                if (OktaApi.TotalWaitCount > 0)
                {
                    Logger.LogWarning($"API throttled {OktaApi.TotalWaitCount} times for a total of {OktaApi.TotalWaitDuration.TotalSeconds} seconds.");
                }
                Logger.LogDebug($"Successfully synced organization '{OrgConfig.OrgRoute}' (SyncType: {Settings.SyncType})");
            }
            catch (Exception ex)
            {
                string msg = $"Failed to complete synchronization for organization (SyncType: {Settings.SyncType}) Error: {ex.Message} and Stack: {ex.StackTrace}";
                OrgResult.AddError(ex, "Error during synchronization process.");
                Logger.LogError(ex, msg);
            }
        }

        #endregion

        #region Methods - Synchronization

        /// <summary>
        /// Responsible for synchronizing users from the directory to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncUsers()
        {
            // Do not process users if this is a partial sync and the user sync is marked complete
            if (!Settings.IsPartialSync || !Settings.UserSyncComplete)
            {
                if (Settings.ImportAllUsers)
                {
                    await SyncAllUsers();
                }
                else if (Settings.Users?.Length > 0)
                {
                    await SyncConfiguredUsers(Settings.Users);
                }

                // Store off the fact the user sync is complete to avoid running it again during
                Settings.UserSyncComplete = !IsAborted;
                Settings.NextPage = IsAborted ? Settings.NextPage : null;
                await Store.SaveStrategyConfig(Settings);
            }
        }

        protected async Task<TokenPageResponse<OktaUserResponse>> GetUserSyncStartingPoint()
        {
            return await GetSyncStartingPoint(async (page, after) =>
                await OktaApi.GetUsers(page, Settings.ModifiedDateFilterStart, Settings.ModifiedDateFilterEnd, after),
                i => i.id,
                () => OrgResult.UserNetworkRequests++);
        }

        protected async Task<TokenPageResponse<OktaGroupResponse>> GetGroupSyncStartingPoint()
        {
            return await GetSyncStartingPoint(async (page, after) =>
                await OktaApi.GetGroups(page, Settings.ModifiedDateFilterStart, Settings.ModifiedDateFilterEnd, after),
                i => i.id,
                () => OrgResult.GroupNetworkRequests++);
        }

        protected async Task<TokenPageResponse<TItem>> GetSyncStartingPoint<TItem>(
            Func<TokenPageRequest, string, Task<TokenPageResponse<TItem>>> webApiCall,
            Func<TItem, string> getItemId,
            Action incWebApiCall)
        {
            TokenPageResponse<TItem> response = null;

            // Determine whether this is a fresh sync operation or one that is resuming
            if (!IsFindingResumePoint || Settings.MostRecentItems.Count == 0)
            {
                // Fresh sync so just call into things normally
                incWebApiCall();
                response = await webApiCall(null, null);
                Settings.NextPage = response.SkipToken;
            }
            else
            {
                // Attempt to resume the sync by running through the most recent IDs and using them
                // as the starting point for the query. The only issue is that if an item was
                // updated since the query, it falls out of the query and the API call fails.  So we
                // want to iterate over a series of them in the hopes that one works.  If none work
                // then we just restart the query entirely.
                bool done = false;

                // Reverse the queue so the most recent items are processed first
                Queue<string> queue = new Queue<string>(Settings.MostRecentItems.Reverse());

                while (!done)
                {
                    if (queue.TryDequeue(out string itemId))
                    {
                        try
                        {
                            response = await webApiCall(null, itemId);
                            done = true;
                            Logger.LogDebug($"Sync resumed successfully");
                        }
                        catch
                        {
                            // Failure implies the item was updated and is no longer in the query
                            // So just move on to the next item
                        }
                    }
                    else
                    {
                        // Attempted all items so just start over
                        Logger.LogDebug($"Sync cannot be resumed - starting from beginning.");
                        response = await webApiCall(null, null);
                        done = true;
                    }
                }
            }

            // Clear most recent items so it can start fresh
            Settings.MostRecentItems.Clear();

            return response;
        }

        /// <summary>
        /// Responsible for synchronizing all of the users in the directory to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncAllUsers()
        {
            try
            {
                Logger.LogDebug($"Syncing all users for organization '{OrgConfig.OrgRoute}' (SyncType: {Settings.SyncType})");
                bool done = false;
                OrgResult.UserNetworkRequests++;
                TokenPageResponse<OktaUserResponse> response = await GetUserSyncStartingPoint();
                while (!done)
                {
                    // Process all of the users in the page
                    foreach (OktaUserResponse directoryUser in response.Result)
                    {
                        await SyncUser(directoryUser);
                    }

                    // Determine if there are more pages to process
                    if (response.HasMorePages() && !ShouldSyncAbort())
                    {
                        OrgResult.UserNetworkRequests++;
                        response = await OktaApi.GetUsers(response.NextPage());

                        // Extra check to determine if the last skip token is the same as the new
                        // skip token.  In theory this should not happen but if it does it could
                        // lead to an endless loop
                        if (response.SkipToken == Settings.NextPage)
                        {
                            done = true;
                        }

                        // Update the last skip token with the current skip token and save the settings
                        Settings.NextPage = response.SkipToken;
                        await Store.SaveStrategyConfig(Settings);
                    }
                    else
                    {
                        done = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Failed to synchronize all users.";
                OrgResult.AddError(ex, msg);
                Logger.LogError(ex, msg);
            }
        }

        /// <summary>
        /// Responsible for synchronizing any users configured for synchronization to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <param name="usersToSync">List of the users configured for synchronization.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncConfiguredUsers(UserSyncConfig[] usersToSync)
        {
            foreach (UserSyncConfig userToSync in usersToSync)
            {
                try
                {
                    bool hasError = false;
                    if (userToSync == null)
                    {
                        OrgResult.AddError($"User configured for synchronization was null.");
                        hasError = true;
                    }
                    if (string.IsNullOrEmpty(userToSync.Id.Trim()))
                    {
                        OrgResult.AddError($"User configured for synchronization has no user ID.");
                        hasError = true;
                    }
                    if (!hasError)
                    {
                        await SyncConfiguredUser(userToSync);
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"Failed to synchronize configured user (User ID: {userToSync.Id})";
                    Logger.LogError(ex, msg);
                    OrgResult.AddError(ex, msg);
                }
            }
        }

        /// <summary>
        /// Responsible for synchronizing a single configured user to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <param name="userToSync"></param>
        /// <returns></returns>
        protected async Task SyncConfiguredUser(UserSyncConfig userToSync)
        {
            OktaUserResponse directoryUser;
            try
            {
                OrgResult.UserNetworkRequests++;
                directoryUser = await OktaApi.GetUser(userToSync.Id);
                await SyncUser(directoryUser);
            }
            catch (Exception ex)
            {
                string msg = $"Failed to sync configured user.";
                OrgResult.AddError(ex, msg);
                Logger.LogError(ex, msg);
            }
        }

        /// <summary>
        /// Responsible for synchronizing a single user from the third-party directory to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <param name="directoryUser">User to synchronize.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncUser(OktaUserResponse directoryUser)
        {
            if (directoryUser != null)
            {
                IUserFieldsWithAliases sbUser = ConvertToUserFields(directoryUser);
                bool isActive = IsActiveStatus(directoryUser.status);
                if (isActive)
                {
                    await Store.AddPerson(sbUser, SyncOpType.AddOrUpdate);
                    SyncedUsers.Add(directoryUser.id);
                }
                else
                {
                    await Store.RemovePerson(sbUser.Email);
                    SyncedUsers.RemoveWhere(i => i == directoryUser.id);
                }

                // Determine whether the last sync value should be updated
                if (Settings.MaxModifiedDateFound == null || Settings.MaxModifiedDateFound < directoryUser.lastUpdated)
                {
                    Settings.MaxModifiedDateFound = directoryUser.lastUpdated;
                }
            }
            else
            {
                OrgResult.AddError($"User configured for synchronization could not be found (UserID: {directoryUser.id})");
            }

            // Store in the most recent queue to use in the event of resuming a sync
            if (Settings.MostRecentItems.Count == 10)
            {
                Settings.MostRecentItems.Dequeue();
            }
            Settings.MostRecentItems.Enqueue(directoryUser.id);
        }

        /// <summary>
        /// Responsible for synchronizing groups from the directory to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncGroups()
        {
            if (Settings.GroupsMode != CollectionSyncMode.None && !ShouldSyncAbort())
            {
                if (Settings.ImportAllGroups)
                {
                    await SyncAllGroups();
                }
                else if (Settings?.Groups?.Length > 0)
                {
                    await SyncConfiguredGroups(Settings.Groups);
                }
            }
        }

        /// <summary>
        /// Responsible for synchronizing all groups in the directory to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncAllGroups()
        {
            try
            {
                OrgResult.UserNetworkRequests++;
                TokenPageResponse<OktaGroupResponse> response = await GetGroupSyncStartingPoint();

                // Clear out the User IDs from the list of most recent items. This needs to be done
                // after getting the starting point because this sometimes contains group IDs that
                // help identify where to resume a partial sync.
                Settings.MostRecentItems.Clear();

                bool done = false;
                while (!done)
                {
                    // Process all of the users in the page
                    foreach (OktaGroupResponse directoryGroup in response.Result)
                    {
                        //TODO: need to pass in Settings.GroupsMode instead of enum value
                        await SyncGroup(directoryGroup, MemberSyncOpType.SyncAllMembers);
                    }

                    // Determine if there are more pages to process
                    if (response.HasMorePages() && !ShouldSyncAbort())
                    {
                        OrgResult.UserNetworkRequests++;
                        response = await OktaApi.GetGroups(response.NextPage());

                        // Extra check to determine if the last skip token is the same as the new
                        // skip token.  In theory this should not happen but if it does it could
                        // lead to an endless loop
                        if (response.SkipToken == Settings.NextPage)
                        {
                            done = true;
                        }

                        // Update the last skip token with the current skip token and save the settings
                        Settings.NextPage = response.SkipToken;
                        await Store.SaveStrategyConfig(Settings);
                    }
                    else
                    {
                        done = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Failed to synchronize all groups.";
                OrgResult.AddError(ex, msg);
                Logger.LogError(ex, msg);
            }
        }

        /// <summary>
        /// Responsible for synchronizing any groups configured for synchronization to Soundbite.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <param name="groupsToSync">List of the groups configured for synchronization.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncConfiguredGroups(GroupSyncConfig[] groupsToSync)
        {
            if (groupsToSync == null)
            {
                return;
            }

            foreach (GroupSyncConfig groupToSync in groupsToSync)
            {
                OrgResult.GroupNetworkRequests++;
                OktaGroupResponse directoryGroup = await OktaApi.GetGroup(groupToSync.GroupId);
                if (directoryGroup != null)
                {
                    await SyncGroup(directoryGroup, groupToSync.MemberSyncType);
                }
            }
        }

        /// <summary>
        /// Responsible for synchronizing the specified group and any group members.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <param name="directoryGroup">Third-party directory group to synchronize.</param>
        /// <param name="memberSyncType">Specifies how members should be imported.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncGroup(OktaGroupResponse directoryGroup, MemberSyncOpType memberSyncType)
        {
            bool isValid = true;

            if (directoryGroup == null)
            {
                isValid = false;
                OrgResult.AddError("Cannot synchronize group because the group is null.");
            }

            if (string.IsNullOrEmpty(directoryGroup.id))
            {
                isValid = false;
                OrgResult.AddError("Cannot synchronize group because the ID is missing.");
            }

            if (directoryGroup.profile == null)
            {
                isValid = false;
                OrgResult.AddError("Cannot synchronize group because profile information is missing.");
            }

            if (string.IsNullOrEmpty(directoryGroup.profile.name))
            {
                isValid = false;
                OrgResult.AddError("Cannot synchronize group because name is missing.");
            }

            // Make sure we have a directory group to work with
            if (directoryGroup != null)
            {
                // Acquire the more recent of the group modified or group membership modified date
                DateTime mostRecentUpdateDate = directoryGroup.lastUpdated > directoryGroup.lastMembershipUpdated
                    ? directoryGroup.lastUpdated : directoryGroup.lastMembershipUpdated;

                // Determine whether the last sync value should be updated. Modified dates on an
                // invalid group are still valid modification dates which is why this check is done
                // outside the isValid group condition.
                if (Settings.MaxModifiedDateFound == null || Settings.MaxModifiedDateFound < mostRecentUpdateDate)
                {
                    Settings.MaxModifiedDateFound = mostRecentUpdateDate;
                }
            }

            if (isValid)
            {
                IGroupFields groupFields = new Group()
                {
                    UniversalId = directoryGroup.id,
                    Name = directoryGroup.profile.name,
                    Description = directoryGroup.profile.description
                };
                await Store.AddGroup(groupFields, SyncOpType.AddOrUpdate);
                await SyncGroupMembers(groupFields, memberSyncType);
            }
            else
            {
                OrgResult.AddError($"Cannot synchronize group because it is null.");
            }

            // Store in the most recent queue to use in the event of resuming a sync
            if (Settings.MostRecentItems.Count == 10)
            {
                Settings.MostRecentItems.Dequeue();
            }
            Settings.MostRecentItems.Enqueue(directoryGroup.id);
        }

        /// <summary>
        /// Responsible for synchronizing group members for the specified group.
        /// </summary>
        /// <param name="syncResults">Synchronization results instance used for registering statistics and log messages.</param>
        /// <param name="groupFields">Group with which members are associated.</param>
        /// <param name="memberSyncType">Specifies how members should be imported.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected async Task SyncGroupMembers(IGroupFields groupFields, MemberSyncOpType memberSyncType)
        {
            try
            {
                bool done = false;
                OrgResult.GroupNetworkRequests++;
                TokenPageResponse<OktaUserResponse> response = await OktaApi.GetGroupMembers(groupFields.UniversalId);
                string lastSkipToken = response.SkipToken;
                while (!done)
                {
                    // Process all of the users in the page
                    foreach (OktaUserResponse directoryUser in response.Result)
                    {
                        try
                        {
                            if (directoryUser != null)
                            {
                                IUserFieldsWithAliases userFields = ConvertToUserFields(directoryUser);

                                if (IsActiveStatus(directoryUser.status))
                                {
                                    await Store.AddMember(groupFields, userFields, memberSyncType);
                                }
                                else
                                {
                                    await Store.RemoveMember(groupFields.UniversalId, userFields.Email);
                                }
                            }
                            else
                            {
                                OrgResult.AddError("Failed to import user because the user was null.");
                            }
                        }
                        catch (UserSafeException usex)
                        {
                            OrgResult.AddError(usex);
                        }
                        catch (Exception ex)
                        {
                            OrgResult.AddError(ex, "Failed to import a group member.");
                        }
                    }

                    // Determine if there are more pages to process
                    if (!string.IsNullOrEmpty(response.SkipToken?.Trim()))
                    {
                        OrgResult.UserNetworkRequests++;
                        response = await OktaApi.GetUsers(response.NextPage());

                        // Extra check to determine if the last skip token is the same as the new
                        // skip token.  In theory this should not happen but if it does it could
                        // lead to an endless loop
                        if (response.SkipToken == lastSkipToken)
                        {
                            done = true;
                        }

                        // Update the last skip token with the current skip token
                        lastSkipToken = response.SkipToken;
                    }
                    else
                    {
                        done = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Failed to synchronize group members.";
                OrgResult.AddError(ex, msg);
                Logger.LogError(ex, msg);
            }
        }

        #endregion

        #region Methods - Helper

        /// <summary>
        /// Determines whether processes should stop becaues the abort time has been exceeded.
        /// </summary>
        /// <returns><c>true</c> if sync should be aborted, otherwise <c>false</c>.</returns>
        private bool ShouldSyncAbort()
        {
            bool result = DateTime.Now > AbortTime;
            IsAborted = IsAborted || result;
            return result;
        }

        /// <summary>
        /// Determines whether the user is active in Soundbite based on the OKTA status.
        /// </summary>
        /// <param name="status">Status of the user in the OKTA directory.</param>
        /// <returns><c>true</c> if the user is active otherwise <c>false</c>.</returns>
        private bool IsActiveStatus(string status)
        {
            return status.ToUpper() switch
            {
                "ACTIVE" => true,
                "STAGED" => true,
                "PROVISIONED" => true,
                "RECOVERY" => true,
                "PASSWORD EXPIRED" => true,
                _ => false
            };
        }

        /// <summary>
        /// Responsible for converting the directory specific data into an IUserFields instance that
        /// can be consumed by the directory synchronization process.
        /// </summary>
        /// <param name="directoryUser">Directory object to convert.</param>
        /// <returns>an <see cref="IUserFields"/> instance populated with directory user data.</returns>
        /// <exception cref="UserSafeException">Thrown if the user or user profile is null or the email address is missing.</exception>
        protected IUserFieldsWithAliases ConvertToUserFields(OktaUserResponse directoryUser)
        {
            if (directoryUser == null || directoryUser.profile == null)
            {
                throw new UserSafeException("Cannot import user because the user or user profile is missing.");
            }
            if (string.IsNullOrEmpty(directoryUser.profile.email.Trim()))
            {
                throw new UserSafeException("Cannot import user because the email address is missing.");
            }

            IUserFieldsWithAliases user = new UserWithAliases
            {
                UniversalId = directoryUser.id,
                Email = directoryUser.profile.email,
                Phone = directoryUser.profile.mobilePhone,
                GivenName = directoryUser.profile.firstName,
                FamilyName = directoryUser.profile.lastName
            };
            return user;
        }

        #endregion
    }
}