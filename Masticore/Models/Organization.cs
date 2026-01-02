using Masticore.Resources;
using Newtonsoft.Json;

namespace Masticore.Models
{
    /// <summary>
    /// Represents the core data fields in an organization.
    /// </summary>
    /// <seealso cref="Masticore.Models.ResourceBase" />        
    public class Organization : ResourceBase, IUniversal
    {
        /// <summary>
        /// Display name of the organization.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the organization
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an ID linking the item to an external third-party resource.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string UniversalId { get; set; }
    }
}