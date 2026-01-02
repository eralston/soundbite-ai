using System.Threading.Tasks;

namespace Masticore.Providers.Calendar
{
    /// <summary>
    /// Defines the contract required for interacting with a third-party calendar provider.    
    /// </summary>
    public interface ICalendarProvider
    {
        /// <summary>
        /// Gets the ProviderType of the provider assocaited with the ICalendarProvider implementation.
        /// </summary>
        ProviderType ProviderType { get; }

        /// <summary>
        /// Attempts to create a new event from the backend.  Method returns a <see cref="ClientOpWrapper{TResult}"/>
        /// instance containing either the unique ID of the event that was created if backend creation 
        /// was successful, or a client operation requesting event creation in the front end.
        /// </summary>
        /// <param name="appointmentInfo">The appointment information.</param>
        /// <returns>
        /// a <see cref="ClientOpWrapper{TResult}"/> containing a unique value of the event that 
        /// was created, or client operation request to create the event in the front end.
        /// </returns>
        Task<ClientOpWrapper<string>> CreateEvent(CalendarEntry appointmentInfo);

        /// <summary>
        /// Deletes the specified calendar event.
        /// </summary>
        /// <param name="id">Unique identifier that can be used to locate the event.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        Task DeleteEvent(string id);
    }
}