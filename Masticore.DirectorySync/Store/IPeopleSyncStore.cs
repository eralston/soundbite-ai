using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{

    /// <summary>
    /// Adds and removes records that connect <see cref="IUserFieldsWithAliases"/> and related objects to the org
    /// </summary>
    public interface IPeopleSyncStore : ISyncStore
    {
        // Users
        Task AddPerson(IUserFieldsWithAliases userFields, SyncOpType opType);
        Task RemovePerson(string userEmail);
    }
}
