namespace Masticore.Acs
{
    /// <summary>
    /// Settings for the Azure Communication Services
    /// </summary>
    public class AcsSettings
    {
        /// <summary>
        /// Gets or sets the connection string value for this ACS settings instance
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the default from email address for this ACS settings instance
        /// </summary>
        public string? FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the default from name for this ACS settings instance
        /// </summary>
        public string? FromName { get; set; }
    }
}
