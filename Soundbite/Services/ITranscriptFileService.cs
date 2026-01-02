using Masticore.Services;
using Masticore.Transcription;
using System.Threading.Tasks;

namespace Soundbite
{
    /// <summary>
    /// Upload and download transcript files from storage
    /// </summary>
    public interface ITranscriptFileService : IService
    {
        /// <summary>
        /// Upload the given <see cref="TranscriptionResult"/> to the given clip
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        Task<string> UploadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, TranscriptionResult result);

        /// <summary>
        /// Download the <see cref="TranscriptionResult"/> for the given clip
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<TranscriptionResult> DownloadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Get the download URL for the given clip transcript
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<string> DownloadUrlAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);
    }
}
