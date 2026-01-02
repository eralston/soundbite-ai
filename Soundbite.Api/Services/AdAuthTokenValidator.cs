using Masticore.Ad;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

// LINK: Claims Info - https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens
// LINK: V1 Token - https://jwt.ms/#access_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Imk2bEdrM0ZaenhSY1ViMkMzbkVRN3N5SEpsWSIsImtpZCI6Imk2bEdrM0ZaenhSY1ViMkMzbkVRN3N5SEpsWSJ9.eyJhdWQiOiJlZjFkYTlkNC1mZjc3LTRjM2UtYTAwNS04NDBjM2Y4MzA3NDUiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9mYTE1ZDY5Mi1lOWM3LTQ0NjAtYTc0My0yOWYyOTUyMjIyOS8iLCJpYXQiOjE1MzcyMzMxMDYsIm5iZiI6MTUzNzIzMzEwNiwiZXhwIjoxNTM3MjM3MDA2LCJhY3IiOiIxIiwiYWlvIjoiQVhRQWkvOElBQUFBRm0rRS9RVEcrZ0ZuVnhMaldkdzhLKzYxQUdyU091TU1GNmViYU1qN1hPM0libUQzZkdtck95RCtOdlp5R24yVmFUL2tES1h3NE1JaHJnR1ZxNkJuOHdMWG9UMUxrSVorRnpRVmtKUFBMUU9WNEtjWHFTbENWUERTL0RpQ0RnRTIyMlRJbU12V05hRU1hVU9Uc0lHdlRRPT0iLCJhbXIiOlsid2lhIl0sImFwcGlkIjoiNzVkYmU3N2YtMTBhMy00ZTU5LTg1ZmQtOGMxMjc1NDRmMTdjIiwiYXBwaWRhY3IiOiIwIiwiZW1haWwiOiJBYmVMaUBtaWNyb3NvZnQuY29tIiwiZmFtaWx5X25hbWUiOiJMaW5jb2xuIiwiZ2l2ZW5fbmFtZSI6IkFiZSAoTVNGVCkiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMjIyNDcvIiwiaXBhZGRyIjoiMjIyLjIyMi4yMjIuMjIiLCJuYW1lIjoiYWJlbGkiLCJvaWQiOiIwMjIyM2I2Yi1hYTFkLTQyZDQtOWVjMC0xYjJiYjkxOTQ0MzgiLCJyaCI6IkkiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiJsM19yb0lTUVUyMjJiVUxTOXlpMmswWHBxcE9pTXo1SDNaQUNvMUdlWEEiLCJ0aWQiOiJmYTE1ZDY5Mi1lOWM3LTQ0NjAtYTc0My0yOWYyOTU2ZmQ0MjkiLCJ1bmlxdWVfbmFtZSI6ImFiZWxpQG1pY3Jvc29mdC5jb20iLCJ1dGkiOiJGVnNHeFlYSTMwLVR1aWt1dVVvRkFBIiwidmVyIjoiMS4wIn0.D3H6pMUtQnoJAGq6AHd
// LINK: V2 Token - https://jwt.ms/#access_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Imk2bEdrM0ZaenhSY1ViMkMzbkVRN3N5SEpsWSJ9.eyJhdWQiOiI2ZTc0MTcyYi1iZTU2LTQ4NDMtOWZmNC1lNjZhMzliYjEyZTMiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3L3YyLjAiLCJpYXQiOjE1MzcyMzEwNDgsIm5iZiI6MTUzNzIzMTA0OCwiZXhwIjoxNTM3MjM0OTQ4LCJhaW8iOiJBWFFBaS84SUFBQUF0QWFaTG8zQ2hNaWY2S09udHRSQjdlQnE0L0RjY1F6amNKR3hQWXkvQzNqRGFOR3hYZDZ3TklJVkdSZ2hOUm53SjFsT2NBbk5aY2p2a295ckZ4Q3R0djMzMTQwUmlvT0ZKNGJDQ0dWdW9DYWcxdU9UVDIyMjIyZ0h3TFBZUS91Zjc5UVgrMEtJaWpkcm1wNjlSY3R6bVE9PSIsImF6cCI6IjZlNzQxNzJiLWJlNTYtNDg0My05ZmY0LWU2NmEzOWJiMTJlMyIsImF6cGFjciI6IjAiLCJuYW1lIjoiQWJlIExpbmNvbG4iLCJvaWQiOiI2OTAyMjJiZS1mZjFhLTRkNTYtYWJkMS03ZTRmN2QzOGU0NzQiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJhYmVsaUBtaWNyb3NvZnQuY29tIiwicmgiOiJJIiwic2NwIjoiYWNjZXNzX2FzX3VzZXIiLCJzdWIiOiJIS1pwZmFIeVdhZGVPb3VZbGl0anJJLUtmZlRtMjIyWDVyclYzeERxZktRIiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidXRpIjoiZnFpQnFYTFBqMGVRYTgyUy1JWUZBQSIsInZlciI6IjIuMCJ9.pj4N-w_3Us9DrBLfpCt

namespace Soundbite.Api
{
    /// <summary>
    /// Responsible for validating tokens issued by Azure Active Directory for Authentication.
    /// </summary>
    public class AdAuthTokenValidator
    {
        #region Fields

        private readonly ISecurityContextService _securityContextService;
        private readonly ISecurityContext _securityContext;
        private readonly SpaConfig _spaConfig;
        private readonly TeamsAppAzureSettings _teamsConfig;
        private readonly IUserService _users;
        private string _clientId;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a flag indicating whether the email claim can be used if a user 
        /// cannot be found by Universal ID.
        /// </summary>
        public bool AllowEmailClaim { get; set; } = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="AdAuthTokenValidator"/> instance.
        /// </summary>
        /// <param name="securityContext">DI reference to the security context.</param>
        /// <param name="securityContextService">DI reference to the security context service.</param>
        /// <param name="spaConfig">DI reference to the application configuration settings.</param>
        /// <param name="teamsConfig">DI reference to the teams configuration settings.</param>
        /// <param name="users">DI reference to the user service.</param>
        public AdAuthTokenValidator(
            ISecurityContext securityContext,
            ISecurityContextService securityContextService,
            SpaConfig spaConfig,
            TeamsAppAzureSettings teamsConfig,
            IUserService users
            )
        {
            _securityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
            _securityContextService = securityContextService ?? throw new ArgumentNullException(nameof(securityContextService));
            _spaConfig = spaConfig ?? throw new ArgumentNullException(nameof(spaConfig));
            _teamsConfig = teamsConfig ?? throw new ArgumentNullException(nameof(teamsConfig));
            _users = users ?? throw new ArgumentNullException(nameof(users));
            UsePlatformTokenValidation();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Configures the token validator to validate tokens from the Soundbite SPA application.
        /// </summary>
        public void UseSpaTokenValidation()
        {
            _clientId = _spaConfig.AdClientConfig.ClientId;
        }

        /// <summary>
        /// Configures the token validator to validate tokens from the Soundbite Platform application.
        /// </summary>
        public void UsePlatformTokenValidation()
        {
            _clientId = _spaConfig.AdPlatformConfig.AppId;
        }

        /// <summary>
        /// Configures the token validator to validate tokens from the Teams application.
        /// </summary>
        public void UseTeamsAppTokenValidation()
        {
            _clientId = _teamsConfig.ClientId;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="token"/> is valid.
        /// </summary>
        /// <param name="token">A string representing a JWT token.</param>
        /// <param name="updateSecurityContext">Flag indicating whether to update the security context with information from the token.</param>
        /// <returns>a flag indicating whether the token is valid (<c>true</c>) or invalid (<c>false</c>).</returns>
        public async Task<bool> ValidateToken(string token, bool updateSecurityContext)
        {
            if (!string.IsNullOrEmpty(token))
            {
                // We have to parse the incoming token to extract version claims information
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken unvalidatedToken = GetUnvalidatedToken(token, tokenHandler);
                AdAuthTokenVersionHandler versionHandler = GetTokenVersionHandler(unvalidatedToken);

                // Validate the token
                await versionHandler.ValidateToken(token, tokenHandler, unvalidatedToken, _clientId);

                // Populate the security context (if applicable)
                if (versionHandler.IsValid && updateSecurityContext)
                {
                    await PopulateSecurityContext(versionHandler);
                }

                return versionHandler.IsValid;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Responsible for parsing the raw token string into a <see cref="JwtSecurityToken"/> instance.
        /// </summary>
        /// <param name="token">Raw JWT token.</param>
        /// <param name="tokenHandler">JWT token handler instance.</param>
        /// <returns>a <see cref="JwtSecurityToken"/> populated with info.</returns>
        private JwtSecurityToken GetUnvalidatedToken(string token, JwtSecurityTokenHandler tokenHandler)
        {
            try
            {
                JwtSecurityToken unvalidatedToken = null;
                unvalidatedToken = tokenHandler.ReadJwtToken(token);
                return unvalidatedToken;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse the specified token.", ex);
            }
        }

        /// <summary>
        /// Extracts the version claim and returns an appropriate version handler.
        /// </summary>
        /// <param name="jwtToken">JWT token containing version information.</param>
        /// <returns>an <see cref="AdAuthTokenVersionHandler"/> instance ready for use.</returns>
        private AdAuthTokenVersionHandler GetTokenVersionHandler(JwtSecurityToken jwtToken)
        {
            string version = jwtToken.Claims
                .Where(i => i.Type == "ver")
                .Select(i => i.Value)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(version))
            {
                throw new Exception("Token does not contain Version claim.");
            }

            return version switch
            {
                "1.0" => new AdAuthTokenVersion1Handler(),
                "2.0" => new AdAuthTokenVersion2Handler(),
                _ => throw new Exception($"Token version '{version}' is unsupported"),
            };
        }

        /// <summary>
        /// Populates the security context with user information based on claims data in the token.
        /// </summary>
        /// <param name="handler">Token version handler containing information about the token.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        private async Task PopulateSecurityContext(AdAuthTokenVersionHandler handler)
        {
            // Both versions of the token have the same "oid" claim that points to the universal ID of the user.
            AdAccessTokenClaims claims = new AdAccessTokenClaims(handler.ValidatedJwtToken);
            string azureUserUid = claims.UniversalId;

            if (azureUserUid == null)
            {
                throw new SecurityException("User Universal ID could not be retrieved from token.");
            }
            else
            {
                //TODO: Remove this current user check when the Azure token security middleware gets removed.
                if (_securityContext.CurrentUser != null)
                {
                    if (_securityContext.UniversalId != azureUserUid)
                    {
                        // Wrong user -- highly unlikely unless someone is doing something nefarious
                        throw new Exception("UID from middleware does not match UID from Azure AD token.");
                    }
                    else
                    {
                        // Correct user is already setup but make sure the organization route is set
                        await _securityContextService.SetCurrentOrgByOrgUniversalId(_securityContext, handler.TenantId);
                    }
                }
                else
                {
                    // TODO: Make implementation type swappable
                    User userRef = await _users.UpsertMeAsync(claims);
                    if (AllowEmailClaim && string.Equals(claims.Email, userRef.Email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //TODO: Revisit this -- technically the email value is always overwritten
                        //      by the upsert, but the user associated with the email has to be
                        //      associated with the org to actually login.  Not ideal but I believe
                        //      it is secure enough for now.  Should be revisited again when
                        //      implementing the Universal ID fixes for one-to-many third-party
                        //      directories per user.

                        // Email claims are allowed, and the Email claim matches the email on file
                        // for the user. Log the user in with the Universal ID associated with the
                        // user record from the upsert operation. 
                        await _securityContextService.SetUserByAzureTenantAndUserUniversalId(_securityContext, handler.TenantId, userRef.UniversalId);
                    }
                    else
                    {
                        await _securityContextService.SetUserByAzureTenantAndUserUniversalId(_securityContext, handler.TenantId, azureUserUid);
                    }
                }
            }
        }

        #endregion
    }
}
