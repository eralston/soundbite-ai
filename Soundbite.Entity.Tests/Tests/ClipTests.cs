using Masticore;
using Masticore.Entity.Tests;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Entity.Tests
{
    public class ClipTests : ResourceTestBase<ClipEntity, SbDb, MockSbDbInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        protected override void SetCreated(ClipEntity entity, SbDb db)
        {
            entity.ClipType = ClipType.Comment;
            entity.FileType = FileType.Mp3;
        }

        protected override void AssertCreated(ClipEntity entity)
        {
            Assert.Equal(ClipType.Comment, entity.ClipType);
            Assert.Equal(FileType.Mp3, entity.FileType);
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

        private readonly ClipType updatedClipType = ClipType.Prompt;

        protected override void SetUpdated(ClipEntity entity, SbDb db)
        {
            entity.ClipType = updatedClipType;
        }

        protected override void AssertUpdated(ClipEntity originalEntity, ClipEntity updatedEntity)
        {
            Assert.Equal(updatedClipType, updatedEntity.ClipType);
        }

        [Fact]
        public async Task SoftDelete()
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
