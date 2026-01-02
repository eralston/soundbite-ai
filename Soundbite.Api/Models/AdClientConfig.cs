namespace Soundbite.Api
{
    /// <summary>
    /// Captures the client-side configuration necessary for MSAL.js powered client auth strategies
    /// https://github.com/AzureAD/microsoft-authentication-library-for-js
    /// </summary>
    public class AdClientConfig
    {
        /// <summary>
        /// Gets or sets the Client ID for the app in AAD
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the API app's location on the net
        /// </summary>
        public string ApiUri { get; set; }

        /// <summary>
        /// Gets or sets the URI that will receive the login redirect
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the URI that will receive the logout redirect
        /// </summary>
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the list of AAD scopes that the client-side app will use
        /// </summary>
        public string[] AdScopes { get; set; }

        /// <summary>
        /// Gets or sets the list of scopes the server-side app will use
        /// </summary>
        public string[] ApiScopes { get; set; }
    }
}
