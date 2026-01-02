using Masticore.Models;
using System;

namespace Soundbite.Models
{
    /// <summary>
    /// TODO: REMOVE WHEN ABLE
    /// </summary>
    [Obsolete("Deprecated for perf reasons; use OrganizationWithPermissions")]
    public class OrganizationWithPermissions_Obsolete
    {
        /// <summary>
        /// Gets or sets the RBAC customization for this org
        /// </summary>
        public OrgPermissions Permissions { get; set; }

        /// <summary>
        /// Contains details of the requested organiation.
        /// </summary>
        public OrganizationDetails_Obsolete Details { get; set; }
    }
}