using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Jobs;
using Masticore.Queue;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Queue.Tests
{
    public class SbTestJob : SbJobBase
    {
        public bool DidProcess { get; set; } = false;

        public string NewGivenName { get; set; }

        public override async Task Process()
        {
            // ASSERT
            Assert.NotNull(Id);
            Assert.NotNull(NewGivenName);
            Assert.False(DidProcess);

            // PROCESS
            Logger.LogDebug($"Processing {nameof(SbTestJob)}...");

            SbDb db = await Infrastructure.DbAsync();
            UserEntity user = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();
            user.GivenName = NewGivenName;
            await db.SaveChangesAsync();

            DidProcess = true;

            Logger.LogDebug($"Processing {nameof(SbTestJob)} Completed");
        }
    }

    public class AzQueueTests : EntityTestBase<MockSbDbInfrastructure, SbDb>
    {
        /// <summary>
        /// This is here just to match Masticore.Storage.Tests and make it possible to turn-key unit test w/o figuring out Azurite in the pipeline or requiring devs to install it locally
        /// </summary>
        /// <remarks>
        /// TODO Swap for local Azurite;
        /// In theory two devs running unit tests at the same time could trample each other since this is global;
        /// One may also get old messages in the queue in event of failure, so consider opening up the Azure console and clearing the relevant queue from time-to-time if you're having trouble
        /// </remarks>
        private const string ConnectionString = "[SB STORAGE CONNECTION STRING]";

        [Fact]
        public async Task QueueThenProcess()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;
            UserEntity user = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();
            const string newGivenName = "Spock";
            Assert.NotSame(newGivenName, user.GivenName);
            SbTestJob job = new SbTestJob
            {
                NewGivenName = "Spock"
            };
            SbJobWorker<SbTestJob> worker = new SbJobWorker<SbTestJob>(Builder.Infrastructure.Value, Builder.Logger<SbJobWorker<SbTestJob>>());
            AzQueue queue = new AzQueue(ConnectionString);

            // ACT
            string jobId = await queue.Add(job);
            Assert.NotNull(jobId);
            Assert.NotNull(job.Id);

            // WARNING: Until these tests are running local-only, this may be processing an old job from the queue
            // Consider clearing the queue from the Azure console manually from time-to-time
            IJob processedJob = await queue.ProcessNext(worker, job.Type);

            // ASSERT
            Assert.NotNull(processedJob);
            SbTestJob processedTestJob = processedJob as SbTestJob;
            Assert.NotNull(processedTestJob);
            Assert.NotNull(processedJob.Id);
            Assert.True(processedTestJob.DidProcess);
            UserEntity userAfterJob = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();
            Assert.Equal(newGivenName, userAfterJob.GivenName);
        }
    }
}
