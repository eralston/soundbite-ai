using System;

namespace Masticore.Media
{
    public class Fade : IMediaEffect
    {
        #region IMediaEffect Implementation

        /// <inheritdoc />
        public string Type => nameof(Fade);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fade direction of the fade effect
        /// </summary>
        public FadeDirType FadeDir { get; set; }

        /// <summary>
        /// Gets or sets whether the fade is target audio, video, or both.
        /// </summary>
        public FadeTargetType FadeTarget { get; set; } = FadeTargetType.Video;

        /// <summary>
        /// Gets or sets the location of the effect
        /// Value is in time format hh:MM:ss.mm
        /// </summary>
        public TimeSpan Location { get; set; }

        /// <summary>
        /// Gets or sets the duration of the fade in.
        /// Value is in time format hh:MM:ss.mm
        /// </summary>
        public TimeSpan Duration { get; set; }

        #endregion
    }
}