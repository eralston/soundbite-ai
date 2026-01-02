using Masticore.Transcription.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masticore.Transcription
{
    /// <summary>
    /// Contains transcription result information with details
    /// </summary>
    [CodeGenModel()]
    public class TranscriptionResult : ITranscriptAstt, ITranscriptAms
    {
        #region Constants

        /// <summary>
        /// ASTT formatted results are defined in sections that break the transcript into 
        /// paragraphs. AMS does not provide these easily identifiable sections, but the transcript
        /// is broken down into parts with each part containing a start and end time. It appears 
        /// that the start/end time from one part to another is seamless (no gaps) unless the 
        /// speaker has a long enough pause.  When a gap exists, Soundbite interprets that gap as
        /// a paragraph break to provide a way to break up an AMS formatted transcript.  This 
        /// constant defines the minimum threshold for a "gap" to be considered a new paragraph.
        /// </summary>
        private readonly int AmsGapThresholdinMs = 500;

        #endregion

        #region ITranscriptAstt / ITranscriptAms Shared Implementation

        public TimeSpan Duration { get; set; }
        public Version Version { get; set; }

        #endregion

        #region ITranscriptAstt Implementation

        public string Format { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan ProcessingDuration { get; set; }
        public IList<TranscriptionSection> Sections { get; set; } = new List<TranscriptionSection>();
        public string State { get; set; }
        public object ErrorDetails { get; set; }

        #endregion

        #region ITranscriptAms Implementation

        public string SourceLanguage { get; set; }
        public string[] Languages { get; set; }
        public AmsTranscript[] Transcript { get; set; }

        #endregion

        /// <summary>
        /// Extracts the transcript text.
        /// </summary>
        /// <returns>A string containing a text transcript.</returns>
        public string GetText()
        {
            return (Format == "ASTT" || Sections.Any())
                ? GetAsttText()
                : GetAmsText();
        }

        /// <summary>
        /// Extracts the transcript from an Azure Speech to Text (ASTT) formatted result.
        /// </summary>
        /// <returns>a string containing a text transcript.</returns>
        private string GetAsttText()
        {
            bool isFirstAppend = true;
            StringBuilder builder = new StringBuilder();
            IEnumerable<string> sections = Sections.Where(s => !string.IsNullOrWhiteSpace(s.Text)).Select(s => s.Text);
            sections.ForEach((str) =>
            {
                if (!isFirstAppend)
                {
                    isFirstAppend = false;
                }
                else
                {
                    builder.Append(' ');
                }
                builder.Append(str);
            });

            string ret = builder.ToString().Trim();
            return ret;
        }

        /// <summary>
        /// Extracts the transcript from an Azure Media Services (AMS) formatted result.
        /// </summary>
        /// <returns>a string containing a text transcript.</returns>
        private string GetAmsText()
        {
            StringBuilder builder = new StringBuilder();
            TimeSpan last = TimeSpan.Zero;
            bool isNewSectionNeeded = false;
            bool isFirstSectionLine = true;

            // Iterate over each part of the transcript
            Transcript?.ForEach(item =>
            {
                // Determine whether this is a new section
                AmsTranscriptInstance? instance = item?.Instances?.FirstOrDefault();
                if (instance != null)
                {
                    isNewSectionNeeded = instance.Start.Subtract(last).TotalMilliseconds > AmsGapThresholdinMs;
                    last = instance.End;
                }

                // Append the new section if needed
                if (isNewSectionNeeded && builder.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                    isNewSectionNeeded = false;
                    isFirstSectionLine = true;
                }

                // Make sure we have text with which to work
                if (!string.IsNullOrEmpty(item?.Text))
                {
                    // AMS text trims whitespace so we have to add it back between parts.
                    if (!isFirstSectionLine)
                    {
                        builder.Append(" ");
                    }
                    builder.Append(item.Text);
                    isFirstSectionLine = false;
                }
            });

            string result = builder.ToString().Trim();
            return result;
        }
    }
}