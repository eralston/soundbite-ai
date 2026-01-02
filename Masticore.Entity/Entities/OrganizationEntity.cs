using Masticore.Models;
using Masticore.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masticore.Entity
{
    /// <summary>
    /// Provides data on an organization
    /// </summary>
    public class OrganizationEntity : ResourceEntityBase, IUniversal, IOrgSyncFields
    {
        // IOrganizationFields

        [StringLength(128, MinimumLength = 3)]
        [JsonProperty]
        public string Name { get; set; }

        [StringLength(512)]
        [JsonProperty]
        public string Description { get; set; }

        /// <summary>
        /// Property for tracking a cross-system unique identifer
        /// This should be big enough to track:
        /// - User IDs in AAD
        /// - Tenant IDs in AAD
        /// - Directory IDs in Okta
        /// - User IDs in Okta
        /// - Container names in Azure Storage
        /// - Partition and row keys in Azure Storage tables
        /// - Docuemnt ID in CosmosDB
        /// - etc
        /// It's based on a GUID w/ dashes (AAD IDs format)
        /// </summary>
        [Display(Name = "Universal ID")]
        [StringLength(36)]
        [Required]
        public string UniversalId { get; set; }

        // IOrgSyncFields

        /// <summary>
        /// Gets or sets a string identifying the provider used for directory sync operations.
        /// </summary>
        [MaxLength(25)]
        public string SyncType { get; set; }

        /// <summary>
        /// Gets or sets the JSON string containing serialized directory sync settings.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Column(TypeName = "ntext")]
        public string SyncConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the general-purpose configuration JSON for the org; this allows for an arbitrary JSON object for metadata on the org
        /// </summary>
        /// <remarks>
        /// This can be used for anything specific to the application, but a good start is notifications and permissions
        /// </remarks>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Column(TypeName = "ntext")]
        public string ConfigJson { get; set; }

        // Relationships

        // To-One
        public int TenantId { get; set; }
        public virtual TenantEntity Tenant { get; set; }

        /// <summary>
        /// To many on people
        /// </summary>
        public ICollection<PersonEntity> People { get; set; }

        /// <summary>
        /// To many on Team
        /// </summary>
        public ICollection<GroupEntity> Groups { get; set; }

        /// <summary>
        /// Collection of authentication providers the organization supports.
        /// </summary>
        public ICollection<OrganizationAuthProviderEntity> AuthProviders { get; set; }

        /// <summary>
        /// Collection of token settings associated with the tenant.  While there can be many
        /// token settings there should only be 1 active setting at a time.
        /// </summary>
        public ICollection<TokenSettingsEntity> TokenSettings { get; set; }
    }

    /// <summary>
    /// Extension methods for <see cref="OrganizationEntity"/>
    /// </summary>
    public static class OrganizationEntityExtensions
    {
        /// <summary>
        /// Gets the <see cref="OrgSyncConfig"/> object from the <see cref="OrgSyncConfig.SyncConfigJson"/> field
        /// and checking the <see cref="OrgSyncConfig.SyncType"/> field has a value; otherwise, it returns null
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public static OrgSyncConfig GetOrgSyncConfig(this OrganizationEntity org)
        {
            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (org.SyncType == null)
            {
                return null;
            }

            return new OrgSyncConfig
            {
                OrgRoute = org.Route,
                SyncType = org.SyncType,
                SyncConfigJson = org.SyncConfigJson
            };
        }
    }
}
