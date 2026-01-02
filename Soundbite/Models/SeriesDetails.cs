using System.Collections.Generic;

namespace Soundbite.Models
{
    /// <summary>
    /// Contains series information along with a reference to the session template and a list of
    /// sessions associated with the series.
    /// </summary>
    /// <seealso cref="Soundbite.Resources.ISeriesWithTemplateAndSessions" />
    public class SeriesDetails : Series
    {
        /// <summary>
        /// <see cref="SessionDetails"/> associated with the template of this series
        /// </summary>
        public SessionDetails Template { get; set; }

        /// <summary>
        /// Gets or sets a list of sessions associated with the recurring series.
        /// </summary>
        public IEnumerable<SessionPreview> Sessions { get; set; }
    }
}
