using System;
using System.Threading.Tasks;

namespace Masticore.Jobs
{
    /// <summary>
    /// A useful base class for <see cref="IJobWorker"/> that implements a two-step prepare and process workflow
    /// </summary>
    /// <typeparam name="TJobType"></typeparam>
    public class JobWorker<TJobType> : IJobWorker where TJobType : IJob
    {
        /// <summary>
        /// Prepares the given job to be run; by default this does nothing
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual Task<TJobType> Prepare(TJobType job)
        {
            return Task.FromResult(job);
        }

        /// <inheritdoc/>
        public async Task<IJob> Process(string message, string messageId)
        {
            Validator.NotNullOrWhitespace(nameof(message), message);
            Validator.NotNullOrWhitespace(nameof(messageId), messageId);

            TJobType job = JsonUtils.FromLowerCamelJson<TJobType>(message);
            if (job == null)
            {
                throw new Exception($"Could not process message in {nameof(JobWorker<TJobType>)}; message could not convert to given {nameof(TJobType)}");
            }

            job.Id = messageId;
            job = await Prepare(job);
            await job.Process();
            return job;
        }
    }
}
