namespace Masticore.Transcription
{
    public class TranscriptionSection
    {
        public string Text { get; set; }
        public long DurationTicks { get; set; }
        public long OffsetTicks { get; set; }
        public object? Data { get; set; }
    }
    public class TranscriptionSection<T> : TranscriptionSection
        where T : class
    {
        public new T? Data
        {
            get => base.Data as T;
            set => base.Data = value;
        }
    }
}