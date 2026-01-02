using Microsoft.EntityFrameworkCore;
using System;

namespace Masticore.Entity
{
    /// <summary>
    /// DbContext implementing identity in the system (Users, Orgs, and Teams)
    /// </summary>
    public class IdentityDb : DbContext, IIdentityDb
    {
        public DbSet<MemberEntity> Members { get; set; }
        public DbSet<OrganizationEntity> Organizations { get; set; }
        public DbSet<OrganizationAuthProviderEntity> OrganizationAuthProviders { get; set; }
        public DbSet<PersonEntity> People { get; set; }
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<TenantEntity> Tenants { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserIdentityEntity> UserIdentities { get; set; }
        public DbSet<QueuedJobStatusEntity> QueuedJobStatuses { get; set; }
        public DbSet<TokenSettingsEntity> TokenSettings { get; set; }
        public DbSet<EnvSetting> EnvSettings { get; set; }

        private bool IsDisposed = false;

        public IdentityDb(DbContextOptions<IdentityDb> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            OnModelCreatingIdentityDb(builder);
            Seed(builder);
        }

        protected virtual void Seed(ModelBuilder builder)
        {
        }

        public static void OnModelCreatingIdentityDb(ModelBuilder builder)
        {
            AddConstraints(builder);
            AddRelationships(builder);
            AddEntities(builder);
        }

        /// <summary>
        /// Called during Model Creation to apply constraints to the model
        /// </summary>
        /// <param name="modelBuilder"></param>
        private static void AddConstraints(ModelBuilder modelBuilder)
        {
            // HasIndex + Unique => Unique constraint where the assignment can be changed
            // HasAlternateKey => Unique constaint where the assignment can NEVER change

            // Identity
            modelBuilder.Entity<TenantEntity>()
                        .HasAlternateKey(t => t.Route);
            modelBuilder.Entity<UserEntity>()
                        .HasAlternateKey(t => t.Route);
            modelBuilder.Entity<UserEntity>()
                        .HasIndex(u => u.Email)
                        .IsUnique();
            modelBuilder.Entity<UserEntity>()
                        .HasIndex(u => u.UniversalId)
                        .IsUnique();

            modelBuilder.Entity<UserIdentityEntity>()
                        .HasIndex(u => new { u.Identifier, u.ProviderType })
                        .IsUnique();

            modelBuilder.Entity<OrganizationEntity>()
                        .HasAlternateKey(o => o.Route);
            modelBuilder.Entity<OrganizationEntity>()
                        .HasIndex(u => u.UniversalId)
                        .IsUnique();

            modelBuilder.Entity<PersonEntity>()
                        .HasAlternateKey(p => p.Route);
            modelBuilder.Entity<PersonEntity>()
                        .HasIndex(p => new { p.UserId, p.OrganizationId })
                        .IsUnique();

            modelBuilder.Entity<GroupEntity>()
                        .HasAlternateKey(t => t.Route);

            modelBuilder.Entity<MemberEntity>()
                        .HasAlternateKey(t => t.Route);
            modelBuilder.Entity<MemberEntity>()
                        .HasAlternateKey(m => new { m.PersonId, m.GroupId });
        }

        /// <summary>
        /// In the given builder, disable cascade delete for some relationships
        /// This is primarily for one-to-many relationships with a backlink to the parent
        /// </summary>
        /// <param name="builder"></param>
        private static void AddRelationships(ModelBuilder builder)
        {
            // https://www.yogihosting.com/fluent-api-one-to-many-relationship-entity-framework-core/

            // Identity

            // UserEntity has two self-referential relationships, so they must be explicit
            builder.Entity<UserEntity>()
                    .HasOne(user => user.MergedToUser)
                    .WithMany()
                    .HasForeignKey(user => user.MergedToUserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserEntity>()
                    .HasOne(user => user.CreatedBy)
                    .WithMany()
                    .HasForeignKey(user => user.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserEntity>()
                    .HasMany(user => user.UserIdentities)
                    .WithOne(identity => identity.User);

            // PersonEntity needs help understanding its one-to-many on Org and User
            builder.Entity<PersonEntity>()
                    .HasOne(person => person.Organization)
                    .WithMany(org => org.People)
                    .Metadata.DeleteBehavior = DeleteBehavior.Restrict;

            builder.Entity<PersonEntity>()
                    .HasOne(person => person.User)
                    .WithMany(user => user.People)
                    .Metadata.DeleteBehavior = DeleteBehavior.Restrict;
        }

        /// <summary>
        /// Responsible for setting up the EnvSettings entity.
        /// </summary>
        /// <param name="builder"></param>
        private static void AddEntities(ModelBuilder builder)
        {
            // EnvSettings
            builder.Entity<EnvSetting>(i =>
            {
                i.HasKey(i => i.ID);
                i.HasIndex(i => i.GroupKey);
                i.Property(i => i.GroupKey).HasMaxLength(255);
                i.Property(i => i.Description).HasMaxLength(2000);
            });
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

            base.Dispose();
        }
    }
}
