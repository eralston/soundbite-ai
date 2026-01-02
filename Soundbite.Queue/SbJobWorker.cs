using Masticore.Jobs;
using Microsoft.Extensions.Logging;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Queue
{
    /// <summary>
    /// Specialized <see cref="IJobWorker"/> for processing <see cref="SbJobBase"/> objects
    /// </summary>
    /// <remarks>This class and all child classes should be able to be surfaced from dependency injection so they can receive infrastructure and loggers as needed</remarks>
    /// <typeparam name="TJobType"></typeparam>
    public class SbJobWorker<TJobType> : JobWorker<TJobType> where TJobType : SbJobBase
    {
        protected ISbInfrastructure Infrastructure { get; }
        protected ILogger Logger { get; }

        public SbJobWorker(ISbInfrastructure infrastructure, ILogger<SbJobWorker<TJobType>> logger)
        {
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<TJobType> Prepare(TJobType job)
        {
            job.Infrastructure = Infrastructure;
            job.Logger = Logger;
            return base.Prepare(job);
        }
    }
}
