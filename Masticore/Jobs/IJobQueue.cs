using System.Threading.Tasks;

namespace Masticore.Jobs
{
    /// <summary>
    /// An object for delegating <see cref="IJob"/> objects to an async queue
    /// </summary>
    public interface IJobQueue
    {
        /// <summary>
        /// Async add the given object to the queue for processing, returning when it's successfully queue, but not yet processed
        /// </summary>
        /// <typeparam name="TJobType"></typeparam>
        /// <param name="job"></param>
        /// <returns>A unique identifier for the given job in the queue system</returns>
        Task<string> Add<TJobType>(TJobType job) where TJobType : IJob;

        // It's possible to support cancel, but that is an anti-pattern
        // If you're about to put a cancel method right here, don't
    }
}
