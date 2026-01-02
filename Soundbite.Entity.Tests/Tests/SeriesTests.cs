using Masticore;
using Masticore.Entity.Tests;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Entity.Tests
{
    public class SeriesTests : ResourceTestBase<SeriesEntity, SbDb, MockSbDbInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        private const string SeriesName = "Rage in  the Cage";
        private const Recurrence SeriesRecurrence = Recurrence.Weekday;
        private const string RecurrenceData = "12345";

        protected override void SetCreated(SeriesEntity entity, SbDb db)
        {
            entity.Name = SeriesName;
            entity.Recurrence = SeriesRecurrence;
            entity.RecurrenceData = RecurrenceData;

            entity.Organization = db.Organizations.Find(1);
            entity.Template = db.Sessions.Find(1);
        }

        protected override void AssertCreated(SeriesEntity entity)
        {
            Assert.Equal(SeriesName, entity.Name);
            Assert.Equal(SeriesRecurrence, entity.Recurrence);
            Assert.Equal(RecurrenceData, entity.RecurrenceData);

            Assert.Equal(1, entity.TemplateId);
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

        private const string UpdateSeriesName = "The Tussle with Muscles";
        private const Recurrence UpdatedSeriesRecurrence = Recurrence.Weekly;
        private const string UpdatedRecurrenceData = "7890";

        protected override void SetUpdated(SeriesEntity entity, SbDb db)
        {
            entity.Name = UpdateSeriesName;
            entity.Recurrence = UpdatedSeriesRecurrence;
            entity.RecurrenceData = UpdatedRecurrenceData;
        }

        protected override void AssertUpdated(SeriesEntity originalEntity, SeriesEntity updatedEntity)
        {
            Assert.Equal(UpdateSeriesName, updatedEntity.Name);
            Assert.Equal(UpdatedSeriesRecurrence, updatedEntity.Recurrence);
            Assert.Equal(UpdatedRecurrenceData, updatedEntity.RecurrenceData);
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
