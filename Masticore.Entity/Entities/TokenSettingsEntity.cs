namespace Masticore.Entity
{
    /// <summary>
    /// Class for storing token settings for tenants and organizations.
    /// </summary>
    public class TokenSettingsEntity : ResourceEntityBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the ID of the tenant with which the settings are associated.
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the oragnization wich which the settings are assocaited.
        /// </summary>
        public int? OrganizationId { get; set; }

        #endregion

        #region ITokenServiceSettings Implementation

        /// <summary>
        /// Gets or sets a JSON string containing the serialized settings for the token configuration.
        /// </summary>
        public string Config { get; set; }

        #endregion

        #region Relational Properties

        /// <summary>
        /// Gets or sets the tenant with which the settings are associated.
        /// </summary>
        public TenantEntity Tenant { get; set; }

        /// <summary>
        /// Gets or sets the oragnization wich which the settings are assocaited.
        /// </summary>
        public OrganizationEntity Organization { get; set; }

        #endregion

    }
}