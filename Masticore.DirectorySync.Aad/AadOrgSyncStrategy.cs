using Masticore.Ad;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Aad
{

    /// <summary>
    /// AAD implementation of the <see cref="IOrgSyncStrategy"/> interface.
    /// </summary>
    public class AadOrgSyncStrategy : AadOrgSyncStrategyBase
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="AadOrgSyncStrategy"/> instance.
        /// </summary>
        public AadOrgSyncStrategy(
            ILogger logger,
            OrgSyncConfig config,
            IGraphFactory graph,
            IOrgSyncStore syncStore,
            IIdentityInfrastructure infrastructure
            ) :
            base(logger, syncStore, config, graph, infrastructure)
        {
        }

        #endregion

        #region IOrgSyncStrategy Implementation        

        /// <inheritdoc />
        public override async Task Sync(OrgSyncResult orgResult)
        {
            Logger.LogDebug($"Syncing organization '{OrgConfig.OrgRoute}' with App ID '{Settings.GetAppId()}' targeting tenant '{Settings.TenantId}'");
            try
            {
                OrgResult = orgResult;
                await Store.StartSync(OrgConfig, OrgResult);

                await SyncUsers();

                await SyncGroups();

                await Store.EndSync();

                Logger.LogInformation($"Successfully synced organization '{OrgConfig.OrgRoute}' with App ID '{Settings.GetAppId()}' targeting tenant '{Settings.TenantId}'");
            }
            catch (Exception ex)
            {

                Logger.LogError("Failed to complete synchronization for organization {0} with App ID {1} targeting tenant {2} error {3} and stack {4}", OrgConfig.OrgRoute, Settings.GetAppId(), Settings.TenantId, ex.Message, ex.StackTrace);
                // Logger message like above but for ex.InnerException
                if (ex.InnerException != null)
                {
                    Logger.LogError("Failred to complete sync for org {0} with inner exception message {1} and stack trace {2}", OrgConfig.OrgRoute, ex.InnerException?.Message ?? "NONE", ex.InnerException.StackTrace ?? "NONE");
                }

                OrgResult.AddError(ex, "Could not complete Active Directory synchronization process");
                throw;
            }
        }

        #endregion

        #region Methods - Synchronization

        private async Task SyncUsers()
        {
            if (Settings.ImportAllUsers)
            {
                await SyncUsersFromDelta();
            }

            if (Settings.Users != null)
            {
                await SyncUsers(Settings.Users);
            }
        }

        private async Task SyncGroups()
        {
            if (Settings.ImportAllGroups)
            {
                await SyncGroupsFromDelta();
            }

            if (Settings.Groups != null)
            {
                await SyncGroups(Settings.Groups);
            }
        }

        protected async Task SyncUsersFromDelta()
        {
            Logger.LogDebug($"Syncing all users for organization '{OrgConfig.OrgRoute}'");

            string url = GraphUser.AllUsersDeltaUrl;

            // Use a next/delta link if available
            if (!string.IsNullOrEmpty(Settings.NextLinkUsers))
            {
                url = Settings.NextLinkUsers;
            }
            else if (!string.IsNullOrEmpty(Settings.DeltaLinkUsers))
            {
                url = Settings.DeltaLinkUsers;
            }

            bool done = false;
            while (!done)
            {
                OrgResult.UserNetworkRequests++;
                GraphODataContext<GraphUser> users = await (await GraphClient()).Get<GraphODataContext<GraphUser>>(url);
                if (users == null)
                {
                    string msg = $"Failed to synchronize users for organization '{OrgConfig.OrgRoute}' because Graph API call had an invalid response.";
                    Logger.LogError(msg);
                    OrgResult.AddError(msg);
                    throw new UserSafeException(msg);
                }

                Logger.LogDebug($"Synchronizing {users.Value.Length} users");

                foreach (GraphUser user in users.Value)
                {
                    await SyncUser(user, SyncOpType.AddOrUpdate);
                }

                done = string.IsNullOrEmpty(users.NextLink);
                if (done)
                {
                    Settings.NextLinkUsers = null;
                    Settings.DeltaLinkUsers = users.DeltaLink;
                }
                else
                {
                    url = users.NextLink;
                    Settings.NextLinkUsers = users.NextLink;
                }
            }

            // Make sure to save off the _configuration to persists next/delta links
            await Store.SaveStrategyConfig(Settings);
        }

        protected async Task SyncGroupsFromDelta()
        {
            Logger.LogDebug($"Syncing ALL groups for organization '{OrgConfig.OrgRoute}'");
            string groupUrl = GraphGroup.AllGroupsDeltaUrl;
            bool done = false;

            // Use a next/delta link if available
            if (!string.IsNullOrEmpty(Settings.NextLinkGroups))
            {
                groupUrl = Settings.NextLinkGroups;
            }
            else if (!string.IsNullOrEmpty(Settings.DeltaLinkGroups))
            {
                groupUrl = Settings.DeltaLinkGroups;
            }

            // Request data from the API and loop through until all data has been received
            while (!done)
            {
                // Acquire data from MS Graph
                OrgResult.GroupNetworkRequests++;
                GraphODataContext<GraphGroup> data = await (await GraphClient()).Get<GraphODataContext<GraphGroup>>(groupUrl);

                if (data == null)
                {
                    string msg = $"Failed to synchronize groups for organization '{OrgConfig.OrgRoute}' because Graph API call had an invalid response.";
                    Logger.LogError(msg);
                    OrgResult.AddError(msg);
                    throw new UserSafeException(msg);
                }

                Logger.LogDebug($"Synchronizing {data.Value.Length} groups");
                foreach (GraphGroup grp in data.Value)
                {
                    await SyncGroup(grp, MemberSyncOpType.SyncAllMembers);
                }
                done = string.IsNullOrEmpty(data.NextLink);
                if (done)
                {
                    Settings.NextLinkGroups = null;
                    Settings.DeltaLinkGroups = data.DeltaLink;
                }
                else
                {
                    groupUrl = data.NextLink;
                    Settings.NextLinkGroups = data.NextLink;
                }
            }

            // Make sure to save off the _configuration to persists next/delta links
            await Store.SaveStrategyConfig(Settings);
        }

        /// <summary>
        /// Sync list of groups represented by <see cref="GroupSyncConfig"/>
        /// </summary>
        /// <returns>a task indicating success or failure of the operation.</returns>
        internal async Task SyncGroups(GroupSyncConfig[] groupConfigs)
        {
            try
            {
                if (groupConfigs == null)
                {
                    return;
                }

                Logger.LogDebug($"Syncing configured groups for organization '{OrgConfig.OrgRoute}'");

                // Store off how many items we need to skip to process the current batch
                int skip = 0;
                string url = GraphBase.GetByIdsUrl;

                // Iterate over the _configured groups in batches until all items are processed.
                while (skip < (groupConfigs?.Length ?? 0))
                {
                    // Split out this batch of IDs into expected graph format       
                    IEnumerable<GroupSyncConfig> batchConfigs = groupConfigs.Skip(skip).Take(MsGraphMaxGetByIdItems);
                    IEnumerable<string> groupsIds = batchConfigs.Where(g => g.GroupId != null && g.GroupId.Trim().Length > 0).Select(g => g.GroupId.Trim());
                    string requestBody = OidRequest.Serialize(groupsIds, OidRequest.Groups);

                    // Make the call and ensure its response is valid
                    OrgResult.GroupNetworkRequests++;
                    GraphODataContext<GraphGroup> groups = await (await GraphClient()).Post<GraphODataContext<GraphGroup>>(url, requestBody);
                    AssertNotNull(groups, $"Failed to synchronize selected groups for organization '{OrgConfig.OrgRoute}' because Graph API call had an invalid response.");
                    skip += MsGraphMaxGetByIdItems;

                    // Split the response
                    Dictionary<string, GraphGroup> groupsById = groups.Value.ToDictionary(v => v.UniversalId);

                    // Iterate over all of the IDs in the batch
                    foreach (GroupSyncConfig config in batchConfigs)
                    {
                        groupsById.TryGetValue(config.GroupId, out GraphGroup group);
                        if (group == null)
                        {
                            await Store.RemoveGroup(config.GroupId);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(config?.RenameTo)) { group.Name = config.RenameTo; }
                            await SyncGroup(group, config.MemberSyncType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = $"Trying to sync configured groups'{string.Join(",", groupConfigs.Select(g => g.GroupId))}' with error {ex.Message} and stack {ex.StackTrace}";
                Logger.LogError(ex, msg);
                OrgResult.AddError(ex, "Could not sync selected groups");
                throw;
            }
        }

        private async Task SyncGroup(GraphGroup group, MemberSyncOpType memberSyncType)
        {
            if (group == null)
            {
                return;
            }

            Logger.LogDebug($"Syncing group '{group.UniversalId}' in organization '{OrgConfig.OrgRoute}'");

            if (group.IsActiveAndValid)
            {
                // Group exists and has not been deleted so synchronize it
                await Store.AddGroup(group, SyncOpType.AddOrUpdate);
                await SyncMembers(group, memberSyncType);
            }
            else
            {
                // Remove item because it was deleted or not found in the directory
                await Store.RemoveGroup(group.UniversalId);
            }
        }

        private async Task SyncMembers(GraphGroup group, MemberSyncOpType memberSyncType)
        {
            if (memberSyncType == MemberSyncOpType.SyncNoMembers)
            {
                return;
            }

            Logger.LogDebug($"Syncing members for group '{group.UniversalId}' in organization '{OrgConfig.OrgRoute}'");

            string groupId = group.UniversalId;
            string url = MemberUrl(groupId);
            while (!string.IsNullOrEmpty(url))
            {
                OrgResult.GroupNetworkRequests++;
                GraphODataContext<GraphUser> members = await (await GraphClient()).Get<GraphODataContext<GraphUser>>(url);
                AssertNotNull(members, $"Failed to synchronize group members for group '{groupId}' in organization '{OrgConfig.OrgRoute}' because Graph API call had an invalid response.");

                foreach (GraphUser member in members.Value)
                {
                    // We may get users who are actually groups in areas like distribution lists
                    // If they don't look genuine, we should skip them
                    bool? isUser = member.IsUser();
                    if (isUser.HasValue && !isUser.Value)
                    {
                        return;
                    }

                    Logger.LogDebug($"Syncing member '{member.Email.RedactEmail()}' for group '{group.UniversalId}' in organization '{OrgConfig.OrgRoute}'");
                    if (member.IsActiveAndValid)
                    {
                        await Store.AddMember(group, member, memberSyncType);
                    }
                    else
                    {
                        await Store.RemoveMember(group.UniversalId, member.Email);
                    }
                }
                url = members.NextLink;
            }
        }

        private void AssertNotNull(object response, string msg)
        {
            if (response == null)
            {
                Logger.LogError(msg);
                OrgResult.AddError(msg);
                throw new UserSafeException(msg);
            }
        }

        /// <summary>
        /// Responsible for synchronizing groups explicitly _configured for synchronization.
        /// </summary>
        /// <returns>a task indicating success or failure of the operation.</returns>
        internal async Task SyncUsers(UserSyncConfig[] userConfigs)
        {
            try
            {
                if (userConfigs == null)
                {
                    return;
                }

                Logger.LogDebug($"Syncing configured users for organization '{OrgConfig.OrgRoute}'");

                string[] userIds = userConfigs.Select(u => u.Id).ToArray();
                await SyncUsers(userIds);
            }
            catch (Exception ex)
            {
                string msg = $"Trying to sync configured users {string.Join(",", userConfigs.Select(u => u.Id))} for org '{OrgConfig.OrgRoute}' with error {ex.Message} and stack {ex.StackTrace}";
                Logger.LogError(ex, msg);
                OrgResult.AddError(ex, "Cannot sync selected users");
                throw;
            }
        }

        /// <summary>
        /// Gets a set of <see cref="GraphUser"/> objects for the given OIDs
        /// </summary>
        /// <param name="objectIds"></param>
        /// <returns>A list of the active <see cref="GraphUser"/> instances requested by the given OIDs</returns>
        private async Task SyncUsers(IEnumerable<string> objectIds)
        {
            string[] usersIdsToFetch = objectIds.Where(id => !SyncedUsers.Contains(id)).ToArray();

            // Store off how many items we need to skip to process the current batch
            int skip = 0;
            string url = "https://graph.microsoft.com/v1.0/directoryObjects/getByIds";
            while (skip < usersIdsToFetch.Length)
            {
                // This uses the getByIds graph call which requires you to send the type of item
                // being requested and a list of all ids to be returned.                
                IEnumerable<string> oidsForBatch = objectIds.Skip(skip).Take(MsGraphMaxGetByIdItems);
                string requestBody = OidRequest.Serialize(oidsForBatch, OidRequest.Users);
                OrgResult.UserNetworkRequests++;
                GraphODataContext<GraphUser> users = await (await GraphClient()).Post<GraphODataContext<GraphUser>>(url, requestBody);
                AssertNotNull(users, $"Failed to synchronize users in organization '{OrgConfig.OrgRoute}' because Graph API call had an invalid response.");
                Dictionary<string, GraphUser> usersById = users.Value.ToDictionary(u => u.UniversalId);

                // Iterate over all of the IDs in the batch
                foreach (string userId in oidsForBatch)
                {
                    usersById.TryGetValue(userId, out GraphUser user);

                    // TODO: Consider if there is a more robust way of tracking orphans if this user was previously available
                    if (user == null)
                    {
                        continue;
                    }

                    await SyncUser(user);
                }
                skip += MsGraphMaxGetByIdItems;
            }
        }

        /// <summary>
        /// Returns true if the given <see cref="GraphUser"/> will be added (false if removed)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="syncType"></param>
        /// <returns></returns>
        private async Task SyncUser(GraphUser user, SyncOpType syncType = SyncOpType.AddOrUpdate)
        {
            if (user == null)
            {
                return;
            }

            if (user.Email == null)
            {
                Logger.LogWarning($"Cannot sync user without email '{user.UniversalId}' in organization '{OrgConfig.OrgRoute}'");
                return;
            }

            Logger.LogDebug($"Syncing user '{user.Email.RedactEmail()}' in organization '{OrgConfig.OrgRoute}'");

            SyncedUsers.Add(user.UniversalId);

            if (user.IsActiveAndValid)
            {
                // Group exists and has not been deleted so synchronize it
                await Store.AddPerson(user, syncType);
            }
            else
            {
                // Remove item because it was deleted or not found in the directory
                await Store.RemovePerson(user.Email);
            }
        }

        #endregion       
    }
}

