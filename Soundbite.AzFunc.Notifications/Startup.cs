using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soundbite.Api;
using Soundbite.AzFun;

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

            // Resource services
            services.AddSbApi(configuration, false);
            services.AddRbac(false);
            services.AddMessaging(configuration, false);

            // Mapper
            services.AddAutoMapper(typeof(Entity.MappingProfile), typeof(Masticore.Entity.MappingProfile));
        }

        /// <summary>
        /// Loads the appsettings json
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            //TODO: Udpate to use the Soundbite.EnvConfig settings!!!
            builder.LoadSbSecrets("local.settings.json", (string dbStr, string storStr) =>
            {
                AzInfrastructure.Configure(dbStr, storStr);
            });
        }
    }
}
