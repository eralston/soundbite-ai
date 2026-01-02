using Masticore.Models;
using Masticore.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masticore.Entity
{
    /// <summary>
    /// Class for tracking individuals who can log into the system
    /// Users exist in the Region, not within a particular tenant
    /// </summary>
    public class UserEntity : ResourceEntityBase, IInviteLifecycle, IUserFields
    {
        /// <summary>
        /// Property for tracking a cross-system unique identifer
        /// This should be big enough to track:
        /// - User IDs in AAD
        /// - Tenant IDs in AAD
        /// - Directory IDs in Okta
        /// - User IDs in Okta
        /// - Container names in Azure Storage
        /// - Partition and row keys in Azure Storage tables
        /// - Docuemnt ID in CosmosDB
        /// - etc
        /// It's based on a GUID w/ dashes (AAD IDs format)
        /// </summary>
        [Display(Name = "Universal ID")]
        [StringLength(36)]
        public string UniversalId { get; set; }

        /// <summary>
        /// Stores the refresh token associated with the user.
        /// </summary>
        [Display(Name = "Refresh Token")]
        [StringLength(500)]
        public string RefreshToken { get; set; }

        // IUserFields

        /// <summary>
        /// Email address for the user; must be unique
        /// </summary>
        /// <remarks>
        /// Constraint to be between 3 and 320 is from the spec, so enforcing it here
        /// </remarks>
        [Required]
        [EmailAddress]
        [StringLength(320, MinimumLength = 3)]
        [JsonProperty]
        public string Email { get => _email; set => _email = value.ToEmail(); }
        protected string _email;

        [Display(Name = "Given Name")]
        [JsonProperty]
        public string GivenName { get; set; }

        [Display(Name = "Family Name")]
        [JsonProperty]
        public string FamilyName { get; set; }

        [Display(Name = "Phone")]
        [JsonProperty]
        public string Phone { get; set; }

        [Display(Name = "Title")]
        [JsonProperty]
        public string Title { get; set; }

        /// <summary>
        /// Flag indicating if the user has opted out of email entirely
        /// </summary>
        [JsonProperty]
        public bool AllowEmail { get; set; }

        /// <summary>
        /// Flag indicating if the user has opted-in to training and announcements for the product
        /// </summary>
        [JsonProperty]
        public bool AllowNews { get; set; }

        /// <summary>
        /// Flag indicating if the user has enabled email as a channel for communication
        /// </summary>
        [JsonProperty]
        public bool AllowMarketing { get; set; }

        /// <summary>
        /// Flag indicatoed if the user has enabled sms as a channel for communication
        /// </summary>
        [JsonProperty]
        public bool AllowSms { get; set; }

        // IInvited

        [Display(Name = "Invite Date")]
        public DateTime? InviteUtc { get; set; }

        [Display(Name = "Invite Accept Date")]
        public DateTime? InviteAcceptUtc { get; set; }

        // Properties

        /// <summary>
        /// Gets or sets the application-level role of the user.
        /// </summary>
        public UserRole UserRole { get; set; }

        /// <summary>
        /// Gets or sets the provider used to authenticate the user.
        /// </summary>        
        public ProviderType ProviderType { get; set; }

        /// <summary>
        /// Gets or sets calendar settings for the user.  This is a JSON string value because
        /// different calendar providers may require slightly different settings.
        /// </summary>        
        public string CalendarSettings { get; set; }

        /// <summary>
        /// Gets or sets the general-purpose configuration JSON for the user; this allows for an arbitrary JSON object for metadata on the user
        /// </summary>        
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Column(TypeName = "ntext")]
        public string ConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the User Principal Name
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public string Upn { get; set; }

        // Relationships

        /// <summary>
        /// To many
        /// The User could be a part of many organizations!
        /// </summary>
        public ICollection<PersonEntity> People { get; set; }

        /// <summary>
        /// To many
        /// The user identities associated with this user
        /// </summary>
        public ICollection<UserIdentityEntity> UserIdentities { get; set; }


        /// <summary>
        /// Optional ID of a <see cref="UserEntity"/> that this entity was merged into
        /// </summary>
        public int? MergedToUserId { get; set; }

        /// <summary>
        /// Optional pointer to a <see cref="UserEntity"/> that this entity was merged into
        /// </summary>
        public virtual UserEntity MergedToUser { get; set; }
    }
}