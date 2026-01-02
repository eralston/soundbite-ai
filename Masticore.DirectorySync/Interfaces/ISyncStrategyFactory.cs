using Masticore.Models;
using Microsoft.Extensions.Logging;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Factory that generates IOrgSyncProvider objects based on the OrgSyncConfig object
    /// </summary>
    public interface ISyncStrategyFactory
    {
        /// <summary>
        /// Responsible for returning an <see cref="IOrgSyncStrategy"/> instance based on the given arguments
        /// </summary>
        /// <param name="config">The config object reprsenting the given org</param>
        /// <param name="logger">Reference to a logger used for logging errors in the provider</param>
        /// <param name="orgSyncStore">Persistence provider for the organization</param>
        /// <param name="result">Result object that will accumulate the sync result for the provider</param>
        /// <returns></returns>
        IOrgSyncStrategy GetOrgStrategy(OrgSyncConfig config, ILogger logger, IOrgSyncStore orgSyncStore, OrgSyncConfig oldConfig = null);
    }
}