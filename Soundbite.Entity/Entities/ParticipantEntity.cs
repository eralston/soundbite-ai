using Masticore.Entity;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Soundbite.Entity
{
    /// <summary>
    /// Linking a Person to a Session
    /// </summary>
    public class ParticipantEntity : ResourceEntityBase
    {
        /// <summary>
        /// Gets or sets whether this participant is a direct participant or if they were added as a member of a group
        /// </summary>
        [JsonProperty]
        public bool IsDirectParticipant { get; set; }

        /// <summary>
        /// Gets or sets the role for this record, which determines a combination of permissions and potentially behavior in the session workflow
        /// </summary>
        [JsonProperty]
        public ParticipantRole ParticipantRole { get; set; }

        /// <summary>
        /// Gets or sets the state of the participant as it moves through the session workflow
        /// </summary>
        [JsonProperty]
        public ParticipantState ParticipantState { get; set; }

        // Relationships

        /// <summary>
        /// To one on Person
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the person record associated with this participant, the ID of which is reflected in <see cref="PersonId"/>
        /// </summary>
        public virtual PersonEntity Person { get; set; }

        /// <summary>
        /// To one on Session
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Gets or sets the session record associated with this participant, the ID of which is reflected in <see cref="SessionId"/>
        /// </summary>
        public virtual SessionEntity Session { get; set; }

        /// <summary>
        /// One to many relationship linking a participant to a participant group.
        /// </summary>
        public virtual ICollection<ParticipantGroupMemberEntity> ParticipantGroupMembers { get; set; }

        /// <summary>
        /// Gets or sets the reaction of the user to the session
        /// </summary>
        [JsonProperty]
        public ParticipantReactionType ReactionType { get; set; }
    }
}
