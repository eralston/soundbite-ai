using Masticore;
using Masticore.Entity.Tests;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Entity.Tests
{
    public class SessionTests : ResourceTestBase<SessionEntity, SbDb, MockSbDbInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        private const string SessionName = "Rage in  the Cage";

        protected override void SetCreated(SessionEntity entity, SbDb db)
        {
            entity.Name = SessionName;
            entity.Limit = 180;
            entity.SessionType = SessionType.Meeting;
            entity.Reminder = Time.UtcNow;
            entity.Publish = Time.UtcNow;

            entity.Organization = db.Organizations.Find(1);
        }

        protected override void AssertCreated(SessionEntity entity)
        {
            Assert.Equal(SessionName, entity.Name);
            Assert.Equal(180, entity.Limit);
            Assert.NotNull(entity.Reminder);
            Assert.NotNull(entity.Publish);

            Assert.Equal(1, entity.OrganizationId);
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

        private const string SessionUpdatedName = "The Tussle with Muscles";

        protected override void SetUpdated(SessionEntity entity, SbDb db)
        {
            entity.Name = SessionUpdatedName;
        }

        protected override void AssertUpdated(SessionEntity originalEntity, SessionEntity updatedEntity)
        {
            Assert.Equal(SessionUpdatedName, updatedEntity.Name);
        }

        [Fact]
        public async Task SoftDelete()
        {
            await DoSoftDelete();
        }

        [Fact]
        public async Task Delete()
        {
            await CheckDeleteConstrained();
        }
    }
}
