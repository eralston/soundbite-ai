using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Defines the settings required for identifying a single group to synchronize.
    /// </summary>
    [CodeGenModel]
    public class GroupSyncConfig
    {
        /// <summary>
        /// Gets or sets the unique ID of the group in the external directory.
        /// </summary>
        [Required]
        public string GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group as it should appear in Soundbite.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string RenameTo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how to handle member synchronization for the group.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MemberSyncOpType MemberSyncType { get; set; }
    }
}
