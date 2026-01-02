namespace Masticore.Media
{
    public class FfmpegEncodeToMp4 : FfmpegEncode
    {
        #region IMediaEffect Implementation

        /// <inheritdoc />
        public override string Type => nameof(FfmpegEncodeToMp4);

        #endregion

        public FfmpegEncodeToMp4()
            : base("libx264 -preset veryfast", null, "mp4")
        {
        }
    }
}
