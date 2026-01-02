using AutoMapper;
using Masticore;
using Masticore.Azure.MediaServices;
using Masticore.Entity;
using Masticore.Jobs;
using Masticore.Media;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Masticore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Models.JobRequests;
using Soundbite.Services.Lifecycle;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Soundbite.Services
{
    public class SbMediaService : ISbMediaService
    {
        #region Fields

        readonly bool isMediaProcessingQueued = MasticoreExtensions.GetEnvBool("SB_QUEUED_MEDIAPROCESSING", true);

        #endregion

        #region Properties

        private ILogger Logger { get; set; }
        private ISbInfrastructure Infrastructure { get; set; }
        private ISbTranscriptionService SbTranscriptionService { get; set; }
        private ISecurityContext SecurityContext { get; set; }
        private IJobQueue JobQueue { get; set; }
        private IQueuedJobStatusService QueuedJobStatusService { get; set; }
        private IMediaProcessingService MediaProcessingService { get; set; }
        private IClipFileService ClipFService { get; set; }
        private ILifecycleFactory LifecycleFactory { get; set; }
        private IMapper Mapper { get; set; }
        private IRbac Rbac { get; set; }
        private IAzureMediaServiceConfig AzMediaServiceConfig { get; set; }
        private IAzureMediaService AzMediaService { get; set; }
        private IClipOperationService ClipOperationService { get; set; }
        private IOrgSettingsService OrgSettingsService { get; set; }
        private ISbTranscriptionService TranscriptionService { get; set; }


        #endregion

        #region Constructor

        public SbMediaService(
            ILogger<SbMediaService> logger, ISbInfrastructure infrastructure,
            ISecurityContext securityContext, ISbTranscriptionService sbTranscriptionService,
            IJobQueue jobQueue, IOrgSettingsService orgSettingsService,
            IQueuedJobStatusService queuedJobStatusService, IMediaProcessingService mediaProcessingService,
            IClipFileService clipFileService,
            IClipOperationService clipOperationService, ILifecycleFactory lifecycleFactory,
            IAzureMediaServiceConfig azMediaServiceConfig,
            IAzureMediaService azMediaService, IMapper mapper, IRbac rbac, ISbTranscriptionService transcriptionService)
        {
            Logger = logger;
            Infrastructure = infrastructure;
            SbTranscriptionService = sbTranscriptionService;
            SecurityContext = securityContext;
            JobQueue = jobQueue;
            QueuedJobStatusService = queuedJobStatusService;
            MediaProcessingService = mediaProcessingService;
            ClipFService = clipFileService;
            ClipOperationService = clipOperationService;
            LifecycleFactory = lifecycleFactory;
            Mapper = mapper;
            Rbac = rbac;
            AzMediaServiceConfig = azMediaServiceConfig;
            AzMediaService = azMediaService;
            OrgSettingsService = orgSettingsService;
            TranscriptionService = transcriptionService;
        }

        #endregion

        #region ISbMediaService Implementation

        /// <inheritdoc />
        public async Task SetClipDurationInfo(string orgRoute, string sessionRoute, string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            OrgSettings orgSettings = await GetValidatedOrgSettings(orgRoute);

            //OrganizationEntity orgEntity = await db.OrgWhereRoute(orgRoute);
            //orgEntity.AssertFound();

            ClipEntity clip = await db.ClipAsync(orgRoute, clipRoute);
            ClipMetadata clipMetadata = ClipMetadata.FromJson(clip.MetaData);

            // Setup Media Service Connection Settings
            AzMediaService.SetConnectionInfo(AzMediaServiceConfig.SubscriptionId, AzMediaServiceConfig.ResourceGroupName, orgSettings.Azure.MediaServiceAccountName);

            // Acquire the host asset
            Azure.ResourceManager.Media.MediaAssetResource outputAsset = await AzMediaService.AddOrUpdateAsset(clipMetadata.AmsHostAssetName);
            Validator.NotNull(outputAsset, $"Media Services Output asset named '{clipMetadata.AmsHostAssetName}' was not found.");
            Validator.NotNullOrEmpty(outputAsset.Data.Container, $"Media Services Output asset named '{clipMetadata.AmsHostAssetName}' does not have an associated storage conatiner.");

            // Download the manifest
            string manifestName = $"clip_manifest.json";
            IBlobs blobs = await Infrastructure.BlobsAsync();
            Stream manifestStream = await blobs.DownloadAsync(outputAsset.Data.Container, manifestName);
            string manifestJson = manifestStream.ToText();

            // Parse the manifest into a useable format
            AssetFileManifest manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetFileManifest>(manifestJson);
            TimeSpan videoDuration = XmlConvert.ToTimeSpan(manifest.AssetFile[0].Duration);
            int videoDurationSeconds = Convert.ToInt32(Math.Round(videoDuration.TotalSeconds, 0, MidpointRounding.AwayFromZero));

            // Update Clip
            clip.BillingSeconds = videoDurationSeconds;
            clip.DisplaySeconds = videoDurationSeconds;
            db.Clips.Update(clip);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> ProcessMediaEffects(string orgRoute, string sessionRoute, string clipRoute, IMediaEffect[] mediaEffects)
        {
            // Currently there is only ONE media effect that we are worried about
            SbMediaEncoding item = mediaEffects?.OfType<SbMediaEncoding>().FirstOrDefault();
            if (item != null)
            {
                await RequestSbMediaEncoding(item, orgRoute, sessionRoute, clipRoute);
            }
            return item == null;
        }

        /// <inheritdoc />
        public async Task StreamClip(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity orgEntity = await db.OrgWhereRoute(orgRoute);
            orgEntity.AssertFound();
            ClipEntity clip = await db.ClipAsync(orgRoute, clipRoute);
            bool isPublic = clip.Prompt.Session.SessionSecurity == SessionSecurityType.Public;
            ClipMetadata clipMetadata = ClipMetadata.FromJson(clip.MetaData);
            OrgSettings orgSettings = await GetValidatedOrgSettings(orgRoute);

            // Setup Media Service Connection Settings
            AzMediaService.SetConnectionInfo(AzMediaServiceConfig.SubscriptionId, AzMediaServiceConfig.ResourceGroupName, orgSettings.Azure.MediaServiceAccountName);

            // Delete the job associated with the encoding
            if (!string.IsNullOrEmpty(clipMetadata?.AmsEncodingJobName))
            {
                try
                {
                    await AzMediaService.DeleteJob(clipMetadata.AmsEncodingJobTransform, clipMetadata.AmsEncodingJobName);
                    clipMetadata.AmsEncodingJobName = null;
                    clip.MetaData = clipMetadata.ToLowerCamelJson();
                }
                catch (Exception ex)
                {
                    // Not a critical error so just log it
                    Logger.LogError(ex, $"Failed to remove AMS job associated with clip encoding (Job Name = {clipMetadata.AmsEncodingJobName})");
                }
            }

            // Delete the raw file asset
            if (!string.IsNullOrEmpty(clipMetadata?.AmsRawAssetName))
            {
                try
                {
                    await AzMediaService.DeleteAsset(clipMetadata.AmsRawAssetName);
                    clipMetadata.AmsRawAssetName = null;
                    clip.MetaData = clipMetadata.ToLowerCamelJson();
                }
                catch (Exception ex)
                {
                    // Not a critical error so just log it
                    Logger.LogError(ex, $"Failed to remove AMS asset containing raw clip for encoding encoding (Asset Name = {clipMetadata.AmsRawAssetName})");
                }
            }

            // Create the streaming locator
            Azure.ResourceManager.Media.StreamingLocatorResource locator = await AzMediaService.AddOrUpdateStreamingLocator(
                AzureMediaService.BuildStreamingLocatorName(orgRoute, sessionRoute, promptRoute, clipRoute),
                clipMetadata.AmsHostAssetName,
                "Predefined_ClearKey",
                isPublic ? "Public" : AzureMediaService.BuildOrgContentPolicyKeyName(orgEntity.UniversalId));

            // Save clip changes
            clip.HostingType = ClipHostingType.AzureStreaming;
            clip.HostingData = locator.Data.StreamingLocatorId.ToString(); // Used to build the streaming URL

            db.Update(clip);
            await db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task RequestVideoTranscription(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();

            // Acquire org settings and verify that transcription is enabled
            OrgSettings orgSettings = await OrgSettingsService.ReadOrgSettingsAsync<OrgSettings>(orgRoute);
            if (orgSettings.Sessions.TranscriptionEnabled)
            {
                // Acquire the clip and update metadata to store off AMS transcription job/asset names            
                ClipEntity clip = await db.ClipAsync(orgRoute, clipRoute);

                if (clip.Prompt.Session.Transcribe)
                {
                    // Request Azure Transcription Transform
                    await RequestSbMediaTranscription(orgRoute, sessionRoute, clipRoute);

                    // Update clip to indicate that transcription has been requested
                    clip.TranscriptState = TranscriptState.Requested;
                    db.Clips.Update(clip);
                    await db.SaveChangesAsync();
                }
            }
        }


        /// <inheritdoc />
        public async Task OnTranscriptionComplete(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity orgEntity = await db.OrgWhereRoute(orgRoute);
            OrgSettings orgSettings = await OrgSettingsService.ReadOrgSettingsAsync<OrgSettings>(orgRoute);
            ClipEntity clip = await db.ClipAsync(orgRoute, clipRoute);
            ClipMetadata clipMetadata = ClipMetadata.FromJson(clip.MetaData);

            // Setup Media Service Connection Settings
            AzMediaService.SetConnectionInfo(AzMediaServiceConfig.SubscriptionId, AzMediaServiceConfig.ResourceGroupName, orgSettings.Azure.MediaServiceAccountName);

            // Delete the transcription job
            if (!string.IsNullOrEmpty(clipMetadata?.AmsTranscribeJobName))
            {
                try
                {
                    await AzMediaService.DeleteJob("Transcribe", clipMetadata.AmsTranscribeJobName);
                    clipMetadata.AmsTranscribeJobName = null;
                    clip.MetaData = clipMetadata.ToLowerCamelJson();
                }
                catch (Exception ex)
                {
                    // Not a critical error so just log it
                    Logger.LogError(ex, $"Failed to remove AMS transcription job associated with clip (Job Name = {clipMetadata.AmsTranscribeJobName})");
                }
            }

            // Copy Transcript files into the host asset + session container
            if (!string.IsNullOrEmpty(clipMetadata?.AmsTranscribeAssetName))
            {
                try
                {
                    IBlobs blobs = await Infrastructure.BlobsAsync();
                    Azure.ResourceManager.Media.MediaAssetResource assetTranscription = await AzMediaService.GetAsset(clipMetadata.AmsTranscribeAssetName);
                    Azure.ResourceManager.Media.MediaAssetResource assetHost = await AzMediaService.GetAsset(clipMetadata.AmsHostAssetName);

                    await Task.WhenAll(
                        // Copy files from translation asset container to the host asset container
                        blobs.CopyAsync(assetTranscription.Data.Container, "insights.json", assetHost.Data.Container, "clip.transcript.json"),
                        blobs.CopyAsync(assetTranscription.Data.Container, "metadata.json", assetHost.Data.Container, "clip.transcript.metadata.json"),
                        blobs.CopyAsync(assetTranscription.Data.Container, "transcript.ttml", assetHost.Data.Container, "clip.transcript.ttml"),
                        blobs.CopyAsync(assetTranscription.Data.Container, "transcript.vtt", assetHost.Data.Container, "clip.transcript.vtt"),
                        // Copy transcript from translation asset container to clip container
                        blobs.CopyAsync(assetTranscription.Data.Container, "insights.json", ClipFileService.ContainerName(orgEntity), TranscriptFileService.GetTranscriptBlobName(clip.Prompt.Session.Route, clip.Prompt.Route, clip.Route)));
                }
                catch (Exception ex)
                {
                    // This is a critical error so log and throw it
                    Logger.LogError(ex, $"Failed to copy transcript files to the hosting asset (Transcription Asset Name = {clipMetadata.AmsTranscribeAssetName})");
                    throw;
                }
            }

            // Delete the transcription asset
            if (!string.IsNullOrEmpty(clipMetadata?.AmsTranscribeAssetName))
            {
                try
                {
                    await AzMediaService.DeleteAsset(clipMetadata.AmsTranscribeAssetName);
                    clipMetadata.AmsTranscribeAssetName = null;
                }
                catch (Exception ex)
                {
                    // Not a critical error so just log it
                    Logger.LogError(ex, $"Failed to remove AMS asset containing transcription for clip (Asset Name = {clipMetadata.AmsTranscribeAssetName})");
                }
            }

            // Save clip changes
            clip.TranscriptState = TranscriptState.Available;
            clip.MetaData = clipMetadata.ToLowerCamelJson();
            db.Update(clip);
            await db.SaveChangesAsync();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for retrieving the organization settings and ensuring they are valid.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose settings should be retrieved.</param>
        /// <returns>a reference to the organization settings</returns>
        private async Task<OrgSettings> GetValidatedOrgSettings(string orgRoute)
        {
            // Acquire organization media settings
            OrgSettings orgSettings = await OrgSettingsService.ReadOrgSettingsAsync<OrgSettings>(orgRoute);
            Validator.NotNull(orgSettings, $"Failed to retrieve organization settings for Org Route '{orgRoute}'");
            Validator.NotNull(orgSettings.Azure, $"Azure settings are missing from the organization settings for Org Route '{orgRoute}'.");
            Validator.NotNull(orgSettings.Azure.MediaServiceAccountName, $"Media service account name is missing in Azure settings in the organization settings for Org Route '{orgRoute}'.");
            Validator.NotNull(orgSettings.Azure.MediaServiceResourceGroupName, $"Media service resource group name is missing in Azure settings in the organization settings for Org Route '{orgRoute}'.");
            return orgSettings;
        }

        private async Task ProcessAzureTransformRequest(AzureTransformRequest request, ClipOperation<AzureTransformRequest> clipOp)
        {
            if (request.Transform.IsEncoding)
            {
                await ProcessAzureEncodingRequest(request, clipOp);
            }
            else
            {
                await ProcessAzureTranscriptionRequest(request, clipOp);
            }
        }

        private async Task ProcessAzureEncodingRequest(AzureTransformRequest request, ClipOperation<AzureTransformRequest> clipOp)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                OrgSettings orgSettings = await GetValidatedOrgSettings(request.OrgRoute);
                ClipEntity clip = await db.ClipAsync(request.OrgRoute, request.ClipRoute);
                ClipMetadata clipMetaData = ClipMetadata.FromJson(clip.MetaData);

                // NOTE: RequestAzureTransform omits the PromptRoute and ClipFileType to avoid extra
                //  database calls so we set that information here (for completeness)
                request.PromptRoute = clip.Prompt.Route;
                request.ClipFileType = clip.FileType;

                // Setup Media Service Connection Settings
                AzMediaService.SetConnectionInfo(AzMediaServiceConfig.SubscriptionId, AzMediaServiceConfig.ResourceGroupName, orgSettings.Azure.MediaServiceAccountName);

                // Determine whether the raw asset already exists.  In theory it should be but if
                // this is a retry of sorts then it may be there already.
                if (string.IsNullOrEmpty(clipMetaData.AmsRawAssetName))
                {
                    // Create the asset to hold the raw clip - to ensure assetName uniqueness use routes + date/time
                    string assetInputName = $"Encoding-{request.OrgRoute}-{request.SessionRoute}-{clip.Prompt.Route}-{clip.Route}";
                    Azure.ResourceManager.Media.MediaAssetResource assetInput = await AzMediaService.AddOrUpdateAsset(assetInputName);

                    IBlobs blobs = await Infrastructure.BlobsAsync();
                    OrganizationEntity org = await db.OrgWhereRoute(request.OrgRoute);
                    org.AssertFound();

                    string containerName = ClipFileService.ContainerName(org);
                    string blobName = ClipFileService.BlobName(request.SessionRoute, clip.Prompt.Route, request.ClipRoute, clip.FileType);
                    string destinationBlobName = blobName.Split("/").Last();

                    // Move files into the assetInput
                    await blobs.CopyAsync(containerName, blobName, assetInput.Data.Container, destinationBlobName);

                    // Store the raw asset name along with the clip
                    clipMetaData.AmsRawAssetName = assetInputName;
                    clip.MetaData = clipMetaData.ToLowerCamelJson();
                    db.Clips.Update(clip);
                    await db.SaveChangesAsync();
                }

                // Create the output asset into which encoded files are placed
                string jobName = AzureMediaService.BuildEncodingJobName(request.OrgRoute, request.SessionRoute, clip.Prompt.Route, clip.Route);
                string assetOutputName = AzureMediaService.BuildEncodingAssetOutputName(request.OrgRoute, request.SessionRoute, clip.Prompt.Route, clip.Route);
                Azure.ResourceManager.Media.MediaAssetResource assetOutput = await AzMediaService.AddOrUpdateAsset(assetOutputName);

                // Create the transform job in Azure Media Services
                Azure.ResourceManager.Media.MediaJobResource job = await AzMediaService.CreateJob(jobName,
                    request.Transform.TransformName,
                    clipMetaData.AmsRawAssetName,
                    assetOutput.Data.Name);

                // Store the raw asset name along with the clip
                clipMetaData.AmsHostAssetName = assetOutputName;
                clipMetaData.AmsEncodingJobName = jobName;
                clipMetaData.AmsEncodingJobTransform = request.Transform.TransformName;
                clip.MetaData = clipMetaData.ToLowerCamelJson();
                db.Clips.Update(clip);
                await db.SaveChangesAsync();

                // Update the ClipOperation record
                clipOp.ExternalId = job.Id;
                clipOp.OperationState = ClipOperationStateType.Requested;
                clipOp.OperationDataJson = request.ToLowerCamelJson();
                await ClipOperationService.SaveAsync(clipOp);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ProcessAzureEncodingRequest failed.");
                await ClipOperationService.SetState(clipOp.Route, ClipOperationStateType.Error, ex.Message);
            }
        }

        private async Task ProcessAzureTranscriptionRequest(AzureTransformRequest request, ClipOperation<AzureTransformRequest> clipOp)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                OrgSettings orgSettings = await GetValidatedOrgSettings(request.OrgRoute);
                ClipEntity clip = await db.ClipAsync(request.OrgRoute, request.ClipRoute);
                ClipMetadata clipMetaData = ClipMetadata.FromJson(clip.MetaData);

                // NOTE: RequestAzureTransform omits the PromptRoute and ClipFileType to avoid extra
                //  database calls so we set that information here (for completeness)
                request.PromptRoute = clip.Prompt.Route;
                request.ClipFileType = clip.FileType;

                IBlobs blobs = await Infrastructure.BlobsAsync();
                OrganizationEntity org = await db.OrgWhereRoute(request.OrgRoute);
                org.AssertFound();

                // Setup Media Service Connection Settings
                AzMediaService.SetConnectionInfo(AzMediaServiceConfig.SubscriptionId, AzMediaServiceConfig.ResourceGroupName, orgSettings.Azure.MediaServiceAccountName);

                // Create the output asset into which encoded files are placed
                string jobName = AzureMediaService.BuildTranscriptionJobName(request.OrgRoute, request.SessionRoute, clip.Prompt.Route, clip.Route);
                string assetOutputName = AzureMediaService.BuildTranscriptionAssetName(request.OrgRoute, request.SessionRoute, clip.Prompt.Route, clip.Route);
                Azure.ResourceManager.Media.MediaAssetResource assetOutput = await AzMediaService.AddOrUpdateAsset(assetOutputName);

                // Create the transform job in Azure Media Services
                Azure.ResourceManager.Media.MediaJobResource job = await AzMediaService.CreateJob(
                    jobName,
                    request.Transform.TransformName,
                    clipMetaData.AmsHostAssetName,
                    assetOutput.Data.Name);

                // Store the raw asset name along with the clip
                clipMetaData.AmsTranscribeAssetName = assetOutputName;
                clipMetaData.AmsTranscribeJobName = jobName;
                clip.MetaData = clipMetaData.ToLowerCamelJson();
                db.Clips.Update(clip);
                await db.SaveChangesAsync();

                // Update the ClipOperation record
                clipOp.ExternalId = job.Id;
                clipOp.OperationState = ClipOperationStateType.Requested;
                clipOp.OperationDataJson = request.ToLowerCamelJson();
                await ClipOperationService.SaveAsync(clipOp);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ProcessAzureEncodingRequest failed.");
                await ClipOperationService.SetState(clipOp.Route, ClipOperationStateType.Error, ex.Message);
            }
        }

        /// <summary>
        /// Responsible for adding to the queue that fires the azure function that processes media.
        /// </summary>
        /// <param name="mediaEffect"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        private async Task RequestSbMediaEncoding(SbMediaEncoding mediaEffect, string orgRoute, string sessionRoute, string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            ClipEntity clip = await db.Clips.Where(i => i.Route == clipRoute)
                .Include(i => i.Prompt)
                .Include(i => i.Prompt.Session)
                .Include(i => i.Prompt.Session.Organization)
                .Include(i => i.ClipOperations)
                .FirstAsync();

            // NOTE: There is a bit of wonkiness going on here because the EncodeClipRequest
            //   contains a property for the ClipOpRoute because it is used during the processing of
            //   the video to mark it as complete.  However, we don't have that value when setting
            //   up the ClipOp (but we don't need it).  So we have to set that value before
            //   submitting the job because it needs to get set in the queue data for the video to
            //   process.

            // Create the encode clip request
            EncodeClipRequest request = new EncodeClipRequest()
            {
                OUID = clip.Prompt.Session.Organization.UniversalId,
                OrgRoute = orgRoute,
                SessionRoute = sessionRoute,
                PromptRoute = clip.Prompt.Route,
                ClipRoute = clipRoute,
                ClipOpRoute = "", // Cannot set this here because we don't have it yet
                ClipFileName = $"clip.{FileTypeExtensions.GetExtension(clip.FileType)}",
                ClipFileType = clip.FileType,
                UniqueFolderId = Guid.NewGuid().ToString(),
            };

            // Create the clip operation that houses the clip request
            ClipOperation<EncodeClipRequest> clipOp = new ClipOperation<EncodeClipRequest>()
            {
                BillingCode = "SbMediaEncode",
                ClipRoute = clipRoute,
                OperationState = ClipOperationStateType.Requested,
                OperationType = ClipOperationType.SoundbiteMediaEncoding,
                OperationDataJson = JsonUtils.ToLowerCamelJson(request)
            };

            // Save off the clip operation and make sure to set the cliproute on the request
            // because it will be necessary for the job to process the request.
            clipOp = (await ClipOperationService.SaveAsync(clipOp)) as ClipOperation<EncodeClipRequest>;
            request.ClipOpRoute = clipOp.Route;

            Validator.NotNull(clip, "{0} failed because the clip (route:{1}) was not found.", nameof(RequestSbMediaEncoding), clipRoute);
            Validator.NotNull(clip, "{0} failed because the session/prompt/organization was not found.", nameof(RequestSbMediaEncoding));
            Validator.NotNullOrEmpty(clip.Prompt.Session.Organization.UniversalId, "{0} failed because the UID for organization (route:{1}) is not set.", nameof(RequestSbMediaEncoding), orgRoute);
            Validator.NotNull(clipOp, "{0} failed because the clip (route:{1}) has no clip operation.", nameof(RequestSbMediaEncoding), clipRoute);

            SbVideoEncodingJob job = new SbVideoEncodingJob(request);
            await JobQueue.Add(job);
        }

        private async Task RequestSbMediaTranscription(string orgRoute, string sessionRoute, string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            ClipEntity clip = await db.ClipAsync(orgRoute, clipRoute);

            await SbTranscriptionService.QueueTranscriptionRequest(new TranscriptionRequest()
            {
                ClipFileType = clip.FileType,
                ClipRoute = clipRoute,
                OrgRoute = orgRoute,
                PromptRoute = clip.Prompt.Route,
                SessionRoute = sessionRoute
            });
        }

        /// <summary>
        /// Responsible for creating and storing the clip operation for encoding a clip. After
        /// creating the clip operation this method "handles" the clip operation
        /// </summary>
        /// <param name="transform">Transform that is being requested.</param>
        /// <param name="orgRoute">Route of the organization associated with the session.</param>
        /// <param name="sessionRoute">Route of the session whose clip is being transformed.</param>
        /// <param name="clipRoute">Route of the clip to transform.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        private async Task RequestAzureTransform(AzureTransform transform, string orgRoute, string sessionRoute, string clipRoute)
        {
            // Create the clip operation record
            AzureTransformRequest request = new AzureTransformRequest()
            {
                OrgRoute = orgRoute,
                SessionRoute = sessionRoute,
                PromptRoute = null,
                ClipRoute = clipRoute,
                ClipFileType = FileType.Unknown,
                FileNameNoExt = "clip", //destinationBlobName.Split('.').FirstOrDefault(),
                InputAssetName = null,
                Transform = transform,
                OutputAssetName = null,
                OutputAssetContainer = null
            };
            ClipOperation<AzureTransformRequest> clipOp = new ClipOperation<AzureTransformRequest>()
            {
                BillingCode = transform.TransformName,
                ClipRoute = clipRoute,
                OperationState = ClipOperationStateType.NotRequested,
                OperationType = transform.IsEncoding ? ClipOperationType.AzureMediaSvcEncoding : ClipOperationType.AzureMediaSvcTranscribe,
                OperationDataJson = JsonUtils.ToLowerCamelJson(request)
            };

            await ClipOperationService.SaveAsync(clipOp);
            await ProcessAzureTransformRequest(request, clipOp);
        }

        #endregion
    }
}
