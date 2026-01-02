using Masticore.Entity.Tests;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Entity.Tests
{
    public class PromptTests : ResourceTestBase<PromptEntity, SbDb, MockSbDbInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        private const string PromptText = "Rage in  the Cage";

        protected override void SetCreated(PromptEntity entity, SbDb db)
        {
            entity.Text = PromptText;

            entity.Session = db.Sessions.Find(1);
        }

        protected override void AssertCreated(PromptEntity entity)
        {
            Assert.Equal(PromptText, entity.Text);

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

        private const string PromptUpdatedText = "The Tussle with Muscles";

        protected override void SetUpdated(PromptEntity entity, SbDb db)
        {
            entity.Text = PromptUpdatedText;
        }

        protected override void AssertUpdated(PromptEntity originalEntity, PromptEntity updatedEntity)
        {
            Assert.Equal(PromptUpdatedText, updatedEntity.Text);
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
