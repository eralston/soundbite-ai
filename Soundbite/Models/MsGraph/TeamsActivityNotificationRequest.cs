using Newtonsoft.Json;

namespace Soundbite.Models
{
    public class TeamsActivityNotificationRequest
    {
        #region Properties

        /// <summary>
        /// Contains a reference to the topic which contains a text/url value representing the 
        /// content of the Teams Activity Notification.
        /// </summary>
        [JsonProperty("topic")]
        public TeamsActivityNotificationTopic Topic { get; set; }

        /// <summary>
        /// Specifies the activity type.  This value must match up to an ActivityType value 
        /// specified in the Teams App Manifest.
        /// </summary>
        [JsonProperty("activityType")]
        public string ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the preview text for the notification. Microsoft Teams only shows the first 150 characters.
        /// </summary>
        [JsonProperty("previewText")]
        public TeamsActivityNotificationPreviewText PreviewText { get; set; }

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
        public TeamsActivityNotificationRequest(string topicSource, string topicValue, string topicWebUrl, string activityType, string previewTextContent)
        {
            Topic = new TeamsActivityNotificationTopic()
            {
                Source = topicSource,
                Value = topicValue,
                WebUrl = topicWebUrl
            };
            ActivityType = activityType;
            PreviewText = new TeamsActivityNotificationPreviewText()
            {
                Content = previewTextContent
            };
        }

        #endregion
    }
}
