using System.ComponentModel.DataAnnotations;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Defines the settings required for identifying a single user to synchronize.
    /// </summary>
    [CodeGenModel]
    public class UserSyncConfig
    {
        /// <summary>
        /// Gets or sets the ID of the user to import
        /// </summary>
        [Required]
        public string Id { get; set; }
    }
}
