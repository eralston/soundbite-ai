using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Soundbite.AzFunc.Scheduler.Startup))]

namespace Soundbite.AzFunc.Scheduler
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.GetContext().Configuration;
            IServiceCollection services = builder.Services;
            services.AddLogging();
        }

        /// <summary>
        /// Loads the appsettings json
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
        }
    }
}
