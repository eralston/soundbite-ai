using Masticore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Interact
{

    /// <summary>
    /// <see cref="IOrgSyncStrategy"/> implementation for Interact.
    /// </summary>
    public class InteractOrgSyncStrategy : InteractOrgSyncStrategyBase
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="InteractOrgSyncStrategy"/> instance.
        /// </summary>
        /// <param name="logger">DI reference to a logger.</param>
        /// <param name="apiService">DI reference to the Interact API service.</param>
        /// <param name="orgSettings">DI reference to the organization sync settings.</param>
        /// <param name="syncStore">DI reference to a org synchronization store.</param>
        /// <param name="syncConfig">DI reference to the organization configuration defining the sync process for the org.</param>
        public InteractOrgSyncStrategy(InteractApiService apiService, OrgSyncConfig orgSettings, ILogger logger, IOrgSyncStore syncStore, ISyncInfrastucture infrastructure)
            : base(apiService, orgSettings, logger, syncStore, infrastructure)
        {
        }

        #endregion

        #region Overridden Methods

        /// <inheritdoc />
        public override async Task Sync(OrgSyncResult orgResult)
        {
            Logger.LogDebug($"Syncing organization '{OrgConfig.OrgRoute}' with targeting tenant '{Settings.TenantId}'");
            try
            {
                OrgResult = orgResult;

                await Store.StartSync(OrgConfig, OrgResult);

                await SyncUsers();

                //await SyncGroups();

                await Store.EndSync();

                Logger.LogInformation($"Successfully synced organization '{OrgConfig.OrgRoute}' with targeting tenant '{Settings.TenantId}'");
            }
            catch (Exception ex)
            {
                string msg = $"Failed to complete synchronization for organization '{OrgConfig.OrgRoute}' with targeting tenant '{Settings.TenantId}' error {ex.Message} and stack {ex.StackTrace}";
                Logger.LogError(ex, msg);
                OrgResult.AddError(ex, "Could not complete Interact directory synchronization process");
                throw;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for processing any user synchronization tasks.
        /// </summary>
        /// <returns>a task indicating completion of the operation.</returns>
        private async Task SyncUsers()
        {
            if (Settings.ImportAllUsers)
            {
                await SyncAllUsers();
            }
            else
            {
                await SyncConfiguredUsers();
            }
        }

        /// <summary>
        /// Synchronizes all users in the directory.
        /// </summary>
        /// <returns>a task indicating completion of the operation.</returns>
        private async Task SyncAllUsers()
        {
            const int maxFails = 3;
            bool done = false;
            int currentSkip = 0;
            int apiFails = 0;
            string alphaFilter = null;
            List<char> alphaFilters = new List<char>(Settings.IsLargeOrg
                ? "abcdefghijklmnopqrstuvwxyz".ToCharArray() : null);

            while (!done)
            {
                // Acquire the current alpha filter (if available)
                if (alphaFilter == null && alphaFilters.Any())
                {
                    alphaFilter = alphaFilters[0].ToString();
                    alphaFilters.RemoveAt(0);
                }

                try
                {
                    // Acquire a page of directory data
                    OrgResult.UserNetworkRequests++;
                    IndexPageResponse<PersonResponse> data =
                        await ApiService.GetPeople(new IndexPageRequest()
                        {
                            Take = 50,
                            Skip = currentSkip
                        }, alphaFilter);

                    foreach (PersonResponse person in data.Result)
                    {
                        await SyncUser(ConvertUser(person), SyncOpType.AddOrUpdate);
                    }

                    // Increment the skip
                    currentSkip += 50;

                    // Determine if this is the last page of data
                    if (currentSkip >= data.TotalCount)
                    {
                        // Reset the skip and clear out the alpha filter.  The next alpha filter
                        // will be picked up on the next iteration.
                        currentSkip = 0;
                        alphaFilter = null;
                    }
                }
                catch (Exception ex)
                {
                    apiFails++;
                    Logger.LogError(ex, $"Interact Directory Sync call to people API failed (attempt {apiFails} of {maxFails})");
                    if (apiFails >= maxFails)
                    {
                        throw;
                    }
                    else
                    {
                        // Wait a second before sending off another request
                        await Task.Delay(1000);
                    }
                }

                // Determine whether there are any more alpha filters to process
                if (alphaFilters.Count == 0)
                {
                    done = true;
                }
            }
        }

        private IUserFieldsWithAliases ConvertUser(PersonResponse user)
        {
            UserWithAliases result = null;
            if (user != null)
            {
                result = new UserWithAliases()
                {
                    Email = user.Email,
                    FamilyName = user.LastName,
                    GivenName = user.FirstName,
                    Phone = user.WorkPhone,
                    UniversalId = user.Id.ToString(),
                    Title = user.JobTitle,
                    UserRole = UserRole.User
                };
            }

            return result;
        }

        private async Task SyncUser(IUserFieldsWithAliases user, SyncOpType syncType = SyncOpType.AddOrUpdate)
        {
            if (user == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                Logger.LogWarning($"Cannot sync user without email '{user.UniversalId}' in organization '{OrgConfig.OrgRoute}'");
                return;
            }

            SyncedUsers.Add(user.UniversalId);

            await Store.AddPerson(user, syncType);
        }

        /// <summary>
        /// Synchronizes only those users that have been configured for import.
        /// </summary>
        /// <returns>a task indicating completion of the operation.</returns>
        private async Task SyncConfiguredUsers()
        {
            try
            {
                if (Settings.Users?.Length > 0)
                {
                    Logger.LogDebug($"Syncing configured users for organization '{OrgConfig.OrgRoute}'");
                    int failCount = 0;
                    foreach (UserSyncConfig user in Settings.Users)
                    {
                        if (!await SyncConfiguredUser(user))
                        {
                            failCount++;
                        }
                    }
                    Logger.LogDebug($"Completed syncing configured users for organization '{OrgConfig.OrgRoute}' - ({Settings.Users.Length - failCount}/{Settings.Users.Length} succeeded)");
                }
            }
            catch (Exception ex)
            {
                string msg = $"Failed to sync configured users for org '{OrgConfig.OrgRoute}' with error {ex.Message} and stack {ex.StackTrace}";
                Logger.LogError(ex, msg);
                OrgResult.AddError(ex, "Cannot sync selected users");
                throw;
            }
        }

        /// <summary>
        /// Responsible for synchronizing a single user from the directory sync configuration.
        /// </summary>
        /// <param name="user">User to synchronize.</param>
        /// <returns>a flag indicating whether or not the synchronization succeeded.</returns>
        private async Task<bool> SyncConfiguredUser(UserSyncConfig user)
        {
            try
            {
                PersonResponse externalUser = await ApiService.GetPerson(user.Id);
                UserWithAliases sbUser = ConvertToSbUser(externalUser);
                if (!string.IsNullOrEmpty(sbUser.Email))
                {
                    await Store.AddPerson(ConvertToSbUser(externalUser), SyncOpType.AddOrUpdate);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to sync configured user '{user.Id}' for org '{OrgConfig.OrgRoute}' with error {ex.Message} and stack {ex.StackTrace}";
                Logger.LogError(ex, msg);
                OrgResult.AddError(ex, msg);
                return false;
            }
        }

        /// <summary>
        /// Responsible for converting a person record from the Interact API into an <see cref="IUserFields"/> instance.
        /// </summary>
        /// <param name="externalUser">External user record.</param>
        /// <returns>an instance supporting the <see cref="IUserFields"/> interface populated with any available data.</returns>
        private UserWithAliases ConvertToSbUser(PersonResponse externalUser)
        {
            UserWithAliases user = new UserWithAliases()
            {
                UniversalId = externalUser?.Id.ToString(),
                Email = externalUser?.Email,
                FamilyName = externalUser?.LastName,
                GivenName = externalUser?.FirstName,
                Phone = externalUser?.WorkPhone,
                Title = externalUser?.JobTitle
            };
            return user;
        }


        #endregion
    }
}