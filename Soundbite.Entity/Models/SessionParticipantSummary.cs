namespace Soundbite.Entity
{
    /// <summary>
    /// A summary of a session and its participants
    /// </summary>
    public class SessionParticipantSummary
    {
        /// <summary>
        /// Gets or sets the session
        /// </summary>
        public SessionEntity Session { get; set; }

        /// <summary>
        /// Gets or sets the number of times it was played (may include repeats of frequent listeners)
        /// </summary>
        public int ListensCount { get; set; }

        /// <summary>
        /// Gets or sets the number of unique people who listened
        /// </summary>
        public int ListenersCount { get; set; }

        /// <summary>
        /// Gets or sets the number of unique people who were sent the session in the first place
        /// </summary>
        public int AudienceCount { get; set; }
    }
}