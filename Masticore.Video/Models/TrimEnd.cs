using System;

namespace Masticore.Media
{
    /// <summary>
    /// Media effect that trims off the end of an audio/video file.
    /// </summary>
    public class TrimEnd : IMediaEffect
    {
        #region IMediaEffect Implementation

        /// <inheritdoc />
        public string Type => nameof(TrimEnd);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the time from which to trim all remaining content.
        /// </summary>
        public TimeSpan Location { get; set; }

        #endregion
    }
}
