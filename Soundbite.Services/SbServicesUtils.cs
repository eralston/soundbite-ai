using Masticore;
using Masticore.Ai;
using Masticore.DirectorySync;
using Masticore.DirectorySync.Aad;
using Masticore.DirectorySync.Jobs;
using Masticore.Entity;
using Masticore.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Soundbite.Api;
using Soundbite.Services.Lifecycle;
using Soundbite.Services.Services;
using System;

namespace Soundbite.Services
{
    /// <summary>
    /// Utility methods for messaging and its lifecycle
    /// </summary>
    public static class SbServicesUtils
    {
        /// <summary>
        /// Applies SB-related <see cref="IService"/> classes to the given DI context
        /// </summary>
        /// <param name="services"></param>
        public static void AddSbResourceServices(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            services.AddScoped<IMemberService, SbMemberService>();
            services.AddScoped<ISbOrganizationService, SbOrganizationService>();

            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ISessionFeedService, SessionFeedService>();
            services.AddScoped<ISessionCommentService, SessionCommentService>();
            services.AddScoped<ISeriesService, SeriesService>();
            services.AddScoped<IClipService, ClipService>();
            services.AddScoped<IClipFileService, ClipFileService>();
            services.AddScoped<IClipOperationService, ClipOperationService>();

            services.AddScoped<ITranscriptFileService, TranscriptFileService>();
            services.AddScoped<ISbTranscriptionService, SbTranscriptionService>();
            services.AddScoped<ISbMediaService, SbMediaService>();
            services.AddScoped<IMediaStreamingService, MediaStreamingService>();
        }

        /// <summary>
        /// AI-related services, primarily supporting content insights
        /// </summary>
        /// <param name="services"></param>
        public static void AddOpenAi(this IServiceCollection services, IConfiguration configuration)
        {
            Validator.ArgNotNull(nameof(services), services);
            Validator.ArgNotNull(nameof(configuration), configuration);

            OpenAiSettings settings = OpenAiSettings.FromConfiguration(configuration);
            services.AddSingleton(settings);
            services.AddScoped<IAiClient, OpenAiCompletionClient>();
            services.AddScoped<IAiService, AiService>();
        }

        /// <summary>
        /// Load <see cref="OktaDataService"/> and friends
        /// </summary>
        /// <param name="services"></param>
        public static void AddOkta(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            services.AddScoped<IOktaDataService, OktaDataService>();
        }

        /// <summary>
        /// Load <see cref="LifecycleFactory"/> and its constituent <see cref="ILifecycleStrategy"/> concretions
        /// </summary>
        /// <param name="services"></param>
        public static void AddSessionLifecycles(this IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));
            services.AddScoped<ILifecycleFactory>((s) =>
            {
                return new LifecycleFactory(s.GetService<IServiceProvider>());
            });
            services.AddScoped<AnnouncementLifecycleStrategy>();
        }

        /// <summary>
        /// Load <see cref="AzInfrastructure"/> for all <see cref="IInfrastructure"/> related purposes
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddAzInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isConfigAvailable = true)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            // Soundbite Infrastructure
            if (isConfigAvailable)
            {
                AzInfrastructure.Configure(configuration);
            }

            services.AddScoped<ISbInfrastructure, AzInfrastructure>();
            services.AddScoped<IIdentityInfrastructure, AzInfrastructure>();
            services.AddScoped<ISyncInfrastucture, AzInfrastructure>();
        }

        /// <summary>
        /// Load <see cref="DirectorySyncStrategyFactory"/> and friends
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddOrgSync(this IServiceCollection services, IConfiguration configuration, bool isQueued = true)
        {
            Validator.NotNull(services, nameof(services));

            // Services
            services.AddScoped<IRegionSyncStore, RegionSyncStore>();
            services.AddScoped<IOrgSyncStore, OrgSyncStore>();
            services.AddScoped<IOrgSyncService, OrgSyncService>();

            if (isQueued)
            {
                services.AddScoped<IOrgSyncRunner, QueuedOrgSyncRunner>();
            }
            else
            {
                services.AddScoped<IOrgSyncRunner, OrgSyncService>();
            }

            services.AddScoped<ISyncJobWorker, SyncJobWorker>();
            services.AddScoped<ISyncStrategyFactory, DirectorySyncStrategyFactory>();
            services.AddAdOrgSync(configuration, "SbDb");
        }
    }
}
