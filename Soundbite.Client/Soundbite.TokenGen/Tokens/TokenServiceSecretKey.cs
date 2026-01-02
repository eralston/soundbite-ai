using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Soundbite.TokenGen
{
    /// <summary>
    /// Responsible for managing tokens in the application.
    /// </summary>
    public class TokenServiceSecretKey : ITokenService
    {
        #region Fields

        /// <summary>
        /// Gets or sets a reference to the configuration settings applied to this service.
        /// </summary>
        private readonly ITokenServiceSettings _settings;        

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenServiceSecretKey"/> class.
        /// </summary>
        /// <param name="settings">Token service settings DI reference.</param>
        /// <param name="logger">Logger DI reference. Pass <c>null</c> if logging is not desired.</param>
        public TokenServiceSecretKey(ITokenServiceSettings settings)
        {
            _settings = settings;
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
        /// <param name="throwEx">Specifies whether to throw an exception if token validation fails.</param>
        /// <returns>a JwtSecurityToken containing information about the specified token, or <c>null</c> if the token is invalid in any way.</returns>
        public JwtSecurityToken GetTokenValidated(string token, bool throwEx = false)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = GetSecretKeyBytes(_settings);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewInSeconds)
                }, out SecurityToken validatedToken);

                return (JwtSecurityToken)validatedToken;
            }
            catch
            {
                if(throwEx)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Generates a string representing an authentication token.
        /// </summary>
        /// <param name="issuer">Name of the issuer of the token.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="userEmail">Email address of the user.</param>
        /// <returns>a JWOT string representing an authentication token.</returns>
        public string GenerateToken(string issuer, string orgRoute, string userEmail)
        {
            return GenerateToken(issuer,
                new Claim(Constants.Tokens.OrgRouteClaim, orgRoute), 
                new Claim(Constants.Tokens.UserEmailClaim, userEmail));
        }

        /// <summary>
        /// Generates a string representing an authentication token.
        /// </summary>
        /// <param name="claims">Claims to put into the token.</param>
        /// <returns>a JWOT string representing an authentication token.</returns>
        public string GenerateToken(string issuer, params Claim[] claims)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = GetSecretKeyBytes(_settings);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_settings.TokenTimeoutInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)                
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion

        #region Methods

        private byte[] GetSecretKeyBytes(ITokenServiceSettings settings)
        {
            return Encoding.ASCII.GetBytes(settings.Config);
        }

        #endregion
    }
}