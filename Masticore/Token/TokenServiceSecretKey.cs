using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Masticore.Token
{
    /// <summary>
    /// Token service for managing tokens generated using a string-based secret key value.
    /// </summary>
    public class TokenServiceSecretKey : ITokenService
    {
        #region Fields

        /// <summary>
        /// Gets or sets a reference to the configuration settings applied to this service.
        /// </summary>
        private readonly ITokenServiceSettings _settings;

        private readonly ILogger _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenServiceSecretKey"/> class.
        /// </summary>
        /// <param name="settings">Token service settings DI reference.</param>
        /// <param name="logger">Logger DI reference. Pass <c>null</c> if logging is not desired.</param>
        public TokenServiceSecretKey(ITokenServiceSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        #endregion

        #region ITokenService Implementation

        /// <summary>
        /// Generates a <see cref="JwtSecurityToken"/> based on the specified <paramref name="token"/>.
        /// This method does NOT validate the token in any way.
        /// </summary>
        /// <param name="token">Token whose information is being sought.</param>
        /// <returns>a JwtSecurityToken containing information about the specified token.</returns>
        public JwtSecurityToken GetToken(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken result = tokenHandler.ReadJwtToken(token);
            return result;
        }

        /// <summary>
        /// Generates a <see cref="JwtSecurityToken"/> based on the specified <paramref name="token"/>.
        /// This method returns <c>null</c> if the specified token is invalid in any way.
        /// </summary>
        /// <param name="token">Token whose information is being sought.</param>
        /// <returns>a JwtSecurityToken containing information about the specified token, or <c>null</c> if the token is invalid in any way.</returns>
        public JwtSecurityToken GetTokenValidated(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = GetSecretKeyBytes(_settings);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    //TODO: implement a third-party issuer value and store it in the org token configuration
                    //NOTE: do not validate issuer for now.  We probably need to coordinate with third-party
                    // clients and store an "issuer" value along with org-level token configuration. The
                    // issuer value for soundbite tokens is stored in TokenServiceSettings.TokenIssuerClaimValue
                    // but the check was taken out to avoid issues with validating from third-paty sources.                    
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewInSeconds),
                    RequireExpirationTime = false,
                }, out SecurityToken validatedToken);

                return (JwtSecurityToken)validatedToken;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{func} failed to validate token", nameof(GetTokenValidated));
                return null;
            }
        }

        /// <summary>
        /// Generates a string representing an authentication token.
        /// </summary>
        /// <param name="claims">Claims to put into the token.</param>
        /// <returns>a JWOT string representing an authentication token.</returns>
        public string GenerateToken(params Claim[] claims)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = GetSecretKeyBytes(_settings);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = TokenServiceSettings.TokenIssuerClaimValue,
                Subject = new ClaimsIdentity(claims),
                Expires = Time.UtcNow.AddMinutes(_settings.TokenTimeoutInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion

        #region Methods

        private byte[] GetSecretKeyBytes(ITokenServiceSettings settings)
        {
            return Convert.FromBase64String(settings.Config);
        }

        #endregion
    }
}