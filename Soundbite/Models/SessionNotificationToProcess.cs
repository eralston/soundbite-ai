using Masticore;
using System;
using System.Text.Json.Serialization;

namespace Soundbite
{
    /// <summary>
    /// Stores all of the information
    /// </summary>
    public class SessionNotificationToProcess
    {
        #region Fields

        private string _author;
        private string _statusJson;
        private SessionNotificationStatus _status;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ID of the session.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Gets or sets the route of the session.
        /// </summary>
        public string SessionRoute { get; set; }

        /// <summary>
        /// Gets or sets the title of the session.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Author of the session.
        /// </summary>
        public string Author
        {
            // NOTE: this is done because when building it in the database it could have leading,
            // trailing, or be made up of a single space depending on whether the given and family
            // names of the author are available. Trimming it makes it possible to detect when there
            // is no author name so we can revert to the OrgName to provide an author.  Easier to
            // solve here than in the database query.

            get
            // NOTE: this is done because when building it in the database it could have leading,
            // trailing, or be made up of a single space depending on whether the given and family
            // names of the author are available. Trimming it makes it possible to detect when there
            // is no author name so we can revert to the OrgName to provide an author.  Easier to
            // solve here than in the database query.

            => string.IsNullOrEmpty(_author)
                    ? null
                    : _author;
            set => _author = value?.Trim();
        }

        /// <summary>
        /// Name of the organization associated with the session.
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the recipient.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user route of the recipient.
        /// </summary>
        public string UserRoute { get; set; }

        /// <summary>
        /// Gets or sets the JSON config associated with the user information for the recipient.
        /// </summary>
        public string UserConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the ID associated with the Session Notification
        /// </summary>
        public int SessionNotificationId { get; set; }

        #endregion

        #region Status w/[Json]

        /// <summary>
        /// Gets or sets the JSON string associated with the Session Notification StatusJson property.
        /// </summary>
        public string StatusJson
        {
            get =>
                // When retrieving the status JSON opt for the instance when it is available
                // because the assumption is that it may have been modified.
                Status == null
                    ? _statusJson
                    : Status.ToLowerCamelJson();
            set
            {
                // If the StatusJson value is updated while there is a Status instance then clear
                // out the instance because it is considered obsolete.
                Status = null;
                _statusJson = value;
            }
        }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public SessionNotificationStatus Status
        {
            get
            {
                if (_status == null)
                {
                    try
                    {
                        _status = string.IsNullOrEmpty(_statusJson)
                            ? new SessionNotificationStatus()
                            : JsonUtils.FromLowerCamelJson<SessionNotificationStatus>(_statusJson);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to parse JSON from Status in SessionNotifiationToProcess.", ex);
                    }
                }
                return _status;
            }
            set => _status = value;
        }

        #endregion
    }
}