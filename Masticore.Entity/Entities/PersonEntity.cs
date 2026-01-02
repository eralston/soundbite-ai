using Masticore.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// Connects a User to an Organization
    /// </summary>
    public class PersonEntity : ResourceEntityBase, IInviteLifecycle
    {
        // IPersonFields

        /// <summary>
        /// Gets or sets the organization-level role of the user.
        /// </summary>
        [JsonProperty] //TODO: figure out if we need this -- I don't think we do
        public PersonRole PersonRole { get; set; }

        // IInvited

        [Display(Name = "Invite Date")]
        public DateTime? InviteUtc { get; set; }

        [Display(Name = "Invite Accept Date")]
        public DateTime? InviteAcceptUtc { get; set; }

        // Relationships

        // To-One
        public int UserId { get; set; }
        public virtual UserEntity User { get; set; }

        // To-One
        public int OrganizationId { get; set; }
        public virtual OrganizationEntity Organization { get; set; }

        // To-Many
        public ICollection<MemberEntity> Members { get; set; }
    }
}
