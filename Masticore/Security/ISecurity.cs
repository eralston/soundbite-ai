using System.Security.Claims;

namespace Masticore.Security
{
    /// <summary>
    /// Interface for an object that provides the actions and implementation of a directory providing users and groups to the system
    /// </summary>
    public interface ISecurity
    {
        /// <summary>
        /// Extracts an <see cref="IClaims"/> object from the given <see cref="ClaimsPrincipal"/>.
        /// This mapping is specific to the security provider.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        IClaims GetClaims(ClaimsPrincipal principal);
    }
}
