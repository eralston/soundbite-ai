using Masticore;
using Masticore.Providers.Calendar;
using Soundbite.Entity;

namespace Soundbite.Services
{
    internal static class Utils
    {
        /// <summary>
        /// Builds and populates a <see cref="RecurrenceInfo"/> for a given series.
        /// </summary>
        /// <param name="series">Seires from which the recurrence info is populated.</param>        
        /// <returns>a <see cref="RecurrenceInfo"/> instance populated from the series data or <c>null</c> if the series is <c>null</c>.</returns>
        public static RecurrencePattern GetReminderRecurrenceInfo(SeriesEntity series)
        {
            // LINK: https://docs.microsoft.com/en-us/previous-versions/office/office-365-api/api/version-2.0/complex-types-for-mail-contacts-calendar#RecurrencePattern

            RecurrencePattern recurrence = null;
            if (series != null && series.Recurrence != Recurrence.NoRepeat)
            {
                Validator.NotNull(series.Template, "Series must have a template to create reminder recurrence information.");
                Validator.NotNull(series.Template.Reminder, "Series template must have a reminder value set to create reminder recurrence information.");
                recurrence = new RecurrencePattern();
                switch (series.Recurrence)
                {
                    case Recurrence.Daily:
                        // Daily is handled by the recurrence value itself and requires no additional info
                        break;
                    case Recurrence.Monthly:
                        recurrence.DayOfMonth = series.Template.Reminder.Value.Day;
                        break;
                    case Recurrence.Weekday:
                        recurrence.FirstDayOfWeek = 0;
                        recurrence.DaysOfWeek = new int[] { 1, 2, 3, 4, 5 };
                        break;
                    case Recurrence.Weekly:
                        recurrence.FirstDayOfWeek = 0;
                        recurrence.DaysOfWeek = new int[] { (int)series.Template.Reminder.Value.DayOfWeek };
                        break;
                }
            }
            return recurrence;
        }
    }
}