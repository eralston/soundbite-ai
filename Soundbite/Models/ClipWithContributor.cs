using Masticore.Models;

namespace Soundbite.Models
{
    /// <summary>
    /// A clip is an audio contribution from one of the participants in a session.
    /// </summary>
    public class ClipWithContributor : Clip
    {
        /// <summary>
        /// URL with short-lived access token to the clip audio file.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Contributor who recorded or uploaded the clip.
        /// </summary>
        public User Contributor { get; set; }
    }
}