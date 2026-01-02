using Newtonsoft.Json;

namespace Masticore.DirectorySync.Interact
{
    /// <summary>
    /// Defines the organization-level configuration settings for the Interact directory provider.    
    /// </summary>
    [CodeGenModel]
    public class InteractOrgConfig : SyncStrategyConfigBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the API URL associated with the organization.  Different organizations can
        /// be on different API servers.        
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the account.  This value must be sent along
        /// with API requests via the X-Tenant header.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the username used to connect to the API.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password used to connect to the API.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the organization is larger than 10,000 users.
        /// By default, Interact is only capable of paging through 10,000 results so large 
        /// organizations must be queried differently than smaller organizations.
        /// </summary>
        public bool IsLargeOrg { get; set; }

        #endregion

        #region Overrides

        /// <inheritdoc />
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public override string SyncType => InteractOrgSyncStrategyBase.Name;

        #endregion
    }
}
