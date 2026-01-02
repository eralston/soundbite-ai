using Newtonsoft.Json;

namespace Masticore.DirectorySync
{
    public abstract class SyncStrategyConfigBase : ISyncStrategyConfig
    {
        #region Abstract Members

        /// <inheritdoc />
        public abstract string SyncType { get; }

        #endregion

        #region Properties

        /// <inheritdoc />
        [CodeGenField(IsNullable = true)]
        public CollectionSyncMode GroupsMode { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool ImportAllGroups => GroupsMode == CollectionSyncMode.All;

        /// <inheritdoc />
        [CodeGenField(IsNullable = true)]
        public bool? ImportPhoneNumbers { get; set; }

        /// <inheritdoc />
        [CodeGenField(IsNullable = true)]
        public GroupSyncConfig[] Groups { get; set; }

        /// <inheritdoc />
        [CodeGenField(IsNullable = true)]
        public CollectionSyncMode UsersMode { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool ImportAllUsers => UsersMode == CollectionSyncMode.All;

        /// <inheritdoc />
        [CodeGenField(IsNullable = true)]
        public UserSyncConfig[] Users { get; set; }

        #endregion
    }
}
