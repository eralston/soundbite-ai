using Masticore;
using Soundbite.Services;
using System.Collections.Generic;

namespace Soundbite.Models
{
    [CodeGenModel]
    public class OrgContentReport : ContentReport
    {
        /// <summary>
        /// The range of listens per time period
        /// </summary>
        public ActivityRange ConsumeCountOverTime { get; set; }

        /// <summary>
        /// The range of sessions per time period
        /// </summary>
        public ActivityRange SessionsCountOverTime { get; set; }

        /// <summary>
        /// Gets the number of sessions in the org and timespan
        /// </summary>
        public ActivityHighlight SessionsCount { get; set; }

        /// <summary>
        /// Gets the number of sessions that were consumed in the given org and timespan
        /// </summary>
        public ActivityHighlight SeriesCount { get; set; }

        // Key Content

        /// <summary>
        /// A list of recently created sessions and their duration
        /// </summary>
        public IEnumerable<SessionPreview> RecentSessions { get; set; }
    }
}
