using Masticore.Security;
using System.Security.Claims;

namespace Masticore.Ad
{
    /// <summary>
    /// Implements ISecurity for Active Directory
    /// </summary>
    public class AdSecurity : ISecurity
    {
        /// <summary>
        /// Returns the IClaims implementor for Active Directory
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public IClaims GetClaims(ClaimsPrincipal principal)
        {
            return new AdAccessTokenClaims(principal);
        }
    }
}
