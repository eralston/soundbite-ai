using Masticore.Exceptions;
using System.Threading.Tasks;

namespace Masticore.Security
{
    /// <summary>
    /// Interface for Role-Based Access Control (RBAC) utility methods
    /// </summary>
    /// <remarks>
    /// This allows for a swappable implementation of RBAC between a user-centric, token-based API and a background task daemon
    /// </remarks>
    public interface IRbac
    {
        /// <summary>
        /// Async get the Universal ID for the current user
        /// </summary>
        /// <remarks>Concrete classes may return null, indicating there is no current user</remarks>
        /// <returns></returns>
        string UniversalIdForCurrentUser { get; }

        /// <summary>
        /// Throws a <see cref="NotFoundException"/> if the current user does not have at least a <see cref="UserRole.User"/> level role.
        /// </summary>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task AssertCurrentUser();

        /// <summary>
        /// Throws a <see cref="NotFoundException"/> if the current user is NOT in the given <see cref="UserRole"/>
        /// </summary>
        /// <param name="minimumRole"></param>
        /// <returns></returns>
        Task AssertCurrentUserInRole(UserRole minimumRole);

        /// <summary>
        /// Throws a <see cref="NotFoundException"/> if the current user is NOT in the given <see cref="PersonRole"/> in the given org.
        /// The current user's <see cref="UserRole"/> may override.
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="minimumRole"></param>
        /// <returns></returns>
        Task AssertCurrentUserInRole(string orgRoute, PersonRole minimumRole);

        /// <summary>
        /// Throws a <see cref="NotFoundException"/> if the current user i NOT in the given <see cref="MemberRole"/>.
        /// The current user's <see cref="UserRole"/> or <see cref="PersonRole"/> may override.
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="minimumRole"></param>
        /// <returns></returns>
        Task AssertCurrentUserInRole(string orgRoute, string groupRoute, MemberRole minimumRole);

        /// <summary>
        /// Async get the <see cref="UserRole"/> value based on <see cref="UniversalIdForCurrentUser"/>
        /// </summary>
        /// <remarks>If the current user does not exist, this should throw <see cref="NotFoundException"/></remarks>
        /// <returns></returns>
        Task<UserRole> CurrentUserRole();

        /// <summary>
        /// Async get the <see cref="PersonRole"/> value based on <see cref="UniversalIdForCurrentUser"/> in the given org
        /// </summary>
        /// <remarks>If the current user does not exist, this should throw <see cref="NotFoundException"/>; otherwise, this will return <see cref="PersonRole.Unknown"/> even if the org doesn't exist</remarks>
        /// <returns></returns>
        Task<PersonRole> CurrentPersonRole(string orgRoute);

        /// <summary>
        /// Async get the <see cref="MemberRole"/> value based on <see cref="UniversalIdForCurrentUser"/> in the given group in the given org
        /// </summary>
        /// <remarks>If the current user does not exist, this should throw <see cref="NotFoundException"/>; otherwise, this will return <see cref="MemberRole.Unknown"/> even if the org and/or group doesn't exist</remarks>
        /// <returns></returns>
        Task<MemberRole> CurrentMemberRole(string orgRoute, string groupRoute);
    }
}
