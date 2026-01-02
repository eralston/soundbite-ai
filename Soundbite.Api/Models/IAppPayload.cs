using Masticore.Models;

namespace Soundbite.Api
{
    /// <summary>
    /// Base interface for an app payload
    /// </summary>
    public interface IAppPayload
    {
        /// <summary>
        /// Front-end app config
        /// </summary>
        SpaConfig Config { get; set; }

        /// <summary>
        /// User's org refresh token
        /// </summary>
        string RefreshToken { get; set; }

        /// <summary>
        /// User's org token
        /// </summary>
        string Token { get; set; }

        /// <summary>
        /// Current user attibutes
        /// </summary>
        User User { get; set; }
    }
}