using Microsoft.EntityFrameworkCore;
using System;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// DbContext implementing identity in the system (Users, Orgs, and Teams)
    /// </summary>
    public class MockIdentityDb : IdentityDb
    {
        private bool IsDisposed = false;

        /// <summary>
        /// Constructor that uses DbContextOptions
        /// </summary>
        /// <param name="options"></param>
        public MockIdentityDb(DbContextOptions<IdentityDb> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDb).Assembly);
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
        /// Seeds the database with a set of real data
        /// </summary>
        /// <param name="builder"></param>
        protected override void Seed(ModelBuilder builder)
        {
            IdentityDbSeed seeder = new IdentityDbSeed();
            seeder.Seed(builder);
        }
    }
}
