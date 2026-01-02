using Masticore;
using Masticore.Models;
using System;

namespace Soundbite.Models
{
    /// <summary>
    /// An abstration of the state of a session, providing understanding of its current state
    /// </summary>
    public enum SessionLifecycleStage
    {
        /// <summary>
        /// We cannot definitively determine the current state
        /// </summary>
        /// <remarks>Unless it is definitely in one of the other </remarks>
        Unknown = 0,

        /// <summary>
        /// The sessions shows no sign of moving through any workflow whatsoever, but definitely exists and can be edited
        /// </summary>
        Draft = 100,

        /// <summary>
        /// TODO: Session is pending review by moderators
        /// </summary>
        UnderReview = 200,

        /// <summary>
        /// The session has a reminder scheduled, but it is not sent yet
        /// </summary>
        WaitingToRemind = 300,

        /// <summary>
        /// The session has content, but it waiting for its scheduled time
        /// </summary>
        WaitingToPublish = 400,

        /// <summary>
        /// The session is available for listening and should be waiting in people's feeds for them
        /// </summary>
        Published = 500,

        /// <summary>
        /// TODO: The session is retired from all feeds, but still accessible for history and can be deleted
        /// </summary>
        Archived = 600,

        /// <summary>
        /// TODO: The session is invisible to non-admins and not-deletable except if it is taken out of hold
        /// </summary>
        /// <remarks>This is like a "legal hold" in M365 where an item is frozen and hidden</remarks>
        Hold = 700,

        /// <summary>
        /// The sessions was soft-deleted
        /// </summary>
        Removed = 800
    }

    /// <summary>
    /// A session is a collaborative gathering of people and their audio contributions revolving 
    /// around a specific topic or activity.
    /// </summary>
    public class Session : ResourceBase
    {
        /// <summary>
        /// Gets or sets the UTC-based date/time when reminders for the session were sent out.
        /// </summary>
        public DateTime? ReminderSent { get; set; }

        /// <summary>
        /// Gets or sets teh UTC-based date/time when the session was published.
        /// </summary>
        public DateTime? PublishSent { get; set; }

        /// <summary>
        /// Gets or sets the Title of the session.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the maximum duration of the session in seconds.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets a value identifying the session type.  See <see cref="Soundbite.SessionType" />
        /// for more information on the various types of sessions.
        /// </summary>
        public SessionType SessionType { get; set; }

        /// <summary>
        /// Gets or sets the type of security associated with the session.
        /// </summary>
        public SessionSecurityType SessionSecurity { get; set; }

        /// <summary>
        /// Gets or sets the settings for comments
        /// </summary>
        public SessionCommentPolicy SessionCommentPolicy { get; set; }

        /// <summary>
        /// Gets or sets the UTC-based date/time when to send reminders for the session.
        /// </summary>
        public DateTime? Reminder { get; set; }

        /// <summary>
        /// Gets or sets the UTC-based date/time when to publish the session.
        /// </summary>
        public DateTime? Publish { get; set; }

        /// <summary>
        /// Gets or sets a provider-specified identifier that can be used by the provider to locate
        /// the calendar entry associated with the session.
        /// </summary>        
        public string ReminderCalEventId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether transcription is enabled at the session level.
        /// </summary>        
        public bool Transcribe { get; set; }

        /// <summary>
        /// Gets the abstracted <see cref="SessionLifecycleStage"/> based on current attributes
        /// </summary>
        /// <remarks>This imply universal rules on the <see cref="Session"/> object state that may vary one day; in the future this may need to move to the whatever abstraction handles session lifecycles</remarks>
        /// <returns></returns>
        public SessionLifecycleStage LifecycleStage()
        {
            // Order to check follows the most strict criteria first more so than in linear order of draft lifecycle

            // TODO: Draft, UnderReview, and Archived

            if (DeletedUtc != null)
            {
                return SessionLifecycleStage.Removed;
            }

            if (PublishSent != null)
            {
                return SessionLifecycleStage.Published;
            }

            if (Reminder != null && ReminderSent == null)
            {
                return SessionLifecycleStage.WaitingToRemind;
            }

            if (Publish != null && PublishSent == null)
            {
                return SessionLifecycleStage.WaitingToPublish;
            }

            if (Reminder == null && ReminderSent == null && Publish == null && PublishSent == null)
            {
                return SessionLifecycleStage.Draft;
            }

            // If it doesn't meet any definitive criteria above, then it's unknown
            return SessionLifecycleStage.Unknown;
        }
    }
}