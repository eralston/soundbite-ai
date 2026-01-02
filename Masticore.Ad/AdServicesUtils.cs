using Masticore.Graph;
using Masticore.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System;

namespace Masticore.Ad
{
    /// <summary>
    /// Utils for loading <see cref="IServiceCollection"/>
    /// </summary>
    public static class AdServicesUtils
    {
        /// <summary>
        /// Add <see cref="IAdAppSettings"/> with <see cref="AdAppSettings"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddAdServices(this IServiceCollection services, IConfiguration configuration)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            AdAppSettings adSettings = new AdAppSettings
            {
                AppId = configuration["AzureAd:ClientId"] ?? Environment.GetEnvironmentVariable("AzureAd:ClientId"),
                AppSecret = configuration["AzureAd:ClientSecret" ?? Environment.GetEnvironmentVariable("AzureAd:ClientSecret")],
            };
            AdAppSettings.Init(adSettings);

            services.AddSingleton<IAdAppSettings>(AdAppSettings.Instance);
        }

        /// <summary>
        /// Load <see cref="ISecurity"/> with <see cref="AdSecurity"/>
        /// </summary>
        /// <remarks>This adds middleware components to <see cref="IServiceCollection"/> that interferes with the AzFun runtime; do NOT add to Functions!</remarks>
        /// <param name="services"></param>
        public static void AddAdSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            services.AddScoped<ISecurity, AdSecurity>();

            // Adding middleware right here will break Azure Functions
            // This implicitly read the "AzureAd" section of the appsettings.json
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  // AD Auth
                                                                                // NOT 100% if we need the following things any more because we only process AD tokens from a specific entry point
                                                                                // Needs further testing and a way to not need ITokenAcquisition in MS Graph services
                    .AddMicrosoftIdentityWebApi(configuration)      // Presumably reads AzureAd section of config and processes incomming JWT tokens
                    .EnableTokenAcquisitionToCallDownstreamApi()    // This registers ITokenAcquisition for DI
                    .AddInMemoryTokenCaches();

            services.AddAuthorization();
        }

        /// <summary>
        /// Load <see cref="IGraph"/> with <see cref="MsGraph"/>
        /// </summary>
        /// <param name="services"></param>
        public static void AddMsGraph(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            services.AddHttpClient();
            services.AddScoped<IGraph, MsGraph>();
        }
    }
}
