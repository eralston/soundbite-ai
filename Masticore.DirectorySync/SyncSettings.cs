namespace Masticore.DirectorySync
{
    /// <summary>
    /// Stores configuration settings for the Azure functions.
    /// </summary>
    public static class SyncSettings
    {
        /// <summary>
        /// Checks if the settings object is loaded
        /// </summary>
        public static bool IsLoaded => ConnectionString != null;

        /// <summary>
        /// Gets / sets the connection string to the Soundbite database.
        /// </summary>
        public static string ConnectionString { get; set; }
    }
}