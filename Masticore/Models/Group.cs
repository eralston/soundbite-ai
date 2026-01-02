using Newtonsoft.Json;

namespace Masticore.Models
{
    /// <summary>
    /// Implements <see cref="IGroupFields"/>, representing an association of people within an organization.
    /// </summary>    
    public class Group : ResourceBase, IGroupFields
    {
        #region IGroupsFields

        /// <summary>
        /// Gets or sets the name of the team.
        /// </summary>        
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the team.
        /// </summary>        
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the universal ID for this group, which is a UUID
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string UniversalId { get; set; }

        #endregion

        /// <summary>
        /// Gets a flag indicating whether there are outstanding invitations for the group.        
        /// NOTE: this value is currently always false for a group.
        /// </summary>
        public bool IsAccepting { get; set; }

        /// <summary>
        /// Gets a URL that points to the "avatar" image associated with the group.  The URL also 
        /// contains a shorted lived token required to acccess the image.  This value should not 
        /// be persisted in third-party systems.
        /// </summary>        
        public string ImageSrc { get; set; }
    }
}