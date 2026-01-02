using System.Collections.Generic;

namespace Soundbite.Models
{
    public class PromptWithClips : Prompt
    {
        /// <summary>
        /// Gets or sets the list of audio clips associated with prompt.
        /// </summary>
        public IEnumerable<ClipWithContributor> Clips { get; set; }
    }
}