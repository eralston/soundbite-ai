namespace Soundbite
{
    /// <summary>
    /// Stores the settings for the Soundbite Teams application.  This information is required when
    /// attempting to acquire an access token that can be used to make Teams Graph API calls on
    /// behalf of the Soundbite Teams App.
    /// </summary>
    public class TeamsAppAzureSettings
    {
        /// <summary>
        /// Client ID of the teams application for the current environment.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Secret Key for the teams application for the current environment.
        /// </summary>
        public string SecretKey { get; set; }

        //TODO: discuss having a higher-level "EnvironmentalSettings" class to store stuff that
        // is defined in appSettings.config / SPA level but used in lower-level classes.

        /// <summary>
        /// Gets or sets the base URL for the environment.
        /// </summary>
        public string BaseUrl { get; set; }
    }
}