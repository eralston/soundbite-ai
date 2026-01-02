namespace Masticore.Media
{
    /// <summary>
    /// Media effect representing an Azure Transform operation.
    /// </summary>
    public class SbMediaEncoding : IMediaEffect
    {
        #region IMediaEffect Implementation

        /// <inheritdoc />
        /// </summary>
        public string Type => nameof(SbMediaEncoding);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SbMediaEncoding"/> instance.
        /// </summary>        
        public SbMediaEncoding()
        {

        }

        #endregion
    }
}