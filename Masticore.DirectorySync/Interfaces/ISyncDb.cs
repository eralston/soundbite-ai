using Masticore.Entity;
using Microsoft.EntityFrameworkCore;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Interface for an EF database that will hold Sync data, such as <see cref="SyncRunEntity"/>
    /// </summary>
    /// <remarks>
    /// This must include the <see cref="IIdentityDb"/> implementation since it hinges on syncing users, groups, and members
    /// </remarks>
    public interface ISyncDb : IIdentityDb
    {
        DbSet<SyncRunEntity> SyncRuns { get; set; }

        /// <summary>
        /// Sets the number of seconds to allow for the command to execute before timing out
        /// </summary>
        /// <param name="seconds">Number of seconds to allow for the command to execute before timing out.</param>
        void SetCommandTimeout(int seconds);
    }
}
