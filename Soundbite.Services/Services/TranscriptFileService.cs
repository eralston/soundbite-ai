using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Services;
using Masticore.Storage;
using Masticore.Transcription;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using System.IO;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// <see cref="ITranscriptFileService"/> implementation over EF
    /// </summary>
    /// <remarks>Does NOT implement RBAC</remarks>
    public class TranscriptFileService : InfrastructureServiceBase<ISbInfrastructure>, ITranscriptFileService
    {
        public TranscriptFileService(
            ISbInfrastructure infrastructure,
            ILogger<BillingService> logger)
            : base(infrastructure, logger)
        {

        }

        /// <summary>
        /// Generates the blob location for the given clip
        /// </summary>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        public static string GetTranscriptBlobName(string sessionRoute, string promptRoute, string clipRoute)
        {
            // TODO: Remove the dollar sign in this route once we do migration work
            return $"ses${sessionRoute}/pro{promptRoute}/clp{clipRoute}/clip.transcript.json";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        public async Task<(string containerName, string blobName)> GetContainerAndBlobNames(
            string orgRoute,
            string sessionRoute,
            string promptRoute,
            string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();

            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound();


            string containerName = ClipFileService.ContainerName(org);
            string blobName = GetTranscriptBlobName(sessionRoute, promptRoute, clipRoute);
            return (containerName, blobName);
        }

        #region ITranscriptFileService

        /// <inheritdoc />
        public async Task<TranscriptionResult> DownloadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            (string containerName, string blobName) = await GetContainerAndBlobNames(orgRoute, sessionRoute, promptRoute, clipRoute);
            IBlobs blobService = await Infrastructure.BlobsAsync();
            using Stream stream = await blobService.DownloadAsync(containerName, blobName);
            string content = await stream.ToStringContent();
            TranscriptionResult result = JsonUtils.FromLowerCamelJson<TranscriptionResult>(content);
            return result;
        }

        /// <inheritdoc />
        public async Task<string> UploadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, TranscriptionResult result)
        {
            (string containerName, string blobName) = await GetContainerAndBlobNames(orgRoute, sessionRoute, promptRoute, clipRoute);
            MemoryStream jsonStream = result.ToLowerCamelJson().ToMemoryStream();
            IBlobs blobService = await Infrastructure.BlobsAsync();
            return await blobService.UploadAsync(containerName, blobName, jsonStream);
        }

        /// <inheritdoc />
        public async Task<string> DownloadUrlAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            (string containerName, string blobName) = await GetContainerAndBlobNames(orgRoute, sessionRoute, promptRoute, clipRoute);
            IBlobs blobService = await Infrastructure.BlobsAsync();
            return await blobService.DownloadUrlAsync(containerName, blobName);
        }

        #endregion
    }
}
