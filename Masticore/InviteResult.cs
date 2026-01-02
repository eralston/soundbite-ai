namespace Masticore
{
    /// <summary>
    /// Accumulates the identifiers linked with the result of an invite
    /// These may or may not be new entities in the system based on what was pre-existing
    /// </summary>
    public class InviteResult
    {
        /// <summary>
        /// Routes for the newly invited user.
        /// This should ALWAYS be set.
        /// </summary>
        public string UserRoute { get; set; }

        /// <summary>
        /// Optional route for the newly invited person.
        /// If this is set, then <see cref="UserRoute"/> will also be set
        /// </summary>
        public string PersonRoute { get; set; }

        /// <summary>
        /// Optional rouite for the newly invited member.
        /// If this is seet, then <see cref="PersonRoute"/> will also be set
        /// </summary>
        public string MemberRoute { get; set; }
    }
}
