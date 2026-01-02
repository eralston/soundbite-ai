using Masticore.Models;
using Soundbite.Models;
using System;
using System.Collections.Generic;

namespace Soundbite.Api
{
    /// <summary>
    /// An obsolete version of <see cref="AppPayload"/>
    /// </summary>
    [Obsolete("Deprecated for perf reasons, use AppPayload")]
    public class AppPayload_Obsolete : IAppPayload
    {
        /// <summary>
        /// Gets or sets a reference to the user who is logged into the system.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets an authorization token that can be used to authenticate against the API.
        /// This value is populated in the <see cref="AppPayload"/> in response to receiving an
        /// HTTP-only cookie containing a valid refresh token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the refresh token that can be used to request a new API token.  This value 
        /// is populated in the <see cref="AppPayload"/> in response to receiving an HTTP-only 
        /// cookie containing a valid refresh token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets SPA configuration settings to send to the client.
        /// </summary>
        public SpaConfig Config { get; set; }

        /// <summary>
        /// Gets or sets a reference to the organization the user is logging into.  This contains 
        /// additional information about the current organization that is not present in the list
        /// of organizations stored in <see cref="Organizations"/>.
        /// </summary>        
        public OrganizationWithPermissions_Obsolete Organization { get; set; }

        /// <summary>
        /// Gets a list of organization to which the user belongs (across all tenants).
        /// </summary>        
        public IEnumerable<OrganizationExtended> Organizations { get; set; }
    }
}
