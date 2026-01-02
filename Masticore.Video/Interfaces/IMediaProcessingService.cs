using System;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Media
{
    /// <summary>
    /// Contract that exposes various video encoding and manipulation methods.
    /// </summary>
    public interface IMediaProcessingService
    {
        /// <summary>
        /// Determines the length of the specified video.
        /// </summary>
        /// <param name="videoPath">Path to the video.</param>
        /// <returns>a <see cref="TimeSpan"/> populated with the duration of the video.</returns>
        Task<TimeSpan> GetVideoDuration(string videoPath);

        /// <summary>
        /// Determines the length of the specified video.
        /// </summary>
        /// <param name="videoStream">Stream containing video data.</param>
        /// <returns>a <see cref="TimeSpan"/> populated with the duration of the video.</returns>
        Task<TimeSpan> GetVideoDuration(Stream videoStream);

        /// <summary>
        /// Responsible for processing the specified effects a video file
        /// </summary>
        /// <param name="inputFile">Input path to the video to modify.</param>
        /// <param name="outputFile">Output path to the resulting video.</param>
        /// <param name="effects">List of media effects to apply during rendering.</param>
        /// <returns>a string containing the path of the processed video file.</returns>
        Task<string> ProcessVideoFile(string inputFile, string outputFile, IMediaEffect[] effects);

        /// <summary>
        /// Ensures that the duration header is set in a video file.
        /// </summary>
        /// <param name="videoPath">Path to the video file.</param>
        /// <returns>a stream containing the updated video file with the duration header set.</returns>
        Task<string> EnsureDurationHeader(string videoPath);

        /// <summary>
        /// Ensures that the duration header is set in a video file.
        /// </summary>
        /// <param name="videoStream">Stream containing the video file.</param>
        /// <returns>a stream containing the updated video file with the duration header set.</returns>
        Task<string> EnsureDurationHeader(Stream videoStream);

        /// <summary>
        /// Retrieves media details from the specified video.
        /// </summary>
        /// <param name="videoPath"></param>
        Task<MediaDetails> GetMediaDetails(string videoPath);

        /// <summary>
        /// Encodes the given segment
        /// </summary>
        /// <param name="videoPath"></param>
        Task EncodeSegment(string segmentPath, string outputFolder, int width, int height, int rotation);

        /// <summary>
        /// Responsible for splitting a video file into segments.
        /// </summary>
        /// <param name="inputFile">Path to the file to split.</param>
        /// <param name="outputMask">String containing the output file mask specifying where video segments should be placed.</param>
        /// <returns>an array of string identifying the segment files the video was split into.</returns>
        Task<string[]> SplitVideo(string inputFile, string outputMask, int rotation);

        /// <summary>
        /// Extracts a WAV file from a media file.
        /// </summary>
        /// <param name="inputPath">Path to the input file.</param>
        /// <param name="inputFileType">Specifies the format of the input file.</param>
        /// <param name="outputPath">Path to the output WAV file.</param>        
        /// <returns>a string containing the filename of the extracted wav fie.</returns>
        Task ExtractWav(string inputPath, FileType inputFileType, string wavPath);

        /// <summary>
        /// Responsible for extracting audio from a video file.
        /// </summary>
        /// <param name="inputPath">Video input file.</param>
        /// <param name="outputPath">Audio output file.</param>
        /// <returns>a task indicating result of the operation.</returns>
        Task ExtractAudioFromVideoFile(string inputPath, string outputPath);
    }
}