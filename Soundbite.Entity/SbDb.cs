
using Masticore;
using Masticore.DirectorySync;
using Masticore.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Soundbite.Entity
{
    /// <summary>
    /// Soundbite data over EF Core, including <see cref="IIdentityDb"/> and <see cref="ISyncDb"/> objects from Masticore
    /// </summary>
    public class SbDb : DbContext, IIdentityDb, ISyncDb
    {
        // IIdentityDb

        public DbSet<EnvSetting> EnvSettings { get; set; }
        public DbSet<TenantEntity> Tenants { get; set; }
        public DbSet<TokenSettingsEntity> TokenSettings { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserIdentityEntity> UserIdentities { get; set; }
        public DbSet<OrganizationEntity> Organizations { get; set; }
        public DbSet<OrganizationAuthProviderEntity> OrganizationAuthProviders { get; set; }
        public DbSet<PersonEntity> People { get; set; }
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<MemberEntity> Members { get; set; }
        public DbSet<QueuedJobStatusEntity> QueuedJobStatuses { get; set; }

        // ISyncDb

        public DbSet<SyncRunEntity> SyncRuns { get; set; }

        // SbDb

        public DbSet<ClipEntity> Clips { get; set; }
        public DbSet<ClipEventEntity> ClipEvents { get; set; }
        public DbSet<ClipOperationEntity> ClipOperations { get; set; }
        public DbSet<PromptEntity> Prompts { get; set; }
        public DbSet<SessionEntity> Sessions { get; set; }
        public DbSet<SessionCommentEntity> SessionComments { get; set; }
        public DbSet<SessionNotificationEntity> SessionNotifications { get; set; }
        public DbSet<ParticipantGroupEntity> ParticipantGroups { get; set; }
        public DbSet<ParticipantGroupMemberEntity> ParticipantGroupMembers { get; set; }
        public DbSet<ParticipantEntity> Participants { get; set; }
        public DbSet<SeriesEntity> Series { get; set; }

        // Methods

        public SbDb(DbContextOptions<SbDb> options)
            : base(options)
        {
        }

        /// <summary>
        /// Called as the model is generating in the database
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SessionEntity>()
                .Property(p => p.SessionSecurity)
                .HasConversion<int>();

            builder.Entity<ParticipantGroupMemberEntity>(e =>
            {
                e.HasKey(i => new { i.ParticipantId, i.ParticipantGroupId });
            });

            IdentityDb.OnModelCreatingIdentityDb(builder);
            AddConstraints(builder);
            AddRelationships(builder);

            Seed(builder);
        }

        /// <summary>
        /// In the given builder, disable cascade delete for some relationships
        /// This is primarily for one-to-many relationships with a backlink to the parent
        /// </summary>
        /// <param name="builder"></param>
        private static void AddRelationships(ModelBuilder builder)
        {
            // https://www.yogihosting.com/fluent-api-one-to-many-relationship-entity-framework-core/

            // Collaboration

            builder.Entity<SessionEntity>()
                .HasOne(session => session.Series)
                .WithMany(series => series.Sessions)
                .HasForeignKey(session => session.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SeriesEntity>()
                .HasOne(series => series.Template)
                .WithMany(session => session.TemplateSeries)
                .HasForeignKey(session => session.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ParticipantGroupEntity>()
                .HasOne(grp => grp.Session)
                .WithMany(session => session.Groups)
                .HasForeignKey(grp => grp.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        /// <summary>
        /// Called during Model Creation to apply constraints to the model
        /// </summary>
        /// <param name="modelBuilder"></param>
        private static void AddConstraints(ModelBuilder modelBuilder)
        {
            // HasIndex + Unique => Unique constraint where the assignment can be changed
            // HasAlternateKey => Unique constaint where the assignment can NEVER change

            // Sync
            modelBuilder.Entity<SyncRunEntity>()
                        .HasIndex(u => u.UniversalId)
                        .IsUnique();
            modelBuilder.Entity<SyncRunEntity>()
                        .HasAlternateKey(o => o.Route);

            // Collaboration

            modelBuilder.Entity<ClipEntity>()
                        .HasAlternateKey(e => e.Route);

            modelBuilder.Entity<ClipOperationEntity>(i =>
            {
                i.HasIndex(e => e.ExternalId);
                i.HasAlternateKey(e => e.Route);
            });

            modelBuilder.Entity<PromptEntity>()
                        .HasAlternateKey(e => e.Route);

            modelBuilder.Entity<SessionEntity>()
                        .HasAlternateKey(e => e.Route);

            modelBuilder.Entity<ParticipantGroupEntity>()
                        .HasAlternateKey(e => e.Route);

            modelBuilder.Entity<ParticipantEntity>()
                        .HasAlternateKey(e => e.Route);

            modelBuilder.Entity<SeriesEntity>()
                        .HasAlternateKey(e => e.Route);
        }

        /// <summary>
        /// Override as needed in child classes for mocking
        /// WARNING: For SQL, this is evaluated during add-migration, NOT during update-database
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void Seed(ModelBuilder builder)
        {
            // Currently no seed data needed
        }

        /// <inheritdoc />
        public void SetCommandTimeout(int seconds)
        {
            Database.SetCommandTimeout(seconds);
        }
    }

    /// <summary>
    /// Invoked by console actions like "update-database" to read settings from appsettings.json
    /// </summary>
    public class SbDbDesignFactory : IDesignTimeDbContextFactory<SbDb>
    {
        private const string SettingsFilename = "appsettings.development.json";
        private const string ConnectionStringName = "SbDb";

        public SbDb CreateDbContext(string[] args)
        {
            // Read the default connection string
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile(SettingsFilename, optional: false, reloadOnChange: true)
                .Build();

            DbContextOptionsBuilder<SbDb> builder = new DbContextOptionsBuilder<SbDb>();

            // Add connection in SQL mode
            string connectionString = configuration.GetConnectionString(ConnectionStringName);
            builder.UseSqlServer(connectionString);

            // Turns on valuable debugging logs
            builder.EnableSensitiveDataLogging();

            // Send along a new SbDb with the options from above
            return new SbDb(builder.Options);
        }
    }
}
