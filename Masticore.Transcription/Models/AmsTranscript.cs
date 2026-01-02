namespace Masticore.Transcription
{
    public class AmsTranscript
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public float Confidence { get; set; }
        public int SpeakerId { get; set; }
        public string Language { get; set; }
        public AmsTranscriptInstance[] Instances { get; set; }
    }
}