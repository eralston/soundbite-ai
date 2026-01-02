using Masticore.Resources;

namespace Masticore.Models
{
    /// <summary>
    /// Implements the minimum viable fields for a Group
    /// </summary>
    [CodeGenModel(Name = "GroupFields")]
    public interface IGroupFields : IUniversal
    {
        /// <summary>
        /// Gets or sets the name for a group
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description for a group
        /// </summary>
        string Description { get; set; }
    }
}