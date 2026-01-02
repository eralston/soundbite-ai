namespace Soundbite.Models
{
    /// <summary>
    /// Enum for the types of how the clip arrived in the system, with indication
    /// </summary>
    /// <remarks>Tracking where a clip comes from allows for internal stats and potentially different presentation</remarks>
    public enum ClipSource
    {
        Unknown = 0,

        /// <summary>
        /// The clip arrived via custom means and it should be treated as neutral and fully-produced
        /// </summary>
        /// <remarks>This is the default for partner app uploads</remarks>
        Upload = 100,

        /// <summary>
        /// The v1 browser recording NPM package w/ filtering
        /// </summary>
        WidgetRecorderV1 = 200
    }
}
