namespace Soundbite.Models
{
    /// <summary>
    /// Model for the new Group joining a Session
    /// </summary>
    public class NewParticipantGroup
    {
        /// <summary>
        /// The unique route to a Group
        /// </summary>
        public string GroupRoute { get; set; }

        /// <summary>
        /// The role for the every member of the Group in the Session
        /// </summary>
        public ParticipantRole ParticipantRole { get; set; }
    }
}
