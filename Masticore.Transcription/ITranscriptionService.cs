using System.Threading.Tasks;

namespace Masticore.Transcription
{
    /// <summary>
    /// Contains transcription result information with details
    /// </summary>
    public interface ITranscriptionService
    {
        /// <summary>
        /// Configures the transcription service based on the specified organization settings.
        /// </summary>
        /// <param name="orgSettings">Organization settings used to configure the transcription service.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task Initialize(IOrgSettings orgSettings);

        /// <summary>
        /// Processes the specified file and produces a transcription of its contents.
        /// </summary>
        /// <param name="filePath">Path to the file to transcribe.</param>
        /// <param name="format">Format of the file to transcribe.</param>
        /// <returns>A <see cref="TranscriptionResult"/> containing transcription information.</returns>
        Task<TranscriptionResult> TranscribeFile(string filePath, FileType format);
    }
}