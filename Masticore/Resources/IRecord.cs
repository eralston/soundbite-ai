using System;

namespace Masticore.Resources
{
    /// <summary>
    /// Basic interface for an object that has a CRUD lifecycle w/ Soft Delete
    /// </summary>
    [CodeGenModel(Name = "Record")]
    public interface IRecord
    {
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