namespace Soundbite.Models
{
    public class ClipMetadataVideoTrack
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public long BitRate { get; set; }
        public decimal Framerate { get; set; }
        public decimal AspectRatioNumerator { get; set; }
        public decimal AspectRatioDenominator { get; set; }
    }
}