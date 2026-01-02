using Masticore.Exceptions;
using Masticore.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Interact
{
    /// <summary>
    /// Service for calling out to the Interact Web API
    /// </summary>
    public class InteractApiService
    {
        #region Constants

        public const string MissingAccessToken = "Interact API authentication token is missing.";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a reference to the web client used for HTTP calls.
        /// </summary>
        private HttpClient WebClient { get; set; }

        /// <summary>
        /// Gets or sets the token information retrieved from authenticating against the API.
        /// </summary>
        private TokenResponse TokenInfo { get; set; }

        /// <summary>
        /// Gets or sets the directory sync configuration settings containing information on how to
        /// access the Interact API
        /// </summary>
        private InteractOrgConfig Settings { get; set; }

        #endregion

        #region Constructor

        public InteractApiService(IHttpClientFactory httpFactory)
        {
            WebClient = httpFactory.CreateClient();
        }

        #endregion

        #region Methods - API

        /// <summary>
        /// Calls out to the Interact API to retrieve an access token for use with the Interact API.
        /// </summary>
        /// <returns>A <see cref="TokenResponse"/> populated with information from the web service call.</returns>
        public async Task<TokenResponse> GetToken()
        {
            try
            {
                TokenResponse result = await WebClient.PostAsync<TokenResponse>(BuildUrl("token"),
                    new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        grant_type = "password",
                        username = Settings.Username,
                        password = Settings.Password
                    })));
                return result;
            }
            catch (Exception ex)
            {
                throw new UserSafeException("Could not acquire Interact API token.", ex);
            }
        }

        /// <summary>
        /// Gets the specified user's information.
        /// </summary>
        /// <param name="id">Unique ID of the user to acquire.</param>
        /// <returns>a <see cref="PersonResponse"/> containing the person's information.</returns>
        /// <exception cref="UserSafeException">Throws an exception if the user is not found or other issues occur.</exception>
        public async Task<PersonResponse> GetPerson(string id)
        {
            try
            {
                await EnsureAuthToken();
                PersonResponse result = await WebClient.GetAsync<PersonResponse>(BuildUrl($"people/{id}"));
                return result;
            }
            catch (Exception ex)
            {
                throw new UserSafeException($"Could not acquire user (id:'{id}') from Interact API.", ex);
            }
        }

        /// <summary>
        /// Paged query for searching and retrieving people in the Interact directory.
        /// </summary>
        /// <param name="pageRequest">Defines which page of data is being requested.</param>
        /// <param name="alphaFilter">Alphabetic filter used to limit people by letter.  Used when interacting with more than 10k people in a directory.</param>
        /// <returns></returns>
        /// <exception cref="UserSafeException"></exception>
        public async Task<IndexPageResponse<PersonResponse>> GetPeople(IndexPageRequest pageRequest = null, string alphaFilter = null)
        {
            // Populate the page request with default if none were provided
            if (pageRequest == null)
            {
                pageRequest = new IndexPageRequest() { Skip = 0, Take = 50 };
            }

            try
            {
                string url = $"/people?limit={pageRequest.Take}";
                if (pageRequest.Skip > 0)
                {
                    url += $"&offset={pageRequest.Skip}";
                }

                if (!string.IsNullOrEmpty(alphaFilter))
                {
                    url += $"&alphabetFilter={WebUtility.UrlEncode(alphaFilter)}";
                }

                if (!string.IsNullOrEmpty(pageRequest.Filter))
                {
                    url += $"&alphabetFilter={WebUtility.UrlEncode(pageRequest.Filter)}";
                }

                PeopleResponse response = await WebClient.GetAsync<PeopleResponse>(BuildUrl(url));
                IndexPageResponse<PersonResponse> result = new IndexPageResponse<PersonResponse>(pageRequest, response.Results)
                {
                    TotalCount = response.TotalResults
                };
                return result;
            }
            catch (Exception ex)
            {
                throw new UserSafeException($"Could not query people from Interact API (skip:{pageRequest.Skip}|searchTerm:{pageRequest.Filter}|alphaFilter:{alphaFilter})", ex);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ensures that the authentication token for making Interact API calls is available.
        /// </summary>
        /// <returns>a task indicating completion of the operation.</returns>
        /// <exception cref="UserSafeException">Thrown when a token cannot be retrieved.</exception>
        public async Task EnsureAuthToken()
        {
            if (TokenInfo == null)
            {
                // Store token info for use with later use.
                TokenInfo = await GetToken();

                // Make sure the access token is available.
                if (string.IsNullOrEmpty(TokenInfo?.AccessToken))
                {
                    throw new UserSafeException(MissingAccessToken);
                }

                // Setup bearer token for subsequent API calls.
                WebClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("bearer", TokenInfo.AccessToken);
            }
        }

        public void ApplySettings(InteractOrgConfig settings)
        {
            Settings = settings;
            WebClient.DefaultRequestHeaders.Clear();
            WebClient.DefaultRequestHeaders.Add("X-Tenant", Settings.TenantId);
            TokenInfo = null;
        }

        private string BuildUrl(string relativeUrlEndpoint)
        {
            if (Settings == null)
            {
                throw new UserSafeException("Interact API cannot be called because the directory sync settings are missing.");
            }
            return Settings.ApiUrl.EnsureEndsWith("/") + relativeUrlEndpoint;
        }

        #endregion
    }
}
