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
            services.AddAzInfrastructure(configuration);
            services.AddMasticoreMediaServices();
            services.AddMasticoreAzureMediaServices(configuration);
            services.AddSbApi(configuration, false);
            services.AddRbac(false);
            services.AddMessaging(configuration, true);
            services.AddSessionLifecycles();

            // Logic


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
            //string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "UNKNOWN";
            //builder.ConfigurationBuilder.AddSbEnvConfig(envName, config => config
            //    .ForLocalEnv(options =>
            //    {
            //        options.EnableAppSettings = true;
            //        options.EnableAzInfrastructure = true;
            //    })
            //    .ForRemoteEnv(options =>
            //    {
            //        options.VaultRequired = true;
            //        options.EnableEnvVariables = true;
            //        //options.EnableEnvSettings = true;
            //        options.VaultKeysToLoad.Add("ConnectionStrings--SbDb");
            //        options.VaultKeysToLoad.Add("ConnectionStrings--SbStorage");                    
            //        options.EnableAzInfrastructure = true;
            //    }));

            builder.LoadSbSecrets("local.settings.json", (string dbStr, string storStr) =>
            {
                AzInfrastructure.Configure(dbStr, storStr);
            });
        }
    }
}
