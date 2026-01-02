namespace Masticore.Models
{
    /// <summary>
    /// Calendar settings specific to AAD.
    /// </summary>
    /// <seealso cref="Masticore.Models.CalendarPreferences" />
    public class CalendarPreferencesAad : CalendarPreferences
    {
        /// <summary>
        /// Gets or sets the calendar group ID containing the target calendar.  When this value is
        /// null or empty the default calendar group is used.
        /// </summary>        
        public string CalGroupId { get; set; }

        /// <summary>
        /// Gets or sets the calendar ID of the target calendar.  When this value is null or empty
        /// the default calendar is used.
        /// </summary>        
        public string CalId { get; set; }
    }
}