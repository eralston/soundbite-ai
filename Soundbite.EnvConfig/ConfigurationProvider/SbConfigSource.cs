using Microsoft.Extensions.Configuration;

namespace Soundbite.EnvConfig
{
    public class SbConfigSource : IConfigurationSource
    {
        #region Fields

        private readonly string _dbCnxString;
        private readonly SbEnvConfigOptions _options;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SbConfigSource"/> instance.
        /// </summary>
        public SbConfigSource(SbEnvConfigOptions options, string dbCnxString)
        {
            _dbCnxString = dbCnxString;
            _options = options;
        }

        #endregion

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            // Determine whether this is the development
            return new SbConfigProvider(builder, _options, _dbCnxString);
        }
    }
}