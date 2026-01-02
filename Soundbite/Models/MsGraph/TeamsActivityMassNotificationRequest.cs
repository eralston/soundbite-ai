using Newtonsoft.Json;
using System.Collections.Generic;

namespace Soundbite.Models
{
    /// <summary>
    /// Represents Microsoft Teams activity notifiation that is sent to multiple users. 
    /// </summary>    
    public class TeamsActivityMassNotificationRequest : TeamsActivityNotificationRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets a list of recipients for a mass notification.
        /// </summary>
        [JsonProperty("recipients")]
        public IEnumerable<TeamsActivityMassNotificationRecipient> Recipients { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see>TeamsActivityMassNotificationRequest</see> instance.
        /// </summary>
        /// <param name="topicSource">Topic source. Valid values are text or entityUrl.</param>
        /// <param name="topicValue">Topic value.  When the <paramref name="topicSource"/> is entityUrl this value must be a Microsoft Graph URL.  This must be a plain text value.</param>
        /// <param name="topicWebUrl">The link the user follows when they select the notification.</param>
        /// <param name="activityType">Activity type. This must be declared in the Teams app manifest.</param>
        /// <param name="previewTextContent">Preview text for the notification. Microsoft Teams will only show first 150 characters.</param>
        /// <param name="recipients">List of recipients to which the notifications are sent.</param>
        public TeamsActivityMassNotificationRequest(string topicSource, string topicValue, string topicWebUrl, string activityType, string previewTextContent, IEnumerable<TeamsActivityMassNotificationRecipient> recipients)
            : base(topicSource, topicValue, topicWebUrl, activityType, previewTextContent)
        {
            Recipients = recipients;
        }

        #endregion
    }
}
