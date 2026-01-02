using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// The original unit test for an entity
    /// TODO: Convert to child of EntityTestBase
    /// </summary>
    public class TenantTests : ResourceTestBase<TenantEntity, MockIdentityDb, MockSeedInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        protected override void SetCreated(TenantEntity entity, MockIdentityDb db)
        {
            entity.Name = "fabrikam";
            entity.CreatedById = 1;
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

        private readonly string updatedName = "Tested";

        protected override void SetUpdated(TenantEntity entity, MockIdentityDb db)
        {
            entity.Name = updatedName;
        }

        protected override void AssertUpdated(TenantEntity originalEntity, TenantEntity updatedEntity)
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
