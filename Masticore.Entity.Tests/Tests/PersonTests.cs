using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class PersonTests : ResourceTestBase<PersonEntity, MockIdentityDb, MockSeedInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        protected override void SetCreated(PersonEntity entity, MockIdentityDb db)
        {
            entity.PersonRole = PersonRole.Admin;

            entity.Organization = MockData.MockOrg(db);
            entity.User = MockData.MockUser(db, new MockClaims());
        }

        protected override void AssertCreated(PersonEntity entity)
        {
            Assert.Equal(PersonRole.Admin, entity.PersonRole);
            Assert.NotNull(entity.User);
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

        private readonly PersonRole updatedRole = PersonRole.Person;

        protected override void SetUpdated(PersonEntity entity, MockIdentityDb db)
        {
            entity.PersonRole = updatedRole;
        }

        protected override void AssertUpdated(PersonEntity originalEntity, PersonEntity updatedEntity)
        {
            Assert.Equal(updatedRole, updatedEntity.PersonRole);
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
