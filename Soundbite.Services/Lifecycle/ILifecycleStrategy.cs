using Masticore.Models;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using System.Threading.Tasks;

namespace Soundbite.Services.Lifecycle
{
    /// <summary>
    /// Interface for managing the swappable lifecycle of a session by type
    /// This allows for unique session logic, like controlling when and how
    /// </summary>
    public interface ILifecycleStrategy
    {
        /// <summary>
        /// Gets or sets the Logger for this instance; allows reuse of the live instance in Azure Functions
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Called immediately BEFORE a new session is committed
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task BeforeCreateSessionAsync(SbDb db, SessionEntity session);

        /// <summary>
        /// Called immediately AFTER a new session is committed
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task AfterCreateSessionAsync(SbDb db, SessionEntity session);

        /// <summary>
        /// Called immediately BEFORE a new series is committed
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="series">Series associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task BeforeCreateSeriesAsync(SbDb db, SeriesEntity series);

        /// <summary>
        /// Called immediately AFTER a new series is committed
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="series">Series associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task AfterCreateSeriesAsync(SbDb db, SeriesEntity series);

        /// <summary>
        /// Called BEFORE doing series processing
        /// </summary>
        /// <param name="db"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        Task BeforeProcessSeriesAsync(SbDb db, SeriesEntity series);

        /// <summary>
        /// Called immediately AFTER processing the given series
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        Task AfterProcessSeriesAsync(SbDb db, SeriesEntity series);

        /// <summary>
        /// Called before a clip is completed saved
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="org">Organization associated with the session.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <param name="participant">Participant that created the <paramref name="newClip"/>.</param>
        /// <param name="newClip">Clip associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task BeforeCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, NewClip newClip);

        /// <summary>
        /// Called after a clip is completely saved
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="org">Organization associated with the session.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <param name="participant">Participant that created the <paramref name="newClip"/>.</param>
        /// <param name="newClip">Clip associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task AfterCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, ClipEntity newClip);

        /// <summary>
        /// Called before transcription occurs on a clip.
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="org">Organization associated with the session.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <param name="clip">Clip associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task BeforeClipTranscribed(SbDb db, Organization org, SessionEntity session, ClipEntity clip);

        /// <summary>
        /// Called after transcription occurs on a clip.
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="org">Organization associated with the session.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <param name="clip">Clip associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task AfterClipTranscribed(SbDb db, Organization org, SessionEntity session, ClipEntity clip);

        /// <summary>
        /// Called before media processing occurs on a clip.
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="org">Organization associated with the session.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <param name="clip">Clip associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task BeforeClipMediaProcessing(SbDb db, Organization org, SessionEntity session, ClipEntity clip);

        /// <summary>
        /// Called after media processing occurs on a clip.
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="org">Organization associated with the session.</param>
        /// <param name="session">Session associated with the event.</param>
        /// <param name="clip">Clip associated with the event.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task AfterClipMediaProcessing(SbDb db, Organization org, SessionEntity session, ClipEntity clip);

        /// <summary>
        /// Called after a clip operation is marked as complete.  
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="clipOpRoute">Route of the clip operation that has completed.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task AfterClipOperationComplete(SbDb sbDb, string clipOpRoute);

        /// <summary>
        /// Called when a session has hit its reminder time
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="org">Organization associated with the session.</param>
        /// <param name="session">Session associated with the reminder.</param>
        /// <returns>a list of client operations identifying tasks that need to be completed on the client-side.</returns>
        Task RemindAsync(SbDb db, Organization org, SessionEntity session);

        /// <summary>
        /// Called when a session hits its publishing time
        /// </summary>
        /// <param name="db">Reference to the soundbite database.</param>
        /// <param name="session">Session that has hit its publishing time.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task PublishAsync(SbDb db, Organization org, SessionEntity session);
    }
}
