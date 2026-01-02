using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// SQL Implementation of the <see cref="ITenantService"/>.
    /// </summary>
    public class TenantService : IdentityServiceBase, ITenantService
    {
        #region Constructor

        /// <summary>
        /// Creates a new <see cref="TenantService"/> instance.
        /// </summary>
        public TenantService(ISecurityContext securityContext, IIdentityInfrastructure infrastructure, IMapper mapper, ILogger<TenantService> logger) : base(infrastructure, logger, mapper, securityContext)
        {
        }

        #endregion

        #region ITenantService Implementation

        /// <summary>
        /// Gets the tenant associated with the specified user.
        /// </summary>
        /// <param name="userRoute">Route of the user whose tenant is being sought.</param>
        /// <returns>a reference to the tenant associated with the user.</returns>
        public async Task<Tenant> GetByUserRouteAsync(string userRoute)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            TenantEntity tenant = await db.People
                .Where(i => i.User.Route == userRoute
                    && i.User.DeletedUtc == null
                    && i.Organization.Route == SecurityContext.OrgRoute
                    && i.Organization.DeletedUtc == null
                    && i.Organization.Tenant.DeletedUtc == null)
                .Select(i => i.Organization.Tenant)
                .FirstOrDefaultAsync();
            Tenant result = Mapper.MapSafe<Tenant>(tenant);
            return result;
        }

        #endregion
    }
}