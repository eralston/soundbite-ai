using System;

namespace Soundbite.Services
{
    /// <summary>
    /// Extensions on the DateTime struct
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Iteratively add weekdays to the DateTime
        /// </summary>
        /// <param name="start"></param>
        /// <param name="daysToAdd"></param>
        /// <returns></returns>
        public static DateTime AddWeekdays(this DateTime start, int daysToAdd)
        {
            DateTime checkDateTime = start;
            int daysFound = 0;
            while (daysFound < daysToAdd)
            {
                // Add a day
                checkDateTime = checkDateTime.AddDays(1);
                // Make sure it counts
                if (checkDateTime.DayOfWeek == DayOfWeek.Saturday || checkDateTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                // If it's M-F, then it counts
                ++daysFound;
            }
            // Return the final check date
            return checkDateTime;
        }
    }
}
