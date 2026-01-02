namespace Masticore.Media
{
    /// <summary>
    /// Media Effects represent an operation that can be applied to an audio/video file.
    /// </summary>
    public interface IMediaEffect
    {
        /// <summary>
        /// Gets the name of the media effect. Primarily intended for identifying .NET type of the
        /// media effect during JSON deserialization operations.
        /// </summary>
        string Type { get; }
    }
}
