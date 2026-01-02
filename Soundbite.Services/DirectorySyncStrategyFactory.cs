using Masticore.DirectorySync;
using Masticore.DirectorySync.Aad;
using Masticore.DirectorySync.Interact;
using Masticore.DirectorySync.Okta;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Soundbite.Services
{
    /// <summary>
    /// Basic directory sync provider factory.
    /// Should return a stateless directory sync provider
    /// </summary>
    public class DirectorySyncStrategyFactory : ISyncStrategyFactory
    {
        #region Properties

        private IGraphFactory GraphFactory { get; }
        private IHttpClientFactory HttpClientFactory { get; }
        private ISyncInfrastucture Infrastructure { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="DirectorySyncStrategyFactory"/> instance.
        /// </summary>
        /// <param name="graphFactory">DI reference to a Microsoft Graph factory.</param>
        /// <param name="httpClientFactory">DI reference to an HTTP Web Client factory.</param>
        public DirectorySyncStrategyFactory(IGraphFactory graphFactory, IHttpClientFactory httpClientFactory, ISyncInfrastucture infrastructure)
        {
            GraphFactory = graphFactory ?? throw new System.ArgumentNullException(nameof(graphFactory));
            HttpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
            Infrastructure = infrastructure ?? throw new System.ArgumentNullException(nameof(Infrastructure));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for returning an <see cref="IOrgSyncStrategy"/> instance based on the context of the given parameters
        /// </summary>
        /// <param name="orgConfig">Config object to determine the strategy needed</param>
        /// <param name="logger">Reference to a logger used for logging errors in the provider.</param>
        /// <param name="syncStore">The store where data will be persisted by the strategy</param>
        /// <returns>a <see cref="IOrgSyncStrategy"/> instance ready for use or <c>null</c> if the provider type has no implementation.</returns>
        public IOrgSyncStrategy GetOrgStrategy(OrgSyncConfig orgConfig, ILogger logger, IOrgSyncStore syncStore, OrgSyncConfig oldConfig = null)
        {
            return orgConfig.SyncType switch
            {
                AadOrgSyncStrategyBase.Name => CreateAadStrategy(orgConfig, logger, syncStore),
                InteractOrgSyncStrategyBase.Name => CreateInteractStrategy(orgConfig, logger, syncStore),
                OktaOrgSyncStrategyBase.Name => CreateOktaStrategy(HttpClientFactory, orgConfig, logger, syncStore, oldConfig),
                _ => null,
            };
        }

        /// <summary>
        /// Creates a new instance of the AAD sync provider based on the given arguments
        /// </summary>
        /// <param name="orgConfig"></param>
        /// <param name="logger"></param>
        /// <param name="syncStore"></param>
        /// <returns></returns>
        protected IOrgSyncStrategy CreateAadStrategy(OrgSyncConfig orgConfig, ILogger logger, IOrgSyncStore syncStore)
        {
            AadOrgSyncStrategy provider = new AadOrgSyncStrategy(logger, orgConfig, GraphFactory, syncStore, Infrastructure);
            return provider;
        }

        /// <summary>
        /// Creates a new instance of the Interact sync provider based on the given arguments
        /// </summary>
        /// <param name="orgConfig">DI reference to organization settings.</param>
        /// <param name="logger">DI reference to a logger.</param>
        /// <param name="syncStore">DI reference to the sync store.</param>
        /// <returns>a <see cref="InteractOrgSyncStrategy"/> initialized and ready for use.</returns>
        protected IOrgSyncStrategy CreateInteractStrategy(OrgSyncConfig orgConfig, ILogger logger, IOrgSyncStore syncStore)
        {
            InteractOrgSyncStrategy provider = new InteractOrgSyncStrategy(
                new InteractApiService(HttpClientFactory), orgConfig, logger, syncStore, Infrastructure);
            return provider;
        }

        /// <summary>
        /// Creates a new instance of the Interact sync provider based on the given arguments
        /// </summary>
        /// <param name="httpClientFactory">DI reference to an HTTP Client factory.</param>
        /// <param name="orgConfig">DI reference to organization settings.</param>
        /// <param name="logger">DI reference to a logger.</param>
        /// <param name="syncStore">DI reference to the sync store.</param>
        /// <returns>a <see cref="InteractOrgSyncStrategy"/> initialized and ready for use.</returns>
        protected IOrgSyncStrategy CreateOktaStrategy(IHttpClientFactory httpClientFactory, OrgSyncConfig orgConfig, ILogger logger, IOrgSyncStore syncStore, OrgSyncConfig oldConfig)
        {
            OktaOrgSyncStrategy provider = new OktaOrgSyncStrategy(httpClientFactory, orgConfig, oldConfig, logger, syncStore, Infrastructure);
            return provider;
        }

        #endregion
    }
}