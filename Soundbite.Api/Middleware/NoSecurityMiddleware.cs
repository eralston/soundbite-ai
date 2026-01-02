using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.App
{
    /// <summary>
    /// Middleware responsible for managing security and setting up the <see cref="ISecurityContext"/> 
    /// for the Soundbite application.  
    /// </summary>
    /// <remarks>
    /// THIS IS A WORK IN PROGRESS AND IT DOES NOT YET WORK RELIABLY
    /// </remarks>
    public class NoSecurityMiddleware
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
        public NoSecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the middleware executes within the middleware pipeline.
        /// </summary>
        /// <param name="httpContext">HTTP context containing request information.</param>
        /// <param name="securityContext">Security context reference.</param>
        /// <param name="infrastructure">Infrastructure service reference.</param>
        /// <param name="mapper">Mapper reference.</param>
        /// <returns>task indicating success or failure of the operation</returns>
        public async Task InvokeAsync(
            HttpContext httpContext,
            ISecurityContext securityContext,
            IIdentityInfrastructure infrastructure,
            IMapper mapper)
        {
            NoSecurityMiddlewareHandler handler =
                new NoSecurityMiddlewareHandler(
                    httpContext,
                    securityContext,
                    infrastructure,
                    mapper);

            await handler.InvokeAsync();

            // Allow middleware to continue
            await _next(httpContext);
        }

        #endregion
    }

    /// <summary>
    /// This enables running the app locally without internet access by just reading the DB and assuming the current user is the first user and checking routes for other context
    /// </summary>
    internal class NoSecurityMiddlewareHandler
    {
        #region Properties

        private ISecurityContext SecurityContext { get; set; }

        private IIdentityInfrastructure Infrastructure { get; set; }

        private IMapper Mapper { get; set; }

        private HttpContext HttpContext { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="securityContext"></param>
        /// <param name="infrastructure"></param>
        /// <param name="mapper"></param>
        public NoSecurityMiddlewareHandler(
            HttpContext httpContext,
            ISecurityContext securityContext,
            IIdentityInfrastructure infrastructure,
            IMapper mapper)
        {
            HttpContext = httpContext;
            SecurityContext = securityContext;
            Infrastructure = infrastructure;
            Mapper = mapper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when the middleware executes within the middleware pipeline.
        /// </summary>
        public async Task InvokeAsync()
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            SetUser(db);

            RouteData routeData = HttpContext.GetRouteData();
            SetOrg(routeData);
            SetPerson(db);
            SetGroup(routeData);
        }

        private void SetGroup(RouteData routeData)
        {
            if (routeData.Values.TryGetValue("grouproute", out object groupRoute))
            {
                SecurityContext.GroupRoute = groupRoute?.ToString();
            }
        }

        private void SetOrg(RouteData routeData)
        {
            if (routeData.Values.TryGetValue("orgroute", out object orgroute))
            {
                SecurityContext.OrgRoute = orgroute?.ToString();
            }
        }

        private void SetUser(IIdentityDb db)
        {
            UserEntity user = db.Users.Where(u => u.Id == 1).FirstOrDefault();

            if (user == null)
            {
                throw new Exception("Cannot run without internet and without a loaded database");
            }

            SecurityContext.CurrentUserId = user.Id;
            SecurityContext.CurrentUser = Mapper.MapSafe<User>(user);
            SecurityContext.UniversalId = user.UniversalId;
        }

        private void SetPerson(IIdentityDb db)
        {
            if (SecurityContext.OrgRoute == null)
            {
                return;
            }

            PersonEntity person = db.People.Where(p =>
                p.DeletedUtc == null &&
                p.Organization.Route == SecurityContext.OrgRoute &&
                p.User.Id == SecurityContext.CurrentUserId
            ).FirstOrDefault();

            if (person == null)
            {
                return;
            }

            SecurityContext.CurrentPerson = Mapper.MapSafe<Person>(person);
            SecurityContext.CurrentPersonId = person.Id;
        }

        #endregion
    }
}