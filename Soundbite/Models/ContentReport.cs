using Masticore;

namespace Soundbite.Services
{
    [CodeGenModel]
    public class ContentReport : ReportBase
    {
        /// <summary>
        /// Gets the number of "listens"
        /// </summary>
        public ActivityHighlight ConsumeCount { get; set; }

        /// <summary>
        /// The range of listens per time period
        /// </summary>
        public ActivityRange ConsumeCountOverTime { get; set; }

        /// <summary>
        /// Gets the number of acknowledgements
        /// </summary>
        public ActivityHighlight AcknowledgeCount { get; set; }

        // Ad Hoc Highlights

        /// <summary>
        /// A collection of <see cref="ActivityHighlight"/> objects that allows for extensible reporting at runtime
        /// </summary>
        public ActivityHighlight[] Highlights { get; set; }
    }
}
