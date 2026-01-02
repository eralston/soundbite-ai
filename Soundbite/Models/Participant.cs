using Masticore.Models;

namespace Soundbite.Models
{
    /// <summary>
    /// A participant links an individual person to a session.
    /// </summary>
    public class Participant : ResourceBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the particpants role in the session.
        /// </summary>
        public ParticipantRole ParticipantRole { get; set; }

        /// <summary>
        /// Gets or sets the current state of the participant for the session.
        /// </summary>
        public ParticipantState ParticipantState { get; set; }

        /// <summary>
        /// Gets or sets the reaction of the user to the session
        /// </summary>
        public ParticipantReactionType ReactionType { get; set; }

        #endregion

        #region Relationship Properties

        /// <summary>
        /// Gets or sets the route of the session in which the user is participating.
        /// </summary>        
        public string SessionRoute { get; set; }

        /// <summary>
        /// Gets or sets a reference to the user that is participating in the session.
        /// </summary>        
        public Person Person { get; set; }

        #endregion
    }
}
