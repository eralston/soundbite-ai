using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Storage;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Tests.Mocks
{
    /// <summary>
    /// In-memory EF implementation of <see cref="ISyncInfrastucture"/> with some supporting transparency for testing
    /// </summary>
    public class MockSyncInfrastructure : ISyncInfrastucture, IDbInfrastructure<MockSyncDb>
    {
        protected object _lock = new object();
        protected bool _isDisposed = false;
        public MockSyncDb Db { get; protected set; }
        private void EnsureDb()
        {
            lock (_lock)
            {
                if (Db != null)
                {
                    return;
                }

                DbContextOptionsBuilder<IdentityDb> builder = DbExtensions.InMemoryBuilder<IdentityDb>();
                Db = new MockSyncDb(builder.Options);
                bool isCreated = Db.Database.EnsureCreated();
                DbSnapshot = new DbSnapshot(Db);
                Snap();
            }
        }

        public DbSnapshot DbSnapshot { get; protected set; }

        public MockSyncInfrastructure()
        {
            EnsureDb();
        }

        /// <summary>
        /// Reset the tracking numbers for the <see cref="DbSnapshot"/> property
        /// </summary>
        /// <returns></returns>
        public DbSnapshot Snap()
        {
            DbSnapshot
                .Snap<OrganizationEntity>()
                .Snap<PersonEntity>()
                .Snap<UserEntity>()
                .Snap<GroupEntity>()
                .Snap<MemberEntity>()
                .Snap<SyncRunEntity>();
            return DbSnapshot;
        }

        /// <summary>
        /// Checks the add numbers for the given entity types
        /// </summary>
        /// <param name="user"></param>
        /// <param name="person"></param>
        /// <param name="group"></param>
        /// <param name="member"></param>
        /// <param name="org"></param>
        public void AssertAdd(int user = 0, int person = 0, int group = 0, int member = 0, int run = 0, int org = 0)
        {
            DbSnapshot.AssertAdd<UserEntity>(user);
            DbSnapshot.AssertAdd<PersonEntity>(person);
            DbSnapshot.AssertAdd<GroupEntity>(group);
            DbSnapshot.AssertAdd<MemberEntity>(member);
            DbSnapshot.AssertAdd<SyncRunEntity>(run);
            DbSnapshot.AssertAdd<OrganizationEntity>(org);
        }

        public void Dispose()
        {
            // Do nothing
        }

        public Task<ISyncDb> SyncDbAsync()
        {
            EnsureDb();
            return Task.FromResult(Db as ISyncDb);
        }

        public Task<IIdentityDb> IdentityDbAsync()
        {
            EnsureDb();
            return Task.FromResult(Db as IIdentityDb);
        }

        public Task<IBlobs> BlobsAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<MockSyncDb> DbAsync()
        {
            return Task.FromResult(Db);
        }
    }
}
