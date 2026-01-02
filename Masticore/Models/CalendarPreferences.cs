using System;

namespace Masticore.Models
{
    /// <summary>
    /// Class for tracking the user's calendar preferences
    /// </summary>
    public class CalendarPreferences
    {
        /// <summary>
        /// Converts a JSON string into the CalendarPreferences instance specifed in <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">.NET type of the calendar preferences class represented by the JSON.</typeparam>
        /// <param name="calendarPreferencesJson">JSON containing calendar preferences.</param>
        /// <returns>an instance of T if the JSON is valid, <c>null</c> if the JSON string is empty</returns>
        /// <exception cref="FormatException">Thrown when JSON deserialization fails.</exception>
        public static T FromJson<T>(string calendarPreferencesJson)
            where T : CalendarPreferences
        {
            try
            {
                T result = null;
                if (!string.IsNullOrEmpty(calendarPreferencesJson))
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(calendarPreferencesJson);
                }
                return result;
            }
            catch
            {
                throw new FormatException("Calendar preferences could not be retrieved due to a formatting issue.");
            }
        }

        /// <summary>
        /// Gets or sets the user's preference for how to interact with calendars.
        /// </summary>
        public CalendarInteractionType InteractionType { get; set; }
    }
}