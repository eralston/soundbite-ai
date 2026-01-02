using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class GroupTests : ResourceTestBase<GroupEntity, MockIdentityDb, MockSeedInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        protected override void SetCreated(GroupEntity entity, MockIdentityDb db)
        {
            entity.Name = "Test Team";
            entity.Description = "Test Team";

            entity.Organization = db.Organizations.Find(1);
        }

        protected override void AssertCreated(GroupEntity entity)
        {
            Assert.NotNull(entity.Name);
            Assert.NotNull(entity.Description);
            Assert.NotNull(entity.Route);

            Assert.NotNull(entity.Organization);
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

        //PersonRole updatedRole = PersonRole.Person;
        private readonly string UpdatedName = "Hello";

        protected override void SetUpdated(GroupEntity entity, MockIdentityDb db)
        {
            entity.Name = UpdatedName;
        }

        protected override void AssertUpdated(GroupEntity originalEntity, GroupEntity updatedEntity)
        {
            Assert.Equal(UpdatedName, updatedEntity.Name);
        }

        [Fact]
        public async Task Archive()
        {
            await DoSoftDelete();
        }

        [Fact]
        public async Task Delete()
        {
            // There should be no constraints on this record, so it can truly be deleted
            await DoDelete();
        }
    }
}
