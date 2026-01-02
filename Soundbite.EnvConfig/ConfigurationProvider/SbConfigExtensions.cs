using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Soundbite.Api;
using System;
using System.IO;
using System.Linq;

namespace Soundbite.EnvConfig
{
    /// <summary>
    /// Extension method used to add the custom Soundbite Configuration Provider during
    /// application setup / initialization.
    /// </summary>
    public static class SbConfigExtensions
    {
        #region Methods - Extensions

        /// <summary>
        /// Extension method used for loading Soundbite Environment configuration settings.
        /// </summary>
        /// <param name="configBuilder">Reference to the IConfigurationBuilder that is being configured.</param>
        /// <param name="envName">Name of the current environment. Certain environments have differnet loading behaviors (specifically Development and Ngrok)</param>
        /// <param name="options">Optional <see cref="SbEnvConfigOptions"/> containing information about what environment settings should be loaded.</param>
        /// <returns>a reference to the <paramref name="configBuilder"/> reference for method chaining.</returns>
        public static IConfigurationBuilder AddSbEnvConfig(this IConfigurationBuilder configBuilder, string envName, Action<SbEnvConfigOptions> setupConfig)
        {
            SbEnvConfigOptions options = new SbEnvConfigOptions(envName);
            setupConfig?.Invoke(options);
            AddSbEnv(configBuilder, options);
            return configBuilder;
        }

        #endregion

        #region Methods - Utility

        private static void Log(string message)
        {
            Console.Out.WriteLine(message);
        }

        private static void AddSbEnv(IConfigurationBuilder configBuilder, SbEnvConfigOptions options)
        {
            string cnxStringDb = null;
            string cnxStringStorage;
            IConfigurationRoot configRoot = null;

            // Set the base path for the builder
            Log($"Setting config builder base path to: {Directory.GetCurrentDirectory()}");
            configBuilder.SetBasePath(Directory.GetCurrentDirectory());

            // Load environment variables if requested
            if (options.EnableEnvVariables)
            {
                Log($"Adding Environment Variables to config builder");
                configBuilder.AddEnvironmentVariables();
            }

            // Load local settings file if requested
            if (options.EnableLocalSettings)
            {
                string localSettings = "local.settings.json";
                if (File.Exists(localSettings))
                {
                    Log("Adding local.settings.json file to config builder");
                    configBuilder.AddJsonFile(localSettings);
                }
                else
                {
                    Log("local.settings.json NOT located");
                }
            }

            // Load app settings file if requested
            if (options.EnableAppSettings)
            {
                //NOTE: the default JsonConfigurationProvider loads the appSettings.json and
                //      appSettings.{env}.json file automatically, so this should NOT be required
                //      for supporting web applications normally.  They are in here for the sake
                //      of completeness or if something wonky occurs and they need to be forced.

                string appSettingsPath = $"appSettings.json";
                if (File.Exists(appSettingsPath))
                {
                    Log("Adding appSettings.json to config builder");
                    configBuilder.AddJsonFile(appSettingsPath);
                }
                else
                {
                    Log("appSettings.json NOT located");
                }

                string appSettingsEnvPath = $"appSettings.{options.EnvName}.json";
                if (File.Exists(appSettingsEnvPath))
                {
                    Log($"Adding appSettings.{options.EnvName}.json to config builder");
                    configBuilder.AddJsonFile(appSettingsEnvPath);
                }
                else
                {
                    Log($"Adding appSettings.{options.EnvName}.json to config builder");
                }
            }

            // Load vault settings if required
            if (options.EnableVault)
            {
                LoadSecretsFromVault(options, configBuilder, ref configRoot);
            }
            else
            {
                Log("Vault not enabled");
            }

            // Load Env Settings from the database if required
            if (options.EnableEnvSettings)
            {
                Log("Adding EnvSettings from Soundbite Database to config builder");

                // Acquire the connection string from the current configuration
                cnxStringDb = GetDbConnectionString(options, configBuilder, ref configRoot);

                Log($"using {cnxStringDb}");
                // Add the SbConfigSource to the configuration builder
                configBuilder.Add(new SbConfigSource(options, cnxStringDb));
            }

            // Load Azure Infrastructure if required
            if (options.EnableAzInfrastructure)
            {
                cnxStringDb = string.IsNullOrEmpty(cnxStringDb)
                    ? GetDbConnectionString(options, configBuilder, ref configRoot)
                    : cnxStringDb;
                cnxStringStorage = GetStorageCnxString(options, configBuilder, ref configRoot);
                AzInfrastructure.Configure(cnxStringDb, cnxStringStorage);
            }
        }

        private static void LoadSecretsFromVault(SbEnvConfigOptions options, IConfigurationBuilder configBuilder, ref IConfigurationRoot configRoot)
        {
            Log("Loading Vault Secrets");

            // When vault values are requested the VaultUri must be provided as an environment variable
            // or as part of the loaded configuration up to this point
            string vaultUri = Environment.GetEnvironmentVariable("VaultUri");

            // When the Vault URI is not available as an environment setting then build out the
            // configuration settings and see if it was specified in local file.
            if (string.IsNullOrEmpty(vaultUri))
            {
                Log("Building configuration root to try to locate Vault URI");
                configRoot = EnsureConfigurationRoot(configBuilder, configRoot);
                vaultUri = configRoot.GetValue<string>("VaultUri");
            }

            // Ensure that the Vault URI was found
            if (string.IsNullOrEmpty(vaultUri))
            {
                if (options.VaultRequired)
                {
                    throw new Exception("Failed to load application settings because the VaultUri value is not specified in environment or application settings.");
                }
                else
                {
                    Log("Vault URI not located - skipping Vault");
                    // Vault is not required so just skip it
                    return;
                }
            }

            SecretClientOptions secretsOptions = new SecretClientOptions();
            SecretClient secretsClient = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential(), secretsOptions);

            // Iterate over each requested secret and attempt to retrieve it
            options.VaultKeysToLoad.ToList().ForEach(vaultKey =>
            {
                //TODO: determine if this can be done in a way to use the async function or if that would just jack everything up...
                KeyVaultSecret secret;
                try
                {
                    secret = secretsClient.GetSecret(vaultKey);
                    Log($"Read {vaultKey} from Vault");
                }
                catch (Exception ex)
                {
                    Log($"Faile to Read {vaultKey} from Vault");
                    throw new Exception($"Failed to retrieve requested secret '{vaultKey}' value from Vault.  Please verify that access to vault was provided to the application in the Azure Portal.", ex);
                }

                if (secret == null || string.IsNullOrEmpty(secret.Value))
                {
                    throw new Exception($"Value retrieved for secret '{vaultKey}' is null/empty.  Please verify the value is valid in the Azure Portal.");
                }

                options.Settings[vaultKey] = secret.Value;
            });
        }

        private static IConfigurationRoot EnsureConfigurationRoot(IConfigurationBuilder configBuilder, IConfigurationRoot configRoot)
        {
            return configRoot == null ? configBuilder.Build() : configRoot;
        }

        /// <summary>
        /// Responsible for locating the Soundbite Database Connection string.
        /// </summary>
        /// <returns>a string containing the Soundbite Database connection string.</returns>
        private static string GetDbConnectionString(SbEnvConfigOptions options, IConfigurationBuilder configBuilder, ref IConfigurationRoot configRoot)
        {
            // The easiest place to check is in the options to see if they are present from vault            
            if (!options.Settings.TryGetValue("ConnectionStrings--SbDb", out string result))
            {
                // Not located so try to acquire it form the configuration root
                configRoot = EnsureConfigurationRoot(configBuilder, configRoot);
                result = configRoot.GetValue<string>("ConnectionStrings:SbDb");
            }

            // Verify that we have it and if not we have run out of places to look...
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Cannot locate the Soundbite database connection string.");
            }

            return result;
        }

        /// <summary>
        /// Responsible for locating the Soundbite Storage Connection string.
        /// </summary>
        /// <returns>a string containing the Soundbite Database connection string.</returns>
        private static string GetStorageCnxString(SbEnvConfigOptions options, IConfigurationBuilder configBuilder, ref IConfigurationRoot configRoot)
        {
            // The easiest place to check is in the options to see if they are present from vault            
            if (!options.Settings.TryGetValue("ConnectionStrings--SbStorage", out string result))
            {
                // Not located so try to acquire it form the configuration root
                configRoot = EnsureConfigurationRoot(configBuilder, configRoot);
                result = configRoot.GetValue<string>("ConnectionStrings:SbStorage");
            }

            // Verify that we have it and if not we have run out of places to look...
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Cannot locate the Soundbite storage connection string.");
            }

            return result;
        }

        #endregion
    }
}