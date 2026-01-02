using Microsoft.Extensions.Configuration;

namespace Masticore.Ai
{
    /// <summary>
    /// An object offering AI settings
    /// </summary>
    public class OpenAiSettings
    {
        /// <summary>
        /// Returns an object loaded from the given configuration (for ASP.Net apps), falling back to environment variables (for Azure Functions)
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <remarks>This will throw an exception if it would result in an invalid object</remarks>
        public static OpenAiSettings FromConfiguration(IConfiguration configuration)
        {
            // Try the settings JSON
            OpenAiSettings ret = new OpenAiSettings();
            configuration.Bind(nameof(OpenAiSettings), ret);
            ret.ApiKey ??= Environment.GetEnvironmentVariable($"{nameof(OpenAiSettings)}__{nameof(ApiKey)}");
            ret.Deployment ??= Environment.GetEnvironmentVariable($"{nameof(OpenAiSettings)}__{nameof(Deployment)}");
            ret.Endpoint ??= Environment.GetEnvironmentVariable($"{nameof(OpenAiSettings)}__{nameof(Endpoint)}");
            ret.Region ??= Environment.GetEnvironmentVariable($"{nameof(OpenAiSettings)}__{nameof(Region)}");
            ret.Validate();

            return ret;
        }

        /// <summary>
        /// The subscription key for the AI service
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Name of the deployed model
        /// </summary>
        public string? Deployment { get; set; }

        /// <summary>
        /// The endpoint for the AI service
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// The region for the AI service
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Throws an exception if the object is not ready for use
        /// </summary>
        public void Validate()
        {
            Validator.NotNullOrWhitespace(nameof(Deployment), Deployment);
            Validator.NotNullOrWhitespace(nameof(Endpoint), Endpoint);
            Validator.NotNullOrWhitespace(nameof(Region), Region);
            Validator.NotNullOrWhitespace(nameof(ApiKey), ApiKey);
        }
    }
}