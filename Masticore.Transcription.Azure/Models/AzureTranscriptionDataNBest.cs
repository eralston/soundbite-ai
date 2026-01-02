namespace Masticore.Transcription.Azure
{
    public class AzureTranscriptionDataNBest
    {
        public float Confidence { get; set; }
        public string Lexical { get; set; }
        public string ITN { get; set; }
        public string MaskedITN { get; set; }
        public string Display { get; set; }
        public AzureTranscriptionDataWord[] Words { get; set; }
    }
}
