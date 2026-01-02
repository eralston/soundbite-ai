using Masticore.Storage;
using Masticore.Tests;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// Generates an in-memory version of IIdentityInfrastructure
    /// </summary>
    public class MockSeedInfrastructure : IDbInfrastructure<MockIdentityDb>
    {
        public MockIdentityDb Db { get; protected set; }

        public IBlobs Blobs { get; } = new MockBlobs();

        public MockSeedInfrastructure()
        {
            EnsureDb();
        }

        /// <summary>
        /// Async gets the IBlobs for this instance
        /// </summary>
        /// <returns></returns>
        public virtual Task<IBlobs> BlobsAsync()
        {
            EnsureDb();
            return Task.FromResult(Blobs);
        }

        /// <summary>
        /// Async gets the TDbContext
        /// </summary>
        /// <returns></returns>
        public Task<MockIdentityDb> DbAsync()
        {
            EnsureDb();
            return Task.FromResult(Db);
        }

        /// <summary>
        /// Disposes of the IIdentityDb
        /// </summary>
        public virtual void Dispose()
        {
            Db?.Dispose();
        }

        /// <summary>
        /// Async gets the IIdentityDb
        /// </summary>
        /// <returns></returns>
        public virtual Task<IIdentityDb> IdentityDbAsync()
        {
            EnsureDb();
            return Task.FromResult(Db as IIdentityDb);
        }

        /// <summary>
        /// Ensures the SeedIdentityDb exists
        /// </summary>
        private void EnsureDb()
        {
            if (Db == null)
            {
                // Create an in-memory connection
                DbContextOptionsBuilder<IdentityDb> builder = DbExtensions.InMemoryBuilder<IdentityDb>();
                Db = new MockIdentityDb(builder.Options);
                Db.Database.EnsureCreated();
            }
        }
    }
}
