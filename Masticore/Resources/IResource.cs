namespace Masticore.Resources
{
    /// <summary>
    /// A general-purpose interface for an <see cref="IRecord"/> that addressable with an API-friendly secondary key
    /// </summary>
    [CodeGenModel(Name = "Resource")]
    public interface IResource : IRecord
    {
        /// <summary>
        /// Gets the route associated with the data item.  Routes are used like IDs to uniqely
        /// identify data items in the Soundbite platform.  This value is read-only and should
        /// not be updated.
        /// </summary>
        public string Route { get; set; }
    }
}