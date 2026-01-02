using System;

namespace Masticore
{
    /// <summary>
    /// Extensions for the <see cref="FileType"/> enumeration.
    /// </summary>
    public static class FileTypeExtensions
    {
        /// <summary>
        /// Gets the extension associated with the specified <paramref name="fileType"/>. The 
        /// extensions does NOT include a period.
        /// </summary>
        /// <param name="fileType">FileType for which an extension is being sought.</param>
        /// <returns>an extension appropriate for the file type value.</returns>
        /// <exception cref="Exception">Thrown when a file type does not have an associated extension.</exception>
        public static string GetExtension(this FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Avi:
                    return "avi";
                case FileType.F4v:
                    return "f4v";
                case FileType.Flv:
                    return "flv";
                case FileType.Mkv:
                    return "mkv";
                case FileType.Mov:
                    return "mov";
                case FileType.Mp3:
                    return "mp3";
                case FileType.Mp4:
                    return "mp4";
                case FileType.Mpg:
                    return "mpg";
                case FileType.MpgAudio:
                    return "mp3";
                case FileType.Wav:
                    return "wav";
                case FileType.Webm:
                    return "webm";
                case FileType.Wmv:
                    return "wmv";
                default:
                    throw new Exception($"FileType {fileType} does not have an extension defined.");
            }
        }

        /// <summary>
        /// Determines whether the specified file type is associated with video.
        /// </summary>
        /// <param name="fileType"><see cref="FileType"/> value.</param>
        /// <returns><c>true</c> if the file type is associated with video, otherwise <c>false.</c></returns>
        /// <exception cref="Exception"></exception>
        public static bool IsVideo(this FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Mp3:
                case FileType.Wav:
                case FileType.MpgAudio:
                    return false;
                case FileType.Avi:
                case FileType.F4v:
                case FileType.Flv:
                case FileType.Mov:
                case FileType.Mpg:
                case FileType.Mp4:
                case FileType.Mkv:
                case FileType.Webm:
                case FileType.Wmv:
                    return true;
                default:
                    throw new Exception($"Specified file type {fileType} is unknown.");
            }
        }

        /// <summary>
        /// Determines whether the specified file type is associated with audio.
        /// </summary>
        /// <param name="fileType"><see cref="FileType"/> value.</param>
        /// <returns><c>true</c> if the file type is associated with audio, otherwise <c>false.</c></returns>
        /// <exception cref="Exception"></exception>
        public static bool IsAudio(this FileType fileType)
        {
            return !IsVideo(fileType);
        }
    }
}
