using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Security;
using Masticore.Services;
using Masticore.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Soundbite.App
{
    /// <summary>
    /// Middleware responsible for managing security and setting up the <see cref="ISecurityContext"/> 
    /// for the Soundbite application.  
    /// </summary>
    public class SecurityMiddleware
    {
        #region Fields

        /// <summary>
        /// Stores a reference to the next item to execute in the middleware pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="SecurityMiddleware"/> instance.
        /// </summary>
        /// <param name="next">Reference to the next item to execute in the middleware pipeline.</param>
        public SecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the middleware executes within the middleware pipeline.
        /// </summary>
        /// <param name="httpContext">HTTP context containing request information.</param>
        /// <param name="securityContextService">Security context service reference.</param>
        /// <param name="securityContext">Security context reference.</param>
        /// <param name="logger">Logger service reference.</param>
        /// <param name="tenantService">Tenant service reference.</param>
        /// <param name="infrastructure">Infrastructure service reference.</param>
        /// <param name="tokenDataService">Token data service reference.</param>        
        /// <param name="userService">User service reference.</param>
        /// <param name="mapper">Mapper reference.</param>
        /// <param name="organizationService">Organization service DI reference.</param>
        /// <param name="memberService">Member service DI reference.</param>
        /// <returns>task indicating success or failure of the operation</returns>
        public async Task InvokeAsync(
            HttpContext httpContext,
            ISecurityContextService securityContextService,
            ISecurityContext securityContext,
            ILogger<SecurityMiddleware> logger,
            ITenantService tenantService,
            IIdentityInfrastructure infrastructure,
            ITokenDataService tokenDataService,
            IUserService userService,
            IMapper mapper,
            IOrganizationService organizationService,
            IMemberService memberService)
        {
            try
            {
                SecurityMiddlewareHandler handler = new SecurityMiddlewareHandler(httpContext, securityContextService, securityContext, logger,
                tenantService, infrastructure, tokenDataService, userService, mapper, organizationService, memberService);

                await handler.InvokeAsync();

                // Allow middleware to continue
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Exception in {nameof(SecurityMiddleware)} with error {ex.Message} and trace {ex.StackTrace}");
                throw;
            }
        }

        #endregion
    }

    /// <summary>
    /// Contains the processing logic for running the security middleware.  Middleware is only 
    /// instantiated once, so methods directly in a middleware class must be treated like static
    /// methods, which can make it cumbersome to pass around services and data. This class is
    /// instantiated by the middleware on each request to process security logic.
    /// </summary>
    internal class SecurityMiddlewareHandler
    {
        #region Properties

        private string UnvalidatedOrgRoute { get; set; }

        private string UnvalidatedUserRoute { get; set; }

        private string UnvalidatedUserEmail { get; set; }

        private bool IssuerMatchesSoundbiteValue { get; set; }

        private HttpContext HttpContext { get; set; }

        private ISecurityContextService SecurityContextService { get; set; }

        private ISecurityContext SecurityContext { get; set; }

        private IMemberService MemberService { get; set; }

        private ILogger<SecurityMiddleware> Logger { get; set; }

        private ITenantService TenantService { get; set; }

        private IIdentityInfrastructure Infrastructure { get; set; }

        private ITokenDataService TokenDataService { get; set; }

        private IUserService UserService { get; set; }

        private IMapper Mapper { get; set; }

        private IOrganizationService OrganizationService { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Manages services and properties for the Soundbite Security Middleware.
        /// </summary>
        /// <param name="httpContext">HTTP context containing request information.</param>
        /// <param name="securityContextService">Security context service reference.</param>
        /// <param name="securityContext">Security context reference.</param>
        /// <param name="logger">Logger service reference.</param>
        /// <param name="tenantService">Tenant service reference.</param>
        /// <param name="infrastructure">Infrastructure service reference.</param>
        /// <param name="tokenDataService">Token data service reference.</param>        
        /// <param name="userService">User service reference.</param>
        /// <param name="mapper">Mapper reference.</param>
        /// <param name="organizationService">Organization service DI reference.</param>
        /// <param name="memberService">Member service DI reference.</param>
        public SecurityMiddlewareHandler(HttpContext httpContext,
            ISecurityContextService securityContextService, ISecurityContext securityContext,
            ILogger<SecurityMiddleware> logger, ITenantService tenantService,
            IIdentityInfrastructure infrastructure, ITokenDataService tokenDataService,
            IUserService userService, IMapper mapper, IOrganizationService organizationService,
            IMemberService memberService)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            SecurityContextService = securityContextService ?? throw new ArgumentNullException(nameof(securityContextService));
            SecurityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            TenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            TokenDataService = tokenDataService ?? throw new ArgumentNullException(nameof(tokenDataService));
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            OrganizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
            MemberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the middleware executes within the middleware pipeline.
        /// </summary>
        /// <returns>task indicating success or failure of the operation</returns>
        public async Task InvokeAsync()
        {
            // Determine whether the user was authenticated by some other middleware.  Currently this should not
            // happen so we throw an exception.  If third-party middleware is required for later integrations
            // this will need to be changed to allow the middleware to continue on.
            if (HttpContext?.User?.Identity.IsAuthenticated == true)
            {
                //TODO: Re-Enable this when we can pull the Azure/Graph authentication out of the middleware.
                //      We cannot do this quite yet because there are some dependencies for getting user photo
                //      and user information that are semi-dependent on the azure middleware.
                //throw new SecurityException("User authenticated without Soundbite security.");

                //TODO: Remove just the "else" from below and go back to just the IF statement

                // We need to do this to make sure the token information is loaded into the security context
                // correctly to process auto-user creation
                if (PopulateBearerToken())
                {
                    PopulateBearerTokenJwt();
                }
            }
            else if (PopulateBearerToken())
            {
                if (PopulateBearerTokenJwt())
                {
                    if (await PopulateSoundbiteToken())
                    {
                        // Populate the security contexted based on which claim is available
                        bool userPopulated = string.IsNullOrEmpty(UnvalidatedUserRoute)
                            ? await SecurityContextService.SetUserByOrgRouteAndUserEmail(SecurityContext, UnvalidatedOrgRoute, UnvalidatedUserEmail)
                            : await SecurityContextService.SetUserByOrgRouteAndUserRoute(SecurityContext, UnvalidatedOrgRoute, UnvalidatedUserRoute);

                        if (userPopulated)
                        {
                            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(SecurityContext.TokenJwt.Claims, "SoundbiteToken"));

                            ////////////////////////////////////////////////////////////////////////////

                            // TODO: the more I think about it the less I think this should be here.
                            //   I think it should be removed and the group/team security checking should
                            //   occur closer to the actual actions instead of on initial request processing.

                            // Determine whether the team route can be determined by route information
                            RouteData routeData = HttpContext.GetRouteData();
                            if (routeData.Values.TryGetValue("groupRoute", out object groupRoute))
                            {
                                SecurityContext.GroupRoute = groupRoute?.ToString();
                            }
                            ////////////////////////////////////////////////////////////////////////////
                        }
                        else
                        {
                            // Failed to populate user with a validated token with user claims so something is wrong
                            throw new SecurityException("Failed to load claims user.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the bearer token from the request and populates it into the security context.
        /// </summary>
        /// <returns>a string containing the value of the bearer token, or <c>null</c> if no bearer token is found.</returns>
        private bool PopulateBearerToken()
        {
            if (HttpContext.Request.Headers.ContainsKey(Constants.HttpHeaders.Authorization))
            {
                //Acquire the token - Substring call removes the 'Bearer ' portion of the header string
                SecurityContext.TokenRaw = HttpContext.Request.Headers[Constants.HttpHeaders.Authorization].ToString()[7..];
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse the raw bearer token into a <see cref="JwtSecurityToken"/>, and stores
        /// the JWT token in the security context.
        /// </summary>
        /// <returns>a boolean value indicating whether the bearer token was succesfully parsed as a JWT.</returns>
        private bool PopulateBearerTokenJwt()
        {
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                SecurityContext.TokenJwtUnvalidated = handler.ReadJwtToken(SecurityContext.TokenRaw);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Acquires the value of claim from the unvalidated JWT token.
        /// </summary>
        /// <param name="claimName">Name of the claim to retrieve.</param>
        /// <returns>the value of the claim or <c>null</c> if the claim is not present.</returns>
        private string GetUnvalidatedClaim(string claimName)
        {
            return SecurityContext.TokenJwtUnvalidated.Claims
                .Where(i => i.Type == claimName)
                .Select(i => i.Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Responsbile for parsing the token using token encryption settings associated with the
        /// organization specified in the org route claim.  This establishes the token signature is 
        /// valid and claims issued by a trusted source, but no validation has occured on the 
        /// organization / user contained in the claims (e.g. they could be disabled, etc).
        /// </summary>
        /// <returns><c>true</c>if the token has a valid signature, otherwise <c>false</c>.</returns>
        private async Task<bool> PopulateSoundbiteToken()
        {
            // Determine whether the token is a Soundbite token
            string issuer = GetUnvalidatedClaim(Constants.Tokens.IssuerClaim);
            IssuerMatchesSoundbiteValue = issuer == TokenServiceSettings.TokenIssuerClaimValue;

            // Acquire organization route claim (will be validated later).
            UnvalidatedOrgRoute = GetUnvalidatedClaim(Constants.Tokens.OrgRouteClaim);

            // Only proceed if the org route value has been supplied
            if (!string.IsNullOrEmpty(UnvalidatedOrgRoute))
            {
                if (IssuerMatchesSoundbiteValue)
                {
                    // Tokens issued by soundbite use the user route value to identify the user.
                    // Acquire user route claim (will be validated later)
                    UnvalidatedUserRoute = GetUnvalidatedClaim(Constants.Tokens.UserRouteClaim);
                }
                else
                {
                    // Tokens issued by third parties use the user email value to identify the user.
                    UnvalidatedUserEmail = GetUnvalidatedClaim(Constants.Tokens.UserEmailClaim);
                }

                // Acquire org-specific settings used to validate token. If the org route matches
                // the User-Only OrgRoute then this is an intermediary token used between third
                // party authentication and org-specific login so the default token settings are
                // used to validate the token.
                TokenServiceSettings orgTokenSettings = UnvalidatedOrgRoute == Constants.UserOnlyOrgRoute
                    ? TokenServiceSettings.DefaultTokenServiceSettings
                    : await TokenDataService.GetTokenSettingsByOrgRoute(UnvalidatedOrgRoute);

                ITokenService orgTokenService = new TokenServiceFactory(Logger).GetTokenService(orgTokenSettings);
                try
                {
                    SecurityContext.TokenJwt = orgTokenService.GetTokenValidated(SecurityContext.TokenRaw);
                    return SecurityContext.TokenJwt != null;
                }
                catch
                {
                    throw new SecurityException("Token is invalid");
                }
            }

            return false;
        }

        #endregion
    }
}