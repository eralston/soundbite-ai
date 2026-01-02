using Masticore;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Masticore.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Api.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// An insecure controller for pulling client-side configuration dynamically from the app
    /// Unfortunately not a jacuzzi remote controller
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class SpaController : ControllerBase
    {

        #region Debug Login
#if DEBUG

        private readonly ISecurityContextService _securityContextService;
        private readonly ISecurityContext _securityContext;

        /// <summary>
        /// Replace the ProcessRefreshToken in the Spa Load method with this method to login
        /// as a specific user without having to actually go through the login process. Make
        /// sure the user has a UniversalID in the database.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to login to.</param>
        /// <param name="userRoute">Route of the user to login as.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        private async Task AutoLoginAs(string orgRoute, string userRoute)
        {
            await _securityContextService.SetUserByOrgRouteAndUserRoute(_securityContext, orgRoute, userRoute);
        }

#endif
        #endregion

        #region Properties

        private readonly SpaService _spaService;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="SpaController"/> instance.
        /// </summary>
        /// <param name="spaService">DI SPA service reference.</param>
        public SpaController(
            SpaService spaService
#if DEBUG
            , ISecurityContextService securityContextService,
            ISecurityContext securityContext
#endif
           )
        {
            _spaService = spaService ?? throw new ArgumentNullException(nameof(spaService));
#if DEBUG
            _securityContextService = securityContextService;
            _securityContext = securityContext;
#endif
        }

        #endregion

        #region Helper Methods

        private async Task<bool> ProcessRefreshToken()
        {
            bool result = false;
            if (Request.Headers.ContainsKey("rt"))
            {
                Microsoft.Extensions.Primitives.StringValues refreshToken = Request.Headers["rt"];
                result = await _spaService.ProcessRefreshToken(refreshToken);
            }
            return result;
        }

        /// <summary>
        /// Responsible for parsing out the bearer token from the Authorization HTTP header.
        /// </summary>
        /// <returns>a string containing the bearer token value.</returns>
        private string GetBearerToken()
        {
            string result = null;
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authHeaderValue))
            {
                result = authHeaderValue[0].Length > 7 ? authHeaderValue[0][7..] : null;
            }
            return result;
        }

        #endregion

        #region Actions

        /// <summary>
        /// GET login info for the current users, including <see cref="AppPayload"/> to bootstrap client
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [HttpGet, Route("/spa/init")]
        [Obsolete("This method is replaced with Load")]
        public async Task<AppPayload_Obsolete> SpaInit([CodeGenField(IsNullable = true)] string orgRoute)
        {
            await ProcessRefreshToken();
            AppPayload_Obsolete result = await _spaService.GetAppPayload_Obsolete(true, true, orgRoute);
            FixStatusCode();
            return result;
        }

        /// <summary>
        /// A streamlined login processor that returns a minimal response
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [HttpGet, Route("/spa/load")]
        public async Task<AppPayload> Load([CodeGenField(IsNullable = true)] string orgRoute)
        {
            await ProcessRefreshToken();
            AppPayload result = await _spaService.GetAppPayload(true, true, orgRoute);
            FixStatusCode();
            return result;
        }

        /// <summary>
        /// GET a token for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/getToken/{orgRoute}")]
        [HttpGet, Authorize]
        public async Task<TokenInfo> GetToken(string orgRoute)
        {
            TokenInfo result = await _spaService.GetToken(orgRoute);
            return result;
        }

        /// <summary>
        /// Returns a default value to indicate we're alive
        /// </summary>
        /// <returns></returns>
        [Route("/")]
        [HttpGet]
        [HttpHead]
        [AllowAnonymous]
        public string Ping()
        {
            return "Hello, can you hear me?";
        }

        /// <summary>
        /// Returns a default value to indicate we're alive
        /// </summary>
        /// <returns></returns>
        [Route("/version")]
        [HttpGet]
        public string Version()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

#if DEBUG
        /// <summary>
        /// Generates one or more routes for use in administrative or development pusposes. This
        /// action is only available in developer builds of the API and should not be used in
        /// the actual product itself. Routes generated are random and not guaranteed to be unique.
        /// </summary>
        /// <param name="quantity">Number of routes to generate.</param>
        /// <returns>a string containing generated routes</returns>
        [Route("/generateRoute")]
        [HttpGet, AllowAnonymous]
        public string GenerateRoute(int quantity = 1)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            string result = string.Empty;
            for (int i = 0; i < quantity; i++)
            {
                if (i > 0)
                {
                    result += "\r\n";
                }

                result += ResourceExtensions.NewRoute();
            }
            return result;
        }
#endif

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
