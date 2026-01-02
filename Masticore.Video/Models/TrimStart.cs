using System;

namespace Masticore.Media
{
    /// <summary>
    /// Media effect that trims off the start of an audio/video file.
    /// </summary>
    public class TrimStart : IMediaEffect
    {
        #region IMediaEffect Implementation

        /// <inheritdoc />
        public string Type => nameof(TrimStart);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the amount of video to trim from the start of the video.
        /// Value is in time format hh:MM:ss.mm
        /// </summary>
        public TimeSpan Duration { get; set; }

        #endregion
    }
}
