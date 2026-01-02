namespace Masticore.Resources
{
    /// <summary>
    /// An invite for a Person to join a Group. The individual will be sent a notification that they have been invited.
    /// </summary>
    public class NewMember
    {
        /// <summary>
        /// The Person Route or Email Address of the prospective member.
        /// </summary>
        public string PersonToken { get; set; } // PersonRoute OR Email

        /// <summary>
        /// Gets or sets the group-level role of the user.
        /// </summary>
        public MemberRole MemberRole { get; set; }
    }
}