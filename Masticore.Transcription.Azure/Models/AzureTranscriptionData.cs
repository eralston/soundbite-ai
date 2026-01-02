namespace Masticore.Transcription.Azure
{
    public class AzureTranscriptionData
    {
        public string Id { get; set; }
        public string RecognitionStatus { get; set; }
        public long Offset { get; set; }
        public long Duration { get; set; }
        public int Channel { get; set; }
        public string DisplayText { get; set; }
        public AzureTranscriptionDataNBest[] NBest { get; set; }
    }
}
