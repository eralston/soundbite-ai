using Masticore.Exceptions;
using Masticore.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Masticore.DirectorySync.Okta
{
    //NOTE: There is an OKTA API package available on NUGET but for some reason it completely screws
    // up all the versioning in the solution.

    /// <summary>
    /// OKTA API Service used for calling the OKTA web API.
    /// </summary>
    public class OktaApiService
    {
        #region Header Class

        /// <summary>
        /// Stores OKTA information extracted from the response headers.
        /// </summary>
        private class HeaderInfo
        {
            #region Constants

            /// <summary>
            /// Threshold of remaining calls that when met results in the application waiting for
            /// the rate reset limit date/time before making any additional calls.
            /// </summary>
            public const int RateLimitRemainingThreshold = 3;

            #endregion

            #region Properties

            /// <summary>
            /// Stores the URL for the next "page" of data in a paged query.
            /// </summary>
            public string NextLink { get; set; }

            /// <summary>
            /// Gets the number of web API calls remaining before the rate limit is exceeded.
            /// </summary>
            public int? RateLimitRemaining { get; set; }

            /// <summary>
            /// Gets the date/time when the rate limit is reset.
            /// </summary>
            public DateTime? RateLimitReset { get; set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Instantiates a new <see cref="HeaderInfo"/> instance.
            /// </summary>
            /// <param name="parent">Reference to the parent OktaApiService on which the rate reset limit value is maintained.</param>
            /// <param name="response">HTTP response from which headers should be extracted.</param>
            public HeaderInfo(OktaApiService parent, HttpResponseMessage response)
            {
                // Extract the next "page" link header value
                NextLink = response.Headers
                    .Where(i => string.Equals(i.Key.ToLower(), "link"))
                    .Select(i => ExtractLink(
                        i.Value.FirstOrDefault(value =>
                            value.Contains("rel=\"next\"",
                            System.StringComparison.InvariantCultureIgnoreCase))))
                    .FirstOrDefault();

                // Extract the rate limit remaining header value
                RateLimitRemaining = response.Headers
                    .Where(i => string.Equals(i.Key.ToLower(), "x-rate-limit-remaining"))
                    .Select(i => int.TryParse(i.Value.FirstOrDefault(), out int result) ? result : (int?)null)
                    .FirstOrDefault();

                // Extract the rate limit reset header value (if the remaining count is low enough)
                if (RateLimitRemaining <= RateLimitRemainingThreshold)
                {
                    // The rate limit reset is a UNIX epoch integer in minutes.
                    RateLimitReset = response.Headers
                        .Where(i => string.Equals(i.Key.ToLower(), "x-rate-limit-reset"))
                        .Select(i => int.TryParse(i.Value.FirstOrDefault(), out int unixEpochSeconds)
                            ? DateTimeOffset.FromUnixTimeSeconds(unixEpochSeconds).DateTime : (DateTime?)null)
                        .FirstOrDefault();

                    if (parent.RateLimitReset == null || RateLimitReset > parent.RateLimitReset)
                    {
                        parent.RateLimitReset = RateLimitReset;
                    }
                }
            }

            #endregion

            #region Methods

            public static void ProcessRateLimits(OktaApiService parent, HttpResponseMessage response)
            {
                // Constructing a new header sets all of the appropriate values to track rate limits
                new HeaderInfo(parent, response);
            }

            /// <summary>
            /// Extracts the link URL from the link header.
            /// </summary>
            /// <param name="rawLinkValue">Link header value from which the link URL should be extracted.</param>
            /// <returns>Link URL without extraneous characters.</returns>
            private static string ExtractLink(string rawLinkValue)
            {
                return !string.IsNullOrEmpty(rawLinkValue?.Trim())
                    ? rawLinkValue.Split("<")[1].Split(">")[0] : null;
            }

            #endregion
        }

        #endregion

        #region Properties

        /// <summary>
        /// Stores a reference to the web client used to make HTTP calls.
        /// </summary>
        private HttpClient WebClient { get; set; }

        /// <summary>
        /// Gets or sets a reference to the type-specific organization sync settings.
        /// </summary>
        private OktaOrgSyncConfig Settings { get; set; }

        /// <summary>
        /// Stores a value indicating when the rate limit for the API will reset.  When this value
        /// is set the next API call should wait until after this date/time before making the call
        /// to ensure that the API call will not be rejected due to API rate limits.
        /// </summary>
        private DateTime? RateLimitReset { get; set; }

        /// <summary>
        /// Gets or sets the total number of times an API call had to wait due to throttling.
        /// </summary>
        public int TotalWaitCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total duration API calls had to wait due to throttling.
        /// </summary>
        public TimeSpan TotalWaitDuration { get; set; } = new TimeSpan(0);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="OktaApiService"/> instance.
        /// </summary>
        /// <param name="httpClientFactory">DI reference to an HTTP Client Factory used to acquire an HTTP Client for APi calls.</param>
        public OktaApiService(IHttpClientFactory httpClientFactory, OktaOrgSyncConfig currentSettings, OktaOrgSyncConfig oldSettings = null)
        {
            currentSettings.ApiToken ??= oldSettings?.ApiToken;

            Validator.ArgNotNull("OKTA Settings", currentSettings);
            Validator.ArgNotNullOrEmpty("OKTA Settings - Domain", currentSettings.Domain);
            Validator.ArgNotNullOrEmpty("OKTA Settings - API Token", currentSettings.ApiToken);

            Settings = currentSettings;
            WebClient = httpClientFactory.CreateClient();
            WebClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SSWS", currentSettings.ApiToken);
            WebClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            WebClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            //WebClient.DefaultRequestHeaders.TryAddWithoutValidation("okta-response", "omitCredentials,omitCredentialsLinks,omitTransitioningToStatus");

            while (currentSettings.Domain.EndsWith("/"))
            {
                currentSettings.Domain = currentSettings.Domain.Substring(0, currentSettings.Domain.Length - 1);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for prefixing a relative URL with the appropriate OKTA domain value.
        /// </summary>
        /// <param name="relativeUrl">Relative URL of the API call.</param>
        /// <returns>a complete URL ready for use in API calls.</returns>
        private string BuildUrl(string relativeUrl)
        {
            return Settings.Domain + relativeUrl;
        }

        /// <summary>
        /// Responsible for building out the paged / filterd API for api call.
        /// </summary>
        /// <param name="page">Paging information.</param>
        /// <param name="lastModifiedStart">Lower bound of the last modified date filter.</param>
        /// <param name="lastModifiedEnd">Upper bound of the last modified date filter.</param>
        /// <param name="after">ID of the item after which items should be returned (used for paging or resuming).</param>
        /// <returns>a URL containing the paging and filtering elements that can be used to call the GetUsers API</returns>
        private string BuildUrl(string relativeUrl, TokenPageRequest page, DateTime? lastModifiedStart, DateTime? lastModifiedEnd, string after)
        {
            relativeUrl += $"?limit={page.Take}";
            if (!string.IsNullOrEmpty(page.Filter))
            {
                relativeUrl += $"&q={HttpUtility.UrlEncode(page.Filter)}";
            }
            if (lastModifiedStart != null || lastModifiedEnd != null)
            {
                // Parameter is always needed
                relativeUrl += "&search=";

                // Append the starting date filter query if needed
                if (lastModifiedStart != null)
                {
                    relativeUrl += $"{HttpUtility.UrlEncode($"lastUpdated gt \"{lastModifiedStart.Value:yyyy-MM-dd'T'HH:mm:ss.fffZ}\"")}";
                }

                // Append the & between both filters if both filters are in use
                if (lastModifiedStart != null && lastModifiedEnd != null)
                {
                    relativeUrl += " and ";
                }

                // Append the ending date filter query if needed
                if (lastModifiedEnd != null)
                {
                    relativeUrl += $"{HttpUtility.UrlEncode($"lastUpdated le \"{lastModifiedEnd.Value:yyyy-MM-dd'T'HH:mm:ss.fffZ}\"")}";
                }
            }
            if (!string.IsNullOrEmpty(after))
            {
                relativeUrl += $"&after={after}";
            }
            relativeUrl = BuildUrl(relativeUrl);
            return relativeUrl;
        }

        /// <summary>
        /// Determines whether the rate limit reset value has been set and if so delays until the
        /// rate limit reset value has been reached to avoid exceeding API call limits.
        /// </summary>
        /// <returns>a task indicating completion of the operation.</returns>
        private async Task RunRateLimitChecks()
        {
            if (RateLimitReset != null)
            {
                TimeSpan delay = (RateLimitReset != null && RateLimitReset > DateTime.UtcNow)
                    ? (TimeSpan)(RateLimitReset - DateTime.UtcNow)
                    : new TimeSpan(0, 0, 30);
                TotalWaitCount++;
                TotalWaitDuration = new TimeSpan(TotalWaitDuration.Ticks + delay.Ticks);
                await Task.Delay(delay);
                RateLimitReset = null;
            }
        }

        /// <summary>
        /// Retrieves the specified group from OKTA.
        /// </summary>
        /// <param name="groupId">ID of the group in OKTA.</param>
        /// <returns>the requested group information or <c>null</c> if the group is not found.</returns>
        public async Task<OktaGroupResponse> GetGroup(string groupId)
        {
            await RunRateLimitChecks();
            HttpResponseMessage response = await WebClient.GetAsync(BuildUrl($"/api/v1/groups/{groupId}"));
            HeaderInfo.ProcessRateLimits(this, response);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                OktaGroupResponse result = Newtonsoft.Json.JsonConvert.DeserializeObject<OktaGroupResponse>(json);
                return result;
            }
            else
            {
                throw new UserSafeException($"Failed to retrieve group from the OKTA API (GroupID:{groupId}) (HTTP {response.StatusCode} - {response.ReasonPhrase})");
            }
        }

        /// <summary>
        /// Retrieves a "page" of groups from the group service.
        /// </summary>
        /// <param name="lastModifiedDateStart">Lower bound of the last modified date filter.</param>
        /// <param name="lastModifiedDateEnd">Upper bound of the last modified date filter.</param>        
        /// <param name="page">Paging information.</param>
        /// <returns>A page response containing the requested groups with additional paging information.</returns>
        /// <exception cref="UserSafeException">Thrown in the event of an HTTP error.</exception>
        public async Task<TokenPageResponse<OktaGroupResponse>> GetGroups(TokenPageRequest page = null, DateTime? lastModifiedDateStart = null, DateTime? lastModifiedDateEnd = null, string after = null)
        {
            // When no page is requested use a default page
            page = page ?? new TokenPageRequest() { Take = 50 };

            string url = !string.IsNullOrEmpty(page.SkipToken)
                ? page.SkipToken
                : BuildUrl("/api/v1/groups", page, lastModifiedDateStart, lastModifiedDateEnd, after);

            await RunRateLimitChecks();
            HttpResponseMessage response = await WebClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                TokenPageResponse<OktaGroupResponse> result = new TokenPageResponse<OktaGroupResponse>(page);
                HeaderInfo headerInfo = new HeaderInfo(this, response);
                result.SkipToken = headerInfo.NextLink;
                string json = await response.Content.ReadAsStringAsync();
                result.Result = Newtonsoft.Json.JsonConvert.DeserializeObject<OktaGroupResponse[]>(json);
                return result;
            }
            else
            {
                throw new UserSafeException($"Failed to retrieve users from OKTA directory (HTTP {response.StatusCode} - {response.ReasonPhrase})");
            }
        }

        /// <summary>
        /// Retrieves a "page" of group members for the specified group.
        /// </summary>
        /// <param name="groupId">Group ID of the group whose members are being sought.</param>
        /// <param name="page">Page request containing information on which page of group of member is being sought.</param>
        /// <returns>A page response containing the requested group members with additional paging information.</returns>
        /// <exception cref="UserSafeException">Thrown in the event of an HTTP error.</exception>
        public async Task<TokenPageResponse<OktaUserResponse>> GetGroupMembers(string groupId, TokenPageRequest page = null)
        {
            // When no page is requested use a default page
            page = page ?? new TokenPageRequest() { Take = 200 };

            string url = page.SkipToken;
            if (string.IsNullOrEmpty(url))
            {
                url = $"/api/v1/groups/{groupId}/users?limit={page.Take}";
                if (!string.IsNullOrEmpty(page.Filter))
                {
                    url += $"&q={HttpUtility.UrlEncode(page.Filter)}";
                }
                url = BuildUrl(url);
            }

            await RunRateLimitChecks();
            HttpResponseMessage response = await WebClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                TokenPageResponse<OktaUserResponse> result = new TokenPageResponse<OktaUserResponse>(page);
                HeaderInfo headers = new HeaderInfo(this, response);
                result.SkipToken = headers.NextLink;
                string json = await response.Content.ReadAsStringAsync();
                result.Result = Newtonsoft.Json.JsonConvert.DeserializeObject<OktaUserResponse[]>(json);
                return result;
            }
            else
            {
                throw new UserSafeException($"Failed to retrieve users from OKTA directory (HTTP {response.StatusCode} - {response.ReasonPhrase})");
            }
        }

        /// <summary>
        /// Retrieves the specified user from OKTA.
        /// </summary>
        /// <param name="userId">ID of the user in OKTA.</param>
        /// <returns>the requested group information or <c>null</c> if the group is not found.</returns>
        /// <exception cref="UserSafeException">Thrown in the event of an HTTP error.</exception>
        public async Task<OktaUserResponse> GetUser(string userId)
        {
            await RunRateLimitChecks();
            HttpResponseMessage response = await WebClient.GetAsync(BuildUrl($"/api/v1/users/{userId}"));
            HeaderInfo.ProcessRateLimits(this, response);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                OktaUserResponse result = Newtonsoft.Json.JsonConvert.DeserializeObject<OktaUserResponse>(json);
                return result;
            }
            else
            {
                throw new UserSafeException($"Failed to retrieve user from the OKTA API (UserID:{userId}) (HTTP {response.StatusCode} - {response.ReasonPhrase})");
            }
        }

        /// <summary>
        /// Retrieves a "page" of users from the user service.
        /// </summary>
        /// <param name="page">Page request identifying the page of information being sought.</param>
        /// <param name="search">Search parameters to pass to the query.</param>
        /// <returns>a response containing page information and results.</returns>
        /// <exception cref="UserSafeException">Thrown in the event of an HTTP error.</exception>
        public async Task<TokenPageResponse<OktaUserResponse>> GetUsers(TokenPageRequest page = null, DateTime? lastModifiedStart = null, DateTime? lastModifiedEnd = null, string after = null)
        {
            // When no page is requested use a default page
            page = page ?? new TokenPageRequest() { Take = 200 };

            string url = !string.IsNullOrEmpty(page.SkipToken)
                    ? page.SkipToken
                    : BuildUrl("/api/v1/users", page, lastModifiedStart, lastModifiedEnd, after);

            await RunRateLimitChecks();
            HttpResponseMessage response = await WebClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                TokenPageResponse<OktaUserResponse> result = new TokenPageResponse<OktaUserResponse>(page);
                HeaderInfo headerInfo = new HeaderInfo(this, response);
                result.SkipToken = headerInfo.NextLink;
                string json = await response.Content.ReadAsStringAsync();
                result.Result = Newtonsoft.Json.JsonConvert.DeserializeObject<OktaUserResponse[]>(json);
                return result;
            }
            else
            {
                throw new UserSafeException($"Failed to retrieve users from OKTA directory (HTTP {response.StatusCode} - {response.ReasonPhrase})");
            }
        }

        /// <summary>
        /// Responsible for accessing the API in some trivial way to verify access is working.
        /// </summary>
        /// <returns>Flag indicating whether access is working (<c>true</c>) or not (<c>false</c>).</returns>
        public async Task<bool> VerifyAccess()
        {
            try
            {
                await RunRateLimitChecks();
                HttpResponseMessage response = await WebClient.GetAsync(BuildUrl($"/api/v1/users?limit=1"));
                HeaderInfo.ProcessRateLimits(this, response);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
