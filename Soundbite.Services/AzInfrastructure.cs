using Masticore.DirectorySync;
using Masticore.Entity;
using Masticore.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Api
{
    /// <summary>
    /// Implements <see cref="ISbInfrastructure"/> and <see cref="ISyncInfrastucture"/> over Entity Framework
    /// </summary>
    public class AzInfrastructure : ISbInfrastructure, ISyncInfrastucture
    {
        /// <summary>
        /// <see cref="ILoggerFactory"/> for all <see cref="DbContext"/> instances emitted from this object
        /// </summary>
        protected static ILoggerFactory LogFactory { get; } = LogUtils.CreateFactory();

        /// <summary>
        /// Configures using connection strings from the given configuration
        /// </summary>
        /// <param name="configuration"></param>
        public static void Configure(IConfiguration configuration)
        {
            // Should match what's in AppSettings.json or Azure App AppSettings
            string db = configuration.GetConnectionString("SbDb");
            string stor = configuration.GetConnectionString("SbStorage");
            Configure(db, stor);
        }

        /// <summary>
        /// Configures using the given database and storage connection string
        /// </summary>
        /// <param name="dbString"></param>
        /// <param name="storString"></param>
        public static void Configure(string dbString, string storString = null)
        {
            SbDbConnectionString = dbString ?? throw new ArgumentNullException(nameof(dbString));
            StorageConnectionString = storString;
        }

        /// <summary>
        /// Confirms that we have connection strings loaded
        /// </summary>
        public static bool HasConnection => !string.IsNullOrEmpty(StorageConnectionString) && !string.IsNullOrEmpty(SbDbConnectionString);

        /// <summary>
        /// Storage connection string
        /// </summary>
        public static string StorageConnectionString { get; set; }
        /// <summary>
        /// Db Connection string
        /// </summary>
        public static string SbDbConnectionString { get; set; }

        private SbDb _sbDb;

        /// <summary>
        /// Gets the <see cref="SbDb"/> instance
        /// </summary>
        protected SbDb Db
        {
            get
            {
                if (_sbDb == null)
                {
                    if (string.IsNullOrEmpty(SbDbConnectionString))
                    {
                        throw new Exception("Cannot initialize database without connection string");
                    }
                    string dbString = SbDbConnectionString;
                    SqlConnectionStringBuilder connSringBuilder = new SqlConnectionStringBuilder(dbString)
                    {
                        ConnectTimeout = 60
                    };
                    Logger.LogInformation($"Connecting to SQL server {connSringBuilder.DataSource}");
                    DbContextOptionsBuilder<SbDb> builder = new DbContextOptionsBuilder<SbDb>();
                    builder.UseSqlServer(connSringBuilder.ConnectionString);
                    builder.UseLoggerFactory(LogFactory);
                    _sbDb = new SbDb(builder.Options);
                }

                return _sbDb;
            }
        }

        private IBlobs _blobs;

        /// <summary>
        /// Gets the <see cref="ILogger"/> instance for this object
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Constructor for <see cref="AzInfrastructure"/>
        /// </summary>
        /// <param name="logger"></param>
        public AzInfrastructure(ILogger<AzInfrastructure> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a database client
        /// </summary>
        /// <returns></returns>
        public Task<SbDb> DbAsync()
        {
            return Task.FromResult(Db);
        }

        /// <summary>
        /// Creates a file client
        /// </summary>
        /// <returns></returns>
        public Task<IBlobs> BlobsAsync()
        {
            if (_blobs == null)
            {
                _blobs = new AzBlobs(StorageConnectionString, Logger);
            }

            return Task.FromResult(_blobs);
        }

        /// <summary>
        /// Call to dispose of inner context and reset the builder
        /// </summary>
        public void Dispose()
        {
            if (_sbDb != null)
            {
                _sbDb.Dispose();
                _sbDb = null;
            }
        }

        /// <summary>
        /// Async gets the <see cref="IIdentityDb"/> instance for Azure
        /// </summary>
        /// <returns></returns>
        public Task<IIdentityDb> IdentityDbAsync()
        {
            return Task.FromResult(Db as IIdentityDb);
        }

        /// <summary>
        /// Async gets the <see cref="ISyncDb"/> implementation for Azure
        /// </summary>
        /// <returns></returns>
        public Task<ISyncDb> SyncDbAsync()
        {
            return Task.FromResult(Db as ISyncDb);
        }
    }
}
