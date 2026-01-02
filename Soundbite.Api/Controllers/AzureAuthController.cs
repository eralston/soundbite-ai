using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Api.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// An insecure controller for pulling client-side configuration dynamically from the app
    /// Unfortunately not a jacuzzi remote controller
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class AzureAuthController : ControllerBase
    {
        #region Properties

        private readonly SpaService _spaService;
        private readonly ISecurityContext _securityContext;
        private readonly AdAuthTokenValidator _authTokenValidator;
        private readonly IUserService _userService;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="AzureAuthController"/> instance.
        /// </summary>
        /// <param name="securityContext">DI Security context reference.</param>
        /// <param name="spaService">DI SPA service reference.</param>
        /// <param name="authTokenValidator">DI Azure auth token validator.</param>
        /// <param name="userService">DI User Service reference.</param>
        public AzureAuthController(ISecurityContext securityContext, SpaService spaService, AdAuthTokenValidator authTokenValidator, IUserService userService)
        {
            _securityContext = securityContext;
            _spaService = spaService;
            _authTokenValidator = authTokenValidator;
            _userService = userService;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Responsible for authenticating request against Azure Active Directory (AAD) and sending
        /// a payload containing application initialization data.
        /// </summary>
        /// <param name="externalUserInfo">External user information to import into Soundbite.</param>
        /// <param name="orgRoute">Optional route of the organization associated with the user.</param>
        /// <returns>an application payload containing data required to initialize the application.</returns>
        [Route("/azureAuth/login")]
        [HttpPost, AllowAnonymous]
        public async Task<AppPayload> LoginAzureAD([FromBody] User externalUserInfo, string orgRoute = null)
        {
            return await LoginToOrgAzureAD(orgRoute ?? _securityContext.OrgRoute, externalUserInfo);
        }

        /// <summary>
        /// Authenticate against Azure Active Directory (AAD) for the given organization
        /// </summary>
        /// <param name="orgRoute">Route of the org into which the user is logging in.</param>
        /// <param name="externalUserInfo">External user information used to update current user's info.</param>
        /// <returns>an application payload containing data required to initialize the application.</returns>
        [Route("/azureAuth/login/{orgRoute}")]
        [HttpPost]
        public async Task<AppPayload> LoginToOrgAzureAD(string orgRoute, [FromBody] User externalUserInfo = null)
        {
            bool isValid = _securityContext.TokenJwt != null;
            if (!isValid)
            {
                string token = HttpContext.Request.GetBearerToken();
                isValid = await _authTokenValidator.ValidateToken(token, true);
            }

            if (isValid)
            {
                if (externalUserInfo != null)
                {
                    await _userService.UpdateMeAsync(externalUserInfo);
                }

                //NOTE: SECURITY CONTEXT ELEVATION STARTED
                AppPayload result = await _spaService.GetAppPayload(true, true, orgRoute, true);
                //NOTE: SECURITY CONTEXT ELEVATION ENDED
                FixStatusCode();
                return result;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                return null;
            }
        }

        /// <summary>
        /// Responsible for managing the initial login process for Soundbite's Microsoft Teams app.
        /// Action is responsible for azure token validation.
        /// </summary>
        /// <param name="userSettingsService">DI reference.</param>
        /// <returns>an application payload containing data required to initialize the application.</returns>
        [Route("/azureAuth/LoginTeams")]
        [HttpPost, HttpGet, AllowAnonymous]
        public async Task<AppPayload> LoginTeams([FromServices] IUserSettingsService userSettingsService)
        {
            // Make sure to validate the token against the teams app configuration
            _authTokenValidator.UseTeamsAppTokenValidation();

            bool isValid = _securityContext.TokenJwt != null;
            if (!isValid)
            {
                string token = HttpContext.Request.GetBearerToken();
                _authTokenValidator.AllowEmailClaim = true;
                isValid = await _authTokenValidator.ValidateToken(token, true);
            }

            if (isValid)
            {
                AppPayload result = await _spaService.GetAppPayload(true, true, null, true, true);
                FixStatusCode();
                return result;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                return null;
            }
        }

        /// <summary>
        /// Responsible for managing the initial login process for Soundbite's Microsoft SharePoint app.
        /// Action is responsible for azure token validation.
        /// </summary>
        /// <returns>an application payload containing data required to initialize the application.</returns>
        [Route("/azureAuth/LoginSharePoint")]
        [HttpPost, AllowAnonymous]
        public async Task<AppPayload> LoginSharePoint()
        {
            // Uses Azure AD authentication under the covers
            return await LoginAzureAD(null);
        }

        /// <summary>
        /// Sets up an app payload for the given org, including a token based on the current user's bearer token
        /// </summary>
        /// <param name="orgRoute">Route of the organization the user is accessing.</param>
        /// <returns>an <see cref="AppPayload"/> containing application initialization data or <c>null</c> if there are any auth issues.</returns>
        [Route("/azureAuth/loginToOrg/{orgRoute}")]
        [HttpGet]
        [Obsolete("Deprecated for performance; use LoginToOrgAzureAD")]
        public async Task<AppPayload_Obsolete> LoginToOrg(string orgRoute)
        {
            bool isValid = _securityContext.TokenJwt != null;
            if (!isValid)
            {
                string token = HttpContext.Request.GetBearerToken();
                isValid = await _authTokenValidator.ValidateToken(token, true);
            }

            if (isValid)
            {
                //NOTE: SECURITY CONTEXT ELEVATION STARTED
                AppPayload_Obsolete result = await _spaService.GetAppPayload_Obsolete(true, true, orgRoute, true);
                //NOTE: SECURITY CONTEXT ELEVATION ENDED
                FixStatusCode();
                return result;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                return null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ensures that the status code is 200
        /// </summary>
        private void FixStatusCode()
        {
            //TODO: MsGraph.ClientAsync contains an exception handler that sets the Status Code to
            //      403 and adds a header indicating that the application needs to be trusted by an
            //      administrator.  For now, to get around this we just set the status code back to
            //      400 and proceed like nothing happened. We do need to go back and figure out what
            //      the best approach would be.

            if (Response.StatusCode == 403)
            {
                Response.StatusCode = 200;
            }
        }

        #endregion
    }
}