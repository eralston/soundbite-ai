using System.Threading.Tasks;

namespace Masticore.Jobs
{
    /// <summary>
    /// An object which processes <see cref="IJob"/> objects off of an async queue
    /// </summary>
    public interface IJobWorker
    {
        /// <summary>
        /// Processes the given message, which is assumed to be JSON for a given <see cref="IJob"/> implementation
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<IJob> Process(string message, string id);
    }
}
