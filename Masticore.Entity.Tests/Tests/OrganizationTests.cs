using Masticore.Resources;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class OrganizationTests : ResourceTestBase<OrganizationEntity, MockIdentityDb, MockSeedInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        protected override void SetCreated(OrganizationEntity entity, MockIdentityDb db)
        {
            entity.Name = "Fabrikam";
            entity.Description = "Most folks are about as happy as they make their minds up to be - Abraham Lincoln";
            entity.NewUniversalId();

            entity.Tenant = db.Tenants.Find(1);
        }

        protected override void AssertCreated(OrganizationEntity entity)
        {
            Assert.NotNull(entity.Name);
            Assert.NotNull(entity.Description);
            Assert.NotNull(entity.Route);

            Assert.NotNull(entity.Tenant);
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

        private readonly string updatedName = "Contoso";

        protected override void SetUpdated(OrganizationEntity entity, MockIdentityDb db)
        {
            entity.Name = updatedName;
        }

        protected override void AssertUpdated(OrganizationEntity originalEntity, OrganizationEntity updatedEntity)
        {
            Assert.Equal(updatedName, updatedEntity.Name);
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
