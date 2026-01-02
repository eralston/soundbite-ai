namespace Masticore
{
    /// <summary>
    /// The consolidated configuration object for a user in Soundbite
    /// </summary>
    public class UserSettings : IUserSettings
    {
        #region Fields

        // Property backer fields
        private UserMsTeamsSettings _msTeams;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Microsoft Teams settings for the user.
        /// </summary>
        public UserMsTeamsSettings MsTeams
        {
            get
            {
                _msTeams = _msTeams ?? new UserMsTeamsSettings();
                return _msTeams;
            }
            set => _msTeams = value;
        }

        #endregion
    }
}
