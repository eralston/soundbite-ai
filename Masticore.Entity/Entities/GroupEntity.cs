using Masticore.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// Collection of People for scoping sessions
    /// </summary>
    public class GroupEntity : ResourceEntityBase, IGroupFields
    {
        #region IGroupFields

        /// <summary>
        /// Gets or sets a value allowing for tracking a cross-system unique identifer
        /// </summary>
        public string UniversalId { get; set; }

        /// <summary>
        /// Gets or sets the name for this org
        /// </summary>
        [JsonProperty]
        [StringLength(128, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the descriptive text for this group
        /// </summary>
        [StringLength(Constants.Models.Groups.GroupNameMaxLength)]
        [JsonProperty]
        public string Description { get; set; }

        #endregion

        // Relationships

        // To-One
        public int OrganizationId { get; set; }
        public virtual OrganizationEntity Organization { get; set; }

        // To-Many
        public ICollection<MemberEntity> Members { get; set; }
    }
}
