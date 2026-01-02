using Masticore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Interact
{
    /// <summary>
    /// <see cref="IOrgSyncStrategy"/> implementation for Interact.
    /// </summary>
    public abstract class InteractOrgSyncStrategyBase : OrgSyncStrategyBase<InteractOrgConfig>, IOrgSyncStrategy
    {
        #region Constants

        /// <summary>
        /// Defines the SyncType key for this Interact IOrgSyncStrategy implementation.
        /// </summary>
        public const string Name = "INTERACT";

        public const string MissingPassword = "Interact Sync Configuration is missing the password.";
        public const string MissingUsername = "Interact Sync Configuration is missing the user name.";
        public const string MissingToken = "Interact token is missing.";
        public const string ApiUrlToken = "/token";

        #endregion

        #region Properties

        protected IOrgSyncStore SyncStore { get; set; }
        protected InteractApiService ApiService { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="InteractOrgSyncStrategy"/> instance.
        /// </summary>
        /// <param name="logger">Reference to a logger.</param>
        /// <param name="syncStore">Reference to a org synchronization store.</param>
        /// <param name="syncConfig">Reference to the organization configuration defining the sync process for the org.</param>
        public InteractOrgSyncStrategyBase(InteractApiService apiService, OrgSyncConfig orgSettings, ILogger logger, IOrgSyncStore syncStore, ISyncInfrastucture infrastructure)
            : base(logger, syncStore, orgSettings, infrastructure)
        {
            SyncStore = syncStore;
            ApiService = apiService;
            apiService.ApplySettings(Settings);
        }

        #endregion

        #region IOrgSyncStrategy Implementation

        /// <inheritdoc />
        public override Task<SyncTarget> GroupAsync(string groupId)
        {
            // Interact does not support the concept of a group so this always returns null.
            return Task.FromResult<SyncTarget>(null);
        }

        /// <inheritdoc />
        public override Task<TokenPageResponse<SyncTarget>> GroupsAsync(TokenPageRequest page = null)
        {
            if (page == null)
            {
                page = new TokenPageRequest
                {
                    Take = PageRequestBase.DefaultMaxTake,
                };
            }

            /// Interact does not support the concept of a group so this always returns a zero length array.
            TokenPageResponse<SyncTarget> result = new TokenPageResponse<SyncTarget>(page, new SyncTarget[0]);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public override async Task<bool> HasAccessAsync()
        {
            try
            {
                // EnsureAuthToken throws an error if a token cannot be established.
                await ApiService.EnsureAuthToken();
            }
            catch
            {
                // Disregard the error and return false.
                return false;
            }
            return true;
        }

        /// <inheritdoc />
        public abstract override Task Sync(OrgSyncResult orgResult);

        /// <inheritdoc />
        public override async Task<SyncTarget> UserAsync(string userId)
        {
            PersonResponse person = await ApiService.GetPerson(userId);
            SyncTarget result = person == null ? null : new SyncTarget() { Id = person.Id.ToString(), Name = person.FullName };
            return result;
        }

        /// <inheritdoc />
        public override Task<TokenPageResponse<SyncTarget>> UsersAsync(TokenPageRequest page = null)
        {
            // Not implemented yet while attempting to get API information from Interact
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override async Task<string> ValidationMessagesAsync(OrgSyncConfig orgConfig)
        {
            // Config check
            try
            {
                // Null is valid because it means there are reset values
                if (orgConfig == null)
                {
                    return null;
                }

                // This will throw different exceptions based on validity
                InteractOrgConfig syncConfig = orgConfig.GetSyncConfig<InteractOrgConfig>();

                // If it returns null, then it's also invalid because the AAD config is not an org config
                if (syncConfig == null)
                {
                    return Constants.SyncConfigErrorMessages.MissingConfig;
                }

                if (string.IsNullOrEmpty(syncConfig.Password))
                {
                    return MissingPassword;
                }

                if (string.IsNullOrEmpty(syncConfig.Username))
                {
                    return MissingUsername;
                }

                try
                {
                    if (syncConfig.Users != null)
                    {
                        foreach (UserSyncConfig user in syncConfig.Users)
                        {
                            string msg = await ValidateUser(user.Id);
                            if (msg != null)
                            {
                                return msg;
                            }
                        }
                    }

                    if (syncConfig.Groups != null)
                    {
                        foreach (GroupSyncConfig group in syncConfig.Groups)
                        {
                            string msg = await ValidateGroup(group.GroupId);
                            if (msg != null)
                            {
                                return msg;
                            }
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    string msg = $"Could not validate orgs and users for organization {orgConfig.OrgRoute} because {ex.Message} with stack {ex.StackTrace}";
                    Logger.LogWarning(msg);
                    return ex.Message;
                }
            }
            catch (Exception ex)
            {
                string msg = $"Could not validate sync config for organization {orgConfig.OrgRoute} because {ex.Message} with stack {ex.StackTrace}";
                Logger.LogWarning(msg);
                return ex.Message;
            }
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        protected override Task SanitizeConfig(InteractOrgConfig settings)
        {
            settings.Username = null;
            settings.Password = null;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override Task TransferConfigAsync(InteractOrgConfig configOld, InteractOrgConfig configNew)
        {
            configNew.Username = string.IsNullOrEmpty(configNew.Username)
                ? configNew.Username
                : configOld.Username;
            configNew.Password = string.IsNullOrEmpty(configNew.Password)
                ? configNew.Password
                : configOld.Password;
            return Task.CompletedTask;
        }


        #endregion

        #region Methods

        public Task<string> ValidateUser(string uid)
        {
            //TODO: see about implementing this ..... 
            return Task.FromResult<string>(null);
        }

        public Task<string> ValidateGroup(string uid)
        {
            //TODO: see about implementing this ..... 
            return Task.FromResult<string>(null);
        }

        #endregion
    }
}

