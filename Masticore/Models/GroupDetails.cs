namespace Masticore.Models
{
    /// <summary>
    /// Additional fields for <see cref="Group"/> when analyzing specifics for the current user
    /// </summary>
    public class GroupDetails : Group
    {
        /// <summary>
        /// Gets or sets the <see cref="Masticore.MemberRole"/> for the current user in regard to this group
        /// </summary>
        /// <remarks>
        /// The current user's <see cref="UserRole"/> and/or <see cref="PersonRole"/> may override the assigned <see cref="Masticore.MemberRole"/> value, especially in the event that there is no <see cref="Member"/> object for the current user
        /// </remarks>
        public MemberRole MemberRole { get; set; }
    }
}