namespace Soundbite.Api
{
    /// <summary>
    /// Complete configuration object for the front-end
    /// </summary>
    public class SpaConfig
    {
        /// <summary>
        /// Gets or sets the platform configuration that the SPA needs to know about
        /// </summary>
        public AdPlatformConfig AdPlatformConfig { get; set; }

        /// <summary>
        /// The Active Directory client configuration, with key fields like URLs, keys, and scopes
        /// </summary>
        public AdClientConfig AdClientConfig { get; set; }

        /// <summary>
        /// Features flags for the front-end of the application
        /// </summary>
        public FeatureFlags FeatureFlags { get; set; }

        /// <summary>
        /// The credentials for connecting to the client-side telemetry tooling; in most cases this should be Azure App Insights
        /// </summary>
        public string TelemetryKey { get; set; }
    }
}
