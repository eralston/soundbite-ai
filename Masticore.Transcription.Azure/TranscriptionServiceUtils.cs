using Masticore.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soundbite;
using System;

namespace Masticore.Transcription.Azure
{
    /// <summary>
    /// Contains transcription result information with details
    /// </summary>
    public static class AzureTranscriptionServiceUtils
    {
        /// <summary>
        /// Applies SB-related <see cref="IService"/> classes to the given DI context
        /// </summary>
        /// <param name="services"></param>
        public static void AddAzureTranscription(this IServiceCollection services, IConfiguration configuration)
        {
            AzureTranscriptionSettings settings = new AzureTranscriptionSettings();
            configuration.Bind("AzureTranscriptionSettings", settings);

            settings.Region ??= Environment.GetEnvironmentVariable("AzureTranscriptionSettings:Key") ?? "";
            settings.Key ??= Environment.GetEnvironmentVariable("AzureTranscriptionSettings:Region") ?? "";

            Validator.NotNull("AzureTranscriptionSettings", settings, "AzureTranscriptionSettings configuration section is missing.");
            Validator.NotNullOrEmpty("AzureTranscriptionSettings.Region", settings.Region, "AzureTranscriptionSettings does not contain a region value.");
            Validator.NotNullOrEmpty("AzureTranscriptionSettings.Key", settings.Key, "AzureTranscriptionSettings does not contain a key value.");

            services.AddSingleton<AzureTranscriptionSettings>(settings);

            services.AddScoped<ITranscriptionService, AzureTranscriptionService>(svc =>
            {
                AzureTranscriptionSettings settings = svc.GetService<AzureTranscriptionSettings>();
                return new AzureTranscriptionService(
                    svc.GetRequiredService<ILogger<AzureTranscriptionService>>(),
                    svc.GetRequiredService<IMediaProcessingService>(),
                    settings.Region,
                    settings.Key
                );
            });
        }
    }
}