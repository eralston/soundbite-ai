using Masticore.Resources;
using Newtonsoft.Json;

namespace Masticore.Models
{
    /// <summary>
    /// The the parent for a collection of resources in the system.
    /// It should be poissible to horizontally shard everything under a tenant (EG, all objects in a hierarchy below a <see cref="Tenant"/> can be in a single, distinct data store).
    /// </summary>
    public class Tenant : ResourceBase, IUniversal
    {
        /// <summary>
        /// Gets or sets the name of this tenant
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Globally unique ID for the Tenant.
        /// Keep in mind this may need to be unique across multiple data stores.
        /// If there is a global-versus-region concept amongst the stores, this identifier MUST be the same in both.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string UniversalId { get; set; }
    }
}
