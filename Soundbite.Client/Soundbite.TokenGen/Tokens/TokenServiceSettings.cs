namespace Soundbite.TokenGen
{
    public class TokenServiceSettings : ITokenServiceSettings
    {
        /// <summary>
        /// Gets or sets a string containing configuration data used to initialize token management processes.  
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Gets or sets the duration in minutes that a new token remains valid.
        /// </summary>
        public int TokenTimeoutInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the duration in minutes that a new refresh token remains valid.
        /// </summary>
        public int RefreshTokenTimeoutInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds that a token expiration is allowed to be off without being invalid.
        /// </summary>
        public int ClockSkewInSeconds { get; set; }
    }
}