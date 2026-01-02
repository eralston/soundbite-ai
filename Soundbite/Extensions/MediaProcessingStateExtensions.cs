namespace Soundbite.Extensions
{
    /// <summary>
    /// Extensions for the MediaProcessingState enumeration.
    /// </summary>
    public static class MediaProcessingStateExtensions
    {
        /// <summary>
        /// Performs a check to determine if the state represents a "Ready" state.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns><c>true</c> if the value is a "Ready" state value otherwise <c>false</c>.</returns>
        public static bool IsReady(this MediaProcessingState value)
        {
            switch (value)
            {
                case MediaProcessingState.None:
                case MediaProcessingState.Complete:
                    return true;
                default:
                    return false;
            }
        }
    }
}