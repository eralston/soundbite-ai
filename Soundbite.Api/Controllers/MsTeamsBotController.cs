using Masticore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller for responding to requests from the Azure Bot Framework on behalf of Microsoft teams.
    /// </summary>
    /// <remarks>
    /// As of Feb 2022, this API is hidden from integration with partners and should only be consumed by our first-party Teams app
    /// </remarks>
    [CodeGenModel(Ignore = true)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("msteamsbot2")]
    [ApiController]
    public class MsTeamsBotController : ControllerBase
    {
        #region Fields

        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="MsTeamsBotController"/> instance.
        /// </summary>
        /// <param name="adapter">DI reference to a Bot Framework HTTP adapter.</param>
        /// <param name="bot">DI reference to a Bot Framework bot.</param>
        public MsTeamsBotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Responsible for responding to bot requests from the Azure bot service.  
        /// </summary>
        /// <returns>an async task response from the bot via the adapter.</returns>
        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }

        #endregion
    }
}
