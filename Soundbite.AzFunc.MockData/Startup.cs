using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soundbite.Api;
using Soundbite.AzFun;

[assembly: FunctionsStartup(typeof(Soundbite.AzFunc.MockData.Startup))]
namespace Soundbite.AzFunc.MockData
{
    /// <summary>
    /// Function startup class used to initialize the Azure function.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Responsible for doing any configuration tasks required for function execution.
        /// </summary>
        /// <param name="builder">Azure Function host builder .</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.GetContext().Configuration;
            IServiceCollection services = builder.Services;
            services.AddLogging();
            services.AddSbApi(configuration, false);
            services.AddRbac(false);
        }

        /// <summary>
        /// Loads the appsettings json
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.LoadSbSecrets("local.settings.json", (string dbStr, string storStr) =>
            {
                AzInfrastructure.Configure(dbStr, storStr);
            });
        }
    }
}