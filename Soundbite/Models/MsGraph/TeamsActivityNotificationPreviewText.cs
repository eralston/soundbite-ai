using Newtonsoft.Json;

namespace Soundbite.Models
{
    /// <summary>
    /// Represents the Teams Activity Notification Preview Text content.
    /// </summary>
    public class TeamsActivityNotificationPreviewText
    {
        /// <summary>
        /// Preview text displayed in the activity notification.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
