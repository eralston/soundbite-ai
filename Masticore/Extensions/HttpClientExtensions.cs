using Masticore.Exceptions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masticore
{
    /// <summary>
    /// Contains IHttpClient extension methods.
    /// </summary>
    public static class HttpClientExtensions
    {
        #region Constants

        /// <summary>
        /// Invalid error message
        /// </summary>
        public const string JsonInvalid = "JSON parsing failed.";

        #endregion

        #region Methods

        /// <summary>
        /// Response wrapper that extracts a JSON response from an HTTP call.
        /// </summary>
        /// <typeparam name="T">.NET type used to deserialize JSON response.</typeparam>
        /// <param name="httpVerb">HTTP verb used to make HTTP call (for logging purposes).</param>
        /// <param name="requestUri">HTTP request URI.</param>
        /// <param name="httpAction">Action that executes the HTTP call and returns an <see cref="HttpResponseMessage"/>.</param>
        /// <returns>a <typeparamref name="T"/> instance populated with data from the JSON body of the HTTP response.</returns>        
        private static async Task<T> JsonHttpResponse<T>(string httpVerb, string requestUri, Func<Task<HttpResponseMessage>> httpAction)
        {
            HttpResponseMessage response;

            // Attempt to execute the web API call
            try
            {
                response = await httpAction();
            }
            catch (Exception ex)
            {
                // Error occurred before receiving a response
                throw new UserSafeException($"HTTP call failed.'", ex);
            }

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    string json = await response.Content.ReadAsStringAsync();
                    T result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                    return result;
                }
                catch (Exception ex)
                {
                    throw new UserSafeException(JsonInvalid, ex);
                }
            }
            else
            {
                throw new UserSafeException($"HTTP GET call to '{requestUri} failed with status code {response.StatusCode} - {response.ReasonPhrase}'");
            }
        }

        /// <summary>
        /// Issues a GET HTTP call and returns a populated <typeparamref name="T"/> instance 
        /// populated with data from the JSON body of the request.
        /// </summary>
        /// <typeparam name="T">.NET type used to deserialize JSON response.</typeparam>
        /// <param name="httpClient">HTTP client used to issue the HTTP call.</param>
        /// <param name="requestUri">URI of the HTTP call.</param>
        /// <returns>a populated <typeparamref name="T"/> instance populated with data from the JSON body of the request.</returns>
        public static async Task<T> GetAsync<T>(this HttpClient httpClient, string requestUri)
        {
            return await JsonHttpResponse<T>("GET", requestUri, () => httpClient.GetAsync(requestUri));
        }

        /// <summary>
        /// Issues a GET POST call and returns a populated <typeparamref name="T"/> instance 
        /// populated with data from the JSON body of the request.
        /// </summary>
        /// <typeparam name="T">.NET type used to deserialize JSON response.</typeparam>
        /// <param name="httpClient">HTTP client used to issue the HTTP call.</param>
        /// <param name="requestUri">URI of the HTTP call.</param>
        /// <param name="content">Content to POST.</param>
        /// <returns>a populated <typeparamref name="T"/> instance populated with data from the JSON body of the request.</returns>
        public static async Task<T> PostAsync<T>(this HttpClient httpClient, string requestUri, HttpContent content)
        {
            return await JsonHttpResponse<T>("POST", requestUri, () => httpClient.PostAsync(requestUri, content));
        }

        /// <summary>
        /// Issues a POST HTTP call and returns a populated <typeparamref name="T"/> instance 
        /// populated with data from the JSON body of the request.
        /// </summary>
        /// <typeparam name="T">.NET type used to deserialize JSON response.</typeparam>
        /// <param name="httpClient">HTTP client used to issue the HTTP call.</param>
        /// <param name="requestUri">URI of the HTTP call.</param>
        /// <param name="content">Content to PUT.</param>
        /// <returns>a populated <typeparamref name="T"/> instance populated with data from the JSON body of the request.</returns>
        public static async Task<T> PutAsync<T>(this HttpClient httpClient, string requestUri, HttpContent content)
        {
            return await JsonHttpResponse<T>("POST", requestUri, () => httpClient.PutAsync(requestUri, content));
        }

        #endregion
    }
}