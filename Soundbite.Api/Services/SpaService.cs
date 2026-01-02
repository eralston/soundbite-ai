using Masticore;
using Masticore.Ad;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Masticore.Token;
using Microsoft.Extensions.Logging;
using Soundbite.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Api.Services
{
    /// <summary>
    /// Service exposing methods for managing the single page application (SPA). 
    /// </summary>
    public class SpaService
    {
        #region Fields

        private readonly ISecurityContext _securityContext;
        private readonly IOrganizationService _orgService;
        private readonly ISbOrganizationService _sbOrgService;
        private readonly ITokenDataService _tokenDataService;
        private readonly IUserService _userService;
        private readonly IRbac _rbac;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a reference to the application configuration settings.
        /// </summary>
        public SpaConfig SpaConfig { get; }

        /// <summary>
        /// Logger for accessing output
        /// </summary>
        protected ILogger<SpaService> Logger { get; }



        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SpaService"/> class
        /// </summary>
        /// <param name="securityContext"></param>
        /// <param name="rbac"></param>
        /// <param name="spaConfig"></param>
        /// <param name="userService"></param>
        /// <param name="orgService"></param>
        /// <param name="sbOrgService"></param>
        /// <param name="tokenDataService"></param>
        /// <param name="logger"></param>
        public SpaService(
            ISecurityContext securityContext,
            IRbac rbac,
            SpaConfig spaConfig,
            IUserService userService,
            IOrganizationService orgService,
            ISbOrganizationService sbOrgService,
            ITokenDataService tokenDataService,
            ILogger<SpaService> logger)
        {
            _securityContext = securityContext;
            _rbac = rbac;
            SpaConfig = spaConfig;
            _userService = userService;
            _orgService = orgService;
            _sbOrgService = sbOrgService;
            _tokenDataService = tokenDataService;
            Logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find the orgRoute for the current request
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private string DetermineOrgRouteForPayload(string orgRoute, AppPayload result)
        {
            string tokenOrg = orgRoute;

            if (tokenOrg == null && !string.IsNullOrEmpty(_securityContext.OrgRoute))
            {
                tokenOrg = _securityContext.OrgRoute;
            }
            else if (result.Organizations?.Count() == 1)
            {
                tokenOrg = result.Organizations?.First().Route;
            }

            return tokenOrg;
        }

        /// <summary>
        /// Determines whether the refresh token is valid and if so setups up the security context
        /// for the user specified in the refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        /// <returns><c>true</c> if the security context was successfully updated with user information otherwise <c>false</c>.</returns>
        public async Task<bool> ProcessRefreshToken(string refreshToken)
        {
            return await _userService.SetSecurityContextFromRefreshToken(refreshToken);
        }

        /// <summary>
        /// Retrieves the initial application payload containing information required for the
        /// application to load.  
        /// </summary>
        /// <param name="includeConfig">Flag indicating whether the application configuration should be included in the payload.</param>
        /// <param name="includeUserInfo">Flag indicating whether user information should be included (if available) in the payload.</param>
        /// <param name="orgRoute">Specifies the initial organization route the application is requesting to display (for token retrieval).</param>
        /// <param name="isThirdPartyTokenValid">Flag indicating that the unvalidated token in the security context is a valid third-party token.</param>
        /// <param name="createUser">Optionally use the current user info to create a user if missing</param>
        /// <returns>an <see cref="AppPayload"/> instance containing as much application initialization data as is available.</returns>
        public async Task<AppPayload> GetAppPayload(bool includeConfig, bool includeUserInfo, string orgRoute = null, bool isThirdPartyTokenValid = false, bool createUser = true)
        {
            AppPayload result = new AppPayload() { Config = includeConfig ? SpaConfig : null };

            //TODO: need to look at how to handle this between OKTA/AAD
            if (createUser)
            {
                await HandleInitialUserCreation(result, isThirdPartyTokenValid);
            }

            if (_securityContext.CurrentUser == null)
            {
                return result;
            }

            await _rbac.AssertCurrentUser();

            if (includeUserInfo)
            {
                result.User = await _userService.ReadMeAsync();
                // TODO: A "my orgs" endpoint that provides unpaged, but supported call directly analogous to this obsolete method
                result.Organizations = await _orgService.ReadAllAsync_Obsolete(false);
            }

            // The following code attempts to establish the organization associated with
            // the user. When specified, the requested org route is used. The requested org
            // route normally comes from the "last org route" from the UI or from extracting
            // the org route from the URL path in UI.  If there is no requested org route,
            // then the org route from the security context is used instead. This value is
            // sometimes populated from 3rd party token auth (eg SharePoint & Teams). As a
            // final fallback, if the user is only in a single org, then that organization
            // route is used by default.

            //TODO: Need to add a parameter here called "must match requested org" because
            //  the SPA can handle not having the right organization and it can auto adjust
            //  accordingly.  When coming from Teams/SharePoint however, it would be super
            //  awkward if a user's info from another org came across.  Changes of it
            //  happening are SUPER low but it's still possible.  So I think in Teams/SP
            //  and other third-party entry scenarios you use that flag to throw an error,
            //  but in the SPA just eat the error and issue a token for the org with which
            //  the user is associated.

            string payloadOrgRoute = DetermineOrgRouteForPayload(orgRoute, result);

            if (!string.IsNullOrEmpty(payloadOrgRoute))
            {
                try
                {
                    result.Organization = await _sbOrgService.ReadAsync(payloadOrgRoute);
                }
                catch (Exception ex)
                {
                    // If a user was in an organization but was removed, the lingering
                    // "last org route" stored in the browser may be sent over resulting in
                    // an exception here.  So log it and move on.
                    Logger.LogError(ex, $"Error trying to read Organization for user {_securityContext.CurrentUser.Route} in org '{payloadOrgRoute}' with error {ex.Message} and trace {ex.StackTrace}");
                }

                if (result.Organization != null)
                {
                    // Only provide token information if the user has access to the requested organization
                    TokenInfo tokenInfo = await _tokenDataService.CreateUserToken(payloadOrgRoute);
                    result.Token = tokenInfo.Token;
                    result.RefreshToken = tokenInfo.RefreshToken;
                }
            }

            // User exists but no organization association could be established.  Issue a
            // user-only token so the user can make non org specific API calls.
            if (string.IsNullOrEmpty(result.Token) && result.User != null)
            {
                // Organization is not set but the user has (presumably) authenticated
                // so issue a user-only token that can be used to login to an organization.
                TokenInfo tokenInfo = await _tokenDataService.CreateUserToken(Constants.UserOnlyOrgRoute);
                result.Token = tokenInfo.Token;
                result.RefreshToken = tokenInfo.RefreshToken;
            }

            return result;
        }

        /// <summary>
        ///  TODO: REMOVE WHEN AGED OUT
        /// </summary>
        /// <param name="includeConfig"></param>
        /// <param name="includeUserInfo"></param>
        /// <param name="orgRoute"></param>
        /// <param name="isThirdPartyTokenValid"></param>
        /// <returns></returns>
        /// <exception cref="UserSafeException"></exception>
        [Obsolete("This is carried over for backward compatability; use GetAppPayload")]
        public async Task<AppPayload_Obsolete> GetAppPayload_Obsolete(bool includeConfig, bool includeUserInfo, string orgRoute = null, bool isThirdPartyTokenValid = false)
        {
            AppPayload_Obsolete result = new AppPayload_Obsolete() { Config = includeConfig ? SpaConfig : null };

            await HandleInitialUserCreation(result, isThirdPartyTokenValid);

            if (_securityContext.CurrentUser != null)
            {
                await _rbac.AssertCurrentUser();

                if (includeUserInfo)
                {
                    result.User = await _userService.ReadMeAsync();
                    result.Organizations = await _orgService.ReadAllAsync_Obsolete(false);
                }

                // When an orgRoute is explicitly requested, then explicitly use it with no
                // fallback. It seems resonable to fail in this scenario even if other valid
                // orgRoutes are available. When no orgRoute is explictly specified, fallback
                // to the orgRoute in the security context. This ensures arrivals authenticated
                // from third party tokens (e.g. from SharePoint/Teams) will default to the org
                // authenticated in the request. Never default the org when the orgRoute in the
                // security context is populated because it could lead to a strange scenario
                // where a user is logging in from Org A but then sees data from Org B.  Last,
                // when there is no orgRoute specified and there is only one organization
                // available to the user, assume that is correct and use it.

                string tokenOrg = orgRoute;
                if (tokenOrg == null && !string.IsNullOrEmpty(_securityContext.OrgRoute))
                {
                    tokenOrg = _securityContext.OrgRoute;
                }
                else if (result.Organizations?.Count() == 1)
                {
                    tokenOrg = result.Organizations?.First().Route;
                }

                if (!string.IsNullOrEmpty(tokenOrg))
                {
                    try
                    {
                        result.Organization = await _sbOrgService.ReadAsync_Obsolete(tokenOrg);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Error trying to read Organization for user {_securityContext.CurrentUser.Route} in org '{tokenOrg}' with error {ex.Message} and trace {ex.StackTrace}");
                        throw new UserSafeException("Cannot Find Organization for User", ex);
                    }

                    if (result.Organization != null)
                    {
                        // Only provide token information if the user has access to the requested organization
                        TokenInfo tokenInfo = await _tokenDataService.CreateUserToken(tokenOrg);
                        result.Token = tokenInfo.Token;
                        result.RefreshToken = tokenInfo.RefreshToken;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Attempts a login with a third-party authentication token.
        /// </summary>
        /// <param name="externalUserInfo">Information about the external user that should be added to soundbite.</param>
        /// <returns>an <see cref="AppPayload"/> instance containing as much applicaiton initialization data as is available.</returns>
        public async Task<AppPayload> LoginFromThirdParty(User externalUserInfo)
        {
            if (externalUserInfo != null)
            {
                await _userService.UpsertMeAsync(externalUserInfo);
            }
            AppPayload result = await GetAppPayload(false, true);
            return result;
        }

        /// <summary>
        /// Retrieves a new token for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization for which a token is being sought.</param>
        /// <returns>an API token for the organization</returns>
        public async Task<TokenInfo> GetToken(string orgRoute)
        {
            //TODO: this should probably support a refresh token in the near future
            await _rbac.AssertCurrentUser();

            TokenInfo tokenInfo = null;
            if (await _orgService.IsUserInOrg(_securityContext.CurrentUser.Route, orgRoute))
            {
                tokenInfo = await _tokenDataService.CreateUserToken(orgRoute);
            }
            TokenInfo result = tokenInfo;

            return result;
        }

        /// <summary>
        /// Determines whether an initial user should be created based on the request state.
        /// </summary>
        /// <param name="appPayload">Application payload.</param>
        /// <param name="isThirdPartyTokenValid">Flag indicating that the third party token has been validated.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        private async Task HandleInitialUserCreation(IAppPayload appPayload, bool isThirdPartyTokenValid)
        {
            if (_securityContext.CurrentUser == null && isThirdPartyTokenValid && _securityContext.TokenJwtUnvalidated != null)
            {
                // Attempt to create the user
                try
                {
                    AdAccessTokenClaims claims = new AdAccessTokenClaims(_securityContext.TokenJwtUnvalidated);
                    appPayload.User = await _userService.UpsertMeAsync(claims);
                    if (appPayload.User != null)
                    {
                        _securityContext.CurrentUser = appPayload.User;
                        _securityContext.UniversalId = appPayload.User.UniversalId;
                    }
                    else
                    {
                        // Creating a user from claims sometimes results in a null reference but no exception
                        // so we account for that possibility here
                        throw new Exception("Failed to create user from Claims principal.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical("Failed to create initial user from inbound third-party claims information.", ex);
                    throw;
                }
            }
        }

        #endregion
    }
}