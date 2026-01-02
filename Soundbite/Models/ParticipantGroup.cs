using Masticore.Models;

namespace Soundbite.Models
{
    /// <summary>
    /// A participant group links a group to a session.
    /// </summary>
    /// <seealso cref="Soundbite.Resources.IParticipantGroup" />
    public class ParticipantGroup : ResourceBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the particpants role in the session.
        /// </summary>
        public ParticipantRole ParticipantRole { get; set; }

        #endregion

        #region Relationship Properties

        /// <summary>
        /// Gets or sets the route of the session in which the user is participating.
        /// </summary>        
        public string SessionRoute { get; set; }

        /// <summary>
        /// Gets or sets the route of the group participating in the session.
        /// </summary>
        public string GroupRoute { get; set; }

        /// <summary>
        /// Gets or sets a reference to the group participating in the session.
        /// </summary>        
        public Group Group { get; set; }

        #endregion
    }
}
