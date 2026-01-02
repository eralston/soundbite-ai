using Masticore.Media;

namespace Soundbite
{
    public class InitialClipProcessingResult
    {
        #region Properties

        /// <summary>
        /// Gets or sets an array containing the names of the 
        /// segments resulting from splitting the video.
        /// </summary>
        public string[] Segments { get; set; }

        /// <summary>
        /// Gets or sets the media details extracted from the video.
        /// </summary>
        public MediaDetails MediaDetails { get; set; }

        #endregion

        #region Constructor

        public InitialClipProcessingResult()
        {

        }

        #endregion
    }
}
