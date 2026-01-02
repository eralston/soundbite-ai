using Masticore.DirectorySync;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soundbite.Api;
using Soundbite.AzFun;
using Soundbite.Services;

[assembly: FunctionsStartup(typeof(Soundbite.AzFunc.DirectorySync.Startup))]

namespace Soundbite.AzFunc.DirectorySync
{
    /// <summary>
    /// Function startup class used to initialize the Azure function.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public const string SettingsFilename = "local.settings.json";

        protected IServiceCollection _services;

        /// <summary>
        /// Responsible for doing any configuration tasks required for function execution.
        /// </summary>
        /// <param name="builder">Azure Function host builder .</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.GetContext().Configuration;
            _services = builder.Services;
            _services.AddLogging();

            _services.AddSbApi(configuration, false);
            _services.AddRbac(false);
            _services.AddOrgSync(configuration, false);
        }

        /// <summary>
        /// Loads the appsettings json
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.LoadSbSecrets("local.settings.json", (string dbStr, string storStr) =>
            {
                SyncSettings.ConnectionString = dbStr;
                AzInfrastructure.Configure(dbStr, storStr);
            });
        }
    }
}