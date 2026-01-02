using Masticore.Entity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Soundbite.Entity
{
    /// <summary>
    /// One instance of a comment by a user onto a session
    /// </summary>
    public class SessionCommentEntity : ResourceEntityBase
    {
        /// <summary>
        /// The content of the comment
        /// </summary>
        /// <remarks>Minimum 1 character, maximum 512</remarks>
        [Required]
        [StringLength(512, MinimumLength = 1)]
        [JsonProperty]
        public string Content { get; set; }

        /// <summary>
        /// To One on Session
        /// </summary>
        public int SessionId { get; set; }

        public virtual SessionEntity Session { get; set; }

        /// <summary>
        /// To One on Session
        /// </summary>
        public int PersonId { get; set; }

        public virtual PersonEntity Person { get; set; }
    }
}
