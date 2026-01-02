using Masticore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

// LINK: Claims Info - https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens
// LINK: V1 Token - https://jwt.ms/#access_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Imk2bEdrM0ZaenhSY1ViMkMzbkVRN3N5SEpsWSIsImtpZCI6Imk2bEdrM0ZaenhSY1ViMkMzbkVRN3N5SEpsWSJ9.eyJhdWQiOiJlZjFkYTlkNC1mZjc3LTRjM2UtYTAwNS04NDBjM2Y4MzA3NDUiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9mYTE1ZDY5Mi1lOWM3LTQ0NjAtYTc0My0yOWYyOTUyMjIyOS8iLCJpYXQiOjE1MzcyMzMxMDYsIm5iZiI6MTUzNzIzMzEwNiwiZXhwIjoxNTM3MjM3MDA2LCJhY3IiOiIxIiwiYWlvIjoiQVhRQWkvOElBQUFBRm0rRS9RVEcrZ0ZuVnhMaldkdzhLKzYxQUdyU091TU1GNmViYU1qN1hPM0libUQzZkdtck95RCtOdlp5R24yVmFUL2tES1h3NE1JaHJnR1ZxNkJuOHdMWG9UMUxrSVorRnpRVmtKUFBMUU9WNEtjWHFTbENWUERTL0RpQ0RnRTIyMlRJbU12V05hRU1hVU9Uc0lHdlRRPT0iLCJhbXIiOlsid2lhIl0sImFwcGlkIjoiNzVkYmU3N2YtMTBhMy00ZTU5LTg1ZmQtOGMxMjc1NDRmMTdjIiwiYXBwaWRhY3IiOiIwIiwiZW1haWwiOiJBYmVMaUBtaWNyb3NvZnQuY29tIiwiZmFtaWx5X25hbWUiOiJMaW5jb2xuIiwiZ2l2ZW5fbmFtZSI6IkFiZSAoTVNGVCkiLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMjIyNDcvIiwiaXBhZGRyIjoiMjIyLjIyMi4yMjIuMjIiLCJuYW1lIjoiYWJlbGkiLCJvaWQiOiIwMjIyM2I2Yi1hYTFkLTQyZDQtOWVjMC0xYjJiYjkxOTQ0MzgiLCJyaCI6IkkiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiJsM19yb0lTUVUyMjJiVUxTOXlpMmswWHBxcE9pTXo1SDNaQUNvMUdlWEEiLCJ0aWQiOiJmYTE1ZDY5Mi1lOWM3LTQ0NjAtYTc0My0yOWYyOTU2ZmQ0MjkiLCJ1bmlxdWVfbmFtZSI6ImFiZWxpQG1pY3Jvc29mdC5jb20iLCJ1dGkiOiJGVnNHeFlYSTMwLVR1aWt1dVVvRkFBIiwidmVyIjoiMS4wIn0.D3H6pMUtQnoJAGq6AHd
// LINK: V2 Token - https://jwt.ms/#access_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Imk2bEdrM0ZaenhSY1ViMkMzbkVRN3N5SEpsWSJ9.eyJhdWQiOiI2ZTc0MTcyYi1iZTU2LTQ4NDMtOWZmNC1lNjZhMzliYjEyZTMiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3L3YyLjAiLCJpYXQiOjE1MzcyMzEwNDgsIm5iZiI6MTUzNzIzMTA0OCwiZXhwIjoxNTM3MjM0OTQ4LCJhaW8iOiJBWFFBaS84SUFBQUF0QWFaTG8zQ2hNaWY2S09udHRSQjdlQnE0L0RjY1F6amNKR3hQWXkvQzNqRGFOR3hYZDZ3TklJVkdSZ2hOUm53SjFsT2NBbk5aY2p2a295ckZ4Q3R0djMzMTQwUmlvT0ZKNGJDQ0dWdW9DYWcxdU9UVDIyMjIyZ0h3TFBZUS91Zjc5UVgrMEtJaWpkcm1wNjlSY3R6bVE9PSIsImF6cCI6IjZlNzQxNzJiLWJlNTYtNDg0My05ZmY0LWU2NmEzOWJiMTJlMyIsImF6cGFjciI6IjAiLCJuYW1lIjoiQWJlIExpbmNvbG4iLCJvaWQiOiI2OTAyMjJiZS1mZjFhLTRkNTYtYWJkMS03ZTRmN2QzOGU0NzQiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJhYmVsaUBtaWNyb3NvZnQuY29tIiwicmgiOiJJIiwic2NwIjoiYWNjZXNzX2FzX3VzZXIiLCJzdWIiOiJIS1pwZmFIeVdhZGVPb3VZbGl0anJJLUtmZlRtMjIyWDVyclYzeERxZktRIiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidXRpIjoiZnFpQnFYTFBqMGVRYTgyUy1JWUZBQSIsInZlciI6IjIuMCJ9.pj4N-w_3Us9DrBLfpCt

namespace Soundbite.Api
{
    /// <summary>
    /// Base class for auth token version handlers.
    /// </summary>
    internal abstract class AdAuthTokenVersionHandler
    {
        #region Properties

        public string TenantId { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Scope { get; set; }
        public string ClientId { get; set; }

        public bool IsValid { get; set; }
        public JwtSecurityToken ValidatedJwtToken { get; set; }
        public Exception ValidationExecption { get; set; }

        #endregion

        #region Methods

        public async Task ValidateToken(string token, JwtSecurityTokenHandler tokenHandler, JwtSecurityToken jwtToken, string clientId)
        {
            try
            {
                // Validate arguments
                Validator.ArgNotNullOrEmpty(nameof(token), token);
                Validator.ArgNotNull(nameof(tokenHandler), tokenHandler);
                Validator.ArgNotNull(nameof(jwtToken), jwtToken);
                Validator.ArgNotNull(nameof(clientId), clientId);

                // Reset validation properties (just in case)
                IsValid = false;
                ValidationExecption = null;
                ValidatedJwtToken = null;
                ClientId = clientId;

                // Validate Token
                PopulateClaims(jwtToken);
                ValidateClaims();
                ValidateClaimsCustom();

                //TODO: Determine whether we need to cache config data retrieved from configuration manager
                string stsDiscoveryEndpoint = $"https://login.microsoftonline.com/{TenantId}/.well-known/openid-configuration";
                ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
                OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync();
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false, // Check performed manually in all versions
                    ValidateIssuer = false, // Check performed manually in all versions
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = new TimeSpan(0, 5, 0), // May need to make this configurable
                    IssuerSigningKeys = config.SigningKeys,
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
                ValidatedJwtToken = securityToken as JwtSecurityToken ?? jwtToken;
                IsValid = true;
            }
            catch (Exception ex)
            {
                ValidationExecption = new Exception("Token validation failed.", ex);
                IsValid = false;
            }
        }

        private void PopulateClaims(JwtSecurityToken jwtToken)
        {
            // Acquire claims
            TenantId = jwtToken.Claims.Where(i => i.Type == "tid").Select(i => i.Value).FirstOrDefault();
            Issuer = jwtToken.Claims.Where(i => i.Type == "iss").Select(i => i.Value).FirstOrDefault();
            Audience = jwtToken.Claims.Where(i => i.Type == "aud").Select(i => i.Value).FirstOrDefault();
            Scope = jwtToken.Claims.Where(i => i.Type == "scp").Select(i => i.Value).FirstOrDefault();
        }

        private void ValidateClaims()
        {
            if (string.IsNullOrEmpty(TenantId))
            {
                throw new Exception("Token does not contain Tenant ID claim.");
            }

            if (string.IsNullOrEmpty(Issuer))
            {
                throw new Exception("Token does not contain Issuer claim.");
            }

            if (string.IsNullOrEmpty(Audience))
            {
                throw new Exception("Token does not contain Audience claim.");
            }

            if (string.IsNullOrEmpty(Scope))
            {
                throw new Exception("Token does not contains Scope claim.");
            }
        }

        #endregion

        #region Abstract Members

        protected abstract void ValidateClaimsCustom();

        #endregion
    }

    /// <summary>
    /// Version 1 auth token handler.
    /// </summary>
    internal class AdAuthTokenVersion1Handler : AdAuthTokenVersionHandler
    {
        #region Overrides

        protected override void ValidateClaimsCustom()
        {
            if (!Audience.Contains(ClientId, StringComparison.InvariantCultureIgnoreCase))
            {
                // Invalid audience -- it should either be the <clientId> or api://<clientId>
                throw new Exception("Token does not contain expected audience.");
            }

            if (!Issuer.Equals($"https://sts.windows.net/{TenantId}/"))
            {
                // Invalid issuer
                throw new Exception("Token does not contain expected issuer.");
            }
        }

        #endregion
    }

    /// <summary>
    /// Version 2 auth token handler.
    /// </summary>
    internal class AdAuthTokenVersion2Handler : AdAuthTokenVersionHandler
    {
        #region Overrides

        protected override void ValidateClaimsCustom()
        {
            //TODO: Figure out audiences from Web / Teams / SharePoint and see how they need to be validated
            //if (!Audience.Equals(ClientId, StringComparison.InvariantCultureIgnoreCase))
            if (!Audience.Contains(ClientId, StringComparison.InvariantCultureIgnoreCase))
            {
                // Invalid audience -- it should match the client ID
                throw new Exception("Token does not contain expected audience.");
            }

            if (!Issuer.Equals($"https://login.microsoftonline.com/{TenantId}/v2.0"))
            {
                // Invalid issuer
                throw new Exception("Token does not contain expected issuer.");
            }
        }

        #endregion
    }
}
