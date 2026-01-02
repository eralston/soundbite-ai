using Masticore;
using Masticore.Acs;
using Masticore.Entity;
using Masticore.Jobs;
using Masticore.Mail;
using Masticore.Security;
using Masticore.Services;
using Masticore.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soundbite.Messaging.Jobs;
using Soundbite.Services;
using System;
using System.Reflection;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Utility methods for messaging and its lifecycle
    /// </summary>
    public static class MessagingServicesUtils
    {
        /// <summary>
        /// Applies queued-based notifications to <see cref="IServiceCollection"/>
        /// </summary>
        /// <remarks>This requires a <see cref="IJobQueue"/> concrete class to be register as well</remarks>
        /// <param name="services"></param>
        public static void AddQueuedMessaging(this IServiceCollection services, IConfiguration _)
        {
            Validator.NotNull(services, nameof(services));

            // Fires the full variety of real notifications
            services.AddScoped<ICombinedNotificationService>(factory =>
            {
                ICombinedNotificationService queued = new QueuedNotifications(
                    factory.GetService<ILogger<QueuedNotifications>>(),
                    factory.GetService<ISbInfrastructure>(),
                    factory.GetService<IJobQueue>()
                );
                NotificationPermissionDecorator withPermissions = new NotificationPermissionDecorator(
                    factory.GetService<ILogger<NotificationPermissionDecorator>>(),
                    queued,
                    factory.GetService<IIdentityInfrastructure>(),
                    factory.GetService<IRbac>()
                );
                return withPermissions;
            });
            services.AddScoped<INotificationService>(factory =>
                factory.GetService<ICombinedNotificationService>()
            );
            services.AddScoped<ISbNotificationService>(factory =>
                factory.GetService<ICombinedNotificationService>()
            );
        }

        /// <summary>
        /// Applies in-process notification sending
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDirectMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            AddMail(services, configuration);
            AddSms(services, configuration);
            AddTeams(services, configuration);

            AddNotifications(services);

            services.AddScoped<SbNotificationJobWorker>();
        }

        /// <summary>
        /// Load <see cref="ICombinedNotificationService"/> with immediately fired notifications
        /// </summary>
        /// <param name="services"></param>
        public static void AddNotifications(IServiceCollection services)
        {
            Validator.NotNull(services, nameof(services));

            // Fires the full variety of real notifications
            services.AddScoped<ICombinedNotificationService>(factory =>
            {
                ICombinedNotificationService[] notifiers = new ICombinedNotificationService[] {
                    factory.GetService<EmailNotificationService>(),
                    factory.GetService<SmsNotificationService>(),
                    factory.GetService<TeamsNotificationService>()
                };
                NotificationComposite composite = new NotificationComposite(
                    factory.GetService<ILogger<NotificationComposite>>(),
                    notifiers
                );
                NotificationPermissionDecorator withPermissions = new NotificationPermissionDecorator(
                    factory.GetService<ILogger<NotificationPermissionDecorator>>(),
                    composite,
                    factory.GetService<IIdentityInfrastructure>(),
                    factory.GetService<IRbac>()
                );
                return withPermissions;
            });
            services.AddScoped<INotificationService>(factory =>
                factory.GetService<ICombinedNotificationService>()
            );
            services.AddScoped<ISbNotificationService>(factory =>
                factory.GetService<ICombinedNotificationService>()
            );
        }

        /// <summary>
        /// Loads <see cref="SmsNotificationService"/> and friends
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSms(IServiceCollection services, IConfiguration configuration)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            // Try the settings JSON
            TwilioSettings settings = new TwilioSettings();
            configuration.Bind(nameof(TwilioSettings), settings);
            string twilioAccountSID = settings.AccountSID ?? Environment.GetEnvironmentVariable("TwilioAccountSID");
            string twilioAuthToken = settings.AuthToken ?? Environment.GetEnvironmentVariable("TwilioAuthToken");
            string twilioPhoneNumber = settings.PhoneNumber ?? Environment.GetEnvironmentVariable("TwilioPhoneNumber");

            TwilioSmsGateway.Init(twilioAccountSID, twilioAuthToken, twilioPhoneNumber);
            services.AddScoped<ISmsGateway, TwilioSmsGateway>();

            services.AddScoped<SmsNotificationService>();
        }

        /// <summary>
        /// Loads <see cref="EmailNotificationService"/> and friends
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddMail(IServiceCollection services, IConfiguration configuration)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            services.AddScoped<EmailNotificationService>();
        }

        /// <summary>
        /// Add e-mail support tooling for Azure Communication Services (ACS)
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <remarks>This is separate from the actual <see cref="EmailNotificationService"/> service, though it is a prereq for it</remarks>
        public static void AddAcs(this IServiceCollection services, IConfiguration configuration)
        {
            // Try the settings JSON
            AcsSettings settings = new AcsSettings();
            configuration.Bind("AzureCommunicationServiceSettings", settings);
            settings.ConnectionString ??= Environment.GetEnvironmentVariable("AzureCommunicationServiceConnectionString");
            settings.FromEmail ??= Environment.GetEnvironmentVariable("AzureCommunicationServiceFromEmail");
            settings.FromName ??= Environment.GetEnvironmentVariable("AzureCommunicationServiceFromName");

            // Setup mailbox and set as singleton dependency
            services.AddSingleton(settings);
            services.AddScoped<IAcsSettingFactory, AcsSettingsFactoryForOrg>();
            services.AddScoped<IMailbox, AcsMailbox>();

            services.AddScoped<ITextTemplate, RazorLightTextTemplate>();

            // Add templating
            EmailTemplates.InitWithRazorLight(Assembly.GetExecutingAssembly());
            services.AddScoped<IEmailTemplates, EmailTemplates>();
        }

        /// <summary>
        /// Loads <see cref="TeamsNotificationService"/> and friends
        /// </summary>
        /// <param name="services">Reference to the services container for the application.</param>
        public static void AddTeams(IServiceCollection services, IConfiguration configuration)
        {
            Validator.NotNull(services, nameof(services));
            Validator.NotNull(configuration, nameof(configuration));

            TeamsAppAzureSettings settings = new TeamsAppAzureSettings();
            configuration.Bind("TeamsAppAzureSettings", settings);
            settings.ClientId ??= Environment.GetEnvironmentVariable("TeamsAppAzureSettings__ClientId");
            settings.SecretKey ??= Environment.GetEnvironmentVariable("TeamsAppAzureSettings__SecretKey");
            settings.BaseUrl ??= Environment.GetEnvironmentVariable("TeamsAppAzureSettings__BaseUrl");
            services.AddSingleton(settings);

            services.AddHttpClient();

            services.AddScoped<ITeamsGraphService, TeamsGraphService>();
            services.AddScoped<TeamsNotificationService>();
        }
    }
}
