using Masticore;

namespace Soundbite.Services
{
    /// <summary>
    /// An object describing the summary of billing activity over a time period
    /// </summary>
    [CodeGenModel]
    public class ActivityReport : ReportBase
    {
        /// <summary>
        /// Estimated number of minutes recorded by the org in the time period
        /// </summary>
        public long UnitsProduced { get; set; }

        /// <summary>
        /// The total number of unique People in the org who created content
        /// </summary>
        public long ProducerCount { get; set; }

        /// <summary>
        /// Estimated number of minutes listened to by the org in a time period
        /// </summary>
        public long UnitsConsumed { get; set; }

        /// <summary>
        /// The total number of unique <see cref="Person"/>s who consumed content
        /// </summary>
        public long ConsumerCount { get; set; }

        /// <summary>
        /// A collection of <see cref="ActivityHighlight"/> objects that allows for extensible reporting at runtime
        /// </summary>
        public ActivityHighlight[] Highlights { get; set; }
    }
}
