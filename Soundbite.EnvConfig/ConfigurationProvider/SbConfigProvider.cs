using Masticore;
using Masticore.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Soundbite.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Soundbite.EnvConfig
{
    public class SbConfigProvider : ConfigurationProvider
    {
        #region Fields

        private readonly string _dbCnxString;
        private readonly SbEnvConfigOptions _options;
        private readonly IConfigurationBuilder _configBuilder;
        private IConfigurationRoot _configRoot;

        #endregion

        #region Properties

        protected IConfigurationRoot ConfigRoot
        {
            get
            {
                _configRoot = _configRoot ?? _configBuilder.Build();
                return _configRoot;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SbConfigProvider"/> instance.
        /// </summary>
        /// <param name="dbConnectionString">Database connection string for the Soundbite database.</param>
        /// <param name="groupKeys">Array of group keys to load.</param>
        public SbConfigProvider(IConfigurationBuilder configBuilder, SbEnvConfigOptions options, string dbCnxString)
        {
            _configBuilder = configBuilder;
            _options = options;
            _dbCnxString = dbCnxString;
        }

        #endregion

        #region Overrides

        /// <inheritdoc />
        public override void Load()
        {
            // Ensure the Data property is populated with a valid but empty dictionary
            Data = new Dictionary<string, string>();

            // Load database settings if required
            if (_options.EnableEnvSettings)
            {
                LoadEnvSettingsFromSbDb();
            }
        }

        #endregion

        #region Methods

        private void LoadEnvSettingsFromSbDb()
        {
            // At this point in time we do not have access to dependency injection so we manually
            // create a connection to the soundbite database to retrieve settings.
            DbContextOptionsBuilder<SbDb> builder = new DbContextOptionsBuilder<SbDb>();
            builder.UseSqlServer(_dbCnxString);
            SbDb db = new SbDb(builder.Options);

            //TODO: technically this could result in multiple DB calls if we end up using the
            //  groupKeys to really organize things but for now it will probably only be
            //  one call in practice.

            // Load defaults if requested and they are not part of the requested group keys
            if (_options.LoadEnvSettingsDefaults && !_options.EnvSettingsGroupKeys?.Any(i => string.IsNullOrEmpty(i)) == true)
            {
                IList<EnvSetting> settings = db.ReadEnvSettingsByGroupKey(null).Result;
                settings.ToList().ForEach(setting =>
                {
                    Data[setting.Name] = setting.Value;
                });
            }

            // Iterate over each groupkey and retrieve settings for that group key. This has the
            // effect of overwriting keys from one group to another so the list of group keys should
            // be ordered from the least important to the most important
            _options.EnvSettingsGroupKeys?.ToList().ForEach(groupKey =>
            {
                IList<EnvSetting> settings = db.ReadEnvSettingsByGroupKey(groupKey).Result;
                settings.ToList().ForEach(setting =>
                {
                    Data[setting.Name] = setting.Value;
                });
            });

            // Add any settings from the options to the data as well
            _options?.Settings?.ToList().ForEach(setting =>
            {
                Data[setting.Key] = setting.Value;
            });
        }

        #endregion
    }
}