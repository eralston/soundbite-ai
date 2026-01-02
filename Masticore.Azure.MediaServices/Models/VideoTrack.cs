namespace Masticore.Azure.MediaServices
{
    public class VideoTrack
    {
        public int Id { get; set; }
        public string FourCC { get; set; }
        public string Profile { get; set; }
        public string Level { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public double DisplayAspectRatioNumerator { get; set; }
        public double DisplayAspectRatioDenominator { get; set; }
        public double Framerate { get; set; }
        public int Bitrate { get; set; }
        public int TargetBitrate { get; set; }
        public string MesCodec { get; set; }
        public string MesEncodingComplexity { get; set; }
    }
}
