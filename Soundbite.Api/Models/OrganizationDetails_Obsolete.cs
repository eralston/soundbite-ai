using Masticore.Models;
using System.Collections.Generic;

namespace Soundbite.Api
{
    /// <summary>
    /// Obsolete version of org details
    /// </summary>
    public class OrganizationDetails_Obsolete : OrganizationExtended
    {
        /// <summary>
        /// Gets the config JSON for this org
        /// </summary>
        /// <remarks>
        /// This is for returning a consolidated view to the server and this should NEVER be sent to clients
        /// </remarks>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string ConfigJson { get; set; }

        /// <summary>
        /// A Person record for the current user
        /// </summary>
        public Person Me { get; set; }

        /// <summary>
        /// A collection of Group records for collections of people in the Organization
        /// </summary>
        public IEnumerable<Group> Groups { get; set; }

        /// <summary>
        /// A collection of Group records for all of the Groups to which the current user has access
        /// </summary>
        public IEnumerable<Group> MyGroups { get; set; }
    }
}
