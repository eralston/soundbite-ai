namespace Masticore.Models
{
    /// <summary>
    /// Concrete class for <see cref="IUserNotifications"/> intended to move around notification settings without other fields
    /// </summary>
    [CodeGenModel]
    public class UserNotifications : IUserNotifications
    {
        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to recieve news.
        /// </summary>
        public bool AllowNews { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to recieve marketing communication.
        /// </summary>
        public bool AllowMarketing { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to recieve emails.
        /// </summary>
        public bool AllowEmail { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to text messages.
        /// </summary>
        public bool AllowSms { get; set; }
    }
}
