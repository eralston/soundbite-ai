using Masticore;
using Masticore.Ad;
using Masticore.Azure.MediaServices;
using Masticore.Media;
using Masticore.Queue;
using Masticore.Security;
using Masticore.Services;
using Masticore.Token;
using Masticore.Transcription.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soundbite.Api.Services;
using Soundbite.AzBot.MsTeams;
using Soundbite.Messaging;
using Soundbite.Services;
using System;

namespace Soundbite.Api
{
    /// <summary>
    /// A class for accumulating all the basic dependencies for injecting in the project
    /// </summary>
    public static class SbApiUtils
    {
        /// <summary>
        /// Adds the Soundbite-related services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="isUserInteractive"></param>
        public static void AddSbApi(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isUserInteractive = true)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            services.AddScoped<SpaService>();

            if (isUserInteractive)
            {
                services.AddMsGraph();
            }

            services.AddSecurity();

            // Masticore services must be loaded BEFORE Sb Services below
            services.AddMasticoreResourceServices();
            services.AddMasticoreMediaServices();
            services.AddMasticoreAzureMediaServices(configuration);

            services.AddAzureTranscription(configuration);
            services.AddSbResourceServices();
            services.AddOkta();
            services.AddSessionLifecycles();

            services.AddAdServices(configuration);
            // For now, user interactive is synonymous with DB creds being in the config object
            // Maybe that will change in the future. That might be the case if you start getting:
            // System.ArgumentNullException : Value cannot be null. (Parameter 'dbString')
            services.AddAzInfrastructure(configuration, isUserInteractive);
            services.AddAzQueue(configuration, "SbStorage");

            services.AddMsTeamsBot();

            services.AddOpenAi(configuration);
        }

        /// <summary>
        /// Add the AD token check system
        /// </summary>
        /// <param name="services"></param>
        public static void AddAdAuthToken(this IServiceCollection services)
        {
            services.AddScoped<AdAuthTokenValidator>();
        }

        /// <summary>
        /// Adds <see cref="IRbac"/> concretion for the given settings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="isInteractiveUser">True if the app is handling "real" user requests; otherwise, false to indicate this is a system background process with unlimited access</param>
        public static void AddRbac(this IServiceCollection services, bool isInteractiveUser = true)
        {
            if (isInteractiveUser)
            {
                services.AddDbRbac();
            }
            else
            {
                services.AddSystemRbac();
            }
        }

        /// <summary>
        /// Add <see cref="INotificationService"/> and supporting services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="isQueueMessaging"></param>
        public static void AddMessaging(this IServiceCollection services, IConfiguration configuration, bool isQueueMessaging = true)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            if (isQueueMessaging)
            {
                services.AddQueuedMessaging(configuration);
            }
            else
            {
                services.AddDirectMessaging(configuration);
            }

            services.AddAcs(configuration);
        }

        /// <summary>
        /// Loads the SpaConfig from the application configuration settings.
        /// </summary>
        /// <param name="services">Reference to the services container for the application.</param>
        /// <param name="configuration"></param>
        public static void AddSpaConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // Load front-end configuration into singleton
            SpaConfig spaConfig = new SpaConfig();
            configuration.Bind("SpaConfig", spaConfig);
            string platformAppId = configuration["AzureAd:ClientId"] ?? Environment.GetEnvironmentVariable("AzureAd:ClientId");
            spaConfig.AdPlatformConfig = new AdPlatformConfig { AppId = platformAppId };
            services.AddSingleton(spaConfig);

            // Use the redirect URI as the issuer claim in tokens.  This helps keep tokens are isolated regionally.
            TokenServiceSettings.TokenIssuerClaimValue = spaConfig?.AdClientConfig?.RedirectUri;
        }
    }
}
