using Masticore;
using Masticore.Exceptions;
using Masticore.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Soundbite.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Responsible for managing organization-level tokens for Azure/Graph calls.
    /// </summary>
    public class TeamsGraphService : ITeamsGraphService
    {
        #region Constants

        /// <summary>
        /// Defines the buffer at which at token is considered expired for the sake of caching.  We 
        /// want to hold on to the token for as long as possible without getting so close to the 
        /// expiration that it could be invalid for an API call.
        /// </summary>
        public const int TokenExpirationBufferInSeconds = 60;

        /// <summary>
        /// Defines the total number of times the system will attempt to retry token acquisition
        /// if a token request fails for unspecified reasons.  Some failures may indicate that retry
        /// will not work and in those instances retries will not be attempted.
        /// </summary>
        public const int MaxTokenRequestAttempts = 2;

        #endregion

        #region Classes

        /// <summary>
        /// Cache object used for storing access tokens.  The access token received from Azure
        /// contains an "expires in" value that is relative to the time the token was received.
        /// This class uses that value to create the ExpiresAt value which makes that time 
        /// absolute for the purposes of expiration checking.
        /// </summary>
        private class AccessTokenCacheItem : AccessTokenResponse
        {
            /// <summary>
            /// Gets or sets the absolute date/time of token expiration.
            /// </summary>
            public DateTime ExpiresAt { get; set; }
        }

        #endregion

        #region Static Fields

        /// <summary>
        /// Stores per-organization semaphores used for synchronization locks for token acquisition.
        /// Dictionary key is the organization route.
        /// </summary>
        private static ConcurrentDictionary<string, SemaphoreSlim> OrgSyncLocks { get; } = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Stores per-organization teams app token information. 
        /// Dictionary key is the organization route.
        /// </summary>
        private static ConcurrentDictionary<string, AccessTokenCacheItem> TeamsTokenCache { get; } = new ConcurrentDictionary<string, AccessTokenCacheItem>();

        #endregion

        #region Fields

        private readonly ILogger<TeamsGraphService> _logger;
        private readonly IOrgSettingsService _orgSettings;
        private readonly IUserSettingsService _userSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TeamsAppAzureSettings _teamsAppSettings;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="TeamsGraphService"/> instance.
        /// </summary>        
        /// <param name="logger">DI reference.</param>
        /// <param name="httpClientFactory">DI reference.</param>
        /// <param name="teamsAppSettings">DI reference.</param>
        /// <param name="orgSettings">DI reference.</param>
        /// <param name="userSettings">DI reference.</param>
        public TeamsGraphService(
            ILogger<TeamsGraphService> logger,
            IHttpClientFactory httpClientFactory,
            TeamsAppAzureSettings teamsAppSettings,
            IOrgSettingsService orgSettings,
            IUserSettingsService userSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _teamsAppSettings = teamsAppSettings ?? throw new ArgumentNullException(nameof(teamsAppSettings));
            _orgSettings = orgSettings ?? throw new ArgumentNullException(nameof(orgSettings));
            _userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for retrieving the Teams app access token.
        /// </summary>
        /// <param name="orgRoute">Route of the org on whose behalf the access token is being requested.</param>
        /// <returns>a string containing the access token or <c>null</c> if the access token cannot be acquired.</returns>
        public async Task<string> GetTeamsAppToken(string orgRoute)
        {
            if (!GetTeamsAppTokenFromCache(orgRoute, out string token))
            {
                SemaphoreSlim syncLock = GetOrgSyncLockObject(orgRoute);
                try
                {
                    await syncLock.WaitAsync();
                    if (!GetTeamsAppTokenFromCache(orgRoute, out token))
                    {
                        token = await GetTeamsAppTokenFromAzure(orgRoute, 1);
                    }
                }
                finally
                {
                    syncLock.Release();
                }
            }
            return token;
        }

        /// <summary>
        /// Sends a MS Graph API call that generates a MS Teams Notification inside the Teams application.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the user receiving the notification belongs.</param>
        /// <param name="userRoute">Route of the user to whom the message is being sent.</param>
        /// <param name="userUpn">User principal name (UPN) of the user to which the notification is sent (used to build the Graph API call)</param>
        /// <param name="soundbiteTitle">Title to display in the Teams notification.</param>
        /// <param name="soundbiteUrl">URL to which the user is redirected.</param>
        /// <param name="soundbiteAuthor">Author of the Soundbite for which the notification is being sent.</param>
        /// <returns>A loggable description if an event was sent or failed; otherwise null</returns>
        public async Task<string> SendTeamsAppNotification(string orgRoute, string userRoute, string userUpn, string soundbiteTitle, string soundbiteUrl, string soundbiteAuthor)
        {
            try
            {
                Masticore.UserSettings userSettings = await _userSettings.ReadConfigAsync<Masticore.UserSettings>(userRoute);

                if (!userSettings.MsTeams.IsEnabled.IsTrue())
                {
                    _logger.LogInformation($"Skipping teams notification for '{userRoute}'; not enabled for user");
                    return null;
                }

                userUpn = string.IsNullOrEmpty(userSettings.MsTeams.AltId) ? userUpn : userSettings.MsTeams.AltId;
                string token = await GetTeamsAppToken(orgRoute);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception($"Could not retrive teams app token");
                }

                string url = $"https://graph.microsoft.com/v1.0/users/{WebUtility.UrlEncode(userUpn)}/teamwork/sendActivityNotification";
                HttpClient httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpContent body = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(
                    new TeamsActivityNotificationRequest("text", soundbiteTitle, soundbiteUrl, "soundbiteCreated", soundbiteAuthor)),
                    Encoding.UTF8,
                    "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(url, body);

                // Determine if there was an error and whether that error indicates the
                // user does not have the Soundbite app installed
                // if (response.StatusCode == HttpStatusCode.Forbidden)
                // {
                //     string content = await response.Content.ReadAsStringAsync();
                //     if (content.Contains(_teamsAppSettings.ClientId))
                //     {
                //         await _userSettings.AutoDisableTeams(userRoute);
                //     }
                // }

                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Received status code {response.StatusCode} trying to send message with content '{content}'");
                }

                return $"{userUpn?.RedactEmail()}";
            }
            catch (Exception ex)
            {
                string msg = $"Failed to send Teams notification to user '{userRoute}' with email or token '{userUpn?.RedactEmail()}' in org '{orgRoute}' with error '{ex.Message}' and stack: {ex.StackTrace}";
                _logger.LogError(msg);
                throw new Exception(msg, ex);
            }
        }


        /// <summary>
        /// Sends a MS Graph API call that generates a MS Teams Notification inside the Teams application.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the user receiving the notification belongs.</param>
        /// <param name="userRoute">Route of the user to whom the message is being sent.</param>
        /// <param name="emailOrUserId">Email or ID of the user to which the notification is sent (used to build the Graph API call)</param>
        /// <param name="soundbiteTitle">Title to display in the Teams notification.</param>
        /// <param name="soundbiteUrl">URL to which the user is redirected.</param>
        /// <param name="soundbiteAuthor">Author of the Soundbite for which the notification is being sent.</param>
        /// <returns>A loggable description if an event was sent or failed; otherwise null</returns>
        public async Task SendTeamsAppMassNotification(string orgRoute, IEnumerable<string> userPrincipalNames, string soundbiteTitle, string soundbiteUrl, string soundbiteAuthor)
        {
            try
            {
                string token = await GetTeamsAppToken(orgRoute);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception($"Could not retrive teams app token");
                }

                string url = $"https://graph.microsoft.com/v1.0/teamwork/sendActivityNotificationToRecipients";
                HttpClient httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpContent body = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(
                    new TeamsActivityMassNotificationRequest("text", soundbiteTitle, soundbiteUrl, "soundbiteCreated", soundbiteAuthor,
                    userPrincipalNames.Select(userUpn => new TeamsActivityMassNotificationRecipient() { UserId = userUpn }))),
                    Encoding.UTF8,
                    "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(url, body);

                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Received status code {response.StatusCode} trying to send message with content '{content}'");
                }
            }
            catch (Exception ex)
            {
                string msg = $"Failed to send mass Teams notification in org '{orgRoute}' with error '{ex.Message}' and stack: {ex.StackTrace}";
                _logger.LogError(msg);
                throw new Exception(msg, ex);
            }
        }

        /// <summary>
        /// Sends a MS Graph Batch API call that sends multiple MS Teams Notifications inside the Teams application.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the user receiving the notification belongs.</param>
        /// <param name="userRoute">Route of the user to whom the message is being sent.</param>
        /// <param name="emailOrUserId">Email or ID of the user to which the notification is sent (used to build the Graph API call)</param>
        /// <param name="soundbiteTitle">Title to display in the Teams notification.</param>
        /// <param name="soundbiteUrl">URL to which the user is redirected.</param>
        /// <param name="soundbiteAuthor">Author of the Soundbite for which the notification is being sent.</param>
        /// <returns></returns>        
        public async Task<List<HttpResponseMessage>> SendTeamsAppBatchNotification(string orgRoute, IList<string> userPrincipalNames, string soundbiteTitle, string soundbiteUrl, string soundbiteAuthor)
        {
            try
            {
                // Acquire the Graph API token
                string token = await GetTeamsAppToken(orgRoute);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception($"Could not retrive teams app token");
                }

                // Setup Graph client that uses the API token
                GraphServiceClient graphClient = new(
                new DelegateAuthenticationProvider(r =>
                {
                    r.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                    return Task.FromResult(0);
                }));

                // Setup batch content for population
                BatchRequestContent batchRequestContent = new();


                // Iterate over all requested users
                for (int requestId = 0; requestId < userPrincipalNames.Count; requestId++)
                {
                    HttpRequestMessage httpRequestMsg = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/users/{WebUtility.UrlEncode("damon@soundbite.ai")}/teamwork/sendActivityNotification");
                    httpRequestMsg.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(
                            new TeamsActivityNotificationRequest("text", soundbiteTitle, soundbiteUrl, "soundbiteCreated", soundbiteAuthor)),
                            Encoding.UTF8,
                            "application/json");
                    batchRequestContent.AddBatchRequestStep(new BatchRequestStep($"{requestId}", httpRequestMsg));
                }

                // Execute the batch and retrieve the responses
                BatchResponseContent batchResponseContent = await graphClient.Batch.Request().PostAsync(batchRequestContent);
                Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();
                List<HttpResponseMessage> result = new List<HttpResponseMessage>();

                // Ensure results are matched to list indexes
                for (int requestId = 0; requestId < userPrincipalNames.Count; requestId++)
                {
                    result.Add(responses[$"{requestId}"]);
                }

                return result;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to send batch Teams notification in org '{orgRoute}' with error '{ex.Message}' and stack: {ex.StackTrace}";
                _logger.LogError(msg);
                throw new Exception(msg, ex);
            }
        }

        /// <summary>
        /// Sends a MS Graph Batch API call that sends multiple MS Teams Notifications inside the
        /// Teams application. The response from this call is a dictionary containing HTTP response
        /// information keyed to the SessionNotificationId associated with the call.
        /// </summary>
        /// <param name="orgRoute">Route of the organization associated with the session for which notifications are being sent.</param>
        /// <param name="notificationRequests">Requests to fulfill.</param>
        /// <returns>a dictionary with keys matched to the SessionNotificationId in the <see cref="TeamsAppBatchNotificationItem.Recipients"/> items of the <paramref name="notificationRequests"/>.</returns>
        public async Task<IDictionary<string, HttpResponseMessage>> SendTeamsAppBatchNotification(string orgRoute, IEnumerable<TeamsAppBatchNotificationItem> notificationRequests, string orgGraphToken = null)
        {
            try
            {
                // Ensure Graph API token
                if (string.IsNullOrEmpty(orgGraphToken))
                {
                    orgGraphToken = await GetTeamsAppToken(orgRoute);
                    if (string.IsNullOrEmpty(orgGraphToken))
                    {
                        throw new Exception($"Could not retrive teams app token");
                    }
                }

                // Setup Graph client that uses the API token
                GraphServiceClient graphClient = new(
                new DelegateAuthenticationProvider(r =>
                {
                    r.Headers.Authorization = new AuthenticationHeaderValue("bearer", orgGraphToken);
                    return Task.FromResult(0);
                }));

                // Setup batch content for population
                BatchRequestContent batchRequestContent = new();

                // Iterate over all requests
                notificationRequests.ToList().ForEach((request, requestIndex) =>
                {
                    // Iterate over all of the users in the request
                    request.Recipients.ToList().ForEach((recipient, index) =>
                    {
                        HttpRequestMessage httpRequestMsg = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/users/{recipient.Upn}/teamwork/sendActivityNotification");
                        string contentJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                                new TeamsActivityNotificationRequest("text", request.Title, request.Url, "soundbiteCreated", request.Author));
                        httpRequestMsg.Content = new StringContent(contentJson, Encoding.UTF8, "application/json");
                        batchRequestContent.AddBatchRequestStep(new BatchRequestStep(recipient.SessionNotificationId.ToString(), httpRequestMsg));
                    });
                });

                // Execute the batch and retrieve the responses
                BatchResponseContent batchResponseContent = await graphClient.Batch.Request().PostAsync(batchRequestContent);
                Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();
                return responses;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to send batch Teams notification in org '{orgRoute}' with error '{ex.Message}' and stack: {ex.StackTrace}";
                _logger.LogError(msg);
                throw new Exception(msg, ex);
            }
        }

        #endregion

        #region Methods - Utility

        /// <summary>
        /// Performs basic validation checks on the specified tenant ID value.
        /// </summary>
        /// <param name="tenantId">Tenant ID to validate.</param>
        /// <exception cref="UserSafeException">Thrown when the tenant ID is invalid.</exception>
        private void ValidateTenantId(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new UserSafeException("Tenant ID is null or empty.");
            }
            else
            {
                if (tenantId.Length != 36)
                {
                    throw new UserSafeException($"Tenant ID is invalid (length is wrong)");
                }
            }
        }

        /// <summary>
        /// Acquires the sync lock object for a given organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose sync lock object is being sought.</param>
        /// <returns>a SemaphoreSlim that can be used to syncronize multi-threaded operations for the organization.</returns>
        /// <exception cref="Exception">Thrown if a sync lock object cannot be acquired.</exception>
        private SemaphoreSlim GetOrgSyncLockObject(string orgRoute)
        {
            // Attempt to get the sync lock object
            if (!OrgSyncLocks.TryGetValue(orgRoute, out SemaphoreSlim syncLock))
            {
                // Not found so add it
                syncLock = new SemaphoreSlim(1, 1);
                if (!OrgSyncLocks.TryAdd(orgRoute, syncLock))
                {
                    // Add failed - see if someone else added it
                    if (!OrgSyncLocks.TryGetValue(orgRoute, out syncLock))
                    {
                        // Give up because something is wonky
                        throw new Exception("Failed to acquire organization sync lock object.");
                    }
                }
            }
            return syncLock;
        }

        /// <summary>
        /// Determines whether cached token should be invalidated due to its expiration date.
        /// </summary>
        /// <param name="tokenCacheItem">Token whose validity is being verified.</param>
        /// <returns><c>true</c> if the token is expired, otherwise <c>false</c>.</returns>
        private bool IsTokenExpired(AccessTokenCacheItem tokenCacheItem)
        {
            return tokenCacheItem.ExpiresAt <= DateTime.UtcNow.AddSeconds(TokenExpirationBufferInSeconds);
        }

        /// <summary>
        /// Retrieves a Teams App access token from the cache if available.
        /// </summary>
        /// <param name="orgRoute">Route of the org whose Teams access token is being sought.</param>
        /// <param name="token">Output parameter into which the token value is stored if found.</param>
        /// <returns><c>true</c> if a valid access token was found in the cache, otherwise <c>false</c>.</returns>
        private bool GetTeamsAppTokenFromCache(string orgRoute, out string token)
        {
            if (TeamsTokenCache.TryGetValue(orgRoute, out AccessTokenCacheItem cachedToken))
            {
                if (!IsTokenExpired(cachedToken))
                {
                    // Cached item was found and is valid
                    token = cachedToken.AccessToken;
                    return true;
                }
                else
                {
                    // Attempt to remove the cached token
                    TeamsTokenCache.TryRemove(orgRoute, out cachedToken);
                }
            }

            token = null;
            return false;
        }

        /// <summary>
        /// Calls the Graph API to acquire the Teams App access token.
        /// </summary>
        /// <param name="orgRoute">Route of the organization for which a teams app access token is being sought.</param>
        /// <param name="attempt">Identifies the current attempt index used for retries.</param>
        /// <returns>a string containing the Teams App access token or <c>null</c> if the token cannot be retrieved.</returns>
        private async Task<string> GetTeamsAppTokenFromAzure(string orgRoute, int attempt)
        {
            if (attempt <= MaxTokenRequestAttempts)
            {
                try
                {
                    OrgSettings config = await _orgSettings.ReadOrgSettingsAsync<OrgSettings>(orgRoute);
                    Validator.NotNull(config, "Organization does not have any configuration settings defined.");

                    if (!config.Azure.EnableTeamsNotifications)
                    {
                        // Teams notifications are not allowed so do not bother getting a token.
                        return null;
                    }

                    string tenantId = config?.Azure?.TenantId;
                    ValidateTenantId(tenantId);
                    string url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    HttpClient httpClient = _httpClientFactory.CreateClient();
                    HttpContent body = new FormUrlEncodedContent(
                        new List<KeyValuePair<string, string>>() {
                            new KeyValuePair<string,string>("client_id",_teamsAppSettings.ClientId),
                            new KeyValuePair<string,string>("scope",".default"),
                            new KeyValuePair<string,string>("client_secret",_teamsAppSettings.SecretKey),
                            new KeyValuePair<string,string>("grant_type","client_credentials"),
                        });

                    HttpResponseMessage response = await httpClient.PostAsync(url, body);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation($"Successfully retrived team app token for org '{orgRoute}' on {attempt} attempt");
                        string responseBody = await response.Content.ReadAsStringAsync();
                        AccessTokenCacheItem cacheItem = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenCacheItem>(responseBody);
                        cacheItem.ExpiresAt = DateTime.UtcNow.AddSeconds(cacheItem.ExpiresIn);
                        TeamsTokenCache.TryAdd(orgRoute, cacheItem);
                        return cacheItem.AccessToken;
                    }
                    else
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed to acquire Teams App Token for OrgRoute ({orgRoute}) [Attempt {attempt}/{MaxTokenRequestAttempts}]:Invalid HTTP Response\r\nCode: {response.StatusCode};\r\nReason:{response.ReasonPhrase};\r\nContent:{responseBody};");
                        string truncatedKey = _teamsAppSettings.SecretKey?.Length >= 4 ? _teamsAppSettings.SecretKey.Substring(0, 4) : "none";
                        _logger.LogInformation($"SB INFO: ID={_teamsAppSettings.ClientId} KEY={truncatedKey}**** URL={url}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to acquire Teams App Token for OrgRoute ({orgRoute}) [Attempt {attempt}/{MaxTokenRequestAttempts}] with error {ex.Message}");
                }

                // By virtue of reaching this point the method should retry token acquisition
                return await GetTeamsAppTokenFromAzure(orgRoute, attempt + 1);
            }
            else
            {
                string msg = $"Failed to acquire Teams App Token for OrgRoute ({orgRoute}) after {MaxTokenRequestAttempts} attempts.";
                _logger.LogError(msg);
                throw new Exception(msg);
            }
        }

        #endregion
    }
}