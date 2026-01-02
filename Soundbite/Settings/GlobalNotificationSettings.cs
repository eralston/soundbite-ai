using Masticore;

namespace Soundbite
{
    /// <summary>
    /// Platform level restrictions on notifications (EG, e-mails).
    /// If <see cref="GlobalNotificationSettings.Instance"/>.[flag] && (for a particular org) <see cref="OrgNotificationSettings"/>.[flag] are BOTH true, then the notification should be sent
    /// </summary>
    /// <remarks>
    /// This is a superset of <see cref="OrgNotificationSettings"/>, but they will override the enabling of notifications within that organization
    /// </remarks>
    public class GlobalNotificationSettings : OrgNotificationSettings
    {
        /// <summary>
        /// Sets all flags; default to true
        /// </summary>
        /// <remarks>This is for testing purposes ONLY!</remarks>
        /// <param name="flag"></param>
        public static void SetAllFlags(bool flag = true)
        {
            Instance.IsUserInvitedEnabled = flag;
            Instance.IsWelcomeEnabled = flag;
            Instance.IsPersonInviteEnabled = flag;
            Instance.IsMemberInvitedEnabled = flag;
            Instance.IsSessionReminderEnabled = flag;
            Instance.IsSessionPublishEnabled = flag;
            Instance.IsSessionHostPublishEnabled = flag;
        }

        /// <summary>
        /// Stores the singleton instance
        /// </summary>
        private static GlobalNotificationSettings _instance;

        /// <summary>
        /// Gets the singleton instance of <see cref="GlobalNotificationSettings"/>
        /// </summary>
        public static GlobalNotificationSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalNotificationSettings
                    {
                        IsUserInvitedEnabled = MasticoreExtensions.GetEnvBool("SB_NOTIFICATION_USERINVITE_ENABLED", true),
                        IsWelcomeEnabled = MasticoreExtensions.GetEnvBool("SB_NOTIFICATION_WELCOME_ENABLED", true),
                        IsPersonInviteEnabled = MasticoreExtensions.GetEnvBool("SB_NOTIFICATION_PERSONINVITE_ENABLED", true),
                        IsMemberInvitedEnabled = MasticoreExtensions.GetEnvBool("SB_NOTIFICATION_MEMBERINVITE_ENABLED", true),
                        IsSessionReminderEnabled = MasticoreExtensions.GetEnvBool("SB_NOTIFICATION_SESSIONREMINDER_ENABLED", true),
                        IsSessionPublishEnabled = MasticoreExtensions.GetEnvBool("SB_NOTIFICATION_SESSIONPUBLISH_ENABLED", true),
                        IsSessionHostPublishEnabled = MasticoreExtensions.GetEnvBool("SB_NOTIFICATION_SESSIONHOSTPUBLISH_ENABLED", true)
                    };
                }

                return _instance;
            }
        }

        /// <summary>
        /// Is the invite when one user invites another user (PLG) sent?
        /// </summary>
        public bool IsUserInvitedEnabled { get; set; }
    }
}
