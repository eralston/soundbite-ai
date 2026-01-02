using Masticore;
using Masticore.Models;
using Masticore.Services;
using System.IO;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the service operations required to manage clip files.
    /// </summary>
    public interface IClipFileService : IService
    {
        /// <summary>
        /// Returns true or false that the given clip has a relevant file in the system
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Uploads the given <see cref="Stream"/> for the given clip
        /// </summary>
        /// <param name="org"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="fileType"></param>
        /// <param name="stream"></param>
        /// <returns>Secure download URL for the newly uploaded clip; must be used quickly</returns>
        Task<string> UploadAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType, Stream stream);

        /// <summary>
        /// Returns a secure upload URL for the given clip in the given location
        /// </summary>
        /// <remarks>This does NOT do anything more than make a URL; unless they upload something quickly the location will have NOTHING</remarks>
        /// <param name="org"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="fileType"></param>
        /// <returns>Secure upload URL for the given file</returns>
        Task<string> UploadUrlAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType);

        /// <summary>
        /// Async return the size of the given clip in bytes
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<long> SizeAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Returns a secure download URL for the given clip in the given location
        /// </summary>
        /// <param name="org"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="fileType"></param>
        /// <returns>Secure download URL for the clip; must be used quickly</returns>
        Task<string> DownloadUrlAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType);

        /// <summary>
        /// Downloads the blob associated with the given clip
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<Stream> DownloadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Responsible for downloading the .wav file produced during video encoding that can be
        /// used for transcription.
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<Stream> DownloadAudioExtractedFromVideoClipAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Responsible for getting a URL to a SB Media hosted media stream along with any necessary token.
        /// </summary>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <param name="clipRoute">Route of the clip.</param>
        /// <returns>a string containing the download URL + token for the HLS manifest.</returns>
        Task<string> SbMediaManifestDownloadUrlAsync(string orgRoute, string clipRoute);

        /// <summary>
        /// Responsible for getting a URL to a SB Media hosted media stream along with any necessary token.
        /// </summary>
        /// <param name="orgUid">UID of the organization.</param>
        /// <param name="sessionRoute">Route of the session.</param>
        /// <param name="promptRoute">Route of the prompt.</param>
        /// <param name="clipRoute">Route of the clip.</param>
        /// <returns>a string containing the download URL + token for the HLS manifest.</returns>
        Task<string> SbMediaManifestDownloadUrlAsync(string orgUid, string sessionRoute, string promptRoute, string clipRoute);
    }
}
