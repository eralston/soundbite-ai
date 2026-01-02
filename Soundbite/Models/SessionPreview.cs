using System.Collections.Generic;

namespace Soundbite.Models
{
    /// <summary>
    /// <see cref="Session"/> augmented to appear in Feeds
    /// </summary>
    public class SessionPreview : Session
    {
        /// <summary>
        /// Gets or sets a list of individuals who have been explicitly added as participants in the
        /// session. If a user is included in a group that is participating in the session, then
        /// there is no need to add them to this list.
        /// </summary>
        public IEnumerable<Participant> MyParticipants { get; set; }

        /// <summary>
        /// Gets or sets the people who created the SB; usually just one record
        /// </summary>
        public IEnumerable<Participant> HostParticipants { get; set; }
    }
}