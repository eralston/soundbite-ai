using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// The sandbox function provides a quick place to "play" with code.  It's primarily intended
    /// for manual deployment to Azure Functions to figure out an issue.
    /// </summary>
    public class SandboxFunc
    {

        #region EntryPoint

        [FunctionName(nameof(SandboxFunc))]
        public static void HandleEventGridEvent([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            try
            {
                log.LogInformation(Newtonsoft.Json.JsonConvert.SerializeObject(eventGridEvent));
            }
            catch { }
            try
            {
                log.LogInformation(eventGridEvent.Data.ToString());
            }
            catch { }

        }

        #endregion
    }
}