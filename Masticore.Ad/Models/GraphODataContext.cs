using Newtonsoft.Json;

namespace Masticore.Ad
{
    /// <summary>
    /// Exposes properties for OData calls.
    /// </summary>
    /// <typeparam name="T">.NET type of the wrapped item.</typeparam>
    public class GraphODataContext<T>
    {
        [JsonProperty("@odata.nextLink")]
        public string NextLink { get; set; }

        [JsonProperty("@odata.deltaLink")]
        public string DeltaLink { get; set; }

        [JsonProperty("value")]
        public T[] Value { get; set; }
    }
}
