using Masticore.DirectorySync;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Api.Services;
using Soundbite.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// An insecure controller for pulling client-side configuration dynamically from the app
    /// Unfortunately not a jacuzzi remote controller
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class OktaAuthController : ControllerBase
    {
        #region Properties

        private readonly SpaService _spaService;
        private readonly ISecurityContext _securityContext;
        private readonly ISecurityContextService _securityContextService;
        private readonly IOktaDataService _oktaDataService;
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOrgSyncStore _orgSyncStore;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="AzureAuthController"/> instance.
        /// </summary>
        /// <param name="securityContext">DI Security context reference.</param>
        /// <param name="securityContextService">DI reference to the security context service.</param>
        /// <param name="oktaDataService">DI reference to an OKTA data service.</param>
        /// <param name="spaService">DI SPA service reference.</param>
        /// <param name="userService">DI User Service reference.</param>        
        /// <param name="httpClientFactory">DI HTTP client factory.</param>
        /// <param name="orgSyncStore">DI organization sync store.</param>
        public OktaAuthController(
            ISecurityContext securityContext,
            ISecurityContextService securityContextService,
            SpaService spaService,
            IOktaDataService oktaDataService,
            IUserService userService,
            IHttpClientFactory httpClientFactory,
            IOrgSyncStore orgSyncStore)
        {
            _securityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
            _securityContextService = securityContextService ?? throw new ArgumentNullException(nameof(securityContextService));
            _spaService = spaService ?? throw new ArgumentNullException(nameof(spaService));
            _oktaDataService = oktaDataService ?? throw new ArgumentNullException(nameof(oktaDataService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _orgSyncStore = orgSyncStore ?? throw new ArgumentNullException(nameof(orgSyncStore));
        }

        #endregion

        #region Actions        

        /// <summary>
        /// Gets the OKTA settings associated with the specified user.  Returns <c>null</c> if the
        /// user is not associated with any organizations with a valid OKTA configuration. If the
        /// user is associated with multiple organizations with OKTA settings the first is returned.
        /// </summary>
        /// <param name="email">Email of the person who is attempting to login with OKTA.</param>
        /// <returns>a list of all the active organizations to which the current user has access.</returns>
        [Route("/oktaAuth/settingsByEmail")]
        [HttpGet, AllowAnonymous]
        public async Task<OrgOktaSettingsWithOrgRoute> GetSettingsByEmail(string email)
        {
            OrgOktaSettingsWithOrgRoute result = await _oktaDataService.GetOktaSettingsByEmail(email);
            return result;
        }

        /// <summary>
        /// Responsible for retrieving the OKTA settings for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose OKTA settings are being sought.</param>
        /// <returns>a reference to the organization's OKTA settings if found otherwise <c>null</c>.</returns>
        [Route("/oktaAuth/settingsByRoute")]
        [HttpGet, AllowAnonymous]
        public async Task<OrgOktaSettingsWithOrgRoute> GetSettingsByRoute(string orgRoute)
        {
            OrgOktaSettingsWithOrgRoute result = await _oktaDataService.GetOktaSettingsByOrgRoute(orgRoute);
            return result;
        }

        /// <summary>
        /// Login path for Okta based on given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/oktaAuth/login")]
        [HttpPost, AllowAnonymous]
        public async Task<AppPayload> Login(string orgRoute)
        {
            string bearerToken = HttpContext.Request.GetBearerToken();
            AppPayload result = null;

            if (!string.IsNullOrEmpty(bearerToken) && !string.IsNullOrEmpty(orgRoute))
            {
                OrgOktaSettingsWithOrgRoute oktaSettings = await _oktaDataService.GetOktaSettingsByOrgRoute(orgRoute);
                if (oktaSettings != null)
                {
                    try
                    {
                        OktaTokenValidator tokenValidator = new OktaTokenValidator(oktaSettings);
                        JwtSecurityToken jwtToken = await tokenValidator.ValidateToken(bearerToken);
                        if (jwtToken != null)
                        {
                            string email = jwtToken.Claims.FirstOrDefault(i => i.Type == "sub")?.Value;
                            if (!string.IsNullOrEmpty(email))
                            {
                                //NOTE: SECURITY CONTEXT ELEVATION STARTED
                                User user = await _userService.ReadByEmail(email);
                                if (user != null)
                                {
                                    if (user.UniversalId == null || user.InviteAcceptUtc == null)
                                    {
                                        //TODO: this is not ideal so refactor it out when revisiting a more unified approach to JWT stuff...
                                        string uid = user.UniversalId ?? jwtToken.Claims.FirstOrDefault(i => i.Type == "uid")?.Value;
                                        await _oktaDataService.REFACTOR_SetRequiredInfoOnUser(user, uid);
                                    }
                                }
                                await _securityContextService.SetUserByOrgRouteAndUserEmail(_securityContext, orgRoute, email);
                                result = await _spaService.GetAppPayload(false, true, orgRoute, true, false);
                                //NOTE: SECURITY CONTEXT ELEVATION ENDED
                            }
                        }
                    }
                    catch
                    {
                        // TODO: Logging...
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
