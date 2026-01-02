using System;

namespace Masticore.Providers.Calendar
{
    /// <summary>
    /// Client-side model for calendar events
    /// </summary>
    [CodeGenModel]
    public class CalendarEntry
    {
        /// <summary>
        /// Gets or sets the ID associated with the calendar entry.  This value should be unique
        /// within the calendar containing the entry.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the appointment as it should appear in the calendar.
        /// </summary>        
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body content of the calendar entry.
        /// </summary>        
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the date/time of the appointment.  If the appointment is recurring this
        /// value represents the first appointment in the series.
        /// </summary>        
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the duration of the entry in minutes.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating event recurrence.  When set to a value other then
        /// <see cref="Recurrence.NoRepeat"/> the <see cref="Recurrence"/> property should be
        /// populated with details about recurrence.
        /// </summary>        
        public Recurrence Recurrence { get; set; }

        /// <summary>
        /// Gets or sets specific recurrence pattern when <see cref="Recurrence"/> is set to a value
        /// other than <see cref="Recurrence.NoRepeat"/>.
        /// </summary>        
        public RecurrencePattern RecurrencePattern { get; set; }
    }
}