namespace Masticore.Models
{
    /// <summary>
    /// Settings for each type of notification in the system that needs mass access
    /// </summary>
    public interface IUserNotifications
    {
        /// <summary>
        /// Enables/disables the e-mail channel
        /// </summary>
        bool AllowEmail { get; set; }

        /// <summary>
        /// Enables/disables marketing style content
        /// </summary>
        bool AllowMarketing { get; set; }

        /// <summary>
        /// Enables/disables news-style content, like features update news
        /// </summary>
        bool AllowNews { get; set; }

        /// <summary>
        /// Enables/disables the SMS channel
        /// </summary>
        bool AllowSms { get; set; }
    }
}