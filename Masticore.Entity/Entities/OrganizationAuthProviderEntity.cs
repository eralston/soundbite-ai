using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// Provides data on an organization
    /// </summary>
    public class OrganizationAuthProviderEntity : ResourceEntityBase
    {
        /// <summary>
        /// Gets or sets the type of authentication provider represented
        /// </summary>
        public AuthProviderType ProviderType { get; set; }

        /// <summary>
        /// Gets or sets the ID used by the provider to uniquely identify the organization.
        /// </summary>
        [StringLength(128)]
        public string ProviderUid { get; set; }

        /// <summary>
        /// Gets or sets a JSON string containing any relevant configuration settings for the auth provider.
        /// </summary>        
        public string ConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the ID of the organization with which the authentication provider is associated.
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the organization with which the authentication provider is associated.
        /// </summary>
        public virtual OrganizationEntity Organization { get; set; }
    }
}
