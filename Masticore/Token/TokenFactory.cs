using Microsoft.Extensions.Logging;
using System.Security;

namespace Masticore.Token
{
    /// <summary>
    /// Responsible for acquiring the appropriate token service based on settings.
    /// </summary>
    public class TokenServiceFactory : ITokenServiceFactory
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public TokenServiceFactory(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an appropriate token service based on the specified settings.
        /// </summary>
        /// <param name="settings">Settings containing information on which token service is needed.</param>
        /// <returns>a <see cref="ITokenService"/> ready for use.</returns>
        public ITokenService GetTokenService(ITokenServiceSettings settings)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            return settings.SecurityType switch
            {
                TokenSecurityType.SecretKey => new TokenServiceSecretKey(settings, _logger),
                _ => throw new SecurityException($"Specified token security type '{settings.SecurityType}' is not supported."),
            };
        }
    }
}