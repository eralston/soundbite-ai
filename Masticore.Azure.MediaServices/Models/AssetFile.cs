namespace Masticore.Azure.MediaServices
{
    public class AssetFile
    {
        public List<Source> Sources { get; set; }
        public List<VideoTrack> VideoTracks { get; set; }
        public List<AudioTrack> AudioTracks { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public string Duration { get; set; }
    }
}
