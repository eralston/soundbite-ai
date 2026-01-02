using Masticore;
using Soundbite.Entity;

namespace Soundbite.Services
{
    /// <summary>
    /// The minimum number of occurences in recurrence patterns
    /// </summary>
    public static class MinRecurrence
    {
        public const int NoRepeat = 0;
        public const int Daily = 14;
        public const int Weekday = 10;
        public const int Weekly = 4;
        public const int Monthly = 2;
        public const int Unknown = 2;

        /// <summary>
        /// Returns the number of recurrences expected for a given recurrence type
        /// </summary>
        /// <param name="recurrence"></param>
        /// <returns></returns>
        public static int DesiredMinCount(Recurrence recurrence)
        {
            return recurrence switch
            {
                Recurrence.NoRepeat => NoRepeat, // Should never happen
                Recurrence.Daily => Daily,
                Recurrence.Weekday => Weekday,
                Recurrence.Weekly => Weekly,
                Recurrence.Monthly => Monthly,
                _ => Unknown
            };
        }

        public static int DesiredMinCount(SeriesEntity series)
        {
            return DesiredMinCount(series.Recurrence);
        }
    }
}
