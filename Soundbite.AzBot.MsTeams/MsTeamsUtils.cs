using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Soundbite.AzBot.MsTeams
{
    /// <summary>
    /// Utilities for loading <see cref="IServiceCollection"/> and other helpful methods
    /// </summary>
    public static class MsTeamsUtils
    {
        /// <summary>
        /// Adds <see cref="MsTeamsBot"/> and friends
        /// </summary>
        /// <param name="services"></param>
        public static void AddMsTeamsBot(this IServiceCollection services)
        {
            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, MsTeamsBot>();
        }
    }
}
