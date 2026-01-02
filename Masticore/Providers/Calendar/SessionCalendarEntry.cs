namespace Masticore.Providers.Calendar
{
    /// <summary>
    /// Represents a calendar entry that is associated with a session.
    /// </summary>
    /// <seealso cref="Masticore.Providers.Calendar.CalendarEntry" />
    [CodeGenModel]
    public class SessionCalendarEntry : CalendarEntry
    {
        /// <summary>
        /// Gets or sets the route identifying the organization with which the session is associated.
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// Gets or sets the route identifying the series associated with the session.
        /// </summary>        
        public string SeriesRoute { get; set; }

        /// <summary>
        /// Gets or sets the route identifying the session.
        /// </summary>
        public string SessionRoute { get; set; }
    }
}