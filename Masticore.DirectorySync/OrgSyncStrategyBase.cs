using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Base class for building <see cref="IOrgSyncStrategy"/> implementations.
    /// </summary>
    /// <typeparam name="TSyncConfig">.NET type of the sync configuration settings.</typeparam>
    public abstract class OrgSyncStrategyBase<TSyncConfig> : IOrgSyncStrategy
        where TSyncConfig : class, ISyncStrategyConfig
    {
        #region Constants

        public string JsonConversionFailed = "Failed to deserialize JSON configuration into a valid sync settings object.";

        #endregion

        #region Fields

        private OrgSyncResult _orgResult;

        private TSyncConfig _syncSettings;

        #endregion

        #region Properties

        /// <summary>
        /// Stores a list of users that have been synchronized to avoid repeats.
        /// </summary>
        protected HashSet<string> SyncedUsers { get; } = new HashSet<string>();

        /// <summary>
        /// Gets a reference to a logger for logging operations.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets a reference to the Sync store.
        /// </summary>
        protected IOrgSyncStore Store { get; }

        /// <summary>
        /// Gets or sets the <see cref="OrgSyncResult"/> instance that is populated with 
        /// synchronization details/stats during the directory sync process.
        /// </summary>
        protected OrgSyncResult OrgResult
        {
            get
            {
                if (_orgResult == null)
                {
                    throw new NullReferenceException($"Must set {nameof(OrgResult)} before using it");
                }

                return _orgResult;
            }
            set => _orgResult = value;
        }

        /// <summary>
        /// Gets the organizational level JSON configuration settings for the directory sync.
        /// </summary>
        protected OrgSyncConfig OrgConfig { get; }

        /// <summary>
        /// Gets the strongly-typed configuration settings for the directory sync.
        /// </summary>
        protected TSyncConfig Settings
        {
            get
            {
                if (_syncSettings is null)
                {
                    _syncSettings = OrgConfig.GetSyncConfig<TSyncConfig>();
                }
                return _syncSettings;
            }
        }

        /// <summary>
        /// Underlying resources for this tenant
        /// </summary>
        protected IIdentityInfrastructure Infrastructure { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="OrgSyncStrategyBase{TSyncConfig}"/> instance.
        /// </summary>
        public OrgSyncStrategyBase(
            ILogger logger,
            IOrgSyncStore orgSyncStore,
            OrgSyncConfig orgConfig,
            IIdentityInfrastructure infrastructure
            )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Store = orgSyncStore ?? throw new ArgumentNullException(nameof(orgSyncStore));
            OrgConfig = orgConfig ?? throw new ArgumentNullException(nameof(orgConfig));
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));

            orgSyncStore.SetProviderType(ProviderType.OKTA);
        }

        #endregion

        #region IOrgSyncStrategy Implementation (Abstracted)        

        /// <inheritdoc />
        public abstract Task Sync(OrgSyncResult orgResult);

        /// <inheritdoc />
        public abstract Task<bool> HasAccessAsync();

        /// <inheritdoc />
        public abstract Task<TokenPageResponse<SyncTarget>> UsersAsync(TokenPageRequest page = null);

        /// <inheritdoc />
        public abstract Task<SyncTarget> UserAsync(string userId);

        /// <inheritdoc />
        public abstract Task<TokenPageResponse<SyncTarget>> GroupsAsync(TokenPageRequest page = null);

        /// <inheritdoc />
        public abstract Task<SyncTarget> GroupAsync(string groupId);

        /// <inheritdoc />
        public abstract Task<string> ValidationMessagesAsync(OrgSyncConfig orgConfig);

        #endregion

        #region IOrgSyncStrategy Implementation        

        /// <inheritdoc />
        public async Task<OrgSyncConfig> SanitizeConfig(OrgSyncConfig orgConfig)
        {
            // When the org configuration is null just return null
            if (orgConfig is null)
            {
                return null;
            }

            // The orgRoute is set by the run, not the OrgSyncConfig
            orgConfig.OrgRoute = null;

            // Create a new instance of the sync configuration to sanitize
            TSyncConfig syncConfig = null;

            // Attempt to convert JSON config to typed settings
            try
            {
                syncConfig = orgConfig.GetSyncConfig<TSyncConfig>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, JsonConversionFailed);
                throw new UserSafeException(JsonConversionFailed, ex);
            }

            // If the configuration is null just return the original org config
            if (syncConfig is null)
            {
                return orgConfig;
            }

            // Let the implementing class sanitize the typed settings
            await SanitizeConfig(syncConfig);

            // Create a new OrgSyncConfig to return the sanitized settings
            OrgSyncConfig result = new OrgSyncConfig()
            {
                OrgRoute = orgConfig.OrgRoute,
                SyncType = syncConfig.SyncType,
                SyncConfigJson = syncConfig.ToLowerCamelJson(false),
            };

            return result;
        }

        /// <inheritdoc />
        public async Task<OrgSyncConfig> TransferConfigAsync(OrgSyncConfig oldOrgConfig, OrgSyncConfig newOrgConfig)
        {
            // When the new configuration is null return null
            if (newOrgConfig == null)
            {
                return null;
            }

            // When the old configuration is null return the new setting
            if (oldOrgConfig == null)
            {
                return newOrgConfig;
            }

            // Create old/new typed settings instances
            TSyncConfig settingsOld = oldOrgConfig.GetSyncConfig<TSyncConfig>(true, false);
            TSyncConfig settingsNew = newOrgConfig.GetSyncConfig<TSyncConfig>();

            // Let the implementing class transfer the appropriate settings
            await TransferConfigAsync(settingsOld, settingsNew);

            newOrgConfig.SyncConfigJson = settingsNew.ToLowerCamelJson();
            newOrgConfig.OrgRoute = null; // Route doesn't matter; only set at runtime
            return newOrgConfig;
        }

        #endregion

        #region Abstract Members        

        /// <summary>
        /// Removes sensitive data from the settings so it can be sent to the client.
        /// </summary>
        /// <param name="settings">Settings object whose sensitive data should be scrubbed.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected abstract Task SanitizeConfig(TSyncConfig settings);

        /// <summary>
        /// Responsible for transferring any sanitized values from the current sync settings to the
        /// incoming sync settings. Sensitive information is stripped from the sync settings by
        /// the <see cref="SanitizeConfig(TSyncConfig)"/> method when being sent to the client and
        /// these values "disappear" if not accounted for in this method or set client-side. Please
        /// note that sometimes "sanitized" data is set by client and must be preferred over the
        /// old setting that was sanitized.
        /// </summary>
        /// <param name="settingsOld">Existing sync settings containing sanitized values.</param>
        /// <param name="settingsNew">Incoming sync settings with sanitized values removed (or set by the client).</param>
        /// <returns>a task indicating completion of the operation.</returns>
        protected abstract Task TransferConfigAsync(TSyncConfig configOld, TSyncConfig configNew);

        #endregion
    }
}
