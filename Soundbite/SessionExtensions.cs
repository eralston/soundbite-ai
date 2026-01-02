using Soundbite.Models;
using System;

namespace Soundbite.Resources
{
    /// <summary>
    /// Extension methods for sessions.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Converts <see cref="SessionType"/> value to its string-based representation.
        /// </summary>
        /// <param name="sessionType">SessionType value to convert.</param>
        /// <returns>a string-based representation of the SessionType value.</returns>
        public static string Name(this SessionType sessionType)
        {
            return Enum.GetName(typeof(SessionType), sessionType);
        }

        /// <summary>
        /// Gets the SessionType name from an <see cref="ISessionWithAudience"/> instance.
        /// </summary>
        /// <param name="session">Session information from which to extract the SessionType name.</param>
        /// <returns>a string-based representation of the SessionType value.</returns>
        public static string TypeName(this Session session)
        {
            return session.SessionType.Name();
        }
    }
}