using AutoMapper;
using Masticore.Azure.MediaServices;
using Masticore.Media;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soundbite.Api;
using Soundbite.AzFun;
using Soundbite.Services;

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
            services.AddMasticoreMediaServices();
            services.AddMasticoreAzureMediaServices(configuration);
            services.AddSbApi(configuration, false);
            services.AddRbac(false);
            services.AddMessaging(configuration, true);

            // Logic
            services.AddScoped<SessionScheduler>();
            services.AddScoped<SeriesScheduler>();

            // Jobs
            services.AddScoped<TranscriptionJob>();

            // Mapper
            services.AddAutoMapper(typeof(Entity.MappingProfile), typeof(Masticore.Entity.MappingProfile));
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
