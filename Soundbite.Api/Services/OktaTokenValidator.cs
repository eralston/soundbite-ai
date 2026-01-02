using Masticore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace Soundbite.Api
{
    /// <summary>
    /// Responsible for validating tokens issued by Azure Active Directory for Authentication.
    /// </summary>
    public class OktaTokenValidator
    {
        #region Fields

        private readonly OrgOktaSettings _settings;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="AdAuthTokenValidator"/> instance.
        /// </summary>
        public OktaTokenValidator(OrgOktaSettings settings)
        {
            Validator.NotNull("OKTA Settings", settings);
            Validator.NotNullOrEmpty("OKTA Settings - Client ID", settings.ClientId);
            Validator.NotNullOrEmpty("OKTA Settings - Issuer URL", settings.IssuerUrl);

            _settings = settings;
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
              _settings.IssuerUrl + "/.well-known/oauth-authorization-server",
              new OpenIdConnectConfigurationRetriever(),
              new HttpDocumentRetriever());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for parsing and validating a JWT token.
        /// </summary>
        /// <param name="bearerToken">String-based representation of the JTW token.</param>
        /// <returns>JWT token if valid, otherwise <c>null</c>.</returns>
        public async Task<JwtSecurityToken> ValidateToken(string bearerToken)
        {
            Validator.NotNullOrEmpty(nameof(bearerToken), bearerToken);

            CancellationToken ct = new CancellationToken();
            OpenIdConnectConfiguration discoveryDocument = await _configurationManager.GetConfigurationAsync(ct);
            System.Collections.Generic.ICollection<SecurityKey> signingKeys = discoveryDocument.SigningKeys;

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidIssuer = _settings.IssuerUrl,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                ValidateAudience = false,
            };

            try
            {
                System.Security.Claims.ClaimsPrincipal principal = new JwtSecurityTokenHandler()
                    .ValidateToken(bearerToken, validationParameters, out SecurityToken rawValidatedToken);
                return (JwtSecurityToken)rawValidatedToken;
            }
            catch
            {
                return null;
            }

        }

        #endregion
    }
}
