using System.Collections.Generic;

namespace Soundbite
{
    /// <summary>
    /// Stores all of the information required to send a Teams notification to a series of users.
    /// </summary>
    public class TeamsAppBatchNotificationItem
    {
        #region Fields

        private IList<TeamsAppBatchNotificationRecipient> _recipients;

        #endregion

        #region Properties

        public SessionNotificationType NotificationType { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Author { get; set; }
        //public IList<SessionNotificationRef> UserUpns { get; set; }
        public IList<TeamsAppBatchNotificationRecipient> Recipients
        {
            get
            {
                _recipients = _recipients ?? new List<TeamsAppBatchNotificationRecipient>();
                return _recipients;
            }
            set => _recipients = value;
        }

        #endregion
    }
}