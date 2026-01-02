namespace Masticore.Azure.MediaServices
{
    /// <summary>
    /// Interface defining the configuration settings required by the Azure Media Service.
    /// </summary>
    public class AzureMediaServiceConfig : IAzureMediaServiceConfig
    {
        /// <inheritdoc />
        public string TenantId { get; set; }

        /// <inheritdoc />
        public string SubscriptionId { get; set; }

        /// <inheritdoc />
        public string ResourceGroupName { get; set; }

        /// <inheritdoc />
        public string DefaultPrimaryStorageAccountId { get; set; }

        /// <inheritdoc />        
        public string DefaultPrimaryStorageAccountLocation { get; set; }

        /// <inheritdoc />
        public string MediaTokenIssuer { get; set; }

        /// <inheritdoc />
        public string MediaTokenAudience { get; set; }
    }
}