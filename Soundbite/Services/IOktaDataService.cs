using Masticore.Models;
using Masticore.Services;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the service operations for managing OKTA data.
    /// </summary>
    public interface IOktaDataService : IService
    {
        /// <summary>
        /// Responsible for retrieving the OKTA settings for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose OKTA settings are being sought.</param>
        /// <returns>a reference to the organization's OKTA settings if found otherwise <c>null</c>.</returns>
        Task<OrgOktaSettingsWithOrgRoute> GetOktaSettingsByOrgRoute(string orgRoute);

        /// <summary>
        /// Responsible for determining the appropriate OKTA settings for the user with the 
        /// specified email address.  OKTA settings are defined at an organization level so
        /// this method is responsible for determining which organization settings should be 
        /// retrieved in the event the user is in multiple organizations.
        /// </summary>
        /// <param name="email">Email address associated with the user whose OKTA settings are being sought.</param>
        /// <returns>an <see cref="OrgOktaSettings"/> instance populated with data if found, otherwise <c>null</c>.</returns>
        Task<OrgOktaSettingsWithOrgRoute> GetOktaSettingsByEmail(string email);

        //TODO: Refactor stuff so we do not need this in the future
        Task REFACTOR_SetRequiredInfoOnUser(User user, string universalId);


    }
}