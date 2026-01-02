using Masticore;

namespace Soundbite.Models.JobRequests
{
    /// <summary>
    /// Represents a request for the transcription of a clip.
    /// </summary>   
    public class TranscriptionRequest
    {
        public string OrgRoute { get; set; }
        public string SessionRoute { get; set; }
        public string PromptRoute { get; set; }
        public string ClipRoute { get; set; }
        public FileType ClipFileType { get; set; }

        public void Validate()
        {
            Validator.NotNullOrEmpty(nameof(OrgRoute), OrgRoute, "Transcription request is invalid because OrgRoute was not specified.");
            Validator.NotNullOrEmpty(nameof(SessionRoute), SessionRoute, "Transcription request is invalid because SessionRoute was not specified.");
            Validator.NotNullOrEmpty(nameof(PromptRoute), PromptRoute, "Transcription request is invalid because PromptRoute was not specified.");
            Validator.NotNullOrEmpty(nameof(ClipRoute), ClipRoute, "Transcription request is invalid because ClipRoute was not specified.");
        }
    }
}