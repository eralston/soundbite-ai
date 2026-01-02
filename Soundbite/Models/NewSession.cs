using Masticore;
using System.Collections.Generic;

namespace Soundbite.Models
{
    /// <summary>
    /// Fields of a Session, plus additional properties to help create and update related records
    /// </summary>
    public class NewSession : Session
    {
        /// <summary>
        /// The recurrence pattern of the associated Series for this Session
        /// </summary>
        public Recurrence Recurrence { get; set; }

        /// <summary>
        /// The ad hoc data for the recurrence that guides customization.
        /// For most patterns this is empty.
        /// </summary>
        public string RecurrenceData { get; set; }

        // For ISession

        /// <summary>
        /// The value of the first Prompt for this session.
        /// </summary>
        public string FirstPrompt { get; set; }

        /// <summary>
        /// A simplified list of the participating Groups (ParticipntGroup records) planned for this Session
        /// </summary>
        public IEnumerable<NewParticipantGroup> Groups { get; set; }

        /// <summary>
        /// A simplified list of the participating People (Participant record) planned for this Session
        /// </summary>
        public IEnumerable<NewParticipant> Participants { get; set; }

    }
}