using AutoMapper;
using Masticore.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Soundbite.EnvConfig;
using Soundbite.Messaging;
using Soundbite.Services;

namespace Soundbite.WebJobs.Messaging
{
    // NOTE: Dependency Injection in Conole app original article:
    // LINK: https://csandunblogs.com/byte-sized-01-console-app-with-dependency-injection/

    /// <summary>
    /// Console application encompasing the notification processing web job.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args">Command line parameters (of which we expect none)</param>
        static void Main(string[] args)
        {
            using IHost host = CreateHost();
            using IServiceScope scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            services.GetRequiredService<App>().Run();
        }

        /// <summary>
        /// Builds an IHost instance
        /// </summary>
        /// <returns>an IHost instance ready for use.</returns>
        private static IHost CreateHost()
        {
            IHostBuilder builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(ConfigureLogging)
                .ConfigureAppConfiguration(ConfigureAppSettings)
                .ConfigureServices(ConfigurServices);
            return builder.Build();
        }

        /// <summary>
        /// Responsible for configuring application logging.
        /// </summary>
        /// <param name="loggingBuilder">Application logging configuration mechanism.</param>
        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        }

        /// <summary>
        /// Responsible for configuring application settings.
        /// </summary>
        /// <param name="context">Host builder context.</param>
        /// <param name="configBuilder">Application settings configuration mechanism.</param>
        private static void ConfigureAppSettings(HostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "UNKNOWN";

            configBuilder.AddSbEnvConfig(envName, config => config
                .ForLocalEnv(options =>
                {
                    options.EnableAppSettings = true;
                    options.EnableAzInfrastructure = true;
                })
                .ForRemoteEnv(options =>
                {
                    options.VaultRequired = true;
                    options.VaultKeysToLoad.Add("ConnectionStrings--SbDb");
                    options.VaultKeysToLoad.Add("ConnectionStrings--SbStorage");
                    options.EnableAppSettings = true;
                    options.EnableAzInfrastructure = true;
                }));
        }

        /// <summary>
        /// Responsible for configuring service dependency injection.
        /// </summary>
        /// <param name="context">Host builder context.</param>
        /// <param name="configBuilder">Application settings configuration mechanism.</param>
        private static void ConfigurServices(HostBuilderContext context, IServiceCollection services)
        {
            // Configure Auto Mapper
            services.AddAutoMapper(typeof(Entity.MappingProfile), typeof(Masticore.Entity.MappingProfile));

            // Configure Soundbite / Masticore services used in this application
            services.AddSystemRbac();
            services.AddSecurity();
            services.AddMasticoreResourceServices();
            services.AddAzInfrastructure(context.Configuration, false);
            MessagingServicesUtils.AddTeams(services, context.Configuration);

            // Configure Services Specifically for this Application
            services.AddScoped<INotificationProcessingService, NotificationProcessingService>();
            services.AddScoped<IChannelProcessorFactory, ChannelProcessorFactory>();
            services.AddScoped<TeamsChannelProcessor>();
            services.AddScoped<App>();
        }
    }
}