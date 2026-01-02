using Masticore.Entity;
using System.Linq;

namespace Soundbite.Entity.Tests
{
    /// <summary>
    /// Utility class for generating new mock data for an SbDb
    /// </summary>
    public static class MockData
    {
        /// <summary>
        /// Generates a participant record for the given person
        /// </summary>
        /// <param name="db"></param>
        /// <param name="person"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static ParticipantEntity MockParticipant(this SbDb db, PersonEntity person, SessionEntity session = null)
        {
            ParticipantEntity participant = db.Participants.CreateResource();
            participant.Person = person;
            participant.Session = session ?? db.Sessions.Where(s => s.Route == SbDbSeed.SessionRoute).Single();
            participant.CreatedById = 1;
            return participant;
        }

        /// <summary>
        /// Generates a participant groun record for the given group & session
        /// </summary>
        /// <param name="db"></param>
        /// <param name="grp"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static ParticipantGroupEntity MockParticipantGroup(this SbDb db, GroupEntity grp, SessionEntity session = null)
        {
            ParticipantGroupEntity participant = db.ParticipantGroups.CreateResource();
            participant.Group = grp;
            participant.Session = session ?? db.Sessions.Where(s => s.Route == SbDbSeed.SessionRoute).Single();
            participant.CreatedById = 1;
            participant.ParticipantRole = ParticipantRole.Participant;
            return participant;
        }
    }
}
