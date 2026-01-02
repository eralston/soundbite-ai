namespace Masticore.Providers.Calendar
{
    /// <summary>
    /// Configurates a custom pattern for recurring events
    /// </summary>
    public class RecurrencePattern
    {
        /// <summary>
        /// Gets or sets the number of units of a given recurrence type between occurences.
        /// </summary>        
        public int Interval { get; set; }

        /// <summary>
        /// Gets or sets the day of the month that the item occurs on (if applicable)
        /// </summary>        
        public int DayOfMonth { get; set; }

        /// <summary>
        /// Gets or sets the month that the item occurs on.  Values 1 through 12 are valid.
        /// </summary>        
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the days of the week the item occurs on (0=Sunday ... 6=Saturday)
        /// </summary>        
        public int[] DaysOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the day of the week the item begins occuring on (0=Sunday ... 6=Saturday)
        /// </summary>        
        public int FirstDayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the index of the week in which the item occurs (0=First, 1=Second, 2=Third, 3=Fourth, 4=Last)
        /// </summary>        
        public int WeekIndex { get; set; }

    }
}