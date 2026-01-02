using Masticore.Providers.Calendar;

namespace Masticore.Providers
{
    /// <summary>
    /// Defines the contract for a provider factory that is responsible for instantiating provider
    /// specific classes based on <see cref="ProviderType"/> value.
    /// </summary>
    public interface IProviderFactory
    {
        /// <summary>
        /// Creates an approriate <see cref="ICalendarProvider"/> instance for the specified <paramref name="providerType"/>.
        /// </summary>
        /// <param name="providerType">Provider type for which a <see cref="ICalendarProvider"/> instance is being sought.</param>
        /// <returns>a <see cref="ICalendarProvider"/> instance ready for use, or <c>null</c> if there is no support for the specified provider.</returns>
        ICalendarProvider GetCalendarProvider(ProviderType providerType);
    }
}
