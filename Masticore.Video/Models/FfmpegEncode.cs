namespace Masticore.Media
{
    public class FfmpegEncode : IMediaEffect
    {
        #region IMediaEffect Implementation

        /// <inheritdoc />
        public virtual string Type => nameof(FfmpegEncode);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="FfmpegEncode"/> instance.
        /// </summary>
        /// <param name="videoCodec"></param>
        /// <param name="audioCodec"></param>
        /// <param name="fileExtension"></param>
        public FfmpegEncode(string videoCodec, string audioCodec, string fileExtension)
        {
            VideoCodec = videoCodec;
            AudioCodec = audioCodec;
            FileExtension = fileExtension;

            // Make sure that file extension does not have a starting period
            if (FileExtension?.StartsWith(".") == true)
            {
                FileExtension = FileExtension.Substring(1);
            }
        }

        #endregion

        #region Properties

        public string FileExtension { get; }
        public string VideoCodec { get; }
        public string AudioCodec { get; }

        #endregion
    }
}