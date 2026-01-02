using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Masticore.Azure.MediaServices
{
    /// <summary>
    /// Represents the data received during the Media Job Changed event from Azure Media Services
    /// via the Azure Event Grid.
    /// </summary>
    public class MediaJobChangedEvent
    {
        /// <summary>
        /// Gets or sets the previous state of the media job
        /// </summary>
        [JsonProperty("previousState")]
        [JsonPropertyName("previousState")]
        public string? PreviousState { get; set; }

        [JsonProperty("state")]
        [JsonPropertyName("state")]
        public string? State { get; set; }
    }
}
