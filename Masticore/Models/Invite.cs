namespace Masticore.Models
{
    // Vanilla objects with no relationship implementing domain interfaces

    /// <summary>
    /// Represents the information required to issue an invite to a user.
    /// </summary>
    public class Invite
    {
        /// <summary>
        /// Gets or sets the "token" associated with the user.  Currently this value can be an email
        /// address or a user route.
        /// </summary>
        public string Token { get; set; }
    }
}
