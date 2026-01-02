namespace Masticore.Azure.MediaServices
{
    public class AudioTrack
    {
        public int Id { get; set; }
        public string Codec { get; set; }
        public string Language { get; set; }
        public int Channels { get; set; }
        public int SamplingRate { get; set; }
        public int Bitrate { get; set; }
        public string MesCodec { get; set; }
    }
}
