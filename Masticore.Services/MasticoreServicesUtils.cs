using Masticore.Entity;
using Masticore.Providers;
using Masticore.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Masticore.Services
{
    /// <summary>
    /// Helper methods for putting Masticore services into DI
    /// </summary>
    public static class MasticoreServicesUtils
    {
        /// <summary>
        /// Load <see cref="IRbac"/> with <see cref="CurrentUserDbRbac"/>
        /// </summary>
        /// <param name="services"></param>
        public static void AddDbRbac(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            services.AddScoped<IRbac, CurrentUserDbRbac>();
        }

        /// <summary>
        /// Loads <see cref="IRbac"/> with <see cref="SystemDbRbac"/>
        /// </summary>
        /// <param name="services"></param>
        public static void AddSystemRbac(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            services.AddScoped<IRbac, SystemDbRbac>();
        }

        /// <summary>
        /// Loads <see cref="ISecurityContext"/> and friends
        /// </summary>
        /// <param name="services"></param>
        public static void AddSecurity(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            // Masticore Security Services
            services.AddScoped<ISecurityContextService, SecurityContextService>();
            services.AddScoped<ISecurityContext, SecurityContext>();
            services.AddScoped<ITokenDataService, TokenDataService>();
            services.AddScoped<IProviderFactory, ProviderFactory>();
        }

        /// <summary>
        /// Loads all <see cref="IService"/> concrete classes from <see cref="Masticore.Services"/>
        /// </summary>
        /// <param name="services"></param>
        public static void AddMasticoreResourceServices(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            // Masticore Resource Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IOrgSettingsService, OrgSettingsService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IQueuedJobStatusService, QueuedJobStatusService>();
        }
    }
}
