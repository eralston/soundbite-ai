namespace Masticore.Models
{
    /// <summary>
    /// Represents directory synchronization configuration settings for an organization.
    /// </summary>
    public class OrgSyncConfig : IOrgSyncFields
    {
        #region Properties

        /// <summary>
        /// Gets or sets the route of the organization associated with the directory sync settings
        /// WARNING: When loading and unloading from the database, be sure to overwrite this value with the true current org as it is not check for RBAC during sync
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string OrgRoute { get; set; }

        /// <summary>
        /// Gets or sets a string identifying the type of parent object needing configuration
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string SyncType { get; set; }

        /// <summary>
        /// Gets or sets the JSON string containing serialized directory sync settings.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string SyncConfigJson { get; set; }

        #endregion
    }
}