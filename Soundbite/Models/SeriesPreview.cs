namespace Soundbite.Models
{

    /// <summary>
    /// <see cref="Series"/> plus its template <see cref="SessionPreview"/> object
    /// </summary>
    public class SeriesPreview : Series
    {
        /// <summary>
        /// Gets or sets a reference to a session that acts as the template for generating new 
        /// sessions in the recurring series.
        /// </summary>        
        public SessionPreview Template { get; set; }
    }
}
