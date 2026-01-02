using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Store that persists <see cref="IUserFields"/>,  <see cref="IGroupFields"/>, plus connecting member records to the system
    /// </summary>
    public interface IOrgSyncStore : IGroupSyncStore, IPeopleSyncStore, IMemberSyncStore
    {
        void SetProviderType(ProviderType providerType);

        Task SaveStrategyConfig<T>(T orgConfig);
    }
}
