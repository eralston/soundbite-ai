namespace Soundbite.Entity
{
    /// <summary>
    /// Linking a Person to a Session
    /// </summary>
    public class ParticipantGroupMemberEntity
    {
        #region Properties

        public int ParticipantId { get; set; }

        public int ParticipantGroupId { get; set; }

        #endregion

        #region Relationships

        public virtual ParticipantEntity Participant { get; set; }

        public virtual ParticipantGroupEntity ParticipantGroup { get; set; }

        #endregion
    }
}