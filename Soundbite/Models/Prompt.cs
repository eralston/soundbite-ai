using Masticore.Models;

namespace Soundbite.Models
{
    /// <summary>
    /// A prompt represents a call to action in a session and the audio clips associated with that
    /// response.  The prompt text should indicate how a session participant should respond to the 
    /// prompt or what the audio clips in response to the prompt contain.
    /// </summary>
    /// <seealso cref="Soundbite.Resources.IPromptWithClips" />
    public class Prompt : ResourceBase
    {
        /// <summary>
        /// Gets or sets the text used to prompt users for their contribution to the session.
        /// </summary>
        public string Text { get; set; }
    }
}
