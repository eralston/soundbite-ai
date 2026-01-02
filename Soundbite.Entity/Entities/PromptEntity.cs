using Masticore.Entity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Soundbite.Entity
{
    /// <summary>
    /// Prompt with a set of Clips
    /// </summary>
    public class PromptEntity : ResourceEntityBase
    {
        // IPromptFields

        [Required]
        [StringLength(2048)]
        [JsonProperty]
        public string Text { get; set; }

        // Ordinal?

        // Relationships

        /// <summary>
        /// To on on Session
        /// </summary>
        public int SessionId { get; set; }
        public virtual SessionEntity Session { get; set; }

        /// <summary>
        /// To many on Clips
        /// </summary>
        public ICollection<ClipEntity> Clips { get; set; }
    }
}
