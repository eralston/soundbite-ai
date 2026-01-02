using Masticore.Entity;
using Masticore.Storage;
using Masticore.Tests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Soundbite.Services;
using System.Threading.Tasks;

namespace Soundbite.Entity.Tests
{
    /// <summary>
    /// Generates an in-memory version of SbDb
    /// </summary>
    public class MockSbDbInfrastructure : ISbInfrastructure
    {
        public MockSbDbInfrastructure()
        {
            EnsureDb();
        }

        public SbDb Db { get; set; }
        public MockBlobs Blobs { get; set; } = new MockBlobs();
        public Task<IBlobs> BlobsAsync()
        {
            return Task.FromResult(Blobs as IBlobs);
        }

        public Task<SbDb> DbAsync()
        {
            return Task.FromResult(Db);
        }

        private void EnsureDb()
        {
            if (Db == null)
            {
                // Create an in-memory connection
                SqliteConnection connection = new SqliteConnection("Filename=:memory:");
                connection.Open();

                // Load up the builder and create the database with it
                DbContextOptionsBuilder<SbDb> builder = new DbContextOptionsBuilder<SbDb>().UseSqlite(connection);
                Db = new MockSbDb(builder.Options);
                Db.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            Db?.Dispose();
        }

        public Task<IIdentityDb> IdentityDbAsync()
        {
            return Task.FromResult(Db as IIdentityDb);
        }
    }
}
