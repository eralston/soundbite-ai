using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Soundbite.TokenGen
{
    /// <summary>
    /// Defines the contract required to manage tokens in the application.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a <see cref="JwtSecurityToken"/> based on the specified <paramref name="token"/>.
        /// This method does NOT validate the token in any way.
        /// </summary>
        /// <param name="token">Token whose information is being sought.</param>
        /// <returns>a JwtSecurityToken containing information about the specified token.</returns>
        JwtSecurityToken GetToken(string token);

        /// <summary>
        /// Generates a <see cref="JwtSecurityToken"/> based on the specified <paramref name="token"/>.
        /// This method returns <c>null</c> if the specified token is invalid in any way.
        /// </summary>
        /// <param name="token">Token whose information is being sought.</param>
        /// <param name="throwEx">Specifies whether to throw an exception if token validation fails.</param>
        /// <returns>a JwtSecurityToken containing information about the specified token, or <c>null</c> if the token is invalid in any way.</returns>
        JwtSecurityToken GetTokenValidated(string token, bool throwEx = false);

        /// <summary>
        /// Generates a string representing an authentication token.
        /// </summary>
        /// <param name="issuer">Name of the issuer of the token.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="userEmail">Email address of the user.</param>
        /// <returns>a JWOT string representing an authentication token.</returns>
        string GenerateToken(string issuer, string orgRoute, string userEmail);

        /// <summary>
        /// Generates a string representing an authentication token.
        /// </summary>
        /// <param name="claims">Claims to put into the token.</param>
        /// <returns>a JWOT string representing an authentication token.</returns>
        string GenerateToken(string issuer, params Claim[] claims);
    }
}