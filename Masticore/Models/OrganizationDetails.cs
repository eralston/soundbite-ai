using Newtonsoft.Json;

namespace Masticore.Models
{
    /// <summary>
    /// The extended properties of an organization, providing summaries of the underlying collections related to the Organization
    /// </summary>
    public class OrganizationDetails : OrganizationExtended
    {
        /// <summary>
        /// Gets the config JSON for this org
        /// </summary>
        /// <remarks>
        /// This is for returning a consolidated view to the server and this should NEVER be sent to clients
        /// </remarks>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ConfigJson { get; set; }

        /// <summary>
        /// A Person record for the current user
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public Person Me { get; set; }
    }
}
