using Newtonsoft.Json;

namespace Masticore.DirectorySync.Aad
{
    /// <summary>
    /// Defines the settings required for configuration of the AAD directory sync provider.
    /// This is serialized and deserialized out of the database, so do NOT set any values on here you do not want persisted
    /// </summary>
    [CodeGenModel]
    public class AadOrgConfig : SyncStrategyConfigBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the tenant ID of the organization
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the application ID associated with the app registered in azure that can 
        /// access directory information.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the secret key associated with the <see cref="AppId"/>.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the delta link for users returned from Microsoft graph.  The delta link 
        /// indicates that the full set of changes has been processed and the "next" set of changes
        /// can be acquired using the link.
        /// </summary>
        [CodeGenField(Ignore = true)]
        public string DeltaLinkUsers { get; set; }

        /// <summary>
        /// Gets or sets the delta link for groups returned from Microsoft graph.  The delta link 
        /// indicates that the full set of changes has been processed and the "next" set of changes
        /// can be acquired using the link.
        /// </summary>
        [CodeGenField(Ignore = true)]
        public string DeltaLinkGroups { get; set; }

        /// <summary>
        /// Gets or sets the next link for users returned from Microsoft graph.  The next link 
        /// indicates there are additional "pages" of data that require processing and the "next" 
        /// set of changes can be acquired using the link.
        /// </summary>
        [CodeGenField(Ignore = true)]
        public string NextLinkUsers { get; set; }

        /// <summary>
        /// Gets or sets the next link for groups returned from Microsoft graph.  The next link 
        /// indicates there are additional "pages" of data that require processing and the "next" 
        /// set of changes can be acquired using the link.
        /// </summary>
        [CodeGenField(Ignore = true)]
        public string NextLinkGroups { get; set; }

        #endregion

        #region Overrides

        /// <inheritdoc />
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public override string SyncType => AadOrgSyncStrategyBase.Name;

        #endregion
    }
}
