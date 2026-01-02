using Microsoft.Extensions.Logging;
using Soundbite.Api;
using System;
using System.Threading.Tasks;

namespace Soundbite.AzFun
{
    public abstract class SbAzureFunc
    {
        #region Properties

        /// <summary>
        /// Gets or sets a reference to the DI injected logger.
        /// </summary>
        protected ILogger Logger { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Base constructor for all SbAzureFunc implementations.
        /// </summary>
        /// <param name="logger">DI injected logger.</param>
        public SbAzureFunc(ILogger logger)
        {
            Logger = logger;
        }


        #endregion

        #region Methods

        /// <summary>
        /// If there is no connection, then load infra and run body; also has some handy logging of the lifecycle and failures
        /// </summary>
        /// <param name="azFunName"></param>
        /// <param name="log"></param>
        /// <param name="hasConnection"></param>
        /// <param name="loadInfrastructure"></param>
        /// <param name="azFunBody"></param>
        public async Task EnsureSecretsAndRun(
            string azFunName,
            Func<Task> azFunBody)
        {
            await AzFunUtils.EnsureSecretsAndRun(
                azFunName,
                Logger,
                () => Task.FromResult(AzInfrastructure.HasConnection),
                (dbStr, storStr) => AzInfrastructure.Configure(dbStr, storStr),
                azFunBody);
        }

        #endregion 
    }
}
