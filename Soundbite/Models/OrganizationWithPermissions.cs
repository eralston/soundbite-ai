using Masticore.Models;
using System;

namespace Soundbite.Models
{
    /// <summary>
    /// Represents an organization along with that organization's UI Permissions
    /// </summary>
    [Obsolete("Use OrganizationWithSettings instead.")]
    public class OrganizationWithPermissions
    {
        /// <summary>
        /// Gets or sets the RBAC customization for this org
        /// </summary>
        [Obsolete("Use Permissions on the Settings property on OrganizationWithSettings instead.")]
        public OrgPermissions Permissions { get; set; }

        /// <summary>
        /// Contains details of the requested organization.
        /// </summary>
        public OrganizationDetails Details { get; set; }
    }
}