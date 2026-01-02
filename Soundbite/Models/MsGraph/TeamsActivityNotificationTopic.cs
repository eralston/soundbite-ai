using Newtonsoft.Json;

namespace Soundbite.Models
{
    /// <summary>
    /// Teams Activity Notification Topic that represents the content of the Activity Notification.
    /// </summary>    
    public class TeamsActivityNotificationTopic
    {
        /// <summary>
        /// Gets or sets the topic source. Valid values are text or entityUrl
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; }

        /// <summary>
        /// Topic value.  When the <see name="Source"/> is entityUrl this value must be a Microsoft Graph URL.  This must be a plain text value.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// The link the user follows when they select the notification.
        /// </summary>
        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }
    }
}