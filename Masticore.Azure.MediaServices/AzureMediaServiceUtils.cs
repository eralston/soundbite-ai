using Masticore.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masticore.Azure.MediaServices
{
    /// <summary>
    /// Helper methods for putting Masticore services into DI
    /// </summary>
    public static class AzureMediaServiceUtils
    {
        /// <summary>
        /// Loads all <see cref="IService"/> concrete classes from <see cref="Masticore.Video"/>
        /// </summary>
        /// <param name="services">Reference to the service container to which service references are added.</param>
        public static void AddMasticoreAzureMediaServices(this IServiceCollection services, IConfiguration configuration)
        {
            Validator.NotNull(services, nameof(services));

            // Configuration Settings
            AzureMediaServiceConfig config = new AzureMediaServiceConfig();
            configuration.Bind("AzureMediaServiceSettings", config);
            services.AddSingleton<IAzureMediaServiceConfig>(config);

            // Masticore Video Services
            services.AddScoped<IAzureMediaService, AzureMediaService>();
        }
    }
}
