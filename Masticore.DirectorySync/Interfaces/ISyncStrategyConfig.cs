using Newtonsoft.Json;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Defines the contract requires for a configuration class associated with a directory synchronization strategy.
    /// </summary>
    [CodeGenModel(Ignore = true)]
    public interface ISyncStrategyConfig
    {
        /// <summary>
        /// Gets a string identifying the "key" associated with the synchronization
        /// strategy for which this configuration exists.
        /// </summary>
        [JsonIgnore]
        string SyncType { get; }

        /// <summary>
        /// Gets or sets the <see cref="CollectionSyncMode"/> mode for groups
        /// </summary>
        CollectionSyncMode GroupsMode { get; set; }

        /// <summary>
        /// Get or sets a flag indicating if we want to import all groups
        /// </summary>
        bool ImportAllGroups { get; }

        /// <summary>
        /// Gets or sets a flag indicating if the phone number of individuals should be imported during sync
        /// </summary>
        bool? ImportPhoneNumbers { get; }

        /// <summary>
        /// Gets or sets the list of specific groups to synchronize.
        /// </summary>
        GroupSyncConfig[] Groups { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CollectionSyncMode"/> mode for users
        /// </summary>
        CollectionSyncMode UsersMode { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether to imported all users with user delta calls.
        /// </summary>
        bool ImportAllUsers { get; }

        /// <summary>
        /// Gets or sets the list of specific users to synchronize.
        /// </summary>
        UserSyncConfig[] Users { get; set; }
    }
}