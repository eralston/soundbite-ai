namespace Soundbite
{
    /// <summary>
    /// Extension methods for sessions.
    /// </summary>
    public static class IClipTypeExtensions
    {
        /// <summary>
        /// Determines whether the clip type value is known to support transcription.
        /// </summary>
        /// <param name="value">ClipType value whose transcription status is being sought.</param>
        /// <returns><c>true</c> if the clip type is known to support transscription, otherwise <c>false</c>.</returns>
        public static bool SupportsTranscription(this ClipType value)
        {
            switch (value)
            {
                case ClipType.Prompt:
                case ClipType.Contribution:
                case ClipType.Comment:
                    return true;
            }
            return false;
        }
    }
}