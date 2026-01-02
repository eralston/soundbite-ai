using Newtonsoft.Json;

namespace Soundbite
{
    /// <summary>
    /// Represents the HTTP response to the request for an azure access token
    /// URL: https://login.microsoftonline.com/{{tenant_id}}/oauth2/v2.0/token
    /// </summary>    
    public class AccessTokenResponse
    {
        /// <summary>
        /// Gets or sets the type of token returned.
        /// </summary>        
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets an integer identifying the number of seconds after which the token expires.
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets an integer identifying the number of seconds after which the token expires
        /// during an STS outage. 
        /// </summary>
        [JsonProperty("ext_expires_in")]
        public int ExpiresInExtended { get; set; }

        /// <summary>
        /// Gets or sets the value of the access token that can be used to make API calls.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}