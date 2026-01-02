using Masticore.Resources;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// Associates a Person with a Team
    /// </summary>
    public class MemberEntity : ResourceEntityBase, IInviteLifecycle
    {
        // IMemberFields

        /// <summary>
        /// Gets or sets the group-level role of the user.
        /// </summary>
        [JsonProperty]  //TODO: figure out if we need this
        public MemberRole MemberRole { get; set; }

        // IInvited

        [Display(Name = "Invite Date")]
        public DateTime? InviteUtc { get; set; }

        [Display(Name = "Invite Accept Date")]
        public DateTime? InviteAcceptUtc { get; set; }

        // Relationships

        // To-One
        public int PersonId { get; set; }
        public virtual PersonEntity Person { get; set; }

        // To-One
        public int GroupId { get; set; }
        public virtual GroupEntity Group { get; set; }
    }
}
