using Masticore;
using Masticore.Media;

namespace Soundbite.Models.JobRequests
{
    /// <summary>
    /// Represents a request for processing media for a clip.
    /// </summary>   
    public class MediaOperationRequest
    {
        public string OrgRoute { get; set; }
        public string SessionRoute { get; set; }
        public string PromptRoute { get; set; }
        public string ClipRoute { get; set; }
        public FileType ClipFileType { get; set; }

        public IMediaEffect[] MediaEffects { get; set; }

        public virtual void Validate()
        {
            Validator.NotNullOrEmpty(nameof(OrgRoute), OrgRoute, "Video processing request is invalid because OrgRoute was not specified.");
            Validator.NotNullOrEmpty(nameof(SessionRoute), SessionRoute, "Video processing request is invalid because SessionRoute was not specified.");
            Validator.NotNullOrEmpty(nameof(PromptRoute), PromptRoute, "Video processing request is invalid because PromptRoute was not specified.");
            Validator.NotNullOrEmpty(nameof(ClipRoute), ClipRoute, "Video processing request is invalid because ClipRoute was not specified.");
            Validator.NotNull(MediaEffects != null && MediaEffects.Length > 0, "Video processing request is invalid because there are no media effect requests.");
        }
    }
}