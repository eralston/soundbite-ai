using Masticore.Resources;
using Newtonsoft.Json;

namespace Masticore.Ad
{
    /// <summary>
    /// Base class for a graph object that treats its "id" JSON property as an <see cref="IUniversal.UniversalId"/>
    /// </summary>
    public class GraphBase : IUniversal
    {
        public const string GetByIdsUrl = "https://graph.microsoft.com/v1.0/directoryObjects/getByIds";

        [JsonProperty("id")]
        public string UniversalId { get; set; }

        [JsonProperty("@odata.type")]
        public string Type { get; set; }

        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("@removed")]
        public GraphDeltaReason Removed { get; set; }

        [JsonProperty("deletedDateTime")]
        public string DeletedDateTime { get; set; }

        /// <summary>
        /// Gets true if this is NOT deleted and has necessary ID information
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual bool IsActiveAndValid => Name != null && UniversalId != null && DeletedDateTime == null && Removed == null;
    }
}