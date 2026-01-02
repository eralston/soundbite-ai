using Masticore;

namespace Soundbite.Models
{
    /// <summary>
    /// Response that clarifies the disposition of the given clip
    /// </summary>
    [CodeGenModel]
    public class ClipDetails
    {
        /// <summary>
        /// Gets or sets the route to the attach organization
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// Gets or sets the route to the attached session
        /// </summary>
        public string SessionRoute { get; set; }

        /// <summary>
        /// Gets or sets the route for the prompt
        /// </summary>
        public string PromptRoute { get; set; }

        /// <summary>
        /// Gets or sets the route for the newly available clip
        /// </summary>
        public string ClipRoute { get; set; }

        /// <summary>
        /// If present, this is the secure AzStorage URL for the u
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// If present, this is the secure AzStorage URL to upload the blob. A put to this URL will place the contents into storage in the correction location for SB to read it later
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string UploadUrl { get; set; }

        /// <summary>
        /// Gets or sets the current lifecycle step for this clip
        /// </summary>
        public ClipState State { get; set; }

        /// <summary>
        /// Specifies how the clip file is hosted.
        /// </summary>
        public ClipHostingType HostingType { get; set; }

        /// <summary>
        /// Gets or sets a string containing information that in conjunction with the 
        /// <see cref="ClipHostingType"/> identifies how to locate the clip file.
        /// </summary>
        public string HostingData { get; set; }

        /// <summary>
        /// Gets or sets the 
        /// </summary>
        public TranscriptState TranscriptState { get; set; }
    }
}