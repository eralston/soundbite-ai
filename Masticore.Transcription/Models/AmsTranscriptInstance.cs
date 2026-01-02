using System;

namespace Masticore.Transcription
{
    public class AmsTranscriptInstance
    {
        public TimeSpan AdjustedStart { get; set; }
        public TimeSpan AdjustedEnd { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}