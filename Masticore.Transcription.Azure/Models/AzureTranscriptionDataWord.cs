namespace Masticore.Transcription.Azure
{
    public class AzureTranscriptionDataWord
    {
        public string Word { get; set; }
        public long Offset { get; set; }
        public long Duration { get; set; }
        public float Confidence { get; set; }
    }
}
