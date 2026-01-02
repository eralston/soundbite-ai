using Masticore;
using Masticore.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Soundbite.Entity
{
    /// <summary>
    /// One instance of collaboration between Participants within an Organization
    /// </summary>
    public class SessionEntity : ResourceEntityBase
    {
        // ISessionFields

        /// <summary>
        /// The title of the session
        /// </summary>
        /// <remarks>Minimum 3 character, maximum 128</remarks>
        [Required]
        [StringLength(128, MinimumLength = 3)]
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// The maximum number of seconds <see cref="ClipEntity"/> records can record in this session
        /// </summary>
        /// <remarks>
        /// Zero indicates not limit
        /// </remarks>
        [Range(0, int.MaxValue)]
        [JsonProperty]
        public int Limit { get; set; }

        /// <summary>
        /// The workflow associated with this session
        /// </summary>
        [JsonProperty]
        public SessionType SessionType { get; set; }

        /// <summary>
        /// Gets or sets the type of security associated with the session.
        /// </summary>
        [JsonProperty]
        public SessionSecurityType SessionSecurity { get; set; }

        /// <summary>
        /// Gets or sets the policy on comments for the session
        /// </summary>
        [JsonProperty]
        public SessionCommentPolicy SessionCommentPolicy { get; set; }

        /// <summary>
        /// Gets or sets the reminder time for this session
        /// If this is <c>null</c>, then a reminder will NEVER be sent out
        /// If this has a value, then a reminder will be send at this time regardless of the current session state
        /// </summary>
        [JsonProperty]
        public DateTime? Reminder { get; set; }

        /// <summary>
        /// Gets or sets the publish time for this session
        /// If this is <c>null</c>, then the deadline is "immediately" and the session should be published once the last clip makes it valid
        /// If this has a value, then the clip will be published at that time, but only once it is valid
        /// </summary>
        [JsonProperty]
        public DateTime? Publish { get; set; }

        [JsonProperty]
        public DateTime? ReminderSent { get; set; }
        [JsonProperty]
        public DateTime? PublishSent { get; set; }
        [JsonProperty]
        public bool IsTemplate { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether transcription is enabled at the session level.
        /// </summary>
        [JsonProperty]
        public bool Transcribe { get; set; }

        /// <summary>
        /// Gets or sets a provider-specified identifier that can be used by the provider to locate
        /// the calendar entry associated with the session.
        /// </summary>        
        /// <remarks>
        /// Maximum length 128
        /// </remarks>
        [JsonProperty]
        [StringLength(128)]
        [CodeGenField(IsNullable = true)]
        public string ReminderCalEventId { get; set; }

        // Relationships

        /// <summary>
        /// To one on Organization
        /// </summary>
        public int OrganizationId { get; set; }

        public virtual OrganizationEntity Organization { get; set; }

        /// <summary>
        /// To one on potential parent Series
        /// </summary>
        public int? SeriesId { get; set; }

        public virtual SeriesEntity Series { get; set; }

        /// <summary>
        /// To many on ParticipantGroup
        /// </summary>
        public ICollection<ParticipantGroupEntity> Groups { get; set; }

        /// <summary>
        /// To many on Participant
        /// </summary>
        public ICollection<ParticipantEntity> Participants { get; set; }

        /// <summary>
        /// To many on series where this is the template
        /// </summary>
        public ICollection<SeriesEntity> TemplateSeries { get; set; }

        /// <summary>
        /// To many on prompt where this is its session
        /// </summary>
        public ICollection<PromptEntity> Prompts { get; set; }

        /// <summary>
        /// To many on comments for this session
        /// </summary>
        public ICollection<SessionCommentEntity> Comments { get; set; }
    }
}
