using Masticore;
using Masticore.Media;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Services;
using Soundbite.Services.Lifecycle;
using System;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with clips.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class ClipController : ControllerBase
    {
        #region Testing

        /// <summary>
        /// Allows for the processing of a missed AMS event from the event grid.
        /// </summary>
        /// <param name="clipOperationService">DI service reference.</param>
        /// <param name="infrastructure">DI service reference.</param>
        /// <param name="lifecycleFactory">DI service reference.</param>
        /// <param name="clipOpRoute">Route of the clip operation to process.</param>
        /// <returns>a string containing text about the operation</returns>
        [Route("/SimulateAmsGridEvent/{clipOpRoute}")]
#if DEBUG
        [AllowAnonymous]
        [HttpGet]
#else
        [HttpPost]
#endif
        public async Task<string> SimulateAmsGridEvent(
            [FromServices] IClipOperationService clipOperationService,
            [FromServices] ISbInfrastructure infrastructure,
            [FromServices] ILifecycleFactory lifecycleFactory,
            string clipOpRoute)
        {
            try
            {
                await clipOperationService.SetState(clipOpRoute, ClipOperationStateType.Complete, null);
                SbDb db = await infrastructure.DbAsync();
                SessionType sessionType = await db.ReadSessionTypeByClipOpRoute(clipOpRoute);
                ILifecycleStrategy lifeCycle = lifecycleFactory.StrategyForSession(sessionType);
                await lifeCycle.AfterClipOperationComplete(db, clipOpRoute);
                return "OK";
            }
            catch (Exception ex)
            {
                return $"Error-{ex.Message}";
            }

        }

        #endregion

        #region Static

        /// <summary>
        /// Extracts a <see cref="NewClip"/> object from the given <see cref="HttpRequest"/>
        /// </summary>
        /// <remarks>
        /// If this does NOT have a value for <see cref="NewClip.Stream"/>, then the intention is for the caller to upload direct to storage AFTER creating an initial record for the clip
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        protected static NewClip NewClipFromRequest(HttpRequest request)
        {
            ClipType clipType = request.Form.ToEnum<ClipType>("clipType");
            FileType fileType = request.Form.ToEnum<FileType>("fileType");
            ClipHostingType hostingType = request.Form.ToEnum("hostingType", ClipHostingType.AzureStorage);
            ParticipantRole participantRole = request.Form.ToEnum<ParticipantRole>("participantRole");
            IMediaEffect[] mediaEffects = null;

            if (request.Form.TryGetValue("mediaEffects", out Microsoft.Extensions.Primitives.StringValues mediaOpRequestJson))
            {
                //TODO: need to figure out a way to convert JSON with various types in the media effects
            }
            int seconds = request.Form.ToInt("seconds");

            NewClip newClip = new NewClip
            {
                ClipType = clipType,
                FileType = fileType,
                ParticipantRole = participantRole,
                Seconds = seconds,
                MediaEffects = mediaEffects
            };

            if (request?.Form?.Files?.Count > 0)
            {
                newClip.Stream = request.Form.Files[0].OpenReadStream();
            }

            return newClip;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets to <see cref="ISessionService"/> object for this controller
        /// </summary>
        protected ISessionService SessionService { get; }

        /// <summary>
        /// Gets the <see cref="IClipFileService"/> object for this controller
        /// </summary>
        protected IClipService ClipService { get; }

        /// <summary>
        /// Gets the <see cref="IOrganizationService"/> object for this controller
        /// </summary>
        protected IOrganizationService OrgService { get; }

        /// <summary>
        /// Gets the <see cref="ISbTranscriptionService"/> instance for this controller.
        /// </summary>
        protected ISbTranscriptionService TranscriptionService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionsController"/> class.
        /// </summary>
        /// <param name="sessionService">ISessionService DI reference.</param>
        /// <param name="clipService">IClipService DI reference.</param>
        /// <param name="orgService">IOrganizationService DI reference.</param>
        /// <param name="transcriptionService">ISbTranscriptionService DI reference.</param>
        public ClipController(ISessionService sessionService, IClipService clipService, IOrganizationService orgService, ISbTranscriptionService transcriptionService)
        {
            SessionService = sessionService;
            ClipService = clipService;
            OrgService = orgService;
            TranscriptionService = transcriptionService;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Uploads an audio clip and associates it to a prompt within a session. 
        /// This include pulling the clip stream, clip type, file type, seconds, and participant role from the form data.
        /// </summary>
        /// <remarks>
        /// If there was a file included with the form data, that will be uploaded to Storage. 
        /// This will only work if the file is les than 29MB. 
        /// If no clip is given, then this unlocks a location in Azure Storage for receiving the clip within the hour. 
        /// That allows a clip size of gigabytes: https://docs.microsoft.com/en-us/azure/storage/blobs/scalability-targets#scale-targets-for-blob-storage
        /// </remarks>
        /// <remarks>This can only accept a file in the </remarks>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the prompt.</param>
        /// <param name="promptRoute">Route of the prompt to which the clip should be associated.</param>
        /// <returns>the URL to the location were the clip has been persisted along with client operations that need to run on the client.</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/Prompts/{promptRoute}/Clips")]
        [HttpPost]
        [CodeGenMethod(Scenario = CodeGenScenarioType.FileUploadSingleFromFormData)]
        [Obsolete("Use the CreateAsync unwrapped version below")]
        public async Task<ClientOpWrapper<ClipDetails>> CreateAsync_Obsolete(string orgRoute, string sessionRoute, string promptRoute)
        {
            using NewClip newClip = NewClipFromRequest(Request);

            ClipDetails ret = await ClipService.CreateAsync(orgRoute, sessionRoute, promptRoute, newClip);
            return new ClientOpWrapper<ClipDetails>(ret);
        }

        /// <summary>
        /// Async create a clip; TODO Move to old Create clips route
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/Prompts/{promptRoute}/Clips/New")]
        [HttpPost]
        [CodeGenMethod(Scenario = CodeGenScenarioType.FileUploadSingleFromFormData)]
        public async Task<ClipDetails> CreateAsync(string orgRoute, string sessionRoute, string promptRoute)
        {
            using NewClip newClip = NewClipFromRequest(Request);

            ClipDetails ret = await ClipService.CreateAsync(orgRoute, sessionRoute, promptRoute, newClip);
            return ret;
        }

        /// <summary>
        /// Gets the current state for the given clip
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/Prompts/{promptRoute}/Clips/{clipRoute}/state")]
        [HttpGet]
        public async Task<ClipState> StateAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            // NOTE: Until there is async AzFun processing, this may have side-effects in processing even though it looks like a GET
            // This slightly bent abstraction seems worth it for not having to change the API definition later
            return await ClipService.StateAsync(orgRoute, sessionRoute, promptRoute, clipRoute);
        }

        /// <summary>
        /// Uploads an audio clip and associates it to a only prompt within an announcement session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session containing the prompt.</param>
        /// <returns>the URL to the location were the clip has been persisted along with client operations that need to run on the client.</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/Prompts/Announcements")]
        [HttpPost]
        [CodeGenMethod(Scenario = CodeGenScenarioType.FileUploadSingleFromFormData)]
        public async Task<ClipDetails> CreateAnnouncementClipAsync(string orgRoute, string sessionRoute)
        {
            using NewClip newClip = NewClipFromRequest(Request);

            return await ClipService.CreateAnnouncementAsync(orgRoute, sessionRoute, newClip);
        }

        /// <summary>
        /// Retrieves a URL w/token that can be used to request the transcript file for the specified clip.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the session.</param>
        /// <param name="sessionRoute">Route of the session associated with the clip.</param>
        /// <param name="clipRoute">Route of the clip whose transcript is being sought.</param>
        /// <param name="isPublic">Flag indicating whether the session is public.</param>
        /// <returns>a string containing a URL w/token that can be used to request the transcript.</returns>
        [AllowAnonymous]
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/Prompts/{promptRoute}/Clips/{clipRoute}/transcript")]
        [HttpGet]
        public async Task<string> GetTranscriptUrl(string orgRoute, string sessionRoute, string clipRoute, bool isPublic = false)
        {
            string result = await TranscriptionService.ReadTranscriptFileUrl(orgRoute, sessionRoute, clipRoute, isPublic);
            return result;
        }

        #endregion
    }
}