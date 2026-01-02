using AutoMapper;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// User Settings Service implementation for the Soundbite Database.
    /// </summary>
    public class UserSettingsService : IdentityServiceBase, IUserSettingsService
    {
        #region Constructor

        public UserSettingsService(
            ISecurityContext securityContext,
            IIdentityInfrastructure infrastructure,
            IMapper mapper,
            ILogger<UserSettingsService> logger)
            : base(infrastructure, logger, mapper, securityContext)
        {
        }

        #endregion

        #region IUserSettingsService Implementation

        /// <summary>
        /// Gets user-specific configuration settings.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings object being retrieved.</typeparam>
        /// <param name="userRoute">Route of the user whose settings are being sought.</param>
        /// <returns>an instance of T populated with any applicable configuration settings.</returns>
        public async Task<T> ReadConfigAsync<T>(string userRoute)
            where T : IUserSettings, new()
        {
            T result = default(T);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            string configJson = await db.Users
                .Where(i => i.Route == userRoute
                    && i.DeletedUtc == null)
                .Select(i => i.ConfigJson).FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(configJson))
            {
                result = JsonUtils.FromLowerCamelJson<T>(configJson);
            }
            else
            {
                // Create a default instance to return
                result = new T();
            }

            return result;
        }

        /// <summary>
        /// Saves the user settings for the specified user.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings object being saved.</typeparam>
        /// <param name="userRoute">Route of the user whose settings are being sought.</param>
        /// <param name="userSettings">Settings</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        public async Task SaveConfigAsync<T>(string userRoute, T userSettings)
            where T : IUserSettings, new()
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity user = await db.Users
                .Where(i => i.Route == userRoute
                    && i.DeletedUtc == null)
                .FirstOrDefaultAsync();
            user.AssertFound();
            user.SetUpdatedFields(SecurityContext);
            user.ConfigJson = JsonUtils.ToLowerCamelJson(userSettings);
            db.Users.Update(user);
            await db.SaveChangesAsync();
        }

        #endregion
    }
}