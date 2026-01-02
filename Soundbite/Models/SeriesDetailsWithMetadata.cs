namespace Soundbite.Models
{
    /// <summary>
    /// The complete payload of a <see cref="Models.SeriesDetails"/> plus the template session's route
    /// </summary>
    public class SeriesDetailsWithMetadata
    {
        /// <summary>
        /// <see cref="Models.SeriesDetails"/> for this series
        /// </summary>
        public SeriesDetails SeriesDetails { get; set; }

        /// <summary>
        /// Route for the associated <see cref="Session"/>
        /// </summary>
        public string TemplateSessionRoute { get; set; }
    }
}
