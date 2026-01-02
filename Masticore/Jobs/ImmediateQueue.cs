using System;
using System.Threading.Tasks;

namespace Masticore.Jobs
{
    /// <summary>
    /// A default implementation of <see cref="IJobQueue"/> that just runs the job when added and doesn't actually do anything separate from this thread
    /// </summary>
    /// <remarks>This is helpful for prototyping or local processing that doesn't actually benefit from true async</remarks>
    public class ImmediateQueue : IJobQueue
    {
        /// <summary>
        /// Async run the given <see cref="IJob"/>
        /// </summary>
        /// <typeparam name="TJobType"></typeparam>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<string> Add<TJobType>(TJobType job) where TJobType : IJob
        {
            Validator.NotNull(nameof(job), job);

            job.Id = Guid.NewGuid().ToString();
            await job.Process();
            return job.Id;
        }
    }
}
