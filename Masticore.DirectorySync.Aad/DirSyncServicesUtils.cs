using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masticore.DirectorySync.Aad
{
    public static class DirSyncServicesUtils
    {
        /// <summary>
        /// Add the core graph components like <see cref="OrgSyncGraphFactory"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddAdOrgSync(this IServiceCollection services, IConfiguration configuration, string connectionName)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));
            Validator.NotNull(connectionName, nameof(connectionName));

            services.AddScoped<IGraphFactory, OrgSyncGraphFactory>();
            SyncSettings.ConnectionString = configuration.GetConnectionString(connectionName);
        }
    }
}
