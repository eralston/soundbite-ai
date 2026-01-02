using Masticore;
using Soundbite.Settings;

namespace Soundbite
{
    /***********************************************************************************************
     * WARNING: Organization settings are sent client-side.  Any secure data must be marked with
     *  JsonIgnore attribute to avoid being sent out and compromising security.
     **********************************************************************************************/

    /// <summary>
    /// The consolidated configuration object for an organization in Soundbite
    /// </summary>
    public class OrgSettings : IOrgSettings
    {
        #region Fields

        // Property Backer Fields
        private OrgNotificationSettings _notifications;
        private OrgPermissions _permissions;
        private OrgAzureSettings _azure;
        private OrgSessionSettings _sessions;
        private OrgOktaSettings _okta;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the notification settings for these org settings; potentially further restricting the types of settings sent
        /// </summary>
        public OrgNotificationSettings Notifications
        {
            get
            {
                _notifications = _notifications ?? new OrgNotificationSettings();
                return _notifications;
            }
            set => _notifications = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="OrgPermissions"/> for this org, which customizes the UI and RBAC behavior for the organization
        /// </summary>
        public OrgPermissions Permissions
        {
            get
            {
                _permissions = _permissions ?? new OrgPermissions();
                return _permissions;
            }
            set => _permissions = value;
        }

        /// <summary>
        /// Gets or sets the notification settings for these org settings; potentially further restricting the types of settings sent
        /// </summary>
        public OrgSessionSettings Sessions
        {
            get
            {
                _sessions = _sessions ?? new OrgSessionSettings();
                return _sessions;
            }
            set => _sessions = value;
        }

        /// <summary>
        /// Gets or sets the azure settings for the organization.
        /// </summary>
        public OrgAzureSettings Azure
        {
            get
            {
                _azure = _azure ?? new OrgAzureSettings();
                return _azure;
            }
            set => _azure = value;
        }

        /// <summary>
        /// Gets or sets the OKTA settings for the organization.
        /// </summary>
        public OrgOktaSettings Okta
        {
            get
            {
                _okta = _okta ?? new OrgOktaSettings();
                return _okta;
            }
        }

        /// <summary>
        /// Gets or sets the theme unique to this organization; null is allowed, which just means use the default theme
        /// </summary>
        public Theme Theme { get; set; }

        #endregion
    }
}