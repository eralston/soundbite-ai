using Masticore;
using Soundbite.Services;

namespace Soundbite.Models
{
    /// <summary>
    /// Stats for a <see cref="Session"/> object
    /// </summary>
    [CodeGenModel]
    public class SessionContentReport : ContentReport
    {
        /// <summary>
        /// The unique route for the session
        /// </summary>
        public string SessionRoute { get; set; }

        /// <summary>
        /// Total size of the audience on the soundbite
        /// </summary>
        public ActivityHighlight AudienceSize { get; set; }


        /// <summary>
        /// The number of distinct listeners
        /// </summary>
        public ActivityHighlight ConsumerCount { get; set; }
    }
}
