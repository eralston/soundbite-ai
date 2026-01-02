using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Soundbite.Api;
using Soundbite.AzFun;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Called periodically to process sessions
    /// </summary>
    public class SessionLifecycleFunc
    {
        private SessionScheduler Sessions { get; }

        public SessionLifecycleFunc(SessionScheduler scheduler)
        {
            Sessions = scheduler;
        }

        // Built with: https://crontab.guru/
#if DEBUG
        // Every minute
        private const string SessionTiming = "0 * * * * *";
        private const bool RunOnStartup = true;
#else
        // At 0 and 30 minutes
        private const string SessionTiming = "0 0,30 * * * *";
        private const bool RunOnStartup = false;
#endif

        /// <summary>
        /// Time-triggered function for moving sessions through life-cycles
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="logger"></param>
        [FunctionName(nameof(SessionLifecycleFunc))]
        public async Task Run([TimerTrigger(SessionTiming, RunOnStartup = RunOnStartup)] TimerInfo myTimer, ILogger logger, ExecutionContext context)
        {
            await AzFunUtils.EnsureSecretsAndRun(
               nameof(SessionLifecycleFunc),
               logger,
               () => { return Task.FromResult(AzInfrastructure.HasConnection); },
               (string dbStr, string storStr) =>
               {
                   AzInfrastructure.Configure(dbStr, storStr);
               },
               Sessions.ProcessAsync);
        }
    }
}
