using Masticore.Entity;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Soundbite.Entity
{
    /// <summary>
    /// Linking a group to a session
    /// </summary>
    public class ParticipantGroupEntity : ResourceEntityBase
    {
        // IParticipantGroupFields

        [JsonProperty]
        public ParticipantRole ParticipantRole { get; set; }

        // Relationships

        /// <summary>
        /// To one on Group
        /// </summary>
        public int GroupId { get; set; }
        public virtual GroupEntity Group { get; set; }

        /// <summary>
        /// To one on Session
        /// </summary>
        public int SessionId { get; set; }
        public virtual SessionEntity Session { get; set; }

        public virtual ICollection<ParticipantGroupMemberEntity> ParticipantMembers { get; set; }
    }
}
