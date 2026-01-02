using Masticore.Entity;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// An <see cref="IInfrastructure"/> object that acts as a factory for <see cref="ISyncDb"/>
    /// </summary>
    public interface ISyncInfrastucture : IIdentityInfrastructure
    {
        /// <summary>
        /// Async get the <see cref="ISyncDb"/>
        /// </summary>
        /// <returns></returns>
        Task<ISyncDb> SyncDbAsync();
    }
}
