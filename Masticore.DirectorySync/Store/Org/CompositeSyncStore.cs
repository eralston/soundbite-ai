using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// <see cref="ISyncStore"/> base class over Entity Framework that supports a Composite pattern
    /// </summary>
    public abstract class CompositeSyncStore : ISyncStore
    {
        #region Properties

        public ProviderType ProviderType { get; set; }
        protected bool IsStarted { get; set; } = false;
        /// <summary>
        /// Get or set the <see cref="OrgSyncResult"/> such that anyone change change it, especially unit tests
        /// </summary>
        public OrgSyncResult Result { get; set; }
        /// <summary>
        /// Get or set the <see cref="OrgSyncConfig"/> such that anyone can change it before any action, especially unit tests
        /// </summary>
        public OrgSyncConfig Config { get; set; }
        protected Guid SyncRunId { get; set; }
        protected ILogger Logger { get; }

        private ISyncInfrastucture _syncInfrastructure = null;
        protected ISyncInfrastucture SyncInfrastructure
        {
            get
            {
                if (_syncInfrastructure == null)
                {
                    throw new InvalidOperationException($"Trying to read {GetType().Name}.{nameof(SyncInfrastructure)} before initialization");
                }

                return _syncInfrastructure;
            }
            set => _syncInfrastructure = value;
        }

        private ISyncDb _db;

        private OrganizationEntity _org;

        #endregion

        #region Methods

        /// <summary>
        /// Standalone constructor for a parent that might contains other instances of <see cref="CompositeSyncStore"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="syncInfrastructure"></param>
        public CompositeSyncStore(ILogger logger, ISyncInfrastucture syncInfrastructure)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SyncInfrastructure = syncInfrastructure ?? throw new ArgumentNullException(nameof(syncInfrastructure));
            SyncRunId = Guid.NewGuid();
        }

        /// <summary>
        /// Constructor for a child <see cref="CompositeSyncStore"/> that will inherit properties
        /// </summary>
        /// <param name="parent"></param>
        public CompositeSyncStore(CompositeSyncStore parent)
        {
            if (parent.SyncRunId == Guid.Empty)
            {
                throw new Exception($"Cannot attach {GetType().Name} to parent without an initialized {nameof(SyncRunId)}");
            }

            Logger = parent.Logger ?? throw new ArgumentNullException(nameof(Logger));
            SyncInfrastructure = parent.SyncInfrastructure ?? throw new ArgumentNullException(nameof(SyncInfrastructure));
            SyncRunId = parent.SyncRunId;
        }

        /// <summary>
        /// Async get the <see cref="ISyncDb"/> for this store
        /// </summary>
        /// <returns></returns>
        protected async Task<ISyncDb> GetDb()
        {
            if (_db == null)
            {
                AssertSupportingServices();

                _db = await SyncInfrastructure.SyncDbAsync() ?? throw new Exception($"{nameof(ISyncInfrastucture)} returned null in {GetType().Name}");
            }
            return _db;
        }

        /// <summary>
        /// Async gets the organization for this store's <see cref="OrgSyncConfig"/>
        /// </summary>
        /// <returns></returns>
        protected async Task<OrganizationEntity> GetOrg()
        {
            if (_org == null)
            {
                ISyncDb db = await GetDb();
                _org = await db.OrgWhereRoute(Config.OrgRoute);
                _org.AssertFound($"Cannot find organization for route {Config.OrgRoute}; missing or deleted");
            }

            return _org;
        }

        /// <summary>
        /// Throw an exception if support services are unavailable
        /// </summary>
        private void AssertSupportingServices()
        {
            if (Logger == null)
            {
                throw new InvalidOperationException($"{nameof(Logger)} required before sync");
            }

            if (SyncInfrastructure == null)
            {
                throw new InvalidOperationException($"{nameof(SyncInfrastructure)} required before sync");
            }

            if (SyncRunId == Guid.Empty)
            {
                throw new InvalidOperationException($"{nameof(SyncRunId)} non-empty value required before sync");
            }

            if (ProviderType == ProviderType.Unknown)
            {
                throw new InvalidOperationException($"{nameof(ProviderType)} cannot be unknown before sync");
            }
        }

        /// <summary>
        /// Throw an exception if supporting org config and results are missing
        /// </summary>
        private void AssertConfigAndResult()
        {
            if (Config == null)
            {
                throw new InvalidOperationException($"Cannot sync in {GetType().Name} without a live {nameof(OrgSyncConfig)}");
            }

            if (string.IsNullOrEmpty(Config.OrgRoute))
            {
                throw new InvalidOperationException($"Cannot sync in {GetType().Name} without a valid {nameof(OrgSyncConfig.OrgRoute)}");
            }

            if (Result == null)
            {
                throw new InvalidOperationException($"Cannot sync in {GetType().Name} without a valid {nameof(Result)}");
            }
        }

        /// <summary>
        /// Logs a debug message, suffixing the current org route and SyncRunId
        /// </summary>
        /// <param name="msg"></param>
        protected void LogDebug(string msg)
        {
            AssertSupportingServices();
            AssertConfigAndResult();
            Logger.LogDebug($"{GetType().Name}: {msg} for organization '{Config.OrgRoute}' during run '{SyncRunId}'");
        }

        #endregion

        #region ISyncStore

        /// <summary>
        /// Provides basic validation of current state of this object's lifecycle
        /// Default implementation that sets the <see cref="OrgSyncConfig"/> and <see cref="OrgSyncResult"/>
        /// </summary>
        /// <param name="orgConfig"></param>
        /// <param name="orgResult"></param>
        /// <returns></returns>
        public virtual Task<Guid> StartSync(OrgSyncConfig orgConfig, OrgSyncResult orgResult)
        {
            AssertSupportingServices();

            if (orgConfig == null)
            {
                throw new ArgumentNullException(nameof(orgConfig));
            }

            if (string.IsNullOrEmpty(orgConfig.OrgRoute))
            {
                throw new ArgumentNullException(nameof(orgConfig.OrgRoute));
            }

            Config = orgConfig;
            Result = orgResult ?? throw new ArgumentNullException(nameof(orgResult));

            LogDebug("Starting sync");

            IsStarted = true;

            return Task.FromResult(SyncRunId);
        }

        /// <inheritdoc />
        public async Task SetCommandTimeout(int seconds)
        {
            ISyncDb db = await GetDb();
            db.SetCommandTimeout(seconds);
        }

        /// <summary>
        /// Provides basic validation on the lifecycle of this object
        /// Override this method with the unique logic of the store provider
        /// </summary>
        /// <returns></returns>
        public virtual Task EndSync()
        {
            AssertSupportingServices();
            AssertConfigAndResult();

            LogDebug("Ending sync");

            if (!IsStarted)
            {
                throw new InvalidOperationException($"Must call {nameof(StartSync)} before you can call {nameof(EndSync)}");
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
