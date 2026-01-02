using System.Collections.Generic;

namespace Soundbite.Models
{
    /// <summary>
    /// A summary of the reaction counts across the participants of a session
    /// </summary>
    public class ReactionSummary
    {
        /// <summary>
        /// The reaction type that was counted by this record
        /// </summary>
        public ParticipantReactionType ReactionType { get; set; }

        /// <summary>
        /// Sum of all 
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// <see cref="Session"/> augmented with fully detailed fields and collections
    /// </summary>
    public class SessionDetails : Session
    {
        /// <summary>
        /// Gets or sets a list of prompts in the session.
        /// </summary>
        public IEnumerable<PromptWithClips> Prompts { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Participant"/> records for this session
        /// </summary>
        public IEnumerable<Participant> Participants { get; set; }

        /// <summary>
        /// Gets or sets a list of <see cref="Participant"/> records related to the current user.
        /// </summary>
        public IEnumerable<Participant> MyParticipation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParticipantGroup"/> records for this session
        /// </summary>
        public IEnumerable<ParticipantGroup> Groups { get; set; }

        /// <summary>
        /// Gets or sets the related set of ReactionSummaries; one record for each participant who has been associated with the session via direct or group participation, if only with a reaction none
        /// </summary>
        public IEnumerable<ReactionSummary> Reactions { get; set; }

        /// <summary>
        /// Gets or sets a reference to the series associated with a recurring session
        /// </summary>
        /// <remarks>
        /// When this value is <c>null</c> the session is not recurring.
        /// </remarks>
        public SeriesPreview Series { get; set; }
    }
}