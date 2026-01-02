using Masticore.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masticore.Queue
{
    /// <summary>
    /// Load <see cref="IServiceCollection"/>
    /// </summary>
    public static class AzQueueUtils
    {
        /// <summary>
        /// Load <see cref="IJobQueue"/> with <see cref="AzQueue"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="connectionName"></param>
        public static void AddAzQueue(this IServiceCollection services, IConfiguration configuration, string connectionName)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));
            Validator.NotNull(connectionName, nameof(connectionName));

            string stor = configuration.GetConnectionString(connectionName);
            // Soundbite Support Services
            services.AddScoped<IJobQueue>(factory =>
            {
                return new AzQueue(stor);
            });
        }
    }
}
