namespace Masticore.Models
{
    /// <summary>
    /// Defines an object which persists
    /// </summary>
    [CodeGenModel(Name = "OrgSyncFields")]
    public interface IOrgSyncFields
    {
        /// <summary>
        /// Gets or sets the JSON for the underlying provider configuration object.
        /// EG, the tenant ID, app ID, and/or app secret for AAD
        /// </summary>
        [CodeGenField(IsNullable = true)]
        string SyncConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the provider type (EG, AAD or Okta)
        /// </summary>
        [CodeGenField(IsNullable = true)]
        string SyncType { get; set; }
    }
}