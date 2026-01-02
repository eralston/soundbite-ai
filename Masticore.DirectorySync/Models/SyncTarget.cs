namespace Masticore.DirectorySync
{
    /// <summary>
    /// Provides the most basic information for a single class of entity that can be synced
    /// Useful in basic UIs, etc
    /// </summary>
    [CodeGenModel]
    public class SyncTarget
    {
        #region Constructor

        public SyncTarget() { }
        public SyncTarget(string id, string name)
        {
            Id = id;
            Name = name;
        }

        #endregion

        #region Properties
        public string Id { get; set; }

        [CodeGenField(IsNullable = true)]
        public string Name { get; set; }
        #endregion
    }
}
