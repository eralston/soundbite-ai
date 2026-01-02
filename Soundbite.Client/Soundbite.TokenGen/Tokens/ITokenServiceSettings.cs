namespace Soundbite.TokenGen
{
    /// <summary>
    /// Defines the contract for the settings required to configure token security.    
    /// </summary>
    public interface ITokenServiceSettings
    {
        /// <summary>
        /// Gets or sets a string containing configuration data used to initialize token management processes.  
        /// </summary>
        string Config { get; set; }

        /// <summary>
        /// Gets or sets the duration in minutes that a new token remains valid.
        /// </summary>
        int TokenTimeoutInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds that a token expiration is allowed to be off without being invalid.
        /// </summary>
        int ClockSkewInSeconds { get; set; }
    }
}
