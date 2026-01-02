namespace Masticore
{
    /// <summary>
    /// The consolidated configuration object for an organization in Soundbite
    /// </summary>
    public class UserMsTeamsSettings
    {
        /// <summary>
        /// Gets or sets a flag indicating whether teams notifications for the user are enabled.
        /// </summary>
        public AutoFlagStateType IsEnabled { get; set; } = AutoFlagStateType.AutoEnabled;

        /// <summary>
        /// Gets or sets an alternate ID associated with the user.  This value overrides the email 
        /// address used for constructing API calls to Graph.
        /// </summary>
        public string AltId { get; set; }
    }
}
