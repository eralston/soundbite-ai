using Masticore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Masticore.Media
{
    /// <summary>
    /// Helper methods for putting Masticore services into DI
    /// </summary>
    public static class MasticoreVideo
    {
        /// <summary>
        /// Loads all <see cref="IService"/> concrete classes from <see cref="Masticore.Video"/>
        /// </summary>
        /// <param name="services">Reference to the service container to which service references are added.</param>
        public static void AddMasticoreMediaServices(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            // Masticore Video Services
            services.AddScoped<IMediaProcessingService, MediaProcessingService>();
        }
    }
}
