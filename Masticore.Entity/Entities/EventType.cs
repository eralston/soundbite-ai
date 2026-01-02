namespace Masticore.Entity
{
    /// <summary>
    /// The 4 types of operation one can perform on data, plus a safe default and special category
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Please classify all events, but this is here to be a safer default than create
        /// </summary>
        Unknown,

        Create,
        Read,
        Update,
        Delete,

        /// <summary>
        /// Please favor classification as a CRUD operator
        /// </summary>
        Other
    }
}