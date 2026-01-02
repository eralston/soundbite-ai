using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Defines the contract required for managing tenants.
    /// </summary>
    public interface ITenantService : IService
    {
        /// <summary>
        /// Gets the tenant associated with the specified user.
        /// </summary>
        /// <param name="userRoute">Route of the user whose tenant is being sought.</param>
        /// <returns>a reference to the tenant associated with the user.</returns>
        Task<Tenant> GetByUserRouteAsync(string userRoute);
    }
}