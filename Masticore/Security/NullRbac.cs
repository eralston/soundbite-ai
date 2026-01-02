using System.Threading.Tasks;

namespace Masticore.Security
{
    /// <summary>
    /// <see cref="IRbac"/> implementation that does not actually check anything
    /// </summary>
    /// <remarks>If possible, it's better to use an <see cref="IRbac"/> implementation that validates entities are real against a data store</remarks>
    public class NullRbac : IRbac
    {
        /// <inheritdoc />
        public Task AssertCurrentUser()
        {
            return AssertCurrentUserInRole(UserRole.User);
        }

        /// <summary>
        /// Always allows access
        /// </summary>
        /// <param name="minimumRole"></param>
        /// <returns></returns>
        public Task AssertCurrentUserInRole(UserRole minimumRole)
        {
            // Do nothing, the system has access to everything
            return Task.CompletedTask;
        }

        /// <summary>
        /// Always allows access
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="minimumRole"></param>
        /// <returns></returns>
        public Task AssertCurrentUserInRole(string orgRoute, PersonRole minimumRole)
        {
            // Do nothing, the system has access to everything
            return Task.CompletedTask;
        }

        /// <summary>
        /// Always allows access
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="minimumRole"></param>
        /// <returns></returns>
        public Task AssertCurrentUserInRole(string orgRoute, string groupRoute, MemberRole minimumRole)
        {
            // Do nothing, the system has access to everything
            return Task.CompletedTask;
        }

        /// <summary>
        /// Always returns <see cref="UserRole.God"/>
        /// </summary>
        /// <returns></returns>
        public Task<UserRole> CurrentUserRole()
        {
            return Task.FromResult(UserRole.God);
        }

        /// <summary>
        /// Always returns <see cref="PersonRole.Admin"/>
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public Task<PersonRole> CurrentPersonRole(string orgRoute)
        {
            return Task.FromResult(PersonRole.Admin);
        }

        /// <summary>
        /// Always returns <see cref="MemberRole.Owner"/>
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        public Task<MemberRole> CurrentMemberRole(string orgRoute, string groupRoute)
        {
            return Task.FromResult(MemberRole.Owner);
        }

        /// <summary>
        /// Always returns null
        /// </summary>
        /// <returns></returns>
        public string UniversalIdForCurrentUser => null;
    }
}
