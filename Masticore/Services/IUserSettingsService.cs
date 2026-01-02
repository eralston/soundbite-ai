using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Interface for a User Settings Service
    /// </summary>
    public interface IUserSettingsService : IService
    {
        /// <summary>
        /// Gets user-specific configuration settings.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings object being retrieved.</typeparam>
        /// <param name="userRoute">Route of the user whose settings are being sought.</param>
        /// <returns>an instance of T populated with any applicable configuration settings.</returns>
        Task<T> ReadConfigAsync<T>(string userRoute)
            where T : IUserSettings, new();

        /// <summary>
        /// Saves the user settings for the specified user.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings object being saved.</typeparam>
        /// <param name="userRoute">Route of the user whose settings are being sought.</param>
        /// <param name="orgSettings">Settings</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        Task SaveConfigAsync<T>(string userRoute, T orgSettings)
            where T : IUserSettings, new();
    }
}