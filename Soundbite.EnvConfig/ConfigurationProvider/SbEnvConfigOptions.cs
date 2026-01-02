using System;
using System.Collections.Generic;
using System.Linq;

namespace Soundbite.EnvConfig
{
    /// <summary>
    /// Represents the environment loading configuration options.
    /// </summary>
    public class SbEnvConfigOptions
    {
        #region Static Methods

        /// <summary>
        /// Determines whether the environment is a local environment or in Azure.
        /// </summary>
        /// <param name="envName">Name of the current environment.</param>
        /// <returns><c>true</c> if the environment is local, otherwise <c>false</c>.</returns>
        public static bool IsLocalEnv(string envName)
        {
            string[] localEnvs = new string[] { "Development", "Ngrok" };
            return localEnvs.Any(i => string.Equals(envName, i, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion

        #region Fields

        // Property backer fields
        private bool _vaultRequired = false;

        #endregion

        #region Properties - Flags

        /// <summary>
        /// Gets or Sets a flag indicating whether to retrieve settings from Vault.
        /// </summary>
        public bool EnableVault { get; set; }

        /// <summary>
        /// Gets or Sets a flag indicating whether to retrieve settings from the local.settings.json
        /// file. This is normally associated with settings files for local Azure Func development.
        /// </summary>
        public bool EnableLocalSettings { get; set; }

        /// <summary>
        /// Gets or Sets flag indicating whether to retrieve settings from an appSettings.json file
        /// and from related appSettings.{environment}.json files.
        /// </summary>
        public bool EnableAppSettings { get; set; }

        /// <summary>
        /// Gets or Sets a flag indicating whether to load environment variables.
        /// </summary>
        public bool EnableEnvVariables { get; set; }

        /// <summary>
        /// Gets or Sets a flag indicaing whether to load EnvSetting values from the Soundbite DB.
        /// </summary>
        public bool EnableEnvSettings { get; set; }

        /// <summary>
        /// Gets or Sets a flag indicating whether to initialize the AzInfrastructure. This is
        /// normally need when configuring Azure Functions.
        /// </summary>
        public bool EnableAzInfrastructure { get; set; }

        #endregion

        #region Properties - Configuration

        /// <summary>
        /// Gets or sets a flag specifying whether vault access is required. When required, an 
        /// exception is thrown if the VaultUri is not accessible.  If optional, vault is bypassed
        /// if the URI is not found in the configuration.
        /// </summary>
        public bool VaultRequired
        {
            get => _vaultRequired;
            set
            {
                _vaultRequired = value;
                if (_vaultRequired)
                {
                    // Automatically enable vault if it is required (seems logical).
                    EnableVault = true;
                }
            }
        }

        /// <summary>
        /// Gets or Sets a list containing keys of vault secrets to load into the configuration.
        /// </summary>
        public IList<string> VaultKeysToLoad { get; set; } = new List<string>();

        /// <summary>
        /// Gets or Sets a list of EnvSetting group keys used to load EnvSetting groups from the SB DB.
        /// </summary>
        public IList<string> EnvSettingsGroupKeys { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a dictionary containing key/value pairs to add to the configuration 
        /// provider. This is primarily used to populate Azure Vault Secrets into the provider but
        /// can also be used to add ad-hoc settings to the configuration provider if needed.
        /// </summary>
        public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or Sets a flag indicatign whether to load EnvSetting defaults from the SB DB.
        /// </summary>
        public bool LoadEnvSettingsDefaults { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the environment in which the application is running.
        /// </summary>
        public string EnvName { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SbEnvConfigOptions"/> instance.
        /// </summary>
        /// <param name="envName">Name of the environment in which the application is running.</param>
        public SbEnvConfigOptions(string envName)
        {
            EnvName = envName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Used for configuring options for a remote environment.
        /// </summary>
        /// <param name="configAction">Configuration code to run for a remote environment.</param>
        /// <returns>a self reference for method chaining</returns>        
        /// <remarks>
        /// Usage:
        /// <code>
        ///   var options = SbEnvConfigOptions.GetDefaultOptionsForAzureFunc("envName")
        ///       .ForLocalEnv((cfg) => 
        ///       {
        ///           cfg.EnableLocalSettings = true;
        ///           // More Local Configuration ...
        ///       })
        ///       .ForRemoteEnv((cfg) =>
        ///       {
        ///           cfg.EnableLocalSettings = false;
        ///           // More Remote Configuration...
        ///       });
        ///</code>
        /// </remarks>
        public SbEnvConfigOptions ForRemoteEnv(Action<SbEnvConfigOptions> configAction)
        {
            if (!IsLocalEnv(EnvName))
            {
                configAction?.Invoke(this);
            }
            return this;
        }

        /// <summary>
        /// Used for configuring options for a local environment.
        /// </summary>
        /// <param name="configAction">Configuration code to run for a local environment.</param>
        /// <returns>a self reference for method chaining</returns>        
        /// <code>
        ///   var options = SbEnvConfigOptions.GetDefaultOptionsForAzureFunc("envName")
        ///       .ForLocalEnv((cfg) => 
        ///       {
        ///           cfg.EnableLocalSettings = true;
        ///           // More Local Configuration ...
        ///       })
        ///       .ForRemoteEnv((cfg) =>
        ///       {
        ///           cfg.EnableLocalSettings = false;
        ///           // More Remote Configuration...
        ///       });
        ///</code>
        /// </remarks>
        public SbEnvConfigOptions ForLocalEnv(Action<SbEnvConfigOptions> configAction)
        {
            if (IsLocalEnv(EnvName))
            {
                configAction?.Invoke(this);
            }
            return this;
        }

        /// <summary>
        /// Used for configuring options when matching the specified environment.
        /// </summary>
        /// <param name="envName">Name of the environment that when matched runs the specified <paramref name="configAction"/>.</param>
        /// <param name="configAction">Configuration code to run for a local environment.</param>
        /// <returns>a self reference for method chaining</returns>        
        public SbEnvConfigOptions ForEnv(string envName, Action<SbEnvConfigOptions> configAction)
        {
            if (string.Equals(envName, EnvName, StringComparison.InvariantCultureIgnoreCase))
            {
                configAction?.Invoke(this);
            }
            return this;
        }

        #endregion
    }
}