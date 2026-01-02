namespace Masticore.DirectorySync
{
    /// <summary>
    /// An implementation of <see cref="ISyncStrategyConfig"/> that can be used to read/write a config for any purpose
    /// </summary>
    public class SyncStrategyConfigReader : ISyncStrategyConfig
    {
        /// <inheritdoc />
        public string SyncType { get; set; }

        /// <inheritdoc />
        public CollectionSyncMode GroupsMode { get; set; }

        /// <inheritdoc />
        public bool ImportAllGroups { get; set; }

        /// <inheritdoc />
        public bool? ImportPhoneNumbers { get; set; }

        /// <inheritdoc />
        public GroupSyncConfig[] Groups { get; set; }

        /// <inheritdoc />
        public CollectionSyncMode UsersMode { get; set; }

        /// <inheritdoc />
        public bool ImportAllUsers { get; set; }

        /// <inheritdoc />
        public UserSyncConfig[] Users { get; set; }
    }
}
