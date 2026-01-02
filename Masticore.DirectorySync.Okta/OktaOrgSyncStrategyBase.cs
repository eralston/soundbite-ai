using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Okta
{
    /// <summary>
    /// <see cref="IOrgSyncStrategy"/> implementation for Interact.
    /// </summary>
    public abstract class OktaOrgSyncStrategyBase : OrgSyncStrategyBase<OktaOrgSyncConfig>, IOrgSyncStrategy
    {
        #region Constants

        /// <summary>
        /// Defines the SyncType key for this Interact IOrgSyncStrategy implementation.
        /// </summary>
        public const string Name = "OKTA";

        public const string MissingDomain = "OKTA Sync Configuration is missing the domain.";
        public const string MissingApiToken = "OKTA Sync Configuration is missing the API token.";

        public static string[] InvalidStatuses = new[] { "LOCKED OUT", "SUSPENDED", "DEPROVISIONED" };

        #endregion

        #region Properties

        protected OrgSyncConfig OldConfig { get; set; }
        protected IHttpClientFactory HttpFactory { get; }

        private OktaApiService _oktaApi;
        protected OktaApiService OktaApi
        {
            get
            {
                if (_oktaApi == null)
                {
                    _oktaApi = new OktaApiService(HttpFactory, Settings, OldConfig?.GetSyncConfig<OktaOrgSyncConfig>(false));
                }
                return _oktaApi;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="OktaOrgSyncStrategy"/> instance.
        /// </summary>
        /// <param name="logger">Reference to a logger.</param>
        /// <param name="syncStore">Reference to a org synchronization store.</param>
        /// <param name="syncConfig">Reference to the organization configuration defining the sync process for the org.</param>
        public OktaOrgSyncStrategyBase(
            IHttpClientFactory httpFactory,
            OrgSyncConfig currentConfig,
            OrgSyncConfig oldConfig,
            ILogger logger,
            IOrgSyncStore syncStore,
            IIdentityInfrastructure infrastructure)
            : base(logger, syncStore, currentConfig, infrastructure)
        {
            // Only recycle old config if it's already an Okta config
            if (oldConfig?.SyncType == Name)
            {
                OldConfig = oldConfig;
            }

            HttpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        }

        #endregion

        #region IOrgSyncStrategy Implementation

        /// <inheritdoc />
        public override async Task<SyncTarget> GroupAsync(string groupId)
        {
            OktaGroupResponse response = await OktaApi.GetGroup(groupId);
            return response == null || string.IsNullOrEmpty(response.profile?.name)
                ? null : new SyncTarget(groupId, response.profile.name);
        }

        /// <inheritdoc />
        public override async Task<TokenPageResponse<SyncTarget>> GroupsAsync(TokenPageRequest page = null)
        {
            TokenPageResponse<OktaGroupResponse> data = await OktaApi.GetGroups(page, Settings.ModifiedDateFilterStart, Settings.ModifiedDateFilterEnd);
            TokenPageResponse<SyncTarget> result = new TokenPageResponse<SyncTarget>(data.Request,
                data.Result.Select(i => new SyncTarget(i.id, GetDisplayName(i))).ToList())
            {
                SkipToken = data.SkipToken
            };
            return result;
        }

        /// <inheritdoc />
        public override async Task<bool> HasAccessAsync()
        {
            bool result = await OktaApi.VerifyAccess();
            return result;
        }

        /// <inheritdoc />
        public override async Task<SyncTarget> UserAsync(string userId)
        {
            OktaUserResponse response = await OktaApi.GetUser(userId);
            return response == null
                || (string.IsNullOrEmpty(response.profile.lastName)
                    && string.IsNullOrEmpty(response.profile.firstName))
                        ? null : new SyncTarget(userId, $"{response.profile.firstName} {response.profile.lastName}");
        }

        /// <inheritdoc />
        public override async Task<TokenPageResponse<SyncTarget>> UsersAsync(TokenPageRequest page = null)
        {
            TokenPageResponse<OktaUserResponse> data = await OktaApi.GetUsers(page);
            TokenPageResponse<SyncTarget> result = new TokenPageResponse<SyncTarget>(data.Request,
                data.Result.Select(i => new SyncTarget(i.id, GetDisplayName(i))).ToList())
            {
                SkipToken = data.SkipToken
            };
            return result;
        }

        private string GetDisplayName(OktaUserResponse userInfo)
        {
            Validator.ArgNotNull(nameof(userInfo), userInfo);
            string firstName = userInfo?.profile?.firstName;
            string lastName = userInfo?.profile?.firstName;
            if (!string.IsNullOrEmpty(lastName) && !string.IsNullOrEmpty(firstName))
            {
                return $"{firstName} {lastName}";
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                return lastName;
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                return firstName;
            }

            return $"User ID: {userInfo.id}";

        }

        private string GetDisplayName(OktaGroupResponse groupInfo)
        {
            Validator.ArgNotNull(nameof(groupInfo), groupInfo);

            if (!string.IsNullOrEmpty(groupInfo?.profile?.name))
            {
                return groupInfo.profile.name;
            }

            return $"Group ID: {groupInfo.id}";
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
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                OrganizationEntity org = await db.OrgWhereRoute(orgConfig.OrgRoute);
                org.AssertFound();

                OktaOrgSyncConfig newConfig = orgConfig.GetSyncConfig<OktaOrgSyncConfig>();

                // Carry over any secret fields from old config if available
                OktaOrgSyncConfig oldConfig = org.GetOrgSyncConfig().GetSyncConfig<OktaOrgSyncConfig>(false);
                newConfig.ApiToken = newConfig.ApiToken ?? oldConfig?.ApiToken;

                // Must have a valid sync settings instance to be valid
                if (newConfig == null)
                {
                    return Constants.SyncConfigErrorMessages.MissingConfig;
                }

                if (string.IsNullOrEmpty(newConfig.Domain))
                {
                    return MissingDomain;
                }

                if (string.IsNullOrEmpty(newConfig.ApiToken))
                {
                    return MissingApiToken;
                }

                try
                {
                    if (newConfig.Users != null)
                    {
                        foreach (UserSyncConfig user in newConfig.Users)
                        {
                            string msg = await ValidateUser(user.Id);
                            if (msg != null)
                            {
                                return msg;
                            }
                        }
                    }

                    if (newConfig.Groups != null)
                    {
                        foreach (GroupSyncConfig group in newConfig.Groups)
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

        protected override Task SanitizeConfig(OktaOrgSyncConfig settings)
        {
            settings.ApiToken = null;
            return Task.CompletedTask;
        }

        protected override Task TransferConfigAsync(OktaOrgSyncConfig oldConfig, OktaOrgSyncConfig newConfig)
        {
            newConfig.ApiToken = newConfig.ApiToken ?? oldConfig.ApiToken;
            newConfig.Domain = newConfig.Domain ?? oldConfig.Domain;
            return Task.CompletedTask;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for validating the specified user.
        /// </summary>
        /// <param name="userId">ID of the user to validate.</param>
        /// <returns>Single validation message if there is an error; otherwise <c>null</c>.</returns>
        private async Task<string> ValidateUser(string userId)
        {
            if (userId is null)
            {
                return "Cannot validate user with missing ID";
            }

            OktaUserResponse response = await OktaApi.GetUser(userId);

            if (response == null)
            {
                return $"Could not find user with ID '{userId}'";
            }

            if (InvalidStatuses.Contains(response.status.ToUpper()))
            {
                return $"User with ID '{userId}' exists but has a status of '{response.status}'.";
            }

            return null;
        }

        /// <summary>
        /// Responsible for validating the specified group.
        /// </summary>
        /// <param name="groupId">ID of the group to validate.</param>
        /// <returns>Single validation message if there is an error; otherwise <c>null</c>.</returns>
        private async Task<string> ValidateGroup(string groupId)
        {
            if (groupId is null)
            {
                return "Cannot validate group with missing ID";
            }

            OktaGroupResponse response = await OktaApi.GetGroup(groupId);

            if (response == null)
            {
                return $"Could not find group with ID '{groupId}'";
            }

            return null;
        }

        #endregion        
    }
}

