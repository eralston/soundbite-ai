using Masticore;
using Masticore.Services;
using System.Threading.Tasks;

namespace Soundbite.Extensions
{
    /// <summary>
    /// Extension methods for sessions.
    /// </summary>
    public static class IUserServiceExtensions
    {
        /// <summary>
        /// Responsible for eautomatically nabling teams functionality.  This method should be
        /// called when users perform teams-specific actions that indicate they have the Soundbite
        /// Teams application installed.
        /// </summary>
        /// <param name="userSettingsService">Reference to a User Settings service.</param>
        /// <param name="userRoute">Route of the user who has performed a teams-specific action.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        public static async Task AutoEnableTeams(this IUserSettingsService userSettingsService, string userRoute)
        {
            UserSettings userSettings = await userSettingsService.ReadConfigAsync<UserSettings>(userRoute);
            if (userSettings.MsTeams.IsEnabled == AutoFlagStateType.AutoDisabled)
            {
                userSettings.MsTeams.IsEnabled = AutoFlagStateType.AutoEnabled;
                await userSettingsService.SaveConfigAsync(userRoute, userSettings);
            }
        }

        /// <summary>
        /// Responsible for eautomatically nabling teams functionality.  This method should be
        /// called when users perform teams-specific actions that indicate they have the Soundbite
        /// Teams application installed.
        /// </summary>
        /// <param name="userSettingsService">Reference to a User Settings service.</param>
        /// <param name="userRoute">Route of the user who has performed a teams-specific action.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        public static async Task AutoDisableTeams(this IUserSettingsService userSettingsService, string userRoute)
        {
            UserSettings userSettings = await userSettingsService.ReadConfigAsync<UserSettings>(userRoute);
            userSettings.MsTeams.IsEnabled = AutoFlagStateType.AutoDisabled;
            await userSettingsService.SaveConfigAsync(userRoute, userSettings);

        }
    }
}