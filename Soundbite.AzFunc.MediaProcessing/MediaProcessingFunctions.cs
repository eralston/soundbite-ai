using Masticore;
using Masticore.Entity;
using Masticore.Media;
using Masticore.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Soundbite.Api;
using Soundbite.AzFun;
using Soundbite.Entity;
using Soundbite.Services;
using Soundbite.Services.Lifecycle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

//NOTE: The following URL has information on m3u/m3u8 playlists format
//URL: https://developer.apple.com/documentation/http-live-streaming/example-playlists-for-http-live-streaming

namespace Soundbite.AzFunc.MediaProcessing
{
    /// <summary>s
    /// Acquire functions pertaining to the directory sync process.
    /// </summary>
    public class MediaProcessingFunctions
    {
        #region Constants

        // Near factor is used to determine whether a video size is "close enough" to a desired
        // size that it is not necessary to re-encode it for the new size.  
        const double nearFactor = 1.2;

        /// <summary>
        /// A default retry strategy when starting an orchestration to make the timeout longer and the system less fanatical about retries.
        /// </summary>
        static readonly RetryOptions DefaultRetryOptions = new RetryOptions(TimeSpan.FromSeconds(1800), 1);

        #endregion

        #region Fields

        protected ILogger Logger { get; }
        protected IMediaProcessingService MediaProcessingService { get; }
        protected IIdentityInfrastructure IdentityInfrastructure { get; }
        protected ISbInfrastructure Infrastructure { get; }
        protected IClipOperationService ClipOperationService { get; }
        protected ILifecycleFactory LifecycleFactory { get; }

        #endregion

        #region Constructor and Members

        /// <summary>
        /// Instantiates a new <see cref="MediaProcessingFunctions"/> instance.
        /// </summary>
        /// <param name="logger">DI logger reference.</param>
        /// <param name="mediaProcessingService">DI media processing service reference.</param>
        public MediaProcessingFunctions(
            ILogger<MediaProcessingFunctions> logger,
            IMediaProcessingService mediaProcessingService,
            IIdentityInfrastructure identityInfrastructure,
            ISbInfrastructure infrastructure,
            IClipOperationService clipOperationService,
            ILifecycleFactory lifecycleFactory)
        {
            Logger = logger;
            MediaProcessingService = mediaProcessingService;
            IdentityInfrastructure = identityInfrastructure;
            Infrastructure = infrastructure;
            ClipOperationService = clipOperationService;
            LifecycleFactory = lifecycleFactory;
        }

        private Task<bool> HasConnection()
        {
            return Task.FromResult(AzInfrastructure.HasConnection);
        }

        private void LoadInfrastructure(string dbStr, string storStr)
        {
            AzInfrastructure.Configure(dbStr, storStr);
        }

        #endregion

        #region Azure Functions - Debug
        //#if DEBUG

        [FunctionName(nameof(EncodeClipHttp))]
        public async Task<HttpResponseMessage> EncodeClipHttp(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
           [DurableClient] IDurableOrchestrationClient starter)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                nameof(EncodeClipHttp),
                Logger,
                HasConnection,
                LoadInfrastructure,
                async () =>
                {
                    string instanceId = await starter.StartNewAsync(nameof(EncodeClip), new EncodeClipRequest()
                    {
                        OUID = "[AMS TENANT ID]",
                        OrgRoute = "13MSq7q03gSAbH4QUynWp",
                        SessionRoute = "123",
                        PromptRoute = "123",
                        ClipRoute = "123",
                        ClipOpRoute = "4yk3mml1757yin2ymkvah",
                        ClipFileName = "clip.mov",
                        UniqueFolderId = Guid.NewGuid().ToString()
                    });
                    HttpResponseMessage response = starter.CreateCheckStatusResponse(req, instanceId);
                    return response;
                });
        }

        //#endif

        #endregion

        #region Azure Functions - Entry Point

        [FunctionName(nameof(ProcessVideoEncodingJobFromQueue))]
        public async Task ProcessVideoEncodingJobFromQueue(
            [DurableClient] IDurableOrchestrationClient starter,
            [QueueTrigger("q-sbvideoencodingjob", Connection = "SbStorage")]
            string message, string id, ILogger log)
        {
            await AzFunUtils.EnsureSecretsAndRun(
               nameof(ProcessVideoEncodingJobFromQueue),
               Logger,
               HasConnection,
               LoadInfrastructure,
               async () =>
               {
                   await DoProcessVideoEncodingJobFromQueue(starter, message, log);
               });

        }

        private static async Task DoProcessVideoEncodingJobFromQueue(IDurableOrchestrationClient starter, string message, ILogger log)
        {
            log.LogInformation($"Received encoding request: {message}");
            try
            {
                EncodeClipRequest request = JsonUtils.FromLowerCamelJson<EncodeClipRequest>(message);
                string instanceId = await starter.StartNewAsync(nameof(EncodeClip), request);
                log.LogInformation("Successfully started video encoding azure function.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to invoke encoding azure function.  See error details.");
            }
        }

        #endregion

        #region Azure Functions - Orchestrations

        [FunctionName(nameof(EncodeClip))]
        public async Task EncodeClip([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await AzFunUtils.EnsureSecretsAndRun(
              nameof(EncodeClip),
              Logger,
              HasConnection,
              LoadInfrastructure,
            async () =>
            {
                await DoEncodeClip(context);
            });
        }

        private async Task DoEncodeClip(IDurableOrchestrationContext context)
        {
            EncodeClipRequest clipRequest = context.GetInput<EncodeClipRequest>();

            try
            {
                string targetDir = Path.GetTempPath() + $"Soundbite\\{clipRequest.UniqueFolderId}\\orig\\";
                clipRequest.AzureContainer = GetContainerName(clipRequest);
                clipRequest.AzureBlobPath = GetAzureStoragePathToClip(clipRequest);
                clipRequest.LocalClipFilePath = $"{targetDir}{clipRequest.ClipFileName}";

                Logger.LogInformation($"Encoding clip with target directory: {targetDir} and writing into container {clipRequest.AzureContainer} with blob path {clipRequest.AzureBlobPath} and local file name {clipRequest.LocalClipFilePath}");

                // Ensure the unique target directory exists
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                InitialClipProcessingResult clipInfo = await context.CallActivityWithRetryAsync<InitialClipProcessingResult>(nameof(InitialClipProcessing), DefaultRetryOptions, clipRequest);

                List<EncodeVariantStreamRequest> qualityLevels = new List<EncodeVariantStreamRequest>();
                bool isVertical = clipInfo.MediaDetails.Height > clipInfo.MediaDetails.Width;
                int rotation = clipInfo.MediaDetails.Rotation;
                int originalSize = isVertical ? clipInfo.MediaDetails.Height : clipInfo.MediaDetails.Width;

                // Make sure the original size is large enough to warrant getting it's own size, otherwise just use 1080p
                if (originalSize > (isVertical ? 1080 : 1920) * nearFactor)
                {
                    qualityLevels.Add(new EncodeVariantStreamRequest(clipRequest, "orig", clipInfo.Segments, 0, 0, rotation));
                }
                CheckAddEncodingRequest(clipRequest, "1080p", clipInfo.Segments, originalSize, 1920, 1080, isVertical, qualityLevels, rotation);
                CheckAddEncodingRequest(clipRequest, "720p", clipInfo.Segments, originalSize, 1280, 720, isVertical, qualityLevels, rotation);
                CheckAddEncodingRequest(clipRequest, "360p", clipInfo.Segments, originalSize, 640, 360, isVertical, qualityLevels, rotation);
                CheckAddEncodingRequest(clipRequest, "144p", clipInfo.Segments, originalSize, 256, 144, isVertical, qualityLevels, rotation);

                List<Task<EncodeVariantStreamResult>> encodingTasks = new List<Task<EncodeVariantStreamResult>>();
                foreach (EncodeVariantStreamRequest qualityLevelRequest in qualityLevels)
                {
                    encodingTasks.Add(context.CallSubOrchestratorAsync<EncodeVariantStreamResult>(nameof(EncodeVariantStream), qualityLevelRequest));
                }

                await Task.WhenAll(encodingTasks.ToArray());
                List<EncodeVariantStreamResult> qualityLevelResults = encodingTasks.Select(i => i.Result).ToList();

                await context.CallActivityWithRetryAsync(nameof(CreateMultiVariantM3u8File),
                     DefaultRetryOptions,
                    new CreateMultiVariantM3u8FileRequest()
                    {
                        EncodeClipRequest = clipRequest,
                        QualityLevelResults = qualityLevelResults
                    });

                await context.CallActivityWithRetryAsync(nameof(SetClipOpStatus),
                     DefaultRetryOptions,
                    new SetClipOpStatusRequest(clipRequest.ClipOpRoute, ClipOperationStateType.Complete));
            }
            catch (Exception ex)
            {
                try
                {
                    await context.CallActivityWithRetryAsync(nameof(SetClipOpStatus),
                         DefaultRetryOptions,
                        new SetClipOpStatusRequest(clipRequest.ClipOpRoute, ClipOperationStateType.Complete));
                    Logger.LogError(ex, $"Exception occured during Soundbite Video processing for clip (route:{clipRequest.ClipRoute})");
                }
                catch { /* Pokemon! */ }
            }
        }

        /// <summary>
        /// Responsible for encoding a quality stream variant to the specified size. If the 
        /// incomming sizes are 0 it means the original size should be retained.
        /// </summary>
        /// <param name="request">Contains request infromation.</param>
        /// <param name="segments">List of the segments that need to be encoded.</param>
        /// <param name="height">Encoding height.</param>
        /// <param name="width">Encoding width.</param>
        /// <returns>a task representing the result of the operation.</returns>
        [FunctionName(nameof(EncodeVariantStream))]
        public async Task<EncodeVariantStreamResult> EncodeVariantStream([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
             nameof(EncodeVariantStream),
             Logger,
             HasConnection,
             LoadInfrastructure,
               async () =>
               {
                   return await DoEncodeVariantStream(context);
               });
        }

        private async Task<EncodeVariantStreamResult> DoEncodeVariantStream(IDurableOrchestrationContext context)
        {
            EncodeVariantStreamRequest qualityLevelRequest = context.GetInput<EncodeVariantStreamRequest>();
            List<Task<MediaDetails>> encodingTasks = new List<Task<MediaDetails>>();

            // Ensure the quality level folder exists
            // NOTE: Depending on how file structure is handled with Azure functions, this may need
            // to happen for each EncodeSegment call because they may be running from a different
            // location each time?  NOT 100% sure...
            string outputFolder = Path.GetTempPath() + $"Soundbite\\{qualityLevelRequest.ClipRequest.UniqueFolderId}\\{qualityLevelRequest.QualityLevelName}";
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // JUST DO ONE
            //await context.CallActivityAsync(nameof(EncodeSegment), new EncodeSegmentRequest(qualityLevelRequest, qualityLevelRequest.Segments[0]));

            foreach (string segment in qualityLevelRequest.Segments)
            {
                encodingTasks.Add(context.CallActivityWithRetryAsync<MediaDetails>(
                    nameof(EncodeSegment),
                     DefaultRetryOptions,
                    new EncodeSegmentRequest(qualityLevelRequest, segment)));
            }

            // Wait for everything to finish
            await Task.WhenAll(encodingTasks.ToArray());
            MediaDetails[] segmentDetails = encodingTasks.Select(i => i.Result).ToArray();
            CreateM3u8File(qualityLevelRequest, segmentDetails);

            // Upload files to azure (+1 is for the playlist file)
            Task[] uploadTasks = new Task[encodingTasks.Count + 1];
            for (int i = 0; i < encodingTasks.Count; i++)
            {
                string azureBlobName = GetAzureStoragePathToClipFolder(qualityLevelRequest.ClipRequest)
                    + segmentDetails[i].FileName.Split($"{qualityLevelRequest.ClipRequest.UniqueFolderId}\\").Last();
                uploadTasks[i] = UploadFileToAzure(segmentDetails[i].FileName, qualityLevelRequest.ClipRequest.AzureContainer, azureBlobName);
            }

            // Upload the playlist
            string playlistFile = Path.GetTempPath() + $"Soundbite\\{qualityLevelRequest.ClipRequest.UniqueFolderId}\\{qualityLevelRequest.QualityLevelName}\\playlist.m3u8";
            string playlistAzureBlobName = $"{GetAzureStoragePathToClipFolder(qualityLevelRequest.ClipRequest)}{qualityLevelRequest.QualityLevelName}\\playlist.m3u8";
            uploadTasks[encodingTasks.Count] = UploadFileToAzure(playlistFile, qualityLevelRequest.ClipRequest.AzureContainer, playlistAzureBlobName);

            // Wait for all the upload tasks to complete.
            Task.WaitAll(uploadTasks);

            return new EncodeVariantStreamResult() { Request = qualityLevelRequest, SegmentDetails = segmentDetails };
        }

        #endregion

        #region Azure Functions - Activities

        /// <summary>
        /// There are three operations that occcur on the initial clip. These were originally split
        /// into three individual azure functions, but if the clip is large it results in the file
        /// getting pulled down "locally" three times.  This method exists to pull the file down
        /// once and perform all three actions from a single azure function. We can move them back
        /// to individual functions if the need arises.
        /// </summary>
        /// <param name="request">Contains the clip encoding request information.</param>        
        /// <returns>a task representing the result of the operation.</returns>
        [FunctionName(nameof(InitialClipProcessing))]
        public async Task<InitialClipProcessingResult> InitialClipProcessing([ActivityTrigger] EncodeClipRequest request)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                nameof(InitialClipProcessing),
                Logger,
                HasConnection,
                LoadInfrastructure,
                    async () =>
                    {
                        return await DoInitialClipProcessing(request);
                    });
        }

        private async Task<InitialClipProcessingResult> DoInitialClipProcessing(EncodeClipRequest request)
        {
            InitialClipProcessingResult result = new InitialClipProcessingResult();

            // Ensure that the clip file is present locally
            await EnsureAzureFileIsLocal(request.LocalClipFilePath, request.AzureContainer, request.AzureBlobPath);

            // Run initial processing
            result.MediaDetails = await GetVideoDetails(request);
            request.SourceMediaDetails = result.MediaDetails;
            result.Segments = await SplitVideo(request);
            await ExtractAudio(request);

            return result;
        }

        [FunctionName(nameof(EncodeSegment))]
        public async Task<MediaDetails> EncodeSegment([ActivityTrigger] EncodeSegmentRequest request)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                nameof(EncodeSegment),
                Logger,
                HasConnection,
                LoadInfrastructure,
                    async () =>
                    {
                        string segmentFileName = Path.GetFileName(request.SegmentName).Replace("original_", "seg_");
                        string encodedOutput = Path.GetTempPath() + $"Soundbite\\{request.QualityLevelRequest.ClipRequest.UniqueFolderId}\\{request.QualityLevelRequest.QualityLevelName}\\{segmentFileName}";

                        // Check if there is already a file there and if so, return it
                        if (!File.Exists(encodedOutput))
                        {
                            Logger.LogInformation($"Encoding segment: {request.SegmentName} -> {encodedOutput}");
                            await MediaProcessingService.EncodeSegment(request.SegmentName, encodedOutput, request.QualityLevelRequest.Width, request.QualityLevelRequest.Height, request.QualityLevelRequest.Rotation);
                        }
                        else
                        {
                            Logger.LogInformation($"Already found encoded segment: {request.SegmentName} -> {encodedOutput}; skipping encoding");
                        }

                        MediaDetails result = await MediaProcessingService.GetMediaDetails(encodedOutput);
                        return result;
                    });
        }


        [FunctionName(nameof(CreateMultiVariantM3u8File))]
        public async Task CreateMultiVariantM3u8File([ActivityTrigger] CreateMultiVariantM3u8FileRequest request)
        {
            await AzFunUtils.EnsureSecretsAndRun(
                nameof(CreateMultiVariantM3u8File),
                Logger,
                HasConnection,
                LoadInfrastructure,
                    async () =>
                    {
                        await DoCreateMultiVariantM3u8File(request);
                    });
        }

        private async Task DoCreateMultiVariantM3u8File(CreateMultiVariantM3u8FileRequest request)
        {
            string filePath = Path.GetTempPath() + $"Soundbite\\{request.EncodeClipRequest.UniqueFolderId}\\playlist.m3u8";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");

            // Remove existing file if it is already there for some reason
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Iterate over the variants
            foreach (EncodeVariantStreamResult result in request.QualityLevelResults)
            {
                sb.Append("#EXT-X-STREAM-INF:");
                int maxBandwidth = result.SegmentDetails.Max(i => i.Bandwidth);
                int avgBandwidth = Convert.ToInt32(Math.Ceiling(result.SegmentDetails.Average(i => i.Bandwidth)));
                sb.Append($"BANDWIDTH={maxBandwidth},AVERAGE-BANDWIDTH={avgBandwidth},");
                sb.AppendLine($"RESOLUTION={result.SegmentDetails[0].Width}x{result.SegmentDetails[0].Height}");
                sb.AppendLine($"{result.Request.QualityLevelName}\\playlist.m3u8{{TOKEN}}");
            }

            // Write out the file content and make sure to replace {TARGET_DURATION} value!!!
            File.WriteAllText(filePath, sb.ToString());

            string azureBlobPath = $"{GetAzureStoragePathToClipFolder(request.EncodeClipRequest)}playlist.m3u8";
            await UploadFileToAzure(filePath, request.EncodeClipRequest.AzureContainer, azureBlobPath);
        }

        [FunctionName(nameof(SetClipOpStatus))]
        public async Task SetClipOpStatus([ActivityTrigger] SetClipOpStatusRequest request)
        {
            await AzFunUtils.EnsureSecretsAndRun(
                nameof(SetClipOpStatus),
                Logger,
                HasConnection,
                LoadInfrastructure,
                    async () =>
                    {
                        await ClipOperationService.SetState(request.ClipOpRoute, request.ClipOpState);
                        SbDb db = await Infrastructure.DbAsync();
                        ILifecycleStrategy lifecycle = LifecycleFactory.StrategyForSession(SessionType.Announcement);
                        if (request.ClipOpState == ClipOperationStateType.Complete)
                        {
                            await lifecycle.AfterClipOperationComplete(db, request.ClipOpRoute);
                        }
                    });
        }

        #endregion

        #region Azure Functions - Initial Processing

        //NOTE: these are the initial processing functions.  T

        [FunctionName(nameof(GetVideoDetails))]
        public async Task<MediaDetails> GetVideoDetails([ActivityTrigger] EncodeClipRequest request)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                   nameof(GetVideoDetails),
                   Logger,
                   HasConnection,
                   LoadInfrastructure,
                       async () =>
                       {
                           await EnsureAzureFileIsLocal(request.LocalClipFilePath, request.AzureContainer, request.AzureBlobPath);
                           MediaDetails mediaDetails = await MediaProcessingService.GetMediaDetails(request.LocalClipFilePath);
                           return mediaDetails;
                       });
        }

        /// <summary>
        /// Responsible for splitting a video into individual segments that can be individually but simultaneously processed.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [FunctionName(nameof(SplitVideo))]
        public async Task<string[]> SplitVideo([ActivityTrigger] EncodeClipRequest request)
        {
            return await AzFunUtils.EnsureSecretsAndRun(
                  nameof(SplitVideo),
                  Logger,
                  HasConnection,
                  LoadInfrastructure,
                      async () =>
                      {
                          await EnsureAzureFileIsLocal(request.LocalClipFilePath, request.AzureContainer, request.AzureBlobPath);
                          string outputMask = Path.GetTempPath() + $"Soundbite\\{request.UniqueFolderId}\\orig\\original_%04d.ts";
                          string[] result = await MediaProcessingService.SplitVideo(request.LocalClipFilePath, outputMask, request.SourceMediaDetails.Rotation);

                          // Ensure the files are uploaded to azure for distrubuted processing
                          Task[] uploads = new Task[result.Length];
                          for (int i = 0; i < result.Length; i++)
                          {
                              string azureBlobPath = $"{GetAzureStoragePathToClipFolder(request)}orig\\{result[i].Split("\\").Last()}";
                              uploads[i] = UploadFileToAzure(result[i], request.AzureContainer, azureBlobPath);
                          }

                          // Wait for the azure uploads to complete
                          await Task.WhenAll(uploads);

                          return result;
                      });

        }

        /// <summary>
        /// Responsible for encoding a quality stream variant to the specified size. If the 
        /// incomming sizes are 0 it means the original size should be retained.
        /// </summary>
        /// <param name="request">Contains request infromation.</param>
        /// <param name="segments">List of the segments that need to be encoded.</param>
        /// <param name="height">Encoding height.</param>
        /// <param name="width">Encoding width.</param>
        /// <returns>a task representing the result of the operation.</returns>
        [FunctionName(nameof(ExtractAudio))]
        public async Task ExtractAudio([ActivityTrigger] EncodeClipRequest request)
        {
            await AzFunUtils.EnsureSecretsAndRun(
               nameof(ExtractAudio),
               Logger,
               HasConnection,
               LoadInfrastructure,
                   async () =>
                   {
                       await DoExtractAudio(request);
                   });

        }

        private async Task DoExtractAudio(EncodeClipRequest request)
        {
            //await EnsureAzureFileIsLocal(request.LocalClipFilePath, request.AzureContainer, request.AzureBlobPath);
            string outputMask = Path.GetTempPath() + $"Soundbite\\{request.UniqueFolderId}\\orig\\audio.wav";
            Logger.LogInformation("Extracting audio from video file: " + request.LocalClipFilePath + " -> " + outputMask);

            // If we already have a file at the output mask then we skip
            if (File.Exists(outputMask))
            {
                Logger.LogInformation($"Audio file at '{outputMask}' already exists; skipping extraction");
                return;
            }

            await MediaProcessingService.ExtractAudioFromVideoFile(request.LocalClipFilePath, outputMask);
            string azureBlobPath = $"{GetAzureStoragePathToClipFolder(request)}orig\\audio.wav";
            await UploadFileToAzure(outputMask, request.AzureContainer, azureBlobPath);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds out the path to the clip specified in the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">Encode clip request identifying the clip to encode.</param>
        /// <returns>a path to the clip specified in the <paramref name="request"/>.</returns>
        private string GetAzureStoragePathToClip(EncodeClipRequest request)
        {
            return $"{GetAzureStoragePathToClipFolder(request)}{request.ClipFileName}";
        }

        /// <summary>
        /// Builds out the path to the clip specified in the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">Encode clip request identifying the clip to encode.</param>
        /// <returns>a path to the clip specified in the <paramref name="request"/>.</returns>
        private string GetAzureStoragePathToClipFolder(EncodeClipRequest request)
        {
            return MediaStreamingService.GetAzureStoragePathToClipFolder(request.SessionRoute, request.PromptRoute, request.ClipRoute);
        }

        /// <summary>
        /// Builds out the azure container name for the specified <paramref name="request"/>.
        /// </summary>
        /// <param name="request">Encode clip request identifying the clip to encode.</param>
        /// <returns>the azure container name for the specified <paramref name="request"/>.</returns>
        private string GetContainerName(EncodeClipRequest request)
        {
            return MediaStreamingService.GetContainerName(request.OUID);
        }

        /// <summary>
        /// Repsonsible for uploading local files 
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="azureContainer"></param>
        /// <param name="azureBlobName"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private async Task UploadFileToAzure(string localPath, string azureContainer, string azureBlobName)
        {
            Logger.LogInformation($"Uploading file to azure: {localPath} -> {azureContainer}/{azureBlobName}");

            if (File.Exists(localPath))
            {
                IBlobs blobs = await IdentityInfrastructure.BlobsAsync();
                using FileStream fs = new FileStream(localPath, FileMode.Open);
                await blobs.UploadAsync(azureContainer, azureBlobName, fs);
                fs.Close();
            }
            else
            {
                Logger.LogError($"Error uploading file '{localPath}' because it does not exist locally");
                throw new FileNotFoundException($"Cannot upload file '{localPath}' because it does not exist.");
            }
        }

        /// <summary>
        /// This method exists because I have no idea how azure functions scales out and how local
        /// files are handled during that scale out process.  The idea is simple -- any time we
        /// have a local file, we copy it to azure.  Any time we need a local file, we make sure
        /// it exists locally.  If it does not exist locally, we pull the azure file locally. This
        /// should ensure that local files exists if they don't, but won't bother copying them down
        /// if they already do.
        /// </summary>
        /// <param name="localPath">Local path of the file.</param>
        /// <param name="azureContainer">Azure storage container containing the blob.</param>
        /// <param name="azureBlobName">Name of the blob to retrieve.</param>        
        /// <returns>a task indicating that the blob has been downloaded locally.</returns>
        private async Task EnsureAzureFileIsLocal(string localPath, string azureContainer, string azureBlobName)
        {
            if (!File.Exists(localPath))
            {
                Logger.LogInformation($"Restoring local file from azure: {azureContainer}/{azureBlobName} -> {localPath}");
                Stream s = null;
                FileStream fs = null;

                try
                {
                    IBlobs blobs = await IdentityInfrastructure.BlobsAsync();
                    using (s = await blobs.DownloadAsync(azureContainer, azureBlobName))
                    using (fs = new FileStream(localPath, FileMode.Create))
                    {
                        await s.CopyToAsync(fs);
                        s.Close();
                        fs.Close();
                    }
                }
                catch
                {
                    s?.Dispose();
                    fs?.Dispose();
                }
            }
        }

        /// <summary>
        /// Responsible for converting the name of the codec as it appears in FFPROBE into the name
        /// of the codec expected in an M3u8 multivariant playlist.
        /// </summary>
        /// <param name="codecName">Name of the codec to convert.</param>
        /// <returns>Converted representation of the codec.</returns>
        private string ConvertCodecName(string codecName)
        {
            switch (codecName)
            {
                case "h264": return "H.264";
                case "aac": return "AAC";
                default:
                    return codecName;
            }

        }

        private void CheckAddEncodingRequest(EncodeClipRequest clipRequest, string qualityLevelName, string[] segments, int originalSize, int horizontalWidth, int horizontalHeight, bool isVertical, List<EncodeVariantStreamRequest> qualityLevels, int rotation)
        {
            // Determine whether the original file size is large enough to support conversion
            if (originalSize >= (isVertical ? horizontalHeight : horizontalWidth))
            {
                if ((isVertical))
                {
                    //NOTE: negative one tells FFMPEG to keep aspect ratio
                    qualityLevels.Add(new EncodeVariantStreamRequest(clipRequest, qualityLevelName, segments, horizontalHeight, -1, rotation));
                }
                else
                {
                    //NOTE: negative one tells FFMPEG to keep aspect ratio
                    qualityLevels.Add(new EncodeVariantStreamRequest(clipRequest, qualityLevelName, segments, horizontalWidth, -1, rotation));
                }
            }
        }

        private void CreateM3u8File(EncodeVariantStreamRequest qualityLevelRequest, MediaDetails[] segmentDetailArray)
        {
            string filePath = Path.GetTempPath() + $"Soundbite\\{qualityLevelRequest.ClipRequest.UniqueFolderId}\\{qualityLevelRequest.QualityLevelName}\\playlist.m3u8";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#EXTM3U");
            sb.AppendLine("#EXT-X-VERSION:4");
            sb.AppendLine("#EXT-X-PLAYLIST-TYPE:VOD");
            sb.AppendLine("#EXT-X-TARGETDURATION:{TARGET_DURATION}");
            sb.AppendLine("#EXT-X-MEDIA-SEQUENCE:0");

            double maxDuration = 0;

            // Segment details are not guaranteed to be in order 
            segmentDetailArray.OrderBy(i => i.FileName).ForEach(segmentDetails =>
            {
                if (segmentDetails.DurationInSeconds > maxDuration)
                {
                    maxDuration = segmentDetails.DurationInSeconds;
                }
                sb.AppendLine($"#EXTINF:{segmentDetails.DurationInSeconds}");
                sb.Append(Path.GetFileName(segmentDetails.FileName));
                sb.AppendLine("{TOKEN}");
            });

            sb.Append("#EXT-X-ENDLIST");

            // Remove existing file if it is already there for some reason
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Write out the file content and make sure to replace {TARGET_DURATION} value!!!
            File.WriteAllText(filePath, sb.ToString().Replace("{TARGET_DURATION}", $"{maxDuration}"));
        }

        #endregion
    }
}