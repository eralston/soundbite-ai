using Masticore;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Masticore.Storage;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Implements upload and secure URL generation of clips
    /// WARNING: Does NOT do RBAC
    /// </summary>
    public class ClipFileService : InfrastructureServiceBase<ISbInfrastructure>, IClipFileService
    {
        public ClipFileService(
            ISecurityContext securityContext,
            ISbInfrastructure infrastructure,
            ILogger<ClipFileService> logger)
            : base(infrastructure, logger, securityContext)
        {
        }

        public static string BlobName(string sessionRoute, string promptRoute, string clipRoute, FileType fileType)
        {
            string extension = fileType.GetExtension();
            return $"ses${sessionRoute}/pro{promptRoute}/clp{clipRoute}/clip.{extension}";
        }

        public static string ContainerName(IUniversal orgUid)
        {
            return $"org{orgUid.UniversalId}";
        }

        private async Task CreateClipEventWithDisplaySeconds(string clipRoute, ClipEventType clipEventType)
        {
            SbDb db = await Infrastructure.DbAsync();
            await db.CreateClipEventWithDisplaySeconds(clipRoute, clipEventType, (SecurityContext.CurrentUser == null)
                ? (int?)null
                : SecurityContext.CurrentUserId);
        }

        private async Task<string> DownloadUrl(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType)
        {
            // TODO: Caching?
            // TODO: When there is finally caching, this should NOT save a ClipEventType.ServerUnlockForRead event unless the URL was re-generated from the blobs object
            IBlobs blobs = await Infrastructure.BlobsAsync();
            string containerName = ContainerName(org);
            string blobName = BlobName(sessionRoute, promptRoute, clipRoute, fileType);
            string url = await blobs.DownloadUrlAsync(containerName, blobName);
            // Record an event because we are not sharing it
            await CreateClipEventWithDisplaySeconds(clipRoute, ClipEventType.ServerUnlockForRead);
            return url;
        }

        #region IClipFileService

        public async Task<string> UploadAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType, Stream stream)
        {
            Validator.ArgNotNull(nameof(org), org);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNullOrEmpty(nameof(promptRoute), promptRoute);
            Validator.ArgNotNullOrEmpty(nameof(clipRoute), clipRoute);
            Validator.ArgNotNull(nameof(stream), stream);

            Logger.LogDebug("Uploading clip for org {0} session {1} prompt {2} clip {3}", org.Route, sessionRoute, promptRoute, clipRoute);

            IBlobs blobs = await Infrastructure.BlobsAsync();
            string containerName = ContainerName(org);
            string blobName = BlobName(sessionRoute, promptRoute, clipRoute, fileType);
            string url = await blobs.UploadAsync(containerName, blobName, stream);
            await CreateClipEventWithDisplaySeconds(clipRoute, ClipEventType.ServerUpload);

            return url;
        }

        public async Task<string> UploadUrlAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType)
        {
            Validator.ArgNotNull(nameof(org), org);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNullOrEmpty(nameof(promptRoute), promptRoute);
            Validator.ArgNotNullOrEmpty(nameof(clipRoute), clipRoute);

            Logger.LogDebug("Upload URL for org {0} session {1} prompt {2} clip {3}", org.Route, sessionRoute, promptRoute, clipRoute);

            IBlobs blobs = await Infrastructure.BlobsAsync();
            string containerName = ContainerName(org);
            string blobName = BlobName(sessionRoute, promptRoute, clipRoute, fileType);
            string url = await blobs.UploadUrlAsync(containerName, blobName);
            await CreateClipEventWithDisplaySeconds(clipRoute, ClipEventType.ServerUnlockForWrite);

            return url;
        }

        public async Task<string> DownloadUrlSbManifest(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType)
        {
            Validator.ArgNotNull(nameof(org), org);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNullOrEmpty(nameof(clipRoute), clipRoute);
            Validator.ArgNotNullOrEmpty(nameof(promptRoute), promptRoute);

            Logger.LogDebug("Downloading manifest URL for org {0} session {1} prompt {2} clip {3}", org.Route, sessionRoute, promptRoute, clipRoute);

            IBlobs blobs = await Infrastructure.BlobsAsync();
            string containerName = ContainerName(org);
            string blobName = BlobName(sessionRoute, promptRoute, clipRoute, fileType);
            string url = await blobs.DownloadUrlAsync(containerName, blobName);
            // Record an event because we are not sharing it
            await CreateClipEventWithDisplaySeconds(clipRoute, ClipEventType.ServerUnlockForRead);
            return url;
        }

        public async Task<string> DownloadUrlAsync(Organization org, string sessionRoute, string promptRoute, string clipRoute, FileType fileType)
        {
            Validator.ArgNotNull(nameof(org), org);
            Validator.ArgNotNull(nameof(promptRoute), promptRoute);
            Validator.ArgNotNull(nameof(clipRoute), clipRoute);

            Logger.LogDebug("Downloading URL for org {0} session {1} prompt {2} clip {3}", org.Route, sessionRoute, promptRoute, clipRoute);

            string url = await DownloadUrl(org, sessionRoute, promptRoute, clipRoute, fileType);
            // TODO: determine the secure vs public event type based on a potential future "public" sharing feature
            const ClipEventType eventType = ClipEventType.ServerShareLinkSecure;
            await CreateClipEventWithDisplaySeconds(clipRoute, eventType);
            return url;
        }

        public async Task<bool> ExistsAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(sessionRoute))
            {
                throw new ArgumentException($"'{nameof(sessionRoute)}' cannot be null or empty.", nameof(sessionRoute));
            }

            if (string.IsNullOrEmpty(promptRoute))
            {
                throw new ArgumentException($"'{nameof(promptRoute)}' cannot be null or empty.", nameof(promptRoute));
            }

            if (string.IsNullOrEmpty(clipRoute))
            {
                throw new ArgumentException($"'{nameof(clipRoute)}' cannot be null or empty.", nameof(clipRoute));
            }

            Logger.LogDebug("Checking if clip exists for org {0} session {1} prompt {2} clip {3}", orgRoute, sessionRoute, promptRoute, clipRoute);

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = (await db.OrgWhereRoute(orgRoute)).AssertFound();
            // WARNING: No RBAC!
            ClipEntity clip = (await db.ClipAsync(orgRoute, clipRoute)).AssertFound();
            string containerName = ContainerName(org);
            string blobName = BlobName(sessionRoute, promptRoute, clipRoute, clip.FileType);
            IBlobs blobs = await Infrastructure.BlobsAsync();
            return await blobs.ExistsAsync(containerName, blobName);
        }

        public async Task<long> SizeAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            if (string.IsNullOrEmpty(sessionRoute))
            {
                throw new ArgumentException($"'{nameof(sessionRoute)}' cannot be null or empty.", nameof(sessionRoute));
            }

            if (string.IsNullOrEmpty(promptRoute))
            {
                throw new ArgumentException($"'{nameof(promptRoute)}' cannot be null or empty.", nameof(promptRoute));
            }

            if (string.IsNullOrEmpty(clipRoute))
            {
                throw new ArgumentException($"'{nameof(clipRoute)}' cannot be null or empty.", nameof(clipRoute));
            }

            Logger.LogDebug("Getting size of clip for org {0} session {1} prompt {2} clip {3}", orgRoute, sessionRoute, promptRoute, clipRoute);

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = (await db.OrgWhereRoute(orgRoute)).AssertFound();
            // WARNING: No RBAC!
            ClipEntity clip = (await db.ClipAsync(orgRoute, clipRoute)).AssertFound();
            string containerName = ContainerName(org);
            string blobName = BlobName(sessionRoute, promptRoute, clipRoute, clip.FileType);
            IBlobs blobs = await Infrastructure.BlobsAsync();
            return await blobs.SizeAsync(containerName, blobName);
        }

        public async Task<Stream> DownloadAudioExtractedFromVideoClipAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNullOrEmpty(nameof(promptRoute), promptRoute);
            Validator.ArgNotNullOrEmpty(nameof(clipRoute), clipRoute);

            Logger.LogDebug("Downloading audio from video clip for org {0} session {1} prompt {2} clip {3}", orgRoute, sessionRoute, promptRoute, clipRoute);

            SbDb db = await Infrastructure.DbAsync();
            // WARNING: No RBAC!
            OrganizationEntity org = (await db.OrgWhereRoute(orgRoute)).AssertFound();
            ClipEntity clip = (await db.ClipAsync(orgRoute, clipRoute)).AssertFound();
            string containerName = ContainerName(org);
            string blobName = $"ses${sessionRoute}/pro{promptRoute}/clp{clipRoute}/orig/audio.wav";
            IBlobs blobs = await Infrastructure.BlobsAsync();
            return await blobs.DownloadAsync(containerName, blobName);
        }

        public async Task<Stream> DownloadAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNullOrEmpty(nameof(promptRoute), promptRoute);
            Validator.ArgNotNullOrEmpty(nameof(clipRoute), clipRoute);

            Logger.LogDebug("Downloading clip for org {0} session {1} prompt {2} clip {3}", orgRoute, sessionRoute, promptRoute, clipRoute);

            SbDb db = await Infrastructure.DbAsync();
            // WARNING: No RBAC!
            OrganizationEntity org = (await db.OrgWhereRoute(orgRoute)).AssertFound();
            ClipEntity clip = (await db.ClipAsync(orgRoute, clipRoute)).AssertFound();
            string containerName = ContainerName(org);
            string blobName = BlobName(sessionRoute, promptRoute, clipRoute, clip.FileType);
            IBlobs blobs = await Infrastructure.BlobsAsync();
            return await blobs.DownloadAsync(containerName, blobName);
        }

        public async Task<string> SbMediaManifestDownloadUrlAsync(string orgUid, string sessionRoute, string promptRoute, string clipRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgUid), orgUid);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNullOrEmpty(nameof(promptRoute), promptRoute);
            Validator.ArgNotNullOrEmpty(nameof(clipRoute), clipRoute);

            Logger.LogDebug("Downloading manifest URL for org {0} session {1} prompt {2} clip {3}", orgUid, sessionRoute, promptRoute, clipRoute);

            IBlobs blobs = await Infrastructure.BlobsAsync();
            string containerName = $"org{orgUid}";
            string blobName = $"ses${sessionRoute}/pro{promptRoute}/clp{clipRoute}/playlist.m3u8";
            string url = await blobs.DownloadUrlAsync(containerName, blobName);
            string token = await blobs.TokenAsync(containerName, null);

            // NOTE: we have a URL containing a blob-level token.  We also requested the container
            //   level token. So we are stripping away the blob level token here and returning the
            //   URL with the container level token.  Later on, this container level token will be
            //   split off and passed to the individiual segment in the variant playlist.
            url = $"{url.Split("?")[0]}?{token}";

            // Record an event because we are not sharing it
            await CreateClipEventWithDisplaySeconds(clipRoute, ClipEventType.ServerUnlockForRead);
            return url;
        }

        /// <summary>
        /// Responsible for getting a URL to a SB Media hosted media stream along with any necessary token.
        /// </summary>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <param name="clipRoute">Route of the clip.</param>
        /// <returns>a string containing the download URL + token for the HLS manifest.</returns>
        public async Task<string> SbMediaManifestDownloadUrlAsync(string orgRoute, string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity orgEntity = await db.OrgWhereRoute(orgRoute);
            orgEntity.AssertFound();
            ClipEntity clip = await db.ClipByRouteAsync(clipRoute);
            clip.AssertFound();

            string url = await SbMediaManifestDownloadUrlAsync(orgEntity.UniversalId, clip.Prompt.Session.Route, clip.Prompt.Route, clip.Route);
            return url;
        }

        #endregion
    }
}
