namespace Masticore.Models
{
    /// <summary>
    /// Represents a partition in which a company defines users, groups, and sessions.
    /// </summary>
    /// <seealso cref="Masticore.Models.Organization" />    
    public class OrganizationExtended : Organization
    {
        /// <summary>
        /// Gets a flag indicating whether there are outstanding invitations for the organization.        
        /// </summary>
        public bool IsAccepting { get; set; }

        /// <summary>
        /// Gets a URL that points to the "avatar" image associated with the organization.  The URL
        /// also contains a shorted lived token required to acccess the image.  This value should
        /// not be persisted in third-party systems.
        /// </summary>        
        public string ImageSrc { get; set; }
    }
}
