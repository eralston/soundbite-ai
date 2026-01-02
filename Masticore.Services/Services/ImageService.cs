using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Storage;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// For managing images of domain objects
    /// </summary>
    public class ImageService : IdentityServiceBase, IImageService
    {
        public ImageService(IIdentityInfrastructure infrastructure, ILogger<ImageService> logger)
            : base(infrastructure, logger)
        {
        }

        // Utilities

        /// <summary>
        /// Uploads the given <see cref="Stream"/> to the given container and location
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected async Task<string> UploadAsync(string containerName, string blobName, Stream stream)
        {
            IBlobs blobs = await Infrastructure.BlobsAsync();
            return await blobs.UploadAsync(containerName, blobName, stream);
        }

        /// <summary>
        /// Async retrieves the URL for the given container and blob
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        private async Task<string> UrlAsync(string containerName, string blobName)
        {
            // If you're getting this exception here, then you need to start your local storage emulator (legacy or azurite)
            // System.AggregateException - Retry failed after 6 tries. (No connection could be made because the target machine actively refused it.)
            IBlobs blobs = await Infrastructure.BlobsAsync();
            return await blobs.DownloadUrlAsync(containerName, blobName);
        }

        // User

        /// <summary>
        /// Generates a unique blob path for the given user (as <see cref="IUniversal"/>)
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected (string, string) UserPath(IUniversal user)
        {
            string userContainerName = user.ToUserContainerName();
            string blobName = user.ToOriginalImageBlobName();
            return (userContainerName, blobName);
        }

        /// <summary>
        /// Generates a unique blob path for the given <see cref="Organization"/>
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        protected (string, string) OrgPath(Organization org)
        {
            string containerName = org.ToContainerName();
            string blobName = org.ToImageBlobName();
            return (containerName, blobName);
        }

        // IImageService

        /// <summary>
        /// Uploads the given images as the avatar for the user, then returns the src-friendly URL
        /// </summary>
        /// <param name="user"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task<string> UploadUserImageAsync(IUniversal user, Stream stream)
        {
            (string containerName, string blobName) = UserPath(user);
            return await UploadAsync(containerName, blobName, stream);
        }

        /// <summary>
        /// Async gets the secure URL to the given user's profile pick
        /// If the image doesn't exist, this returns null
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string> UserImageAsync(IUniversal user)
        {
            // TODO: Caching

            (string containerName, string blobName) = UserPath(user);
            string url = await UrlAsync(containerName, blobName);
            return url;
        }

        /// <summary>
        /// Persists an organization image to a data store.
        /// </summary>
        /// <param name="orgArbitraryKey">Unique but non-identifying arbitrary key associated with the organization (for now use orgRoute)</param>
        /// <param name="stream">Stream containing image data.</param>
        /// <returns>a URL pointing to the image that was uploaded.</returns>
        public async Task<string> UploadOrgImageAsync(Organization org, Stream stream)
        {
            (string containerName, string blobName) = OrgPath(org);
            return await UploadAsync(containerName, blobName, stream);
        }

        /// <summary>
        /// Retrieves a URL pointing to the image associated with the organization.
        /// </summary>
        /// <param name="orgArbitraryKey">Unique but non-identifying arbitrary key associated with the organization (for now use orgRoute)</param>
        /// <returns>a URL pointing to the image associated with the organization or <c>null</c> if the image is not found.</returns>
        public async Task<string> OrgImageAsync(Organization org)
        {
            (string containerName, string blobName) = OrgPath(org);
            string url = await UrlAsync(containerName, blobName);
            return url;
        }
    }
}
