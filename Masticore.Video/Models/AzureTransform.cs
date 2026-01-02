namespace Masticore.Media
{
    /// <summary>
    /// Media effect representing an Azure Transform operation.
    /// </summary>
    public class AzureTransform : IMediaEffect
    {
        #region IMediaEffect Implementation

        /// <inheritdoc />
        /// </summary>
        public string Type => nameof(AzureTransform);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the Transform name 
        /// </summary>
        public string TransformName { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this transform is for an encoding operation.
        /// </summary>
        public bool IsEncoding { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="AzureTransform"/> instance.
        /// </summary>
        /// <param name="tranformName">Name of the Azure transform operation to run.</param>
        /// <param name="isEncoding">Flag indicating whether this is an encoding transform.</param>
        public AzureTransform(string tranformName, bool isEncoding)
        {
            TransformName = tranformName;
            IsEncoding = isEncoding;
        }

        #endregion
    }
}