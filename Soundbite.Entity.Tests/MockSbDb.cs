using Microsoft.EntityFrameworkCore;
using System;

namespace Soundbite.Entity.Tests
{
    /// <summary>
    /// Derivces from the given DbContext to provide an in-memory implementation
    /// </summary>
    public class MockSbDb : SbDb
    {
        private bool IsDisposed = false;

        public MockSbDb(DbContextOptions<SbDb> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SbDb).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override void Dispose()
        {
            // Removing dispose means we can look at this after it's used in the service
            // If it's called more than once, then it's a sign the unit test should fail
            if (IsDisposed)
            {
                throw new Exception("Disposed DbContext twice, something must be wrong");
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Will seed objects into the in-memory representation of the SbDb
        /// </summary>
        /// <param name="builder"></param>
        protected override void Seed(ModelBuilder builder)
        {
            SbDbSeed seeder = new SbDbSeed();
            seeder.Seed(builder);
        }
    }
}
