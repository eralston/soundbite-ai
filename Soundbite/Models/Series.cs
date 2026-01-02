using Masticore;
using Masticore.Models;

namespace Soundbite.Models
{
    /// <summary>
    /// Series summary object
    /// </summary>
    public class Series : ResourceBase
    {
        /// <summary>
        /// Gets or sets the name of the Series
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the repetition pattern of the series
        /// </summary>
        public Recurrence Recurrence { get; set; }

        /// <summary>
        /// Gets or sets the recurrence pattern data to do custom patterns
        /// </summary>
        public string RecurrenceData { get; set; }
    }
}
