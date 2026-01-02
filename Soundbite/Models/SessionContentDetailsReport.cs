using Masticore;
using Masticore.Models;
using System;
using System.Text.Json.Serialization;

namespace Soundbite.Models
{
    /// <summary>
    /// A specialized stub of user information associating a person with a time
    /// </summary>
    [CodeGenModel]
    public class SessionContentPersonEvent : IUserFields
    {
        /// <summary>
        /// Gets or sets the unique route for the user
        /// </summary>
        public string UserRoute { get; set; }

        /// <summary>
        /// Gets or sets the system-wide unique email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of their family (in western cultures, this is the last name)
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets their given name (in western cultures, this is the first name)
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets their primary phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets their job title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets their internal system ID; Will always be null
        /// </summary>
        public string UniversalId { get; set; }

        /// <summary>
        /// Gets or set the <see cref="DateTime"/> for the relevant event
        /// </summary>
        public DateTime DateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the User Principal Name of the user.
        /// </summary>
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [CodeGenField(Ignore = true)]
        public string Upn { get; set; }
    }

    /// <summary>
    /// Object describing a notification sent out for a session
    /// </summary>
    [CodeGenModel]
    public class SessionContentNotificiation : SessionContentPersonEvent
    {
        /// <summary>
        /// Get or set the <see cref="NotificationType"/>
        /// </summary>
        public SessionNotificationType NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the optional field for tracking what channel by which this event was initiated
        /// </summary>
        public NotificationChannel Channel { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle status for the notification
        /// </summary>
        public NotificationStatus Status { get; set; }

        /// <summary>
        /// A free-form text field for capturing the 
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string Details { get; set; }
    }


    /// <summary>
    /// Expansion of <see cref="SessionContentReport"/> with lists of listeners and acknowledgers
    /// </summary>
    [CodeGenModel]
    public class SessionContentDetailsReport : SessionContentReport
    {
        /// <summary>
        /// Gets or sets the list of people who were originally sent the session
        /// </summary>
        public Participant[] Audience { get; set; }

        /// <summary>
        /// Gets or sets the list of groups who were originally sent the session
        /// </summary>
        public ParticipantGroup[] AudienceGroups { get; set; }

        /// <summary>
        /// Gets or sets the list of people who have listened to this session
        /// </summary>

        public SessionContentPersonEvent[] Plays { get; set; }

        /// <summary>
        /// Gets or sets the list of people who have acknowledged this session
        /// </summary>
        public SessionContentPersonEvent[] Acknowledgers { get; set; }

        /// <summary>
        /// Gets or sets the list of notification send for this session
        /// </summary>
        public SessionContentNotificiation[] Notifications { get; set; }
    }
}