namespace Soundbite
{
    /***********************************************************************************************
     * WARNING: Organization settings are sent client-side.  Any secure data must be marked with
     *  JsonIgnore attribute to avoid being sent out and compromising security.
     **********************************************************************************************/

    /// <summary>
    /// Collects all of the notification channels available for a given org
    /// </summary>
    public class OrgNotificationChannels
    {
        /// <summary>
        /// Gets or sets if e-mail is available as a channel for this org
        /// </summary>
        public bool IsEmailEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets if SMS messaging is available as a channel for this org
        /// </summary>
        public bool IsSmsEnabled { get; set; } = true;
    }

    /// <summary>
    /// A settings object for org-level notifications
    /// This is used by the <see cref="OrganizationEntity"/> settings
    /// </summary>
    /// <remarks>
    /// The platform is able to disable notifications entirely, but organizations are allowed to use these settings to restrict them yet more.
    /// Refer to <see cref="GlobalNotificationSettings"/> to understand the context for these; they are a subset of those settings
    /// </remarks>
    public class OrgNotificationSettings
    {
        /// <summary>
        /// After someone logs in for the first time
        /// </summary>
        public bool IsWelcomeEnabled { get; set; } = true;

        /// <summary>
        /// When an existing user in an org invites another person into that org
        /// </summary>
        public bool IsPersonInviteEnabled { get; set; } = true;

        /// <summary>
        /// When someone is added to a team
        /// </summary>
        public bool IsMemberInvitedEnabled { get; set; } = true;

        /// <summary>
        /// When a session is asking for a recording
        /// </summary>
        public bool IsSessionReminderEnabled { get; set; } = true;

        /// <summary>
        /// When a session is successfully available to its audience
        /// </summary>
        public bool IsSessionPublishEnabled { get; set; } = true;

        /// <summary>
        /// Sent to the host of a session when it is published, offering instructions on how to disseminate the session
        /// </summary>
        public bool IsSessionHostPublishEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the set of channels enabled or disabled for a given org
        /// </summary>
        /// <remarks>If this is null, then notification services are free to have their own default behavior (generally should be ON)</remarks>
        public OrgNotificationChannels Channels { get; set; } = new OrgNotificationChannels();
    }
}
