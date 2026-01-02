using Masticore.Ad;

namespace Masticore.DirectorySync.Aad
{
    /// <summary>
    /// Extensions for the config that ensures the default values can be used as a fallback
    /// These are extensions so the instances of the class above don't carry around a fallback value that is captured back into the database erroneously
    /// </summary>
    public static class AadSyncExtensions
    {
        /// <summary>
        /// Gets the AppId on the given config OR the Settings.DefaultAppId if it's missing
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string GetAppId(this AadOrgConfig config)
        {
            return string.IsNullOrEmpty(config.AppId) ? AdAppSettings.Instance.AppId : config.AppId;
        }

        /// <summary>
        /// Gets the SecretKey on the config OR the Settings.DefaultAppSecret if it's missing
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string GetAppSecret(this AadOrgConfig config)
        {
            return string.IsNullOrEmpty(config.SecretKey) ? AdAppSettings.Instance.AppSecret : config.SecretKey;
        }
    }
}