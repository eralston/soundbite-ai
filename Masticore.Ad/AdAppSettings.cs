namespace Masticore.Ad
{
    /// <summary>
    /// Stores configuration settings for a single Azure app; offering a singleton default instance
    /// </summary>
    public class AdAppSettings : IAdAppSettings
    {
        protected static readonly object settingsLock = new object();

        /// <summary>
        /// Initializes the AAD Sync Settings based on the given value
        /// </summary>
        /// <param name="settings"></param>
        public static void Init(AdAppSettings settings, bool throwIfSet = true)
        {
            lock (settingsLock)
            {
                if (settings is null)
                {
                    throw new System.ArgumentNullException(nameof(settings));
                }

                if (_instance != null && throwIfSet)
                {
                    throw new System.Exception($"{nameof(AdAppSettings)} already initialized");
                }

                _instance = settings;
            }
        }

        /// <summary>
        /// Gets true if already initialized; otherwise, false
        /// </summary>
        public static bool IsInitialized => _instance != null;

        public static AdAppSettings _instance;

        /// <summary>
        /// Singleton instance of <see cref="AdAppSettings"/>
        /// To initialize, call the <see cref="Init(AdAppSettings)"/> method
        /// </summary>
        public static AdAppSettings Instance
        {
            get
            {
                lock (settingsLock)
                {
                    if (_instance == null)
                    {
                        throw new System.Exception($"Must set singleton instance by calling {nameof(AdAppSettings)}.{nameof(AdAppSettings.Init)} before accessing");
                    }

                    return _instance;
                }
            }
            protected set => _instance = value;
        }

        /// <summary>
        /// Gets or sets the App ID
        /// </summary>
        /// <remarks>
        /// Some environments may require their own app in their own tenant, so this cannot be used for all configurations of AAD
        /// </remarks>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or Sets the App Secret, which is like a password for accessing
        /// TODO: Replace with certificate in PROD scenarios
        /// </summary>
        /// <remarks>
        /// Some environments may require their own app in their own tenant, so this cannot be used for all configurations of AAD
        /// </remarks>
        public string AppSecret { get; set; }
    }
}