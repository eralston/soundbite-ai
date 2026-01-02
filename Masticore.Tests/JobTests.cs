using Masticore.Jobs;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Tests
{
    /// <summary>
    /// <see cref="IJob"/> that simply confirms it ran
    /// </summary>
    public class UnitTestJob : NullJob
    {
        public bool DidProcess { get; protected set; } = false;

        public override Task Process()
        {
            DidProcess = true;
            return base.Process();
        }
    }

    public class JobTest : TestBase
    {
        [Fact]
        public async void ImmediateQueue()
        {
            IJobQueue queue = new ImmediateQueue();
            UnitTestJob job = new UnitTestJob();
            Assert.Null(job.Id);
            await queue.Add(job);
            Assert.True(job.DidProcess);
            Assert.NotNull(job.Id);
        }
    }
}