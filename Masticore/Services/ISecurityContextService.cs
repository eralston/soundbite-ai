using Masticore.Security;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Interface defining data operations involving the <see cref="ISecurityContext"/>.
    /// </summary>
    public interface ISecurityContextService
    {
        /// <summary>
        /// Populates the current organization info by an organization's universal ID.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgUniversalId">Universal ID of the organization whose info should be populated into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        Task<bool> SetCurrentOrgByOrgUniversalId(ISecurityContext securityContext, string orgUniversalId);

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="email">Email address of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        Task<bool> SetUserByOrgRouteAndUserEmail(ISecurityContext securityContext, string orgRoute, string email);

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="userRoute">Route of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        Task<bool> SetUserByOrgRouteAndUserRoute(ISecurityContext securityContext, string orgRoute, string userRoute);

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="orgRoute">Route of the organization with which the user is associated.</param>
        /// <param name="univeralId">Universal ID of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        Task<bool> SetUserByOrgRouteAndUserUniversalId(ISecurityContext securityContext, string orgRoute, string univeralId);

        /// <summary>
        /// Populates the security context with the specified user's context.
        /// </summary>
        /// <param name="securityContext">Security context to update.</param>
        /// <param name="azureTenantId">Azure Tenant ID of the organization with which the user is associated.</param>
        /// <param name="userUniveralId">Universal ID of the user to populate into the security context.</param>
        /// <returns>a flag indicating whether or not a user was loaded into the security context.</returns>
        Task<bool> SetUserByAzureTenantAndUserUniversalId(ISecurityContext securityContext, string azureTenantId, string userUniveralId);
    }
}