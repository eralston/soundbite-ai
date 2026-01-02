namespace Masticore.Entity
{
    /// <summary>
    /// An abstract base class that attaches to another <see cref="EntityBase"/> type, allowing us to accumulate events around CRUD operations and other unique events
    /// These events are intended to be used in aggregate, so they are not "resources" that are intended to be individual and with a unique lifecycle.
    /// </summary>
    /// <remarks>Inheritors of this class should use a 
    /// <see href="https://docs.microsoft.com/en-us/ef/core/modeling/inheritance#table-per-type-configuration">"Table-Per-Type" configuration</see>
    /// when setting up the <see cref="DbContext"/>
    /// </remarks>
    /// <typeparam name="TTarget"></typeparam>
    public abstract class EventEntityBase<TTarget> : EntityBase
        where TTarget : EntityBase
    {
        /// <summary>
        /// Gets or sets the optional classification of the data operation for this event
        /// </summary>
        public EventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the target Entity for this event
        /// </summary>
        public int TargetId { get; set; }

        /// <summary>
        /// Target <see cref="EntityBase"/> object for this event
        /// </summary>
        public virtual TTarget Target { get; set; }

        // Add additional metadata fields in concrete children
    }
}