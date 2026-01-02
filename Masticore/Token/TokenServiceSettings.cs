using System.Security;

namespace Masticore.Token
{
    /// <summary>
    /// Configuration object for <see cref="ITokenService"/>
    /// </summary>
    public class TokenServiceSettings : ITokenServiceSettings
    {
        #region Static Members

        //TODO: Determine if we want to avoid a "default" and force each tenant (at a minimum) to
        //      have their own token settings.  May lessen possibility of spoofing a token(?)

        // Property backer fields
        private static TokenServiceSettings _defaultTokenServiceSettings;
        private static string _tokenIssuerClaimValue;

        /// <summary>
        /// Gets or sets the default token service settings for the application.  
        /// This value can only be set once.
        /// </summary>
        /// <exception cref="SecurityException">Thrown if the default token service value is changed after being set.</exception>
        public static TokenServiceSettings DefaultTokenServiceSettings
        {
            get => _defaultTokenServiceSettings;
            set
            {
                if (_defaultTokenServiceSettings != null)
                {
                    throw new SecurityException("Default token service settings cannot be changed once it has been set.");
                }
                _defaultTokenServiceSettings = value;
            }
        }

        /// <summary>
        /// Gets or sets the token issuer claim value used to set/authenticate tokens.
        /// </summary>
        /// <exception cref="SecurityException">Thrown if the default token service value is changed after being set.</exception>
        public static string TokenIssuerClaimValue
        {
            get => _tokenIssuerClaimValue;
            set
            {
                // NOTE: something wonky happens in the startup where this gets set twice, but the second time
                // the value is the same.  So only throw security exception if the value is changed.
                if (_tokenIssuerClaimValue != null && _tokenIssuerClaimValue != value)
                {
                    throw new SecurityException("Token issuer claim value cannot be changed once it has been set.");
                }
                _tokenIssuerClaimValue = value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the level at which the token service settings are defined.
        /// </summary>
        public TokenSecurityLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the approach used for managing tokens.
        /// </summary>
        public TokenSecurityType SecurityType { get; set; }

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

        #endregion
    }
}