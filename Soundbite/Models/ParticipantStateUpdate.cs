namespace Soundbite.Models
{
    /// <summary>
    /// Container for data required to update participant state in a session.
    /// </summary>
    public class ParticipantStateUpdate
    {
        /// <summary>
        /// Gets or sets the state to apply to applicable participants.
        /// </summary>        
        public ParticipantState ParticipantState { get; set; }

        /// <summary>
        /// Gets or sets the role associated with the participants to update.  When this value is
        /// <see cref="ParticipantRole.Unknown"/> all participants are included in the update.
        /// </summary>        
        public ParticipantRole ParticipantRole { get; set; }
    }
}