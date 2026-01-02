using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Soundbite.Api;
using Soundbite.AzFun;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Called periodically re-populate series
    /// </summary>
    public class SeriesLifecycleFunc
    {
        private SeriesScheduler Series { get; }

        public SeriesLifecycleFunc(SeriesScheduler scheduler)
        {
            Series = scheduler;
        }

        // Built with: https://crontab.guru/
#if DEBUG
        // Every minute
        private const string SeriesTiming = "0 * * * * *";
        private const bool RunOnStartup = true;
#else
        // Once a day at midnight
        private const string SeriesTiming = "0 0 0 * * *";
        private const bool RunOnStartup = false;
#endif

        /// <summary>
        /// Time-triggered function for populating sessions going into the future
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        [FunctionName(nameof(SeriesLifecycleFunc))]
        public async Task Run(
            [TimerTrigger(SeriesTiming, RunOnStartup = RunOnStartup)] TimerInfo myTimer,
            ILogger log,
            ExecutionContext context)
        {
            await AzFunUtils.EnsureSecretsAndRun(
                nameof(SeriesLifecycleFunc),
                log,
                () => { return Task.FromResult(AzInfrastructure.HasConnection); },
                (string dbStr, string storStr) =>
                {
                    AzInfrastructure.Configure(dbStr, storStr);
                },
                Series.ProcessAsync);
        }
    }
}
