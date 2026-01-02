using Masticore.Resources;
using Masticore.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Masticore.Ad
{
    /// <summary>
    /// Azure Active Directory token claims implementation. This implementation should be used
    /// when working with access tokens issued for applications from microsoft graph.
    /// </summary>
    public class AdAccessTokenClaims : ClaimsBase, IClaims
    {
        #region Claims Constants

        internal const string ClaimEmailV1 = "email";
        internal const string ClaimEmailV2 = "preferred_username";
        internal const string ClaimUniqueName = "unique_name";
        internal const string ClaimName = "name";
        internal const string ClaimGivenName = "given_name";
        internal const string ClaimFamilyName = "family_name";
        internal const string ClaimOid = "oid";
        internal const string ClaimTid = "tid";

        #endregion

        #region Constructor

        public AdAccessTokenClaims(JwtSecurityToken jwtToken)
            : base(jwtToken.Claims)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AdMiddlewareClaims"/> instance.
        /// </summary>
        /// <param name="claims">Claims information.</param>
        public AdAccessTokenClaims(IEnumerable<Claim> claims)
            : base(claims)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AdMiddlewareClaims"/> instance.
        /// </summary>
        /// <param name="principal">Claims principal containing claims information.</param>        
        public AdAccessTokenClaims(ClaimsPrincipal principal)
            : base(principal?.Claims)
        {
        }

        #endregion

        #region IClaims Implementation

        /// <summary>
        /// Gets the email for this user
        /// </summary>
        public string Email => GetClaim(new[] { ClaimEmailV1, ClaimEmailV2, ClaimUniqueName }, true, (value) => { return ResourceExtensions.IsEmail(value); });

        /// <summary>
        /// Gets the fullname of the user
        /// </summary>
        public string FullName
        {
            get
            {
                string value = GetClaim(ClaimName, false, null);
                if (string.IsNullOrEmpty(value))
                {
                    value = (GetClaim(ClaimGivenName, false, null) + " " + GetClaim(ClaimFamilyName, false, null)).Trim();
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new Exception("Full name could not be determined by the given claims.");
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// Gets the unique ID for the directory authenticating this user
        /// </summary>
        public string DirectoryUniversalId => GetClaim(ClaimTid, true, null);

        #endregion

        #region IUniversal Implementation

        public string UniversalId
        {
            get => GetClaim(ClaimOid, true, null);
            set => throw new NotImplementedException("Set UniversalId not implemented in AdAccessTokenClaims");
        }

        public ProviderType ProviderType => ProviderType.AAD;

        #endregion
    }
}
