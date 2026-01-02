using AutoMapper;
using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Types;
using Masticore;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Services.Lifecycle;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{

    /// <summary>
    /// Implenentation of a service that handles persisting metadata as a Clip entity then returning a fully loaded IClip
    /// </summary>
    public class ClipService : InfrastructureServiceBase<ISbInfrastructure>, IClipService
    {
        protected IClipFileService ClipFileService { get; }
        protected ILifecycleFactory LifecycleFactory { get; }
        protected IRbac Rbac { get; }
        protected ISbMediaService SbMediaService { get; set; }

        public ClipService(
            ISecurityContext securityContext,
            IClipFileService clipFileService,
            ILifecycleFactory lifecycleFactory,
            ISbInfrastructure infrastructure,
            ISbMediaService sbMediaService,
            IMapper mapper,
            ILogger<ClipService> logger,
            IRbac rbac)
            : base(infrastructure, logger, securityContext, mapper)
        {
            SbMediaService = sbMediaService;
            ClipFileService = clipFileService ?? throw new ArgumentNullException(nameof(clipFileService));
            LifecycleFactory = lifecycleFactory ?? throw new ArgumentNullException(nameof(lifecycleFactory));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
        }

        #region Methods

        /// <summary>
        /// Complete the data store work required when a clip has received its storage-based upload
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="db"></param>
        /// <param name="clip"></param>
        /// <returns></returns>
        private async Task CompleteClipUpload(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, SbDb db, ClipEntity clip)
        {
            clip.ClipState = ClipState.Ready;
            await CheckClipDurationVersusSize(orgRoute, sessionRoute, promptRoute, clipRoute, clip);
            await StrategyPostClipAsync(orgRoute, sessionRoute, db, clip);
            await db.CreateClipEventWithDisplaySeconds(clipRoute, ClipEventType.ServerUpload, SecurityContext.CurrentUserId);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Conditionally updates the clip to have billing seconds matching its size if greater than existing billing seconds
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="clip"></param>
        /// <returns></returns>
        private async Task CheckClipDurationVersusSize(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, ClipEntity clip)
        {
            long bytes = await ClipFileService.SizeAsync(orgRoute, sessionRoute, promptRoute, clipRoute);
            long lengthForbytes = ClipAnalyzer.ByteCountAsSeconds(bytes);

            // Billing seconds up to this point was likely only set based on the API clip create, so this is a backup appraisal
            if (lengthForbytes <= clip.BillingSeconds)
            {
                return;
            }

            clip.BillingSeconds = (int)lengthForbytes;
        }

        /// <summary>
        /// Standalone version of firing <see cref="ILifecycleStrategy.AfterCreateClipAsync(SbDb, Organization, SessionEntity, ParticipantEntity, NewClip)"/>
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private async Task StrategyPostClipAsync(string orgRoute, string sessionRoute, SbDb db, ClipEntity clip)
        {
            int[] groupIds = await db.GroupIdsAsync(orgRoute, SecurityContext.UniversalId);
            SessionEntity session = (await db.SessionAsync(SecurityContext, orgRoute, sessionRoute, groupIds)).AssertFound();

            OrganizationEntity orgEntity = (await db.OrgWhereRoute(orgRoute)).AssertFound();
            Organization org = Mapper.MapSafe<Organization>(orgEntity);
            ILifecycleStrategy strategy = LifecycleFactory.StrategyForSession(session.SessionType);
            // TODO: Wrap this back up into a clientop once we have calendar support
            PersonEntity currentPerson = await db.PersonWhereUid(orgRoute, SecurityContext.UniversalId);
            // TODO: Consider storage uploads where the role is NOT host
            ParticipantEntity participant = GetOrCreateParticipant(db, ParticipantRole.Participant, groupIds, session, currentPerson);
            // TODO: Support sending in the clip object again
            await strategy.AfterCreateClipAsync(db, org, session, participant, clip);
        }

        /// <summary>
        /// Creates and commits the <see cref="ClipEntity"/> and optionally the <see cref="ParticipantEntity"/> connecting the given clip to the given session
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="newClip"></param>
        /// <returns></returns>
        protected async Task<ClipDetails> CreateAndUploadAsync(
            SbDb db,
            string orgRoute,
            string sessionRoute,
            string promptRoute,
            NewClip newClip)
        {
            ValidateStreamFileType(orgRoute, sessionRoute, promptRoute, newClip);

            int[] groupIds = await db.GroupIdsAsync(orgRoute, SecurityContext.UniversalId);
            SessionEntity session = (await db.SessionAsync(SecurityContext, orgRoute, sessionRoute, groupIds)).AssertFound();
            OrganizationEntity orgEntity = (await db.OrgWhereRoute(orgRoute)).AssertFound();
            Organization org = Mapper.MapSafe<Organization>(orgEntity);
            PersonEntity currentPerson = (await db.PersonWhereUid(orgRoute, SecurityContext.UniversalId)).AssertFound();
            ParticipantEntity participant = GetOrCreateParticipant(db, newClip.ParticipantRole, groupIds, session, currentPerson);

            // Run Pre-Commit Logic
            ILifecycleStrategy strategy = LifecycleFactory.StrategyForSession(session.SessionType);
            await strategy.BeforeCreateClipAsync(db, org, session, participant, newClip);

            // Create the actual clip
            promptRoute = CreateClip(db, promptRoute, newClip, session, currentPerson, out ClipAnalyzer analyzer, out ClipEntity clip);

            // Start building the response
            ClipDetails response = new ClipDetails
            {
                OrgRoute = orgRoute,
                SessionRoute = sessionRoute,
                PromptRoute = promptRoute,
                ClipRoute = clip.Route,
                TranscriptState = session.Transcribe ? TranscriptState.Requested : TranscriptState.None
            };

            //TODO: Look to see if the LifeCycle events can handle this better-er
            //  Something feels off about the way it processes here differently based
            //  on whether there is a stream.  Seems like there needs to be a LifeCycle
            //  event for ClipReady because AfterClip event is used two different ways
            //  right now -- one for "After adding clip record" is added and in another
            //  place it it used like file associated with clip exists and it's
            //  "Ready to Send Out Notifications"

            // If we already have a clip here, we upload right away; otherwise, we have to send back a place for them to put it
            if (newClip.Stream != null)
            {
                if (!string.IsNullOrEmpty(analyzer.PathToFileWithUpdatedDuration) && File.Exists(analyzer.PathToFileWithUpdatedDuration))
                {
                    using (FileStream fs = new FileStream(analyzer.PathToFileWithUpdatedDuration, FileMode.Open))
                    {
                        response.DownloadUrl = await ClipFileService.UploadAsync(org, sessionRoute, promptRoute, clip.Route, analyzer.FileType, newClip.Stream);
                        fs.Close();
                    }
                    try
                    {
                        File.Delete(analyzer.PathToFileWithUpdatedDuration);
                    }
                    catch
                    {
                        Logger.LogError($"Failed to delete temp duration header file: {analyzer.PathToFileWithUpdatedDuration}.  File will be cleaned up later.");
                    }
                }
                else
                {
                    response.DownloadUrl = await ClipFileService.UploadAsync(org, sessionRoute, promptRoute, clip.Route, analyzer.FileType, newClip.Stream);
                }

                // Determine whether additional media processing is required to finalize the clip
                if (newClip.MediaEffects?.Length > 0)
                {
                    // Requires additional processing so do not mark the clip ready yet
                    clip.ClipState = ClipState.Committed;
                }
                else
                {
                    // No additional processing so mark the clip as ready
                    clip.ClipState = ClipState.Ready;
                }
            }
            else
            {
                response.UploadUrl = await ClipFileService.UploadUrlAsync(org, sessionRoute, promptRoute, clip.Route, analyzer.FileType);
            }

            await db.SaveChangesAsync();

            response.State = clip.ClipState;
            response.HostingType = clip.HostingType;
            response.HostingData = clip.HostingData;

            // Post-commit logic, but only if we've fully prepped it (EG, upload on this request
            if (clip.ClipState == ClipState.Ready)
            {
                await strategy.AfterCreateClipAsync(db, org, session, participant, clip);
            }

            return response;
        }

        private static string CreateClip(SbDb db, string promptRoute, NewClip newClip, SessionEntity session, PersonEntity currentPerson, out ClipAnalyzer analyzer, out ClipEntity clip)
        {
            analyzer = ClipAnalyzer.CreateNew(newClip).Result;
            promptRoute = CheckForFallbackPrompt(promptRoute, session);
            clip = CreateClip(db, currentPerson.User, session, promptRoute, newClip, analyzer);
            return promptRoute;
        }

        /// <summary>
        /// Determines whether the uploaded file content conatins the expected encoding for the file type.
        /// </summary>
        private void ValidateStreamFileType(string orgRoute, string sessionRoute, string promptRoute, NewClip newClip)
        {
            // If we have a stream, then we must validate its file type
            if (newClip.Stream != null)
            {
                // Validate file type
                IFileType fileType = FileTypeValidator.GetFileType(newClip.Stream);

                // TODO: Figure out why video files (mostly mp4s from browser) come back with no type
                // Perhaps try another library later?
                if (fileType == null && (newClip.FileType == Masticore.FileType.Mp4 || newClip.FileType == Masticore.FileType.Mpg || newClip.FileType == Masticore.FileType.Webm))
                {
                    return;
                }

                switch (newClip.FileType)
                {
                    case Masticore.FileType.Mp3:
                    case Masticore.FileType.Mpg:
                        if (!(fileType is MpegAudio || fileType is Mp3))
                        {
                            Logger.LogError($"User '{SecurityContext?.CurrentUser?.Route ?? "UNKNOWN"}' tried to upload wrong audio file of type {fileType?.Name} to prompt '{promptRoute}' in session '{sessionRoute}' for org '{orgRoute}'");
                            throw new BadClipFileTypeException($"New Clip Stream Does Not Contain a Valid MP3; Appears to be type '{fileType?.Name ?? "UNKNOWN"}'");
                        }
                        break;
                    default:
                        // We're just going to accept all video files until we get this figured out more...
                        if (!newClip.FileType.IsVideo())
                        {
                            // This *should* only get hit during development unless someone is messing with data
                            Logger.LogCritical($"User '{SecurityContext?.CurrentUser?.Route ?? "UNKNOWN"}' tried to upload unsupported file of type {fileType?.Name} to prompt '{promptRoute}' in session '{sessionRoute}' for org '{orgRoute}'");
                            throw new BadClipFileTypeException($"New Clip Stream contains an unsupported file type.");
                        }
                        break;
                }
            }
        }

        private static string CheckForFallbackPrompt(string promptRoute, SessionEntity session)
        {
            // Announcements only have a single prompt so automatically populate the prompt route for announcements
            if (session.SessionType == SessionType.Announcement && promptRoute == null)
            {
                promptRoute = session.Prompts.FirstOrDefault()?.Route;
            }

            return promptRoute;
        }

        private static ParticipantEntity GetOrCreateParticipant(
            SbDb db,
            ParticipantRole role,
            int[] groupIds,
            SessionEntity session,
            PersonEntity currentPerson)
        {
            ParticipantEntity participant = session.Participants
                                                // No need to check the whole chain since we wouldn't have a session if anything else was deleted
                                                .Where(p =>
                                                    p.PersonId == currentPerson.Id &&
                                                    p.ParticipantRole >= role)
                                                .FirstOrDefault();

            // If they had a participant and now it's deleted, then they're not allowed to contribute
            if (participant.DeletedUtc != null)
            {
                throw new NotFoundException();
            }

            if (participant == null)
            {
                ParticipantGroupEntity group = session.Groups
                                                // No need to check the chain because we know we have a good session
                                                .Where(g =>
                                                    groupIds.Contains(g.GroupId) &&
                                                    g.ParticipantRole >= role &&
                                                    g.DeletedUtc == null)
                                                .FirstOrDefault();
                group.AssertFound();

                participant = db.Participants.CreateResource(currentPerson.User);
                participant.ParticipantRole = role;
                participant.Person = currentPerson;
                session.Participants.Add(participant);
            }

            if (participant.ParticipantState == ParticipantState.ContributionRequested)
            {
                participant.ParticipantState = ParticipantState.Contributed;
                participant.UpdatedUtc = Time.UtcNow;
            }

            return participant;
        }

        private static ClipEntity CreateClip(
            SbDb db,
            UserEntity currentUser,
            SessionEntity session,
            string promptRoute,
            NewClip newClip,
            ClipAnalyzer analyzer)
        {
            ClipEntity clip = db.Clips.CreateResource(currentUser);
            // Key fields from call context
            clip.ClipType = newClip.ClipType;
            clip.FileType = analyzer.FileType;
            clip.MediaEffects = newClip.MediaEffects;
            clip.Prompt = session.Prompts.Where(p => p.Route == promptRoute && p.DeletedUtc == null).FirstOrDefault();
            clip.Prompt.AssertFound($"Could not find prompt {promptRoute}");
            // If we don't have a seconds from the API, then trust the analyzer
            clip.DisplaySeconds = newClip.Seconds <= 0 ? analyzer.Seconds : newClip.Seconds;
            clip.BillingSeconds = analyzer.Seconds;
            // Just making the record is getting started
            clip.ClipState = ClipState.Started;
            clip.TranscriptState = session.Transcribe ? TranscriptState.Requested : TranscriptState.None;


            // Save to context
            clip.Prompt.Clips.Add(clip);
            return clip;
        }

        private async Task<ClipState> ProcessAndSaveNextStepAsync(
            string orgRoute,
            string sessionRoute,
            string promptRoute,
            string clipRoute,
            SbDb db,
            ClipEntity clip)
        {
            // We do not yet have async processing in AzFun or anything like that
            // So this will check what it is in the database and optionally process things to the currently simple next step
            // This is useful for clips uploaded to storage due to size
            bool clipFileExists = await ClipFileService.ExistsAsync(orgRoute, sessionRoute, promptRoute, clipRoute);

            // TODO: Make a better status check than just "does the file exist"
            // Right now, the only validation is ensuring it exists, so this is very simple
            if (clip.ClipState != ClipState.Ready && clipFileExists)
            {
                await CompleteClipUpload(orgRoute, sessionRoute, promptRoute, clipRoute, db, clip);
            }

            return clip.ClipState;
        }

        #endregion

        #region IClipService

        /// <summary>
        /// Uploads a clip for a broadcast session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to which the clip belongs.</param>
        /// <param name="newClip">Clip to upload.</param>
        /// <param name="mediaOpRequest">Request containing media operations that need to be processed for the clip.</param>
        /// <returns>the URL to the location were the clip has been persisted along with client operations that need to run on the client.</returns>
        public async Task<ClipDetails> CreateAnnouncementAsync(string orgRoute, string sessionRoute, NewClip newClip)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNull(nameof(newClip), newClip);

            SbDb db = await Infrastructure.DbAsync();
            ClipDetails result = await CreateAndUploadAsync(db, orgRoute, sessionRoute, null, newClip);
            return result;
        }

        /// <summary>
        /// Uploads a clip for a session.
        /// </summary>
        /// <param name="orgRoute">Route identifying the organization containing the session.</param>
        /// <param name="sessionRoute">Route identifying the session containing the clip.</param>
        /// <param name="promptRoute">Route identifying the prompt in response to which the clip was uploaded.</param>
        /// <param name="newClip">Clip to upload.</param>
        /// <param name="mediaOpRequest">Request containing media operations that need to be processed for the clip.</param>
        /// <returns>the URL to the location were the clip has been persisted along with client operations that need to run on the client.</returns>
        public async Task<ClipDetails> CreateAsync(string orgRoute, string sessionRoute, string promptRoute, NewClip newClip)
        {
            // Validate arguments
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNull(nameof(promptRoute), promptRoute);
            Validator.ArgNotNull(nameof(newClip), newClip);

            // Upload clip
            SbDb db = await Infrastructure.DbAsync();
            ClipDetails result = await CreateAndUploadAsync(db, orgRoute, sessionRoute, promptRoute, newClip);
            return result;
        }

        public async Task<ClipState> StateAsync(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            // TODO: Assert use is attached to the prompt as a contributor
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            SbDb db = await Infrastructure.DbAsync();

            // Pull out the clip and determine if it is either unkown or already done
            // Those are the easy ones
            ClipEntity clip = await db.ClipAsync(orgRoute, clipRoute);
            if (clip == null)
            {
                return ClipState.Unknown;
            }

            if (clip.ClipState == ClipState.Ready)
            {
                return ClipState.Ready;
            }

            return await ProcessAndSaveNextStepAsync(orgRoute, sessionRoute, promptRoute, clipRoute, db, clip);
        }

        #endregion
    }
}
