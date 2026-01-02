using System;

namespace Soundbite.Services
{
    /// <summary>
    /// A base class for reporting that includes common fields
    /// </summary>
    public abstract class ReportBase
    {
        /// <summary>
        /// The org for which this content was created; if global then this is null
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// The display name for this report
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Start <see cref="DateTime"/> for any calculated value 
        /// </summary>
        public DateTime StartUtc { get; set; }

        /// <summary>
        /// End <see cref="DateTime"/> for any calculated value
        /// </summary>
        public DateTime EndUtc { get; set; }
    }
}
