using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Adds and remove records that connect <see cref="IGroupFields"/> to the org
    /// </summary>
    public interface IGroupSyncStore : ISyncStore
    {
        // Groups
        Task AddGroup(IGroupFields groupFields, SyncOpType opType);
        Task RemoveGroup(string groupUniversalId);
    }
}
