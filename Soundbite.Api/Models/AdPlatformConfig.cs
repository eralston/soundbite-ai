namespace Soundbite.Api
{
    /// <summary>
    /// Holds the platform-level configuration
    /// TODO: This likely needs to be auth aware such that there is a flavor for AAD and Okta; this is AAD only
    /// </summary>
    public class AdPlatformConfig
    {
        /// <summary>
        /// Gets or sets the App ID for this instance of the platform
        /// </summary>
        public string AppId { get; set; }
    }
}