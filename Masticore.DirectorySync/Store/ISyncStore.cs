using Masticore.Models;
using System;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Outlines the basic lifecycle of a sync store
    /// </summary>
    public interface ISyncStore
    {
        /// <summary>
        /// The type of directory offering the sync information
        /// </summary>
        ProviderType ProviderType { get; set; }

        /// <summary>
        /// Generic beginning of a process
        /// </summary>
        /// <param name="orgConfig"></param>
        /// <param name="orgResult"></param>
        /// <returns></returns>
        /// <remarks>The separation of start and end enables controlling the order of multiple <see cref="ISyncStore"/> concretions</remarks>
        Task<Guid> StartSync(OrgSyncConfig orgConfig, OrgSyncResult orgResult);

        /// <summary>
        /// Generic ending of a process; to be called only after <see cref="StartSync(OrgSyncConfig, OrgSyncResult)"/>
        /// </summary>
        /// <returns></returns>
        Task EndSync();

        /// <summary>
        /// Sets the number of seconds to allow for the command to execute before timing out
        /// </summary>
        /// <param name="seconds">Number of seconds to allow for the command to execute before timing out.</param>
        Task SetCommandTimeout(int seconds);
    }
}
