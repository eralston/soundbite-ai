using Masticore.Entity;
using System.Collections.Generic;

namespace Soundbite.Entity
{

    /// <summary>
    /// For when one must pull out the user by making it a sibling in a query rather than a child; required when root result is not an entity
    /// </summary>
    public class PersonWithUser
    {
        public PersonEntity Person { get; set; }
        public UserEntity User { get; set; }
    }

    /// <summary>
    /// For clustering <see cref="ParticipantGroup"/> with their people
    /// </summary>
    public class ParticipantGroupAndPeople
    {
        public ParticipantGroupEntity ParticipantGroup { get; set; }
        public IEnumerable<PersonWithUser> People { get; set; }
    }
}
