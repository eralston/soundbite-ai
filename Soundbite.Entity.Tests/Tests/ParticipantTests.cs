using Masticore.Entity.Tests;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Entity.Tests
{
    public class ParticipantTests : ResourceTestBase<ParticipantEntity, SbDb, MockSbDbInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        private readonly ParticipantRole ParticipantRole = ParticipantRole.Audience;

        protected override void SetCreated(ParticipantEntity entity, SbDb db)
        {
            entity.ParticipantRole = ParticipantRole;

            entity.Person = db.People.Find(1);
            entity.Session = db.Sessions.Find(1);
        }

        protected override void AssertCreated(ParticipantEntity entity)
        {
            Assert.Equal(ParticipantRole, entity.ParticipantRole);

            Assert.Equal(1, entity.PersonId);
            Assert.Equal(1, entity.SessionId);
        }

        [Fact]
        public async Task ReadToArray()
        {
            await DoReadToArray();
        }

        [Fact]
        public async Task ReadFind()
        {
            await DoReadFind();
        }

        [Fact]
        public async Task Update()
        {
            await DoUpdate();
        }

        private readonly ParticipantRole UpdatedParticipantRole = ParticipantRole.Participant;

        protected override void SetUpdated(ParticipantEntity entity, SbDb db)
        {
            entity.ParticipantRole = UpdatedParticipantRole;
        }

        protected override void AssertUpdated(ParticipantEntity originalEntity, ParticipantEntity updatedEntity)
        {
            Assert.Equal(UpdatedParticipantRole, updatedEntity.ParticipantRole);
        }

        [Fact]
        public async Task SoftDelete()
        {
            await DoSoftDelete();
        }

        [Fact]
        public async Task Delete()
        {
            await DoDelete();
        }
    }
}
