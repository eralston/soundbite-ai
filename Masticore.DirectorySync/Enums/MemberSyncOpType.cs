namespace Masticore.DirectorySync
{
    /// <summary>
    /// Enumeration of the various group synchronization options.
    /// </summary>
    public enum MemberSyncOpType
    {
        /// <summary>
        /// Denotes that no members should be synchronized.
        /// </summary>
        SyncNoMembers = 0,

        /// <summary>
        /// Denotes that all members should be synchronized and users that do not exist should be created.
        /// </summary>
        SyncAllMembers = 1,

        /// <summary>
        /// Denotes that only existing members should be synchronized.
        /// </summary>
        SyncExistingMembers = 2
    }
}