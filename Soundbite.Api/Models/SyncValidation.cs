using Masticore;
using Masticore.Models;

namespace Soundbite.Api
{
    /// <summary>
    /// DTO for sending back indication that the given config is valid and available
    /// </summary>
    public class SyncValidation
    {
        /// <summary>
        /// Org for which we checked access
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// Null if the config is valid; otherwise, a string describing issues with the config
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Flag for whether or not the given org has access
        /// </summary>
        public bool HasAccess { get; set; }

        /// <summary>
        /// A sanitized version of the config used for the check
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public OrgSyncConfig SanitizedConfig { get; set; }
    }
}