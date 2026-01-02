using Masticore.Ad;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Aad
{
    /// <summary>
    /// The utility methods in <see cref="IOrgSyncStrategy"/> that will be invariant to the implementation of the sync process for as long as the <see cref="AadOrgConfig"/> structure is the same
    /// </summary>
    public abstract class AadOrgSyncStrategyBase : OrgSyncStrategyBase<AadOrgConfig>
    {
        #region Constants

        /// <summary>
        /// Defines the maximum number of objects to retrieve from a GetById call to MS Graph API.
        /// </summary>
        protected const int MsGraphMaxGetByIdItems = 1000;

        /// <summary>
        /// Defines the SyncType key for this AAD IOrgSyncStrategy implementation.
        /// </summary>
        public const string Name = "AAD";

        // TODO: Make a simplified query that only returns unified groups; ideally orgs will use this instead of ALL groups
        public const string UnifiedGroupsTargetUrl = "https://graph.microsoft.com/v1.0/groups?$select=id,deletedDateTime,displayName&$filter=groupTypes/any(groupType: groupType eq 'Unified')";

        public const string MissingTenantId = "Azure Active Directory Directory sync settings missing tenant ID";

        #endregion

        #region Static Members

        /// <summary>
        /// Returns a URL to retrieve the given group by ID attached the given (optional) query string
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        protected static string MemberUrl(string groupId, bool hasMembersCheck = false)
        {
            string select = GraphUser.SelectClause;
            List<string> clauses = new List<string> { select };

            if (hasMembersCheck)
            {
                clauses.Add("$top=1");
            }

            string queryString = string.Join('&', clauses);
            return $"{GraphGroup.AllGroupsUrl}{groupId}{GraphUser.MembersUrlSuffix}?{queryString}";
        }

        #endregion

        #region Properties & Fields

        protected IGraphFactory GraphFactory { get; }

        protected IGraphClient _graph;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="AadOrgSyncStrategy"/> instance.
        /// </summary>
        public AadOrgSyncStrategyBase(
            ILogger logger,
            IOrgSyncStore orgSyncStore,
            OrgSyncConfig orgConfig,
            IGraphFactory graph,
            IIdentityInfrastructure infrastructure
            )
            : base(logger, orgSyncStore, orgConfig, infrastructure)
        {

            GraphFactory = graph ?? throw new ArgumentNullException(nameof(graph));
            orgSyncStore.SetProviderType(ProviderType.AAD);
        }

        #endregion

        #region Methods

        protected async Task<IGraphClient> GraphClient()
        {
            if (_graph == null)
            {
                _graph = await GraphFactory.ClientAsync(Logger, OrgConfig);
            }
            return _graph;
        }

        /// <summary>
        /// Async get the set of graph users for the given page
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task<IEnumerable<GraphUser>> GetUsers(TokenPageRequest page)
        {
            if (page is null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            IGraphClient client = await GraphClient();
            GraphRequest request = new GraphRequest(client, GraphUser.AllUsersUrl)
            {
                Select = GraphUser.SelectFields,
                Filter = GraphUser.FilterCriteria,
            };

            // Filter criteria must be merged base on PageRequest AND default criteria
            page.Filter = page.Filter?.Trim();
            if (!string.IsNullOrEmpty(page.Filter))
            {
                // TODO: Figure out if we can make this more secure; though the fact that it's locked to a GET and formatted before return should mean we're fine
                request.AndFilter($"startsWith(mail, '{page.Filter}')");
            }

            // Acquire data from MS Graph, then filter down to relevant data
            GraphODataContext<GraphUser> data = await request.Get<GraphODataContext<GraphUser>>(page);
            // TODO: Add as filter criteria on the request above? At least keep pushing through until we have page.take number of items
            IEnumerable<GraphUser> activeItems = data.Value.Where(
                u => u.Email != null && u.UniversalId != null && u.DeletedDateTime == null
            );

            return activeItems;
        }

        /// <summary>
        /// Async get the given page of groups
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task<IEnumerable<GraphGroup>> GetGroups(TokenPageRequest page)
        {
            if (page is null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            IGraphClient client = await GraphClient();
            GraphRequest request = new GraphRequest(client, GraphGroup.AllGroupsUrl)
            {
                Select = GraphGroup.SelectFields,
            };

            // Filter criteria must be merged base on PageRequest AND default criteria
            page.Filter = page.Filter?.Trim();
            if (!string.IsNullOrEmpty(page.Filter))
            {
                // TODO: Figure out if we can make this more secure; though the fact that it's locked to a GET and formatted before return should mean we're fine
                request.AndFilter($"(startsWith(displayName, '{page.Filter}') or startsWith(mail, '{page.Filter}'))");
            }

            // Acquire data from MS Graph, then filter down to relevant data
            // TODO: TEMPORARY LOG STATEMENT, REMOVE ASAP
            Logger.LogInformation("Attempting get groups with URL:", request.Url());
            GraphODataContext<GraphGroup> data = await request.Get<GraphODataContext<GraphGroup>>(page);
            Logger.LogInformation($"Raw groups data: {data.ToLowerCamelJson(false)}");
            IEnumerable<GraphGroup> activeItems = data.Value.Where(
                g => g.Name != null && g.UniversalId != null && g.DeletedDateTime == null
            );
            // TODO: TEMPORARY LOG STATEMENT, REMOVE ASAP
            Logger.LogInformation($"Filtered groups data: {activeItems.ToLowerCamelJson(false)}");
            return activeItems;
        }

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

            try
            {
                string url = "https://graph.microsoft.com/v1.0/directoryObjects/getByIds";
                // This uses the getByIds graph call which requires you to send the type of item
                // being requested and a list of all ids to be returned.                
                string[] oidsForBatch = new string[] { userId };
                string requestBody = OidRequest.Serialize(oidsForBatch, OidRequest.Users);
                GraphODataContext<GraphUser> users = await (await GraphClient()).Post<GraphODataContext<GraphUser>>(url, requestBody);
                if (users.Value.Length == 0)
                {
                    return $"Could not find user with ID '{userId}'";
                }

                if (!users.Value[0].IsActiveAndValid)
                {
                    return $"User with ID '{userId}' appears to be inactive";
                }

                return null;
            }
            catch (Exception ex)
            {
                string userSafeMessage = $"Error validating user with ID {userId}";
                string msg = $"{userSafeMessage} in organization {OrgConfig.OrgRoute} because {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                return userSafeMessage;
            }
        }

        /// <summary>
        /// Responsible for validating the specified group.
        /// </summary>
        /// <param name="groupId">ID of the group to validate.</param>
        /// <returns>Single validation message if there is an error; otherwise <c>null</c>.</returns>
        private async Task<string> ValidateGroup(string uid)
        {
            if (uid is null)
            {
                return "Cannot validate group with missing ID";
            }

            try
            {
                // Make sure we can get details
                string oidUrl = "https://graph.microsoft.com/v1.0/directoryObjects/getByIds";
                // This uses the getByIds graph call which requires you to send the type of item
                // being requested and a list of all ids to be returned.                
                string[] oidsForBatch = new string[] { uid };
                string requestBody = OidRequest.Serialize(oidsForBatch, OidRequest.Groups);
                GraphODataContext<GraphGroup> groups = await (await GraphClient()).Post<GraphODataContext<GraphGroup>>(oidUrl, requestBody);
                if (groups.Value.Length == 0)
                {
                    return $"Could not find group with ID '{uid}'";
                }

                if (!groups.Value[0].IsActiveAndValid)
                {
                    return $"Group with ID '{uid}' appears to be inactive";
                }

                // TODO: Restore check for membership, but in a way that reports it as a warning instead of locking up the process entirely
                //string hasAccessibleMembersUrl = MemberUrl(uid, true);
                //GraphODataContext<GraphUser> members = await (await GraphClient()).Get<GraphODataContext<GraphUser>>(hasAccessibleMembersUrl);
                //if (members.Value.Length == 0)
                //{
                //    return $"Did not find any members for group with ID '{uid}'; groups must have accessible members to sync";
                //}

                return null;
            }
            catch (Exception ex)
            {
                string userSafeMessage = $"Error validating group with ID {uid}";
                string msg = $"{userSafeMessage} organization {OrgConfig.OrgRoute} because {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                return userSafeMessage;
            }
        }

        #endregion

        #region IOrgSyncProvider Implementation

        /// <inheritdoc />
        public override async Task<bool> HasAccessAsync()
        {
            try
            {
                if (OrgConfig == null || OrgConfig.GetSyncConfig<AadOrgConfig>(false) == null)
                {
                    return false;
                }

                // If we can pull a single user, then we are good
                GraphODataContext<GraphBase> data = await (await GraphClient()).Get<GraphODataContext<GraphBase>>(GraphUser.FirstUserUrl);
                return data.Value.Length >= 0;
            }
            catch (GraphClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden || ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return false;
                }

                string msg = $"Failed to reach organization '{OrgConfig.OrgRoute}', likely due to lack of access because {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to reach organization '{OrgConfig.OrgRoute}', likely due to lack of access because {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                throw;
            }
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
                AadOrgConfig aadConfig = orgConfig.GetSyncConfig<AadOrgConfig>();

                // If it returns null, then it's also invalid because the AAD config is not on orgconfig
                if (aadConfig == null)
                {
                    return Constants.SyncConfigErrorMessages.MissingConfig;
                }

                // Must have a TenantId to be valid
                if (string.IsNullOrEmpty(aadConfig.TenantId))
                {
                    return MissingTenantId;
                }

                try
                {
                    if (aadConfig.Users != null)
                    {
                        foreach (UserSyncConfig user in aadConfig.Users)
                        {
                            string msg = await ValidateUser(user.Id);
                            if (msg != null)
                            {
                                return msg;
                            }
                        }
                    }

                    if (aadConfig.Groups != null)
                    {
                        foreach (GroupSyncConfig group in aadConfig.Groups)
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

        /// <inheritdoc />
        public override async Task<TokenPageResponse<SyncTarget>> UsersAsync(TokenPageRequest page = null)
        {
            try
            {
                Logger.LogDebug($"GETing a page of users for organization '{OrgConfig.OrgRoute}'");

                if (page == null)
                {
                    page = new TokenPageRequest
                    {
                        Take = PageRequestBase.DefaultMaxTake,
                    };
                }

                IEnumerable<GraphUser> activeItems = await GetUsers(page);
                IEnumerable<SyncTarget> result = activeItems.Select(u => new SyncTarget { Id = u.UniversalId, Name = u.Email });
                TokenPageResponse<SyncTarget> ret = new TokenPageResponse<SyncTarget>(page, result);

                return ret;
            }
            catch (GraphClientException ex)
            {

                string msg = $"Failed to query users for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);

                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    return null;
                }

                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to query users for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                throw;
            }
        }

        /// <inheritdoc />
        public override async Task<SyncTarget> UserAsync(string userId)
        {
            try
            {
                IGraphClient client = await GraphClient();
                // Acquire data from MS Graph, then filter down to relevant data
                GraphRequest request = new GraphRequest(client, GraphUser.AllUsersUrl)
                {
                    Select = "id,mail,deletedDateTime",
                    Filter = $"id eq '{userId}'",
                    Top = 1
                };

                GraphODataContext<GraphUser> data = await request.Get<GraphODataContext<GraphUser>>();
                IEnumerable<GraphUser> activeItems = data.Value.Where(u =>
                    u.Email != null && u.UniversalId != null && u.DeletedDateTime == null
                );

                // Map to response
                SyncTarget ret = activeItems.Select(u => new SyncTarget { Id = u.UniversalId, Name = u.Email }).FirstOrDefault();
                if (ret == null)
                {
                    throw new NotFoundException($"Could not find user {userId} in organization");
                }

                return ret;
            }
            catch (GraphClientException ex)
            {
                string msg = $"Failed to query users for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);

                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    return null;
                }

                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to query user for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                throw;
            }
        }

        /// <inheritdoc />
        public override async Task<TokenPageResponse<SyncTarget>> GroupsAsync(TokenPageRequest page = null)
        {
            try
            {
                Logger.LogDebug($"GETing a page of groups for organization '{OrgConfig.OrgRoute}'");

                if (page == null)
                {
                    page = new TokenPageRequest
                    {
                        Take = PageRequestBase.DefaultMaxTake,
                    };
                }

                IEnumerable<GraphGroup> activeItems = await GetGroups(page);
                IEnumerable<SyncTarget> result = activeItems.Select(g => new SyncTarget { Id = g.UniversalId, Name = g.NameWithMail() });
                TokenPageResponse<SyncTarget> ret = new TokenPageResponse<SyncTarget>(page, result);

                return ret;
            }
            catch (GraphClientException ex)
            {

                string msg = $"Failed to query users for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);

                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    return null;
                }

                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to query users for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                throw;
            }
        }

        /// <inheritdoc />
        public override async Task<SyncTarget> GroupAsync(string groupId)
        {
            try
            {
                IGraphClient client = await GraphClient();
                // Acquire data from MS Graph, then filter down to relevant data
                GraphRequest request = new GraphRequest(client, GraphGroup.AllGroupsUrl)
                {
                    Select = GraphGroup.SelectFields,
                    Filter = GraphGroup.FilterById(groupId),
                    Top = 1
                };

                GraphODataContext<GraphGroup> data = await request.Get<GraphODataContext<GraphGroup>>();
                IEnumerable<GraphGroup> activeItems = data.Value.Where(u =>
                    u.Name != null && u.UniversalId != null && u.DeletedDateTime == null
                );

                // Map to response
                GraphGroup grp = activeItems.FirstOrDefault();

                if (grp == null)
                {
                    throw new NotFoundException($"Could not find group {groupId} in organization");
                }
                SyncTarget ret = new SyncTarget { Id = grp.UniversalId, Name = grp.NameWithMail() };
                return ret;
            }

            catch (GraphClientException ex)
            {
                string msg = $"Failed to query group for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);

                if (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    return null;
                }

                throw;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to query group for organization '{OrgConfig.OrgRoute}', error {ex.Message} with stack {ex.StackTrace}";
                Logger.LogError(msg);
                throw;
            }
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        protected override Task SanitizeConfig(AadOrgConfig settings)
        {
            settings.SecretKey = null;
            settings.DeltaLinkGroups = null;
            settings.DeltaLinkUsers = null;
            settings.NextLinkGroups = null;
            settings.NextLinkUsers = null;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override Task TransferConfigAsync(AadOrgConfig oldAad, AadOrgConfig newAad)
        {
            // If it's the same app and tenant, then we can carry over settings
            bool isSameTenant = oldAad.TenantId == newAad.TenantId;
            bool isSameApp = oldAad.AppId == newAad.AppId;
            if (isSameTenant && isSameApp)
            {
                if (oldAad.ImportAllGroups && newAad.ImportAllGroups)
                {
                    newAad.DeltaLinkGroups = oldAad.DeltaLinkGroups;
                    newAad.NextLinkGroups = oldAad.NextLinkGroups;
                }
                else
                {
                    newAad.DeltaLinkGroups = null;
                    newAad.NextLinkGroups = null;
                }

                if (oldAad.ImportAllUsers && newAad.ImportAllUsers)
                {
                    newAad.DeltaLinkUsers = oldAad.DeltaLinkUsers;
                    newAad.NextLinkUsers = oldAad.NextLinkUsers;
                }
                else
                {
                    newAad.DeltaLinkUsers = null;
                    newAad.NextLinkUsers = null;
                }

                // If they didn't set a new secret when they already have one, then carry that over
                if (oldAad.SecretKey != null && newAad.SecretKey == null)
                {
                    newAad.SecretKey = oldAad.SecretKey;
                }
            }
            else
            {
                // Different app/tenant combo
                newAad.DeltaLinkUsers = null;
                newAad.NextLinkUsers = null;
                newAad.DeltaLinkGroups = null;
                newAad.NextLinkGroups = null;

                if (newAad.AppId != null && newAad.SecretKey == null)
                {
                    throw new UserSafeException("A custom App ID requires an app secret");
                }
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
