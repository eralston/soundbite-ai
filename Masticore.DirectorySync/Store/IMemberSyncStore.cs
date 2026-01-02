using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Adds and removes records that connect <see cref="IUserFields"/> to <see cref="IGroupFields"/>
    /// </summary>
    public interface IMemberSyncStore : ISyncStore
    {
        // Members
        Task AddMember(IGroupFields group, IUserFieldsWithAliases user, MemberSyncOpType memberSyncType);
        Task RemoveMember(string groupUniversalId, string userEmail);
    }
}
