using Masticore.Resources;
using System;

namespace Masticore.Models
{
    /// <summary>
    /// Base class containing properties shared by resources in the Soundbite platform.
    /// </summary>
    /// <seealso cref="Masticore.Resources.IResource" />
    public abstract class ResourceBase : IResource
    {
        /// <summary>
        /// Gets the route associated with the data item.  Routes are used like IDs to uniqely
        /// identify data items in the Soundbite platform.  This value is read-only and should
        /// not be updated.
        /// </summary>        
        public string Route { get; set; }

        /// <summary>
        /// Gets the UTC based date/time when the data item was created.  Any modifications to this
        /// value are discarded by the application.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Gets the UTC based date/time when the data item was last updated.  Any modifications to 
        /// this value are discarded by the application.
        /// </summary>        
        public DateTime UpdatedUtc { get; set; }

        /// <summary>
        /// Date the object was soft deleted; otherwise, null
        /// </summary>
        public DateTime? DeletedUtc { get; set; }
    }
}