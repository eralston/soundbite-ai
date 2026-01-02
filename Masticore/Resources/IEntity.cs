namespace Masticore.Resources
{
    /// <summary>
    /// An interface describing an object that is persisted to the database
    /// </summary>
    [CodeGenModel(Ignore = true)]
    public interface IEntity : IRecord
    {
        /// <summary>
        /// All entities in Masticore use a sequential integer as their internal primary key
        /// </summary>
        /// <remarks>
        /// The implementing property should be annotated with a <see cref="System.ComponentModel.DataAnnotations.KeyAttribute"/>.
        /// Do NOT expose this identifier in the UI or API. Make a secondary key for that.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// Identifier for whoever created this Entity. Null indicates it was created by "The System" during some kind of background processing.
        /// </summary>
        public int? CreatedById { get; set; }
    }
}