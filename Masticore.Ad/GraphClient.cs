using Masticore.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Masticore.Ad
{
    /// <summary>
    /// Implements connecting to AAD and calling MS graph https://docs.microsoft.com/en-us/graph/auth-v2-service;
    /// Useful for both user-interactive AND system background MS Graph interactions
    /// </summary>
    /// <remarks>This is intended to support low-level, custom work against the graph client while <see cref="MsGraph"/> provides higher-level interaction via the official MS client</remarks>
    public class GraphClient : IGraphClient
    {
        #region Fields

        protected readonly ILogger _logger;     // Logger reference
        protected readonly string _tenantId;
        protected readonly string _appId;
        protected readonly string _appSecret;

        protected HttpClient _httpClient;       // HTTP client used for making web api calls
        protected string _accessToken;          // Stores the MS graph access token

        #endregion

        #region Constructor

        public GraphClient(ILogger logger, IAdAppSettings adAppSettings, string tenantId)
        {
            if (adAppSettings is null)
            {
                throw new ArgumentNullException(nameof(adAppSettings));
            }

            if (string.IsNullOrEmpty(adAppSettings.AppId))
            {
                throw new ArgumentException($"'{nameof(adAppSettings.AppId)}' cannot be null or empty.", nameof(adAppSettings.AppId));
            }

            if (string.IsNullOrEmpty(adAppSettings.AppSecret))
            {
                throw new ArgumentException($"'{nameof(adAppSettings.AppSecret)}' cannot be null or empty.", nameof(adAppSettings.AppSecret));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or empty.", nameof(tenantId));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantId = tenantId;
            _appId = adAppSettings.AppId;
            _appSecret = adAppSettings.AppSecret;
        }

        /// <summary>
        /// Instantiates a new <see cref="GraphClient"/> instance.
        /// </summary>
        public GraphClient(ILogger logger, string tenantId, string appId, string appSecret)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or empty.", nameof(tenantId));
            }

            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentException($"'{nameof(appId)}' cannot be null or empty.", nameof(appId));
            }

            if (string.IsNullOrEmpty(appSecret))
            {
                throw new ArgumentException($"'{nameof(appSecret)}' cannot be null or empty.", nameof(appSecret));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantId = tenantId;
            _appId = appId;
            _appSecret = appSecret;
        }

        #endregion

        #region Methods - Graph Calls

        public void Reset()
        {
            _httpClient?.Dispose();
            _httpClient = null;
            _accessToken = null;
        }

        /// <summary>
        /// Retrives the Graph access token for the current config
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessToken()
        {
            if (_accessToken != null)
            {
                return _accessToken;
            }

            HttpResponseMessage response;

            try
            {
                string url = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";
                string content = $"client_id={_appId}&scope=https%3A%2F%2Fgraph.microsoft.com%2F.default&client_secret={_appSecret}&grant_type=client_credentials";
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")
                };
                // Use a one-off, fresh HttpClient for the auth request
                using HttpClient authClient = new HttpClient();
                response = await authClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                string msg = $"Failed auth request to MsGraph with error {ex.Message} with stack {ex.StackTrace}";
                _logger.LogError(ex, msg);
                throw;
            }

            try
            {
                GraphTokenResponse tokenResponse = await response.Content.ReadAsAsync<GraphTokenResponse>();
                _accessToken = tokenResponse.access_token;
                return _accessToken;
            }
            catch (Exception ex)
            {
                string msg = $"Auth response sucessfully received but failed to parse graph token response with error {ex.Message} and stack {ex.StackTrace}";
                _logger.LogError(ex, msg);
                throw;
            }
        }

        /// <summary>
        /// Returns a fully initialized and cached httpClient
        /// </summary>
        /// <returns></returns>
        public async Task<HttpClient> GetHttpClient()
        {
            if (_httpClient == null)
            {
                string accessToken = await GetAccessToken();
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            }
            return _httpClient;
        }

        /// <summary>
        /// Async GET the given object type from the given url
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<TResponse> Get<TResponse>(string url)
        {
            _logger.LogDebug("GETing from MS Graph API with URL '{0}'", url);

            HttpResponseMessage response;
            try
            {
                HttpClient client = await GetHttpClient();
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                string msg = $"Failed MsGraph GET request with error {ex.Message} with stack {ex.StackTrace}";
                _logger.LogError(ex, msg);
                throw;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = $"MsGraph GET Request completed, but received status code: {response.StatusCode}";
                _logger.LogError($"{msg} with message: {response.Content}");

                throw new GraphClientException(msg)
                {
                    StatusCode = response.StatusCode
                };
            }

            try
            {
                TResponse data = await response.Content.ReadAsAsync<TResponse>();
                string str = await response.Content.ReadAsStringAsync();
                if (data == null)
                {
                    throw new Exception($"Null response trying to GET url {url}");
                }
                return data;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to read MsGraph response with error {ex.Message} with stack {ex.StackTrace}";
                _logger.LogError(ex, msg);
                throw;
            }
        }

        /// <summary>
        /// Async POST the given body to the given URL
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="jsonBody"></param>
        /// <returns></returns>
        public async Task<TResponse> Post<TResponse>(string url, string jsonBody)
        {
            _logger.LogDebug("POSTing from MS Graph API with URL '{0}' and body '{1}'", url, jsonBody);

            HttpResponseMessage response;
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };
                HttpClient client = await GetHttpClient();
                response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                string msg = $"Failed POST request to MsGraph with error {ex.Message} with stack {ex.StackTrace}";
                _logger.LogError(ex, msg);
                throw;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string msg = $"MsGraph POST Request completed, but received status code: {response.StatusCode}";
                _logger.LogError($"{msg} with message: {response.Content}");

                throw new GraphClientException(msg)
                {
                    StatusCode = response.StatusCode
                };
            }

            try
            {
                TResponse data = await response.Content.ReadAsAsync<TResponse>();
                if (data == null)
                {
                    throw new UserSafeException($"Null response trying to POST to url {url}");
                }
                return data;
            }
            catch (Exception ex)
            {
                string msg = $"Failed to read MsGraph POST response; error {ex.Message} with stack {ex.StackTrace}";
                _logger.LogError(ex, msg);
                throw;
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes of managed .NET resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Responsible for disposing of managed resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reset();
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~GraphClient()
        {
            Dispose(false);
        }

        #endregion
    }
}
