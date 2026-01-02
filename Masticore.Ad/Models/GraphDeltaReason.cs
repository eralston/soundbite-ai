using Newtonsoft.Json;

namespace Masticore.Ad
{
    public class GraphDeltaReason
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
