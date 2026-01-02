namespace Soundbite.Models
{
    /// <summary>
    /// A model for capturing the Person and Role for a new Participant record connected to a session
    /// </summary>
    public class NewParticipant
    {
        /// <summary>
        /// The unique route value for the target Person
        /// </summary>
        public string PersonRoute { get; set; }

        /// <summary>
        /// The role for the new Participant record
        /// </summary>
        public ParticipantRole ParticipantRole { get; set; }
    }
}
