using System.IO;
using System.Threading.Tasks;

namespace Masticore.Storage
{
    /// <summary>
    /// Interface for an object that handles files as streams
    /// </summary>
    public interface IBlobs
    {
        /// <summary>
        /// Determines if the given Blob client is operating in a public or private mode
        /// </summary>
        /// <remarks>
        /// You should upload/download to/from a given container consistently with this flag toggled; otherwise, you may have issues with its initial permissions on first use vs later use. It is *theoretically* fine, but untested.
        /// </remarks>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Uploads the given blob to the given location
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        Task<string> UploadAsync(string containerName, string blobName, Stream stream);

        /// <summary>
        /// Gets the URL for uploading to the given location
        /// </summary>
        /// <remarks>This is for outside producers (EG, Typescript) that wants to upload to the blob store w/o access via the platform</remarks>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        Task<string> UploadUrlAsync(string containerName, string blobName);

        /// <summary>
        /// Downloads the blob at the given location
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        Task<Stream> DownloadAsync(string containerName, string blobName);

        /// <summary>
        /// Gets the URL for the given location
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        Task<string> DownloadUrlAsync(string containerName, string blobName);

        /// <summary>
        /// Deletes the container matching the given name
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        Task DeleteAsync(string containerName);

        /// <summary>
        /// Deletes the specified blob from the specified container.
        /// </summary>
        /// <param name="containerName">Name of the conatiner in which the blob resides.</param>
        /// <param name="blobName">Name of the blob to delete.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task DeleteAsync(string containerName, string blobName);

        /// <summary>
        /// Async return true if the given blob exists; otherwise, false
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string containerName, string blobName);

        /// <summary>
        /// Async get the size of the block blob in bytes
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        Task<long> SizeAsync(string containerName, string blobName);

        /// <summary>
        /// Copies a blob from one container to another container.
        /// </summary>
        /// <param name="sourceContainerName">Name of the container in which the blob to copy resides.</param>
        /// <param name="sourceBlobName">Name of the blob to copy.</param>
        /// <param name="destinationContainerName">Name of the destination container.</param>
        /// <param name="destinationBlobName">Name of the destination blob.</param>        
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task CopyAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName, string destinationBlobName);

        /// <summary>
        /// Renames a blob within a container.
        /// </summary>
        /// <param name="sourceContainerName">Name of the container in which the blob to rename resides.</param>
        /// <param name="currentBlobName">Name of the blob before renaming.</param>
        /// <param name="newBlobName">Name of the blob after renaming.</param>        
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task RenameAsync(string sourceContainerName, string currentBlobName, string newBlobName);

        /// <summary>
        /// Generates an SAS token for use in accessing Azure storage content.
        /// </summary>
        /// <param name="containerName">Name of the container in which data is to be accessed.</param>
        /// <param name="blobName">Name of blob to access.  Leave null for container level SAS token.</param>
        /// <param name="isWrite">Specifies whether this token allows write access (true) or only read access (false).</param>
        /// <param name="durationInMinutes">Duration, in minutes, for which the token is valid.</param>
        /// <returns>a string representation of teh SAS token</returns>
        Task<string> TokenAsync(string containerName, string blobName, bool isWrite = false, int durationInMinutes = 60);
    }
}