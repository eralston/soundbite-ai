namespace Masticore.Azure.MediaServices
{
    /// <summary>
    /// Interface defining the configuration settings required by the Azure Media Service.
    /// </summary>
    public interface IAzureMediaServiceConfig
    {
        /// <summary>
        /// Gets or sets the Azure Tenant ID
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// Gets or sets the ID of the subscription with which the media service is associated.
        /// </summary>
        string SubscriptionId { get; }

        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets the ID of the default storage account to use as the primary storage account for new Media services.
        /// </summary>
        string DefaultPrimaryStorageAccountId { get; }

        /// <summary>
        /// Gets the name of the location in which the default primary storage account exists.
        /// </summary>
        string DefaultPrimaryStorageAccountLocation { get; }

        /// <summary>
        /// Gets the expected issuer claim value of the media token. When the media service 
        /// authenticates the token sent from a user the issuer for the token must match
        /// this expected value. This value is only used when created new token policies
        /// so changing the value here does not automatically update existing policies.         
        /// </summary>
        string MediaTokenIssuer { get; }

        /// <summary>
        /// Gets the expected audience claim value of the media token. When the media service
        /// authenticates the token sent from a user the issuer for the token must match
        /// this expected value. This value is only used when created new token policies
        /// so changing the value here does not automatically update existing policies.         
        /// </summary>
        string MediaTokenAudience { get; }
    }
}