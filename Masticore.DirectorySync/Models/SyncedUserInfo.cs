namespace Masticore.DirectorySync
{
    /// <summary>
    /// Stores information about a synchronized user.
    /// </summary>
    [CodeGenModel]
    public class SyncedUserInfo
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SyncedUserInfo"/> instance.
        /// </summary>
        /// <param name="id">Unique ID of the user in the third-party directory.</param>
        /// <param name="opType">Synchronization operation type performed on the user.</param>
        public SyncedUserInfo(string id, SyncOpType opType)
        {
            Id = id;
            OpType = opType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the unique ID of the user in the third-party directory.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the synchronization operation type performed on the user.
        /// </summary>
        public SyncOpType OpType { get; set; }

        #endregion
    }
}
