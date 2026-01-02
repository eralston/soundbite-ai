namespace Soundbite.Models
{
    /// <summary>
    /// Represents an organization with organization settings.
    /// </summary>
    public class OrganizationWithSettings : OrganizationWithPermissions
    {
        public OrgSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the organization has public Soundbites.         
        /// </summary>
        public bool HasPublicNotifications { get; set; }
    }
}