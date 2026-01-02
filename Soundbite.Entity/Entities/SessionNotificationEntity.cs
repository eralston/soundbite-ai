using Masticore;
using Masticore.Entity;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Soundbite.Entity
{
    /// <summary>
    /// Tracks events for an instance of <see cref="UserEntity"/>
    /// </summary>
    [CodeGenModel]
    public class SessionNotificationEntity : EntityBase
    {
        /// <summary>
        /// To one on User
        /// </summary>
        public int UserId { get; set; }
        public virtual UserEntity User { get; set; }

        /// <summary>
        /// To one on Session
        /// </summary>
        public int SessionId { get; set; }

        public virtual SessionEntity Session { get; set; }

        /// <summary>
        /// Get or set the <see cref="NotificationType"/>
        /// </summary>
        public SessionNotificationType NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the optional field for tracking what channel by which this event was initiated
        /// </summary>
        public NotificationChannel Channel { get; set; }

        /// <summary>
        /// Gets or sets the lifecycle status for the notification
        /// </summary>
        public NotificationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a JSON string containing details about the notification.  This may include
        /// logging, error, or debugging information.
        /// </summary>
        [CodeGenField(Ignore = true)]
        [JsonIgnore()]
        [Newtonsoft.Json.JsonIgnore()]
        public string StatusJson { get; set; }

        /// <summary>
        /// A free-form text field for capturing the 
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string Details { get; set; }
    }

    /// <summary>
    /// Extension methods for the <see cref="SessionNotificationEntity"/> class
    /// </summary>
    public static class SessionNotificationEntityUtils
    {
        /// <summary>
        /// Creates a <see cref="SessionNotificationEntity"/> for the given context
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="type"></param>
        /// <param name="channel"></param>
        /// <param name="status"></param>
        /// <param name="details"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static async Task<SessionNotificationEntity> CreateSessionNotification(
            this SbDb db,
            string userRoute,
            string sessionRoute,
            SessionNotificationType type,
            NotificationChannel channel,
            string details = null,
            NotificationStatus status = NotificationStatus.Success,
            int? createdById = null)
        {
            Validator.ArgNotNull(nameof(db), db);
            Validator.ArgNotNull(nameof(userRoute), userRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);

            SessionNotificationEntity ret = EntityBase.Create<SessionNotificationEntity>(createdById);
            db.SessionNotifications.Add(ret);
            ret.User = await db.SingleLocalOrRemoteAsync<UserEntity>(u => u.Route == userRoute, u => u.Route == userRoute);
            ret.Session = await db.SingleLocalOrRemoteAsync<SessionEntity>(s => s.Route == sessionRoute, s => s.Route == sessionRoute);
            ret.NotificationType = type;
            ret.Channel = channel;
            ret.Details = details;
            ret.Status = status;
            return ret;
        }
    }
}
