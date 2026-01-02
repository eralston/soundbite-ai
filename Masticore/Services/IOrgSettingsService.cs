using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Interface for an Organization Settings Service.
    /// </summary>
    public interface IOrgSettingsService : IService
    {
        /// <summary>
        /// Saves the organization settings for the specified organization.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings object being saved.</typeparam>
        /// <param name="orgRoute">Route of the organization with which the settings are associated.</param>
        /// <param name="orgSettings">Organization settings object to persist.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        Task SaveOrgSettingsAsync<T>(string orgRoute, T orgSettings)
            where T : IOrgSettings, new();

        /// <summary>
        /// Retrieves the organization configuration settings.
        /// </summary>
        /// <typeparam name="T">.NET type of the settings to retrieve.</typeparam>
        /// <param name="orgRoute">Route of the organization whose settings are being sought.</param>        
        /// <returns>a settings object ready for use.  A default settings object is created if settings are not present.</returns>
        Task<T> ReadOrgSettingsAsync<T>(string orgRoute)
            where T : IOrgSettings, new();
    }
}