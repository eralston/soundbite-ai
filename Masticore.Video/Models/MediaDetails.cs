namespace Masticore.Media
{
    /// <summary>
    /// Media effect representing an Azure Transform operation.
    /// </summary>
    public class MediaDetails
    {
        #region Properties

        public string FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitRate { get; set; }
        public int Bandwidth { get; set; }
        public string DisplayAspectRatio { get; set; }
        public double DurationInSeconds { get; set; }
        public string MediaType { get; set; }
        public string AudioCodecName { get; set; }
        public string AudioCodecLongName { get; set; }
        public string AudioStartTime { get; set; }
        public string VideoCodecName { get; set; }
        public string VideoCodecLongName { get; set; }
        public string VideoStartTime { get; set; }
        public int Rotation { get; set; }

        #endregion
    }
}