using Masticore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Object that implements region-level directory sync persistence
    /// </summary>
    public interface IRegionSyncStore
    {
        /// <summary>
        /// Retrieves the list of activated <see cref="OrgSyncConfig"/> objects.
        /// </summary>
        /// <remarks>Each config should still be checked to ensure it's a valid object ready to run since this does NOT validate them</remarks>
        /// <returns></returns>
        Task<IEnumerable<OrgSyncConfig>> GetOrgsToSync();
    }
}
