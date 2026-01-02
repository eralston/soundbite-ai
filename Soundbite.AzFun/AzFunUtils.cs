using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Soundbite.AzFun
{
    public static class AzFunUtils
    {
        /// <summary>
        /// Based on the current environment, this loads the connection strings into the AzInfrastructure class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        public static void LoadSbSecrets(
            this IFunctionsConfigurationBuilder builder,
            string settingsFilename, Action<string, string> loadInfrastructure,
            ILogger logger = null)
        {
            // TODO: Support true multi-tenant once we need multi-region
            FunctionsHostBuilderContext context = builder.GetContext();

            if (context.EnvironmentName == "Development")
            {
                IConfigurationRoot config = builder.ConfigurationBuilder
                    .AddJsonFile(Path.Combine(context.ApplicationRootPath, settingsFilename), optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                string dbStr = config.GetConnectionString("SbDb");
                string storStr = config.GetConnectionString("SbStorage");
                loadInfrastructure(dbStr, storStr);
            }
            else
            {
                LoadSecretsFromVault(loadInfrastructure, logger);
            }
        }

        public static void LoadSecretsFromVault(
            Action<string, string> loadInfrastructure,
            ILogger logger = null)
        {
            string vaultUri = Environment.GetEnvironmentVariable("VaultUri");

            logger?.LogInformation($"Connecting to vault {vaultUri}");

            SecretClientOptions options = new SecretClientOptions();
            SecretClient client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential(), options);

            KeyVaultSecret database = client.GetSecret("ConnectionStrings--SbDb");
            KeyVaultSecret storage = client.GetSecret("ConnectionStrings--SbStorage");

            if (logger != null)
            {
                SqlConnectionStringBuilder connSringBuilder = new SqlConnectionStringBuilder(database.Value);
                logger.LogInformation($"Connecting to SQL server {connSringBuilder.DataSource}");
            }
            loadInfrastructure(database.Value, storage.Value);
        }

        /// <summary>
        /// If there is no connection, then load infra and run body; also has some handy logging of the lifecycle and failures
        /// </summary>
        /// <param name="azFunName"></param>
        /// <param name="log"></param>
        /// <param name="hasConnection"></param>
        /// <param name="loadInfrastructure"></param>
        /// <param name="azFunBody"></param>
        public static async Task EnsureSecretsAndRun(
            string azFunName,
            ILogger log,
            Func<Task<bool>> hasConnection,
            Action<string, string> loadInfrastructure,
            Func<Task> azFunBody)
        {
            log.LogInformation($"Running {azFunName}...");
            try
            {
                if (!await hasConnection())
                {
                    log.LogInformation("Loading secrets from vault...");
                    LoadSecretsFromVault(loadInfrastructure, log);
                }

                log.LogInformation($"Running {azFunName}...");
                await azFunBody();
                log.LogInformation($"{azFunName} Successfully Completed");
            }
            catch (Exception exc)
            {
                log.LogCritical(exc, $"Exception executing {azFunName}");
                log.LogCritical(exc.InnerException, $"Inner exception executing {azFunName}");
                throw;
            }
        }

        /// <summary>
        /// Similar to <see cref="EnsureSecretsAndRun(string, ILogger, Func{Task{bool}}, Action{string, string}, Func{Task})"/>
        /// except it returns a <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <param name="azFunName"></param>
        /// <param name="log"></param>
        /// <param name="hasConnection"></param>
        /// <param name="loadInfrastructure"></param>
        /// <param name="azFunBody"></param>
        /// <returns></returns>
        public static async Task<TReturn> EnsureSecretsAndRun<TReturn>(
            string azFunName,
            ILogger log,
            Func<Task<bool>> hasConnection,
            Action<string, string> loadInfrastructure,
            Func<Task<TReturn>> azFunBody)
        {
            log.LogInformation($"Running {azFunName}...");
            try
            {
                if (!await hasConnection())
                {
                    log.LogInformation("Loading secrets from vault...");
                    LoadSecretsFromVault(loadInfrastructure, log);
                }

                log.LogInformation($"Running {azFunName}...");
                TReturn ret = await azFunBody();
                log.LogInformation($"{azFunName} Successfully Completed");
                return ret;
            }
            catch (Exception exc)
            {
                log.LogCritical(exc, $"Exception executing {azFunName}");
                log.LogCritical(exc.InnerException, $"Inner exception executing {azFunName}");
                throw;
            }
        }
    }
}
