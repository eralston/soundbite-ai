namespace Soundbite.Models
{
    /// <summary>
    /// Enum for when the clip is uploading and processing on its way toward being ready for consumption
    /// </summary>
    public enum ClipState
    {
        Unknown = 0,

        /// <summary>
        /// Denotes that the clip has served its purpose and is no longer applicable to the prompt.
        /// </summary>
        Obsolete = 10,

        /// <summary>
        /// Only the record in the DB exists with no indication that the file itself has been deposited.
        /// This is analogous to being in "draft" status.
        /// </summary>
        Started = 100,

        /// <summary>
        /// The clip recording has been put into the platform and it is ready for validation and processing
        /// </summary>
        Committed = 200,

        /// <summary>
        /// The system has determined it is a valid clip and it is ready for any post-processing
        /// </summary>
        Validated = 300,

        // TODO: Transcription processing? Probably another field on ClipEntity to decouple from validation

        /// <summary>
        /// The clip is ready for a workflow to let it out into the world, though the owning <see cref="PromptEntity"/> and <see cref="SessionEntity"/> have their own process for actually pushing it out into the world
        /// </summary>
        Ready = 400,


    }
}
