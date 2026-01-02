namespace Masticore.DirectorySync
{
    /// <summary>
    /// Enumerations of the various synchronization operations in the sync tables.
    /// </summary>
    public enum SyncOpType
    {
        /// <summary>
        /// The sync operation will skip over this record, taking no action on it
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Denotes a synchronization option where item should be created if it does not exist and
        /// updated if it already exists.
        /// </summary>
        AddOrUpdate = 1,

        /// <summary>
        /// Denotes a synchronization option where item should be updated only if it already exists.
        /// </summary>
        UpdateOnly = 2,

        /// <summary>
        /// Denotes a synchronization option where the item should be removed if it exists.
        /// </summary>
        Remove = 3,

        /// <summary>
        /// Denotes that the item is one item in a complete set of active members for a group and
        /// a user/membership should be created if it does not already exist.
        /// </summary>
        MemberRegistration = 4,

        //TODO: Figure out if this following option is really feasible.  I think we can just nix it
        //  because the member sync option dictates whether the user goes in or not, and the user
        //  existing really dictates whether the membership can be created.  I think we just need to
        //  mark complete groups with MemberRegistration and call it a day, but it works right now
        //  and I don't want to change anything.

        /// <summary>
        /// Denotes that the item is one item in a complete set of active members for a group and
        /// user/membership should only be maintained if the user already exists.
        /// </summary>
        MemberRegistrationExistingUsersOnly = 5,
    }
}