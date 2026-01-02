using Azure.Core;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Storage
{
    // TODO: Consider implementing validation. Azurite doesn't seem to do much, so it would be very useful for local dev & unit testing
    // Container naming: https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata
    // Container name max length 3-63
    // Blob name  1 - 1024 characters

    /// <summary>
    /// Azure Storage implementation of <see cref="IBlobs"/>
    /// </summary>
    public class AzBlobs : IBlobs
    {
        /// <summary>
        /// Flag indicating if the system is running in connection to a local emulator. 
        /// For more info on the emulator, including how to install it, check this out: https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator
        /// </summary>
        /// <remarks>
        /// This will force public behavior, which includes making all containers public and emitting URLs without SAS tokens
        /// </remarks>
        public static bool IsLocalStorageEmulator { get; set; } = false;

        #region Static Methods

        private static void TryRewind(Stream stream)
        {
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            catch (NotSupportedException)
            {
                // No logging for this one because it happens if it's an upload from the client-side
            }
        }

        #endregion

        #region Fields

        private BlobServiceClient _service = null;

        /// <summary>
        /// Service client that connects to Azure Storage
        /// </summary>
        protected BlobServiceClient Service
        {
            get
            {
                if (_service == null)
                {
                    BlobClientOptions options = new BlobClientOptions()
                    {
                        Retry =
                        {
                            Mode = RetryMode.Exponential,
                            Delay = TimeSpan.FromSeconds(5),        // Initial delay between retries
                            MaxRetries = 6,                             // Maximum number of retries
                            MaxDelay = TimeSpan.FromSeconds(60),        // Maximum delay between retries
                            NetworkTimeout = TimeSpan.FromMinutes(5)    // Overall network timeout for each request
                        }
                    };
                    _service = new BlobServiceClient(ConnectionString, options);
                    Logger.LogInformation($"Connecting to Azure Storage Account {_service.AccountName}");
                }
                return _service;
            }
        }

        /// <summary>
        /// Sets the connection string for this client, setting up a new Service instance
        /// </summary>
        protected string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the logger for this instance
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets or sets whether this instance will create public containers and emit URLs w/o SAS tokens
        /// </summary>
        public bool IsPublic { get; set; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="logger"></param>
        public AzBlobs(string connectionString, ILogger logger)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
            }

            ConnectionString = connectionString;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the clients for the given container and blob
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        protected async Task<(BlobContainerClient, BlobClient)> Clients(string containerName, string blobName)
        {
            BlobContainerClient container = await ContainerClient(containerName);
            blobName = blobName.ToLower();
            BlobClient blob = container.GetBlobClient(blobName);
            return (container, blob);
        }

        /// <summary>
        /// Gets the client for the given container
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        protected async Task<BlobContainerClient> ContainerClient(string containerName)
        {
            containerName = containerName.ToLower();
            BlobContainerClient container = Service.GetBlobContainerClient(containerName);
            if (IsLocalStorageEmulator || IsPublic)
            {
                await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
            }
            else
            {
                await container.CreateIfNotExistsAsync(PublicAccessType.None);
            }

            return container;
        }

        /// <summary>
        /// Generate the token for the blob, optionally including a SAS token
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        protected async Task<string> UrlAsync(BlobClient blob, bool isWrite = false)
        {
            // If we're trying to read a non-existent blob, then return null to tell them it's not there
            if (!await blob.ExistsAsync() && !isWrite)
            {
                return null;
            }

            // If we're insecure (who's not really?) and building a read URL, then just return the absolute URL
            if ((IsLocalStorageEmulator || IsPublic) && !isWrite)
            {
                return blob.Uri.AbsoluteUri;
            }

            string token = await TokenAsync(blob.BlobContainerName, blob.Name, isWrite);
            string url = $"{blob.Uri.AbsoluteUri}?{token}";
            return url;
        }

        /// <summary>
        /// Generates a SAS token for the given builder
        /// TODO: Consider if we need an alternative implementation once we're using Key Vault
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected Task<string> TokenAsync(BlobSasBuilder builder)
        {
            StorageSharedKeyCredential sharedKey = ConnectionString.ToStorageSharedKeyCredential();
            string sasToken = builder.ToSasQueryParameters(sharedKey).ToString();
            return Task.FromResult(sasToken);
        }

        #endregion

        #region IBlobs

        /// <summary>
        /// Uploads the given stream to the given location, return the new URL.
        /// NOTE: This will always snapshot existing blobs to keep a record
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<string> UploadAsync(string containerName, string blobName, Stream stream)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("message", nameof(containerName));
            }

            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException("message", nameof(blobName));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                TryRewind(stream);
                (BlobContainerClient container, BlobClient blob) = await Clients(containerName, blobName);
                if ((await blob.ExistsAsync()).Value)
                {
                    await blob.CreateSnapshotAsync();   // NOTE: This will make it impossible to delete the blob until all snaps are gone
                }

                await blob.UploadAsync(stream, true);
                string url = await UrlAsync(blob);
                return url;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to upload blob {blobName} in container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Requests an upload URL that can be used by outside consumers to create or overwrite to the blob's location
        /// </summary>
        /// <remarks>This will always snapshot if it's an existing blob to keep a record</remarks>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<string> UploadUrlAsync(string containerName, string blobName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException($"'{nameof(containerName)}' cannot be null or empty.", nameof(containerName));
            }

            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException($"'{nameof(blobName)}' cannot be null or empty.", nameof(blobName));
            }

            try
            {
                (BlobContainerClient container, BlobClient blob) = await Clients(containerName, blobName);
                if ((await blob.ExistsAsync()).Value)
                {
                    await blob.CreateSnapshotAsync();   // NOTE: This will make it impossible to delete the blob until all snaps are gone
                }

                string uploadUrl = await UrlAsync(blob, true);
                return uploadUrl;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to generate upload url to blob {blobName} in container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Downloads the given blob in the given container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<Stream> DownloadAsync(string containerName, string blobName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("message", nameof(containerName));
            }

            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException("message", nameof(blobName));
            }

            try
            {
                (_, BlobClient blob) = await Clients(containerName, blobName);
                BlobDownloadInfo file = await blob.DownloadAsync();
                return file.Content;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to download blob {blobName} in container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }


        /// <summary>
        /// Gets the SAS token enhanced URL for the given blob in the given container
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<string> DownloadUrlAsync(string containerName, string blobName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("message", nameof(containerName));
            }

            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException("message", nameof(blobName));
            }
            try
            {
                (_, BlobClient blob) = await Clients(containerName, blobName);
                string url = await UrlAsync(blob);
                return url;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to connect to blob {blobName} in container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Deletes the given container, including ALL of its blobs
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public async Task DeleteAsync(string containerName)
        {
            if (containerName is null)
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            try
            {
                BlobContainerClient container = await ContainerClient(containerName);
                await container.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to delete container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string containerName, string blobName)
        {
            Validator.ArgNotNullOrEmpty(nameof(containerName), containerName);
            Validator.ArgNotNullOrEmpty(nameof(blobName), blobName);

            try
            {
                BlobContainerClient container = await ContainerClient(containerName);
                BlobClient blob = container.GetBlobClient(blobName);
                await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to delete container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }


        public async Task<bool> ExistsAsync(string containerName, string blobName)
        {
            try
            {
                (BlobContainerClient container, BlobClient blob) = await Clients(containerName, blobName);
                Azure.Response<bool> response = await blob.ExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to evaluate if blob {blobName} exists in container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }

        public async Task<long> SizeAsync(string containerName, string blobName)
        {
            try
            {
                (BlobContainerClient container, BlobClient blob) = await Clients(containerName, blobName);
                Azure.Response<BlobProperties> properties = await blob.GetPropertiesAsync();
                return properties.Value.ContentLength;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error trying to evaluate size for blob {blobName} in container {containerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Copies a blob from one container to another container.
        /// </summary>
        /// <param name="sourceContainerName">Name of the container in which the blob to copy resides.</param>
        /// <param name="sourceBlobName">Name of the blob to copy.</param>
        /// <param name="destinationContainerName">Name of the destination container.</param>
        /// <param name="destinationBlobName">Name of the destination blob.</param>        
        /// <returns>a task indicating success or failure of the operation.</returns>
        public async Task CopyAsync(string sourceContainerName, string sourceBlobName, string destinationContainerName, string destinationBlobName)
        {
            try
            {
                string url = await DownloadUrlAsync(sourceContainerName, sourceBlobName);
                (BlobContainerClient containerClient, BlobClient blobClient) = await Clients(destinationContainerName, destinationBlobName);
                CopyFromUriOperation op = await blobClient.StartCopyFromUriAsync(new Uri(url));
                await op.WaitForCompletionAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to copy blob {sourceBlobName} in container {sourceContainerName} to destination blob {destinationBlobName} in container {destinationContainerName} with error {ex.Message} with trace {ex.StackTrace}");
                throw;
            }
        }

        public async Task RenameAsync(string containerName, string currentBlobName, string newBlobName)
        {
            string errorMsg = $"Failed to copy {currentBlobName} to {newBlobName} in container {containerName}.";
            try
            {
                await CopyAsync(containerName, currentBlobName, containerName, newBlobName);
                errorMsg = $"Copy succeeded but failed to delete source file {currentBlobName} in container {containerName}.";
                await DeleteAsync(containerName, currentBlobName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, errorMsg);
                throw;
            }
        }

        /// <summary>
        /// Generates an SAS token for use in accessing Azure storage content.
        /// </summary>
        /// <param name="containerName">Name of the container in which data is to be accessed.</param>
        /// <param name="blobName">Name of blob to access.  Leave null for container level SAS token.</param>
        /// <param name="isWrite">Specifies whether this token allows write access (true) or only read access (false).</param>
        /// <param name="durationInMinutes">Duration, in minutes, for which the token is valid.</param>
        /// <returns>a string representation of teh SAS token</returns>
        public async Task<string> TokenAsync(string containerName, string blobName, bool isWrite = false, int durationInMinutes = 60)
        {
            blobName = string.IsNullOrEmpty(blobName) ? null : blobName; // Make sure it is null not empty
            DateTimeOffset startsOn = DateTimeOffset.UtcNow.AddMinutes(-1);
            DateTimeOffset expiresOn = DateTimeOffset.UtcNow.AddMinutes(durationInMinutes);
            BlobSasBuilder builder = new BlobSasBuilder
            {
                StartsOn = startsOn,
                ExpiresOn = expiresOn,
                BlobContainerName = containerName,
                BlobName = blobName
            };

            // Determines the correct permissions for this token
            BlobSasPermissions permissions = isWrite ? BlobSasPermissions.Write | BlobSasPermissions.Create : BlobSasPermissions.Read;
            builder.SetPermissions(permissions);

            string token = await TokenAsync(builder);
            return token;
        }

        // TODO: Deleting a blob would require snapshot clearing functionality, etc. Not needed until we need to be able to purge

        #endregion
    }
}
