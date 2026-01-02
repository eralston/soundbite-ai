using Masticore.Providers.Calendar;

namespace Masticore.Providers
{
    /// <summary>
    /// Contains factory methods for creating various providers required by the application.
    /// </summary>
    public class ProviderFactory : IProviderFactory
    {
        /// <summary>
        /// Creates an approriate <see cref="ICalendarProvider"/> instance for the specified <paramref name="calProviderType"/>.
        /// </summary>
        /// <param name="calProviderType">Provider type for which a <see cref="ICalendarProvider"/> instance is being sought.</param>
        /// <returns>a <see cref="ICalendarProvider"/> instance ready for use, or <c>null</c> if there is no support for the specified provider.</returns>
        public ICalendarProvider GetCalendarProvider(ProviderType calProviderType)
        {
            switch (calProviderType)
            {
                case ProviderType.AAD:
                    return new CalendarProviderAad();
                default:
                    return null;
            }
        }
    }
}