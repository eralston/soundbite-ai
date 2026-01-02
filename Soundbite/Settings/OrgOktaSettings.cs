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
    public class OrgOktaSettings
    {
        /// <summary>
        /// Gets or sets the issuer URL in the format https://{tenantOktaDomain}/oauth2/default
        /// </summary>
        public string IssuerUrl { get; set; }

        /// <summary>
        /// Gets or sets the client id associated with the Soundbite application entry.
        /// </summary>
        public string ClientId { get; set; }
    }
}