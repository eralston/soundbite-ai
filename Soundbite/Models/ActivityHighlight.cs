using System.Collections.Generic;

namespace Soundbite.Services
{
    /// <summary>
    /// A summary of a unique statistic which is sent back based on the most interesting data for the org
    /// </summary>
    public class ActivityHighlight
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// A point-in-time
    /// </summary>
    public class ActivityRangeItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// Container for holding a collection of <see cref="ActivityRangeItem"/>
    /// </summary>
    public class ActivityRange
    {
        public string Title { get; set; }
        public IEnumerable<ActivityRangeItem> Items { get; set; }
    }
}
