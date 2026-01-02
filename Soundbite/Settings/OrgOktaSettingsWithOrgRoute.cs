using Masticore;

namespace Soundbite
{
    /***********************************************************************************************
     * WARNING: Organization settings are sent client-side.  Any secure data must be marked with
     *  JsonIgnore attribute to avoid being sent out and compromising security.
     **********************************************************************************************/

    /// <summary>
    /// Defines azure-specific settings available at an organization level.
    /// </summary>
    [CodeGenModel]
    public class OrgOktaSettingsWithOrgRoute : OrgOktaSettings
    {
        /// <summary>
        /// Gets or sets the organization route associated with the OKTA settings.
        /// </summary>
        public string OrgRoute { get; set; }
    }
}