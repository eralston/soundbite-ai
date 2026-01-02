using Masticore.Acs;
using Masticore.Entity;
using Soundbite.Entity;
using Soundbite.Services;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Factory for <see cref="AcsSettings"/> based on the <see cref="OrgAzureSettings"/> object in the database attached to an <see cref="OrganizationEntity"/>
    /// </summary>
    internal class AcsSettingsFactoryForOrg : IAcsSettingFactory
    {
        ISbInfrastructure Infrastructure { get; }
        public AcsSettingsFactoryForOrg(ISbInfrastructure infrastructure)
        {
            Infrastructure = infrastructure;
        }

        /// <inheritdoc/>
        public async Task<AcsSettings> GetSettingsAsync(string orgRoute = null)
        {
            if (string.IsNullOrEmpty(orgRoute))
            {
                return null;
            }
            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            if (org == null)
            {
                return null;
            }

            OrgSettings settings = org.GetSettings();
            OrgAzureSettings azure = settings?.Azure;
            if (azure == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(azure.AcsConnectionString) || string.IsNullOrEmpty(azure.AcsFromEmail) || string.IsNullOrEmpty(azure.AcsFromName))
            {
                return null;
            }

            return new AcsSettings
            {
                ConnectionString = azure.AcsConnectionString,
                FromEmail = azure.AcsFromEmail,
                FromName = azure.AcsFromName
            };
        }
    }
}
