using Masticore;
using Masticore.Entity;
using Masticore.Jobs;
using Masticore.Models;
using Masticore.Queue;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Masticore.Storage;
using Masticore.Transcription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models.JobRequests;
using Soundbite.Services.Lifecycle;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// EF implementation of <see cref="ISbTranscriptionService"/>
    /// </summary>
    public class SbTranscriptionService : ISbTranscriptionService
    {
        #region Fields

        readonly bool isTranscriptionQueued = MasticoreExtensions.GetEnvBool("SB_QUEUED_TRANSCRIPTS", true);

        #endregion

        #region Properties

        private ILogger Logger { get; set; }
        private ISbInfrastructure Infrastructure { get; set; }
        private ITranscriptionService TranscriptionService { get; set; }
        private ISecurityContext SecurityContext { get; set; }
        private IJobQueue JobQueue { get; set; }
        private IQueuedJobStatusService QueuedJobStatusService { get; set; }
        private ILifecycleFactory LifecycleFactory { get; set; }
        private IRbac Rbac { get; set; }
        public IClipFileService ClipFileService { get; }
        public ITranscriptFileService TranscriptFileService { get; }

        #endregion

        #region Methods

        public SbTranscriptionService(
            ILogger<SbTranscriptionService> logger,
            ISbInfrastructure infrastructure,
            ISecurityContext securityContext,
            ITranscriptionService transcriptionService,
            IJobQueue jobQueue,
            IQueuedJobStatusService queuedJobStatusService,
            IClipFileService clipFileService,
            ITranscriptFileService transcriptFileService,
            ILifecycleFactory lifeCycleFactory,
            IRbac rbac)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            TranscriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
            SecurityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
            JobQueue = jobQueue ?? throw new ArgumentNullException(nameof(jobQueue));
            QueuedJobStatusService = queuedJobStatusService ?? throw new ArgumentNullException(nameof(queuedJobStatusService));
            ClipFileService = clipFileService ?? throw new ArgumentNullException(nameof(clipFileService));
            TranscriptFileService = transcriptFileService ?? throw new ArgumentNullException(nameof(transcriptFileService));
            LifecycleFactory = lifeCycleFactory ?? throw new ArgumentNullException(nameof(lifeCycleFactory));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
        }

        public async Task<TRet> VerifyAndLoadEntities<TRet>(string orgRoute, string sessionRoute, string clipRoute, bool isPublic,
            Func<OrganizationEntity, PromptEntity, ClipEntity, Task<TRet>> func)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNull(nameof(clipRoute), clipRoute);

            SbDb db = await Infrastructure.DbAsync();
            if (!await db.UserHasSessionAccess(SecurityContext.UniversalId, orgRoute, sessionRoute, isPublic))
            {
                return default;
            }

            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Failed to locate org ({orgRoute})");
            ClipEntity clip = await db.ClipAsync(orgRoute, clipRoute);
            clip.AssertFound($"Failed to locate clip ({clipRoute})");
            PromptEntity prompt = await db.Prompts.Where(i => i.Id == clip.PromptId).FirstOrDefaultAsync();
            prompt.AssertFound($"Failed to locate prompt for clip ({clipRoute})");

            TRet ret = await func(org, prompt, clip);
            return ret;

        }

        #endregion

        #region ISbTranscriptionService Implementation

        /// <inheritdoc />
        public Task DeleteTranscript(string orgRoute, string sessionRoute, string clipRoute)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task ProcessTranscriptionRequest(TranscriptionRequest request)
        {
            try
            {
                // Validate
                Validator.NotNull(request, "Request cannot be null");
                request.Validate();

                // Variables
                string clipPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                // Acquire data
                SbDb db = await Infrastructure.DbAsync();
                OrganizationDetails org = await db.OrganizationDetails(request.OrgRoute);
                org.AssertFound($"Transcription failed because the specified organization (route:{request.OrgRoute}) could not be found.");
                SessionEntity sessionEntity = await db.SessionAsync(SecurityContext, request.OrgRoute, request.SessionRoute, new int[] { }, Rbac);
                sessionEntity.AssertFound($"Media processing failed because the specified session (route:{request.SessionRoute}) could not be found.");
                ClipEntity clip = db.Clips.FirstOrDefault(i => i.Route == request.ClipRoute);
                clip.AssertFound($"Transcription failed because the requested clip ({request.ClipRoute}) could not be found.");

                // Call the pre media processing lifecycle event for the session
                ILifecycleStrategy lifecycleStrategy = LifecycleFactory.StrategyForSession(sessionEntity.SessionType);
                await lifecycleStrategy.BeforeClipTranscribed(db, org, sessionEntity, clip);

                try
                {
                    IBlobs blobService = await Infrastructure.BlobsAsync();
                    Validator.NotNull(blobService, "Transcription failed because the Blob service was null.");

                    using (Stream clipStream = await GetClipStream(request))
                    using (FileStream fileStream = File.Create(clipPath))
                    {
                        await clipStream.CopyToAsync(fileStream);
                        fileStream.Close();
                        clipStream.Close();
                    }

                    TranscriptionResult result = await TranscriptionService.TranscribeFile(clipPath, clip.FileType);
                    await TranscriptFileService.UploadAsync(request.OrgRoute, request.SessionRoute, request.PromptRoute, request.ClipRoute, result);

                    // Update clip to denote that the transcript is now available.
                    clip.TranscriptState = TranscriptState.Available;
                    db.Update(clip);
                    await db.SaveChangesAsync();

                    // Call the post clip transcribed lifecycle event for the session                    
                    await lifecycleStrategy.AfterClipTranscribed(db, org, sessionEntity, clip);
                }
                catch
                {
                    // Transcription failed so update the clip to denote the failure.
                    clip.TranscriptState = TranscriptState.Failed;
                    db.Update(clip);
                    await db.SaveChangesAsync();

                    // Let the outter handler log the issue
                    throw;
                }

                // Delete temp file but do NOT stop processing if it cannot be deleted.
                try
                {
                    File.Delete(clipPath);
                }
                catch
                {
                    Logger.LogWarning($"Failed to delete temporary file: {clipPath}");
                }
            }
            catch (Exception ex)
            {
                // Log failure but do not throw an exception.  Since the clip is not available
                // there is no way to denote the failure on the clip in the database.
                Logger.LogError(ex, $"Transcription failed (Org:{request.OrgRoute}|Session:{request.SessionRoute}|Clip:{request.ClipRoute}) with error: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines whether to download the clip directly or the audio.wav file used for the
        /// transcription of video files.
        /// </summary>
        /// <param name="request">Transcription request containing request information.</param>
        /// <returns>a Stream pointing to the appropriate file, ready for use.</returns>
        private async Task<Stream> GetClipStream(TranscriptionRequest request)
        {
            if (request.ClipFileType == FileType.Mp4)
            {
                return await ClipFileService.DownloadAudioExtractedFromVideoClipAsync(request.OrgRoute, request.SessionRoute, request.PromptRoute, request.ClipRoute);
            }
            else
            {
                return await ClipFileService.DownloadAsync(request.OrgRoute, request.SessionRoute, request.PromptRoute, request.ClipRoute);
            }
        }

        /// <inheritdoc />
        public async Task QueueTranscriptionRequest(TranscriptionRequest request)
        {
            if (isTranscriptionQueued)
            {
                QueuedJobStatus status = new QueuedJobStatus()
                {
                    OrgRoute = request.OrgRoute,
                    JobType = TypeNameUtils.TypeNameForClass<TranscriptionJob>(),
                    Route = ResourceExtensions.NewRoute(),
                    Description = $"Transcription Request - Org:{request.OrgRoute} - Session:{request.SessionRoute} - Clip:{request.ClipRoute}",
                    JobStatus = JobStatusType.Requesting,
                    RequestDate = DateTime.UtcNow
                };
                TranscriptionJob job = new TranscriptionJob(request);
                job.JobStatusRoute = status.Route;
                status.QueueMessage = job.Message;

                await QueuedJobStatusService.SaveAsync(status);

                try
                {
                    // Queue the transcription request
                    await JobQueue.Add(job);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to queue transcript request.");
                    try
                    {
                        status.JobStatus = JobStatusType.Failed;
                        status.Details = JsonUtils.ToLowerCamelJson(ex);
                        await QueuedJobStatusService.SaveAsync(status);
                    }
                    catch (Exception exInner)
                    {
                        Logger.LogError(exInner, $"Failed to update job status ({status.Route}) with failure information.");
                    }
                    throw;
                }
            }
            else
            {
                // Process the transcription request immediately
                await ProcessTranscriptionRequest(request);
            }
        }

        /// <inheritdoc />
        public async Task<string> ReadTranscriptFileUrl(string orgRoute, string sessionRoute, string clipRoute, bool isPublic)
        {
            string ret = await VerifyAndLoadEntities(orgRoute, sessionRoute, clipRoute, isPublic, async (org, prompt, clip) =>
            {
                string url = await TranscriptFileService.DownloadUrlAsync(orgRoute, sessionRoute, prompt.Route, clip.Route);
                return url;
            });
            return ret;
        }

        public async Task<TranscriptionResult> ReadTranscript(string orgRoute, string sessionRoute, string clipRoute, bool isPublic)
        {
            TranscriptionResult ret = await VerifyAndLoadEntities(orgRoute, sessionRoute, clipRoute, isPublic, async (org, prompt, clip) =>
            {
                TranscriptionResult result = await TranscriptFileService.DownloadAsync(orgRoute, sessionRoute, prompt.Route, clip.Route);
                return result;
            });
            return ret;
        }

        #endregion
    }
}
