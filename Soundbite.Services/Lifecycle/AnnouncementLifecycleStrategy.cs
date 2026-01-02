using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Media;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Extensions;
using Soundbite.Models;
using Soundbite.Models.JobRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services.Lifecycle
{
    /// <summary>
    /// Controls the lifecycle of an announcement (broadcast) session
    /// </summary>
    public class AnnouncementLifecycleStrategy : LifecycleStrategyBase
    {
        #region Fields

        private Dictionary<string, ParticipantEntity> _newParticipants = new Dictionary<string, ParticipantEntity>();

        #endregion

        #region Properties

        private ISbNotificationService Notifications { get; }
        private ISbTranscriptionService SbTranscriptionService { get; }
        private ISbMediaService SbMediaService { get; }
        public ILogger Logger { get; set; }
        private IMapper Mapper { get; }

        /// <summary>
        /// Stores a list of participants using person route + participant role as the key. Used to
        /// avoid duplicating participant records when expanding groups.
        /// </summary>
        private Dictionary<string, ParticipantEntity> NewParticipants
        {
            get
            {
                _newParticipants = _newParticipants ?? new Dictionary<string, ParticipantEntity>();
                return _newParticipants;
            }
        }
        protected bool IsPromptReadyForPublish(PromptEntity prompt)
        {
            // Does each prompt have a prompt clip on it?
            return prompt.Clips.Where((c) =>
                c.ClipType == ClipType.Prompt
                && c.MediaProcessingState.IsReady()
            ).Count() > 0;
        }

        protected bool IsSessionReadyForPublish(SessionEntity session)
        {
            // Does not have any prompts that are waiting on their prompts
            //return session.Prompts.All((p) => IsPromptReadyForPublish(p));
            return !session.Prompts.Select((p) => IsPromptReadyForPublish(p)).Contains(false);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnouncementLifecycleStrategy"/> class.
        /// </summary>
        /// <param name="notifications">Notification service reference.</param>
        /// <param name="mapper">DI reference to an object mapper.</param>
        /// <param name="logger">Logger reference.</param>
        /// <param name="sbTranscriptionService">DI reference to a Soundbite transcription service.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnnouncementLifecycleStrategy(
            ISbNotificationService notifications,
            IMapper mapper,
            ILogger<AnnouncementLifecycleStrategy> logger,
            ISbMediaService sbMediaService,
            ISbTranscriptionService sbTranscriptionService)
        {
            Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SbMediaService = sbMediaService;
            SbTranscriptionService = sbTranscriptionService;
        }

        #endregion

        #region ILifecycleStrategy Implementation

        /// <inheritdoc />
        public override Task BeforeCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, NewClip newClip)
        {
            // Security validation: Only a host participant can upload to an announcement
            if (participant.ParticipantRole != ParticipantRole.Host)
            {
                Logger.LogWarning("Aborting participant clip upload because only the host can upload a clip for an announcement.");
                throw new NotFoundException();
            }

            // Determine if this is a video file and if so make sure to encode it with the default encoder
            if (newClip.FileType.IsVideo())
            {
                newClip.MediaEffects = new[] { new SbMediaEncoding() };
            }

            return Task.FromResult<ClientOpCollection>(null);
        }

        /// <inheritdoc />
        public override async Task AfterCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, ClipEntity newClip)
        {
            bool isClipReady = true;
            await UpdateParticipantStateAsync(db, participant);

            // Determine whether this is a video file
            if (newClip.FileType.IsVideo() && newClip.MediaEffects.Length > 0)
            {
                // All video files undergo standard encoding at this point
                isClipReady = await SbMediaService.ProcessMediaEffects(org.Route, session.Route, newClip.Route, newClip.MediaEffects);
                newClip.MediaProcessingState = isClipReady ? MediaProcessingState.Complete : MediaProcessingState.Requested;
                db.Clips.Update(newClip);
                await db.SaveChangesAsync();
            }

            if (isClipReady)
            {
                // Clip is ready for transcription and notifications
                await OnClipReady(db, org, session, newClip);
            }
        }

        /// <inheritdoc />        
        public override async Task RemindAsync(SbDb db, Organization org, SessionEntity session)
        {
            Validator.ArgNotNull(nameof(db), db);
            Validator.ArgNotNull(nameof(org), org);
            Validator.ArgNotNull(nameof(session), session);

            // If we are NEVER reminding or have ALREADY reminded, then we skip this session
            if (session.Reminder != null && session.ReminderSent == null && session.Reminder < Time.UtcNow.AddMinutes(5))
            {
                Logger.LogInformation($"Processing reminders for session {session.Route}");

                if (!IsSessionAlreadyContributed(session))
                {
                    await SendNotificationsAsync(db, org, session, ParticipantRole.Host, true);
                }

                session.ReminderSent = Time.UtcNow;
                await db.SaveChangesAsync();
            }
            else
            {
                Logger.LogInformation($"Skipping reminders for session {session.Route}");
            }
        }

        /// <inheritdoc />
        public override async Task PublishAsync(SbDb db, Organization org, SessionEntity session)
        {
            Validator.ArgNotNull(nameof(db), db);
            Validator.ArgNotNull(nameof(org), org);
            Validator.ArgNotNull(nameof(session), session);

            // Only process this section if publish notification has not been sent
            if (session.PublishSent != null)
            {
                Logger.LogInformation($"Aborting publish for session {session.Route}; already sent");
                return;
            }

            // If the deadline has passed and the session is ready
            // TODO: Consider how to notify people that the session is incomplete
            bool isScheduledForNow = session.Publish == null || session.Publish <= Time.UtcNow.AddMinutes(5);
            if (!isScheduledForNow)
            {
                Logger.LogInformation($"Aborting publish for session {session.Route}; not scheduled for now");
                return;
            }

            // Do not publish if the session is not ready
            if (!IsSessionReadyForPublish(session))
            {
                Logger.LogInformation($"Aborting publish for session {session.Route}; not ready");
                return;
            }

            // Acquire announcement clip
            ClipEntity clip = session.Prompts?.FirstOrDefault()?.Clips?.FirstOrDefault();

            // Do not publish if there are pending media effects
            if (clip?.MediaEffects.Any() == true && clip.ClipState != ClipState.Ready)
            {
                Logger.LogInformation($"Aborting publish for session {session.Route}; pending media operations");
                return;
            }

            await ProcessPublishAsync(db, org, session);
        }

        /// <inheritdoc />
        public override async Task AfterProcessSeriesAsync(SbDb db, SeriesEntity series)
        {
            // Retract old and ignored instances
            SessionEntity[] pastSessions = await db.IgnoredPastSessions(series.Id);
            // Only ever leave the newest untouched old one so they have a chance to jump to the last entry in the backlog
            pastSessions.Skip(1).ForEach(s => s.SoftDelete());
        }

        /// <inheritdoc />
        public override async Task AfterClipMediaProcessing(SbDb db, Organization org, SessionEntity session, ClipEntity clip)
        {
            await OnClipReady(db, org, session, clip);
        }

        /// <inheritdoc />
        public override async Task AfterClipOperationComplete(SbDb sbDb, string clipOpRoute)
        {
            Organization org = null;
            ClipOperationEntity clipOpEntity = sbDb.ClipOperations
                .Where(i =>
                    i.Route == clipOpRoute
                    && i.Clip.DeletedUtc == null
                    && i.Clip.Prompt.DeletedUtc == null
                    && i.Clip.Prompt.Session.DeletedUtc == null
                    && i.Clip.Prompt.Session.Organization.DeletedUtc == null
                    && i.Clip.Prompt.Session.Organization.Tenant.DeletedUtc == null)
                .Include(i => i.Clip)
                .Include(i => i.Clip.Prompt)
                .Include(i => i.Clip.Prompt.Session)
                .Include(i => i.Clip.Prompt.Session.Organization)
                .Include(i => i.Clip.Prompt.Session.Groups)
                .Include(i => i.Clip.Prompt.Session.Participants)
                    .ThenInclude(i => i.Person)
                    .ThenInclude(i => i.User)
                .FirstOrDefault();

            if (clipOpEntity == null)
            {
                throw new Exception($"Failed to locate session data based on clip operation route ({clipOpRoute})");
            }

            switch (clipOpEntity.OperationType)
            {
                case ClipOperationType.AzureMediaSvcEncoding:

                    // Clip media processing is complete so update clip state
                    clipOpEntity.Clip.MediaProcessingState = MediaProcessingState.Complete;
                    sbDb.Clips.Update(clipOpEntity.Clip);
                    await sbDb.SaveChangesAsync();

                    // Setup Streaming for Clip Since it is Ready
                    await SbMediaService.StreamClip(
                        clipOpEntity.Clip.Prompt.Session.Organization.Route,
                        clipOpEntity.Clip.Prompt.Session.Route,
                        clipOpEntity.Clip.Prompt.Route,
                        clipOpEntity.Clip.Route
                    );

                    // Ensure the display/billing time
                    await SbMediaService.SetClipDurationInfo(
                        clipOpEntity.Clip.Prompt.Session.Organization.Route,
                        clipOpEntity.Clip.Prompt.Session.Route,
                        clipOpEntity.Clip.Route);

                    // Fire OnClipReady because media processing is complete
                    org = Mapper.Map<Organization>(clipOpEntity.Clip.Prompt.Session.Organization);
                    await OnClipReady(sbDb, org, clipOpEntity.Clip.Prompt.Session, clipOpEntity.Clip);
                    break;

                case ClipOperationType.AzureMediaSvcTranscribe:
                    await SbMediaService.OnTranscriptionComplete(
                        clipOpEntity.Clip.Prompt.Session.Organization.Route,
                        clipOpEntity.Clip.Prompt.Session.Route,
                        clipOpEntity.Clip.Prompt.Route,
                        clipOpEntity.Clip.Route
                    );

                    break;

                case ClipOperationType.SoundbiteMediaEncoding:

                    // Clip media processing is complete so update clip state
                    clipOpEntity.Clip.MediaProcessingState = MediaProcessingState.Complete;
                    clipOpEntity.Clip.HostingType = ClipHostingType.SbStreaming;
                    clipOpEntity.Clip.HostingData = ""; //FARK - not sure if something needs to be here
                    sbDb.Clips.Update(clipOpEntity.Clip);
                    await sbDb.SaveChangesAsync();

                    // Clip is now ready
                    org = Mapper.Map<Organization>(clipOpEntity.Clip.Prompt.Session.Organization);
                    await OnClipReady(sbDb, org, clipOpEntity.Clip.Prompt.Session, clipOpEntity.Clip);

                    break;
            }


        }

        #endregion

        #region Methods

        private async Task OnClipReady(SbDb db, Organization org, SessionEntity session, ClipEntity clip)
        {
            // Clip is ready for transcription and notifications
            await QueueTranscription(db, org, session, clip);
            await RemindAsync(db, org, session);
            await PublishAsync(db, org, session);
        }

        protected bool IsSessionAlreadyContributed(SessionEntity session)
        {
            // Does not have any prompts that are waiting on their prompts
            return session.Participants.Where(p => p.ParticipantRole == ParticipantRole.Host).All(p => p.ParticipantState == ParticipantState.Contributed);
        }

        /// <summary>
        /// Does the actual work of making the session available for consumption, plus notifying participants
        /// </summary>
        /// <param name="db"></param>
        /// <param name="org"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        private async Task ProcessPublishAsync(SbDb db, Organization org, SessionEntity session)
        {
            try
            {
                Logger.LogInformation($"Processing publish for session {session.Route}");

                await MarkSessionPublishedAsync(db, session);

                await SendNotificationsAsync(db, org, session, ParticipantRole.Audience, false);

                await SendPublisherNotificationsAsync(db, org, session);

                // One last save, just in case
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Swallow for the run
                // If this is happening, it might mean this session goes into a zombie state of some kind, so it's critical we get manual intervention
                Logger.LogCritical($"Failed while processing session '{session.Route}' in org '{org.Route}' with error {ex.Message} and stack {ex.StackTrace}");
            }
        }

        private async Task MarkSessionPublishedAsync(SbDb db, SessionEntity session)
        {
            // Save all of the timestamps on there if they're missing
            session.PublishSent = Time.UtcNow;
            Logger.LogInformation($"Set PublishSent for session {session.Route} to {session.PublishSent}");

            if (session.Publish == null)
            {
                session.Publish = session.PublishSent;
                Logger.LogInformation($"Set Publish for session {session.Route} to {session.PublishSent}");
            }

            if (session.ReminderSent == null)
            {
                session.ReminderSent = session.PublishSent;
                Logger.LogInformation($"Set ReminderSent for session {session.Route} to {session.PublishSent}");
            }

            await db.SaveChangesAsync();
        }

        private async Task UpdateParticipantStateAsync(SbDb db, ParticipantEntity participant)
        {
            // Check if each prompt has a prompt clip
            // TODO: Consider how this could change to support multiple prompts, since this locks someone out after one upload
            participant.ParticipantState = ParticipantState.Contributed;
            participant.UpdatedUtc = Time.UtcNow;

            await db.SaveChangesAsync();
        }

        private async Task<ParticipantEntity> GetOrCreateParticipant(SbDb db, SessionEntity session, PersonEntity personEntity, ParticipantRole role, ParticipantGroupEntity participantGroup = null)
        {
            string key = $"{session.Route}_{personEntity.Route}_{role}";
            if (!NewParticipants.TryGetValue(key, out ParticipantEntity participant))
            {
                // Attempt to acquire the participant from the session directly
                participant = session.Participants
                .Where(p =>
                    p.Person.Route == personEntity.Route
                    && p.ParticipantRole == role)
                .OrderBy(p => (int)p.ParticipantRole)
                .FirstOrDefault();
            }

            // Determine if an existing participant entry exists
            if (participant == null)
            {
                // Create the participant entry
                participant = db.Participants.CreateResource();
                participant.Person = personEntity;
                participant.Session = session;
                participant.ParticipantRole = role;
                Logger.LogInformation($"Creating participant {participant.Route} for person {personEntity.Route} in session {session.Route}");
                NewParticipants.Add(key, participant);
            }

            // Determine whether a participant group entry needs to be created
            if (participantGroup != null)
            {
                // Determine whether we are working with a new or existing participant ID
                if (participant.Id == 0)
                {
                    // When the participant is "new" there will always be a participant group member entry created.
                    ParticipantGroupMemberEntity groupMemberEntry = new ParticipantGroupMemberEntity()
                    {
                        ParticipantGroup = participantGroup,
                        Participant = participant
                    };
                    db.ParticipantGroupMembers.Add(groupMemberEntry);
                }
                else
                {
                    // Existing participant so only create teh group member entry if one does not already exist.
                    await db.EnsureParticipantGroupMemberEntry(participantGroup.Id, participant.Id);
                }
            }

            return participant;
        }

        private async Task SendPublisherNotificationsAsync(SbDb db, Organization org, SessionEntity session)
        {
            try
            {
                HashSet<string> peopleRoutes = new HashSet<string>();
                await NotifyHostParticipants(org, session, peopleRoutes);
                await ExpandAndNotifyGroupHosts(db, org, session, peopleRoutes);
            }
            catch (Exception ex)
            {
                Logger.LogCritical($"Failed trying to notify published in org {org.Route} about session {session.Route} with message {ex.Message} and stacktrace {ex.StackTrace}");
            }
        }

        private async Task NotifyHostParticipants(Organization org, SessionEntity session, HashSet<string> peopleRoutes)
        {
            IEnumerable<ParticipantEntity> participants = ParticipantsInSession(session);
            foreach (ParticipantEntity p in participants)
            {
                // Exclude those below the query role
                if (p.ParticipantRole < ParticipantRole.Host)
                {
                    continue;
                }

                // Only send notification once per person
                string personRoute = p.Person.Route;
                if (peopleRoutes.Contains(personRoute))
                {
                    continue;
                }

                // Remember that we sent it so we don't send it again; then send it!
                peopleRoutes.Add(personRoute);
                await SendHostNotificationAsync(org, session, p.Person);
            }
        }

        private async Task ExpandAndNotifyGroupHosts(SbDb db, Organization org, SessionEntity session, HashSet<string> peopleRoutes)
        {
            IEnumerable<PersonEntity> groupPeople = await db.PeopleInSessionGroupsAsync(session.Route, ParticipantRole.Host);
            foreach (PersonEntity person in groupPeople)
            {
                // Check for unique person
                string personRoute = person.Route;
                if (peopleRoutes.Contains(personRoute))
                {
                    continue;
                }

                peopleRoutes.Add(personRoute);

                // Setup participant and send notification
                ParticipantEntity part = await GetOrCreateParticipant(db, session, person, ParticipantRole.Host);
                await SendHostNotificationAsync(org, session, part.Person);
            }
        }

        private async Task SendNotificationsAsync(SbDb db, Organization org, SessionEntity session, ParticipantRole role, bool isReminder = true)
        {
            try
            {
                HashSet<string> peopleRoutes = new HashSet<string>();
                // Send to all of the participants
                await NotifyParticipantsAsync(org, session, role, isReminder, peopleRoutes);
                await ExpandAndNotifyGroupsAsync(db, org, session, role, isReminder, peopleRoutes);
            }
            catch (Exception ex)
            {
                Logger.LogCritical($"Failed trying to notify org {org.Route} about session {session.Route} with message {ex.Message} and stacktrace {ex.StackTrace}");
            }
        }

        private async Task NotifyParticipantsAsync(Organization org, SessionEntity session, ParticipantRole role, bool isReminder, HashSet<string> peopleRoutes)
        {
            IEnumerable<ParticipantEntity> participants = ParticipantsInSession(session);

            Logger.LogInformation($"Notifying unique people amongst {participants.Count()} participants in session {session.Route}...");
            foreach (ParticipantEntity p in participants)
            {
                // Exclude those below the query role
                if (p.ParticipantRole < role)
                {
                    continue;
                }

                // Always change participant state
                p.ParticipantState = isReminder ? ParticipantState.ContributionRequested : ParticipantState.ConsumptionRequested;
                p.UpdatedUtc = Time.UtcNow;

                // Only send notification once per person
                string personRoute = p.Person.Route;
                if (peopleRoutes.Contains(personRoute))
                {
                    continue;
                }

                peopleRoutes.Add(personRoute);
                await SendNotificationAsync(org, session, p.Person, p.Person.User, isReminder);
            }
        }

        private static IEnumerable<ParticipantEntity> ParticipantsInSession(SessionEntity session)
        {
            return session.Participants.Where((p) =>
                p.DeletedUtc == null &&
                p.Person != null &&
                p.Person.DeletedUtc == null &&
                p.Person.User != null &&
                p.Person.User.DeletedUtc == null);
        }

        private async Task ExpandAndNotifyGroupsAsync(SbDb db, Organization org, SessionEntity session, ParticipantRole role, bool isReminder, HashSet<string> peopleRoutes)
        {
            IEnumerable<ParticipantGroupAndPeople> peopleByGroup = await db.PeopleInSessionByGroupAsync(session.Route, role);
            int totalCount = peopleByGroup.SelectMany(i => i.People.Select(i => i.Person.UserId)).Distinct().Count();

            Logger.LogInformation($"Notifying unique people amongst {totalCount} group-based participants in session {session.Route}...");

            foreach (ParticipantGroupAndPeople groupAndPeople in peopleByGroup)
            {
                foreach (PersonWithUser personWithUser in groupAndPeople.People)
                {
                    await ExpandAndNotifyPersonAsync(db, org, session, role, isReminder, peopleRoutes, groupAndPeople, personWithUser);
                }
            }
        }

        private async Task ExpandAndNotifyPersonAsync(
            SbDb db,
            Organization org,
            SessionEntity session,
            ParticipantRole role,
            bool isReminder,
            HashSet<string> peopleRoutes,
            ParticipantGroupAndPeople groupAndPeople,
            PersonWithUser personWithUser)
        {
            try
            {
                // Creates a participant entry if one does not already exist.  If the participant
                // entry already exists, the method below is still responsible for linking the
                // participant to a participant group.
                ParticipantEntity part = await GetOrCreateParticipant(db, session, personWithUser.Person, role, groupAndPeople.ParticipantGroup);

                // Determine whether a notification should be sent out to the person
                if (peopleRoutes.Contains(personWithUser.Person.Route))
                {
                    return;
                }

                peopleRoutes.Add(personWithUser.Person.Route);
                part.ParticipantState = isReminder ? ParticipantState.ContributionRequested : ParticipantState.ConsumptionRequested;
                part.UpdatedUtc = Time.UtcNow;
                await SendNotificationAsync(org, session, personWithUser.Person, personWithUser.User, isReminder);
            }
            catch (Exception ex)
            {
                Logger.LogCritical($"Error trying to {nameof(ExpandAndNotifyPersonAsync)} to person '{personWithUser?.Person.Route}' with error {ex.Message} and stack: {ex.StackTrace}");
            }
        }

        private async Task SendNotificationAsync(Organization org, SessionEntity session, PersonEntity person, UserEntity user, bool isReminder)
        {
            try
            {
                Validator.NotNull(org, nameof(org));
                Validator.NotNull(session, nameof(org));
                Validator.NotNull(person, nameof(person));
                Validator.NotNull(user, nameof(user));

                string kind = isReminder ? "reminder" : "deadline";
                Logger.LogInformation($"Sending session {kind} notification to person {person.Route}");
                User receiver = Mapper.MapSafe<User>(user);

                if (isReminder)
                {
                    await Notifications.SessionReminderAsync(receiver, org, session);
                }
                else
                {
                    await Notifications.SessionPublishAsync(receiver, org, session);
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical($"Error trying to {nameof(SendNotificationAsync)} to person '{person?.Route}' with error {ex.Message} and stack: {ex.StackTrace}");
            }
        }

        private async Task SendHostNotificationAsync(Organization org, SessionEntity session, PersonEntity p)
        {
            Logger.LogInformation($"Sending session host notification to person {p.Route}");
            User user = Mapper.MapSafe<User>(p?.User);
            await Notifications.SessionHostPublishAsync(user, org, session);
        }

        private async Task QueueTranscription(SbDb db, Organization org, SessionEntity session, ClipEntity clip)
        {
            if (session.Transcribe)
            {
                if (clip.FileType.IsVideo())
                {
                    // Request Transcription for Video
                    await SbMediaService.RequestVideoTranscription(
                        session.Organization.Route,
                        session.Route,
                        clip.Prompt.Route,
                        clip.Route
                    );
                }
                else
                {
                    // Request transcription of audio using Azure Speech to Text (ASTT)
                    OrganizationEntity orgEntity = await db.Organizations.FirstOrDefaultAsync(i => i.Route == org.Route);
                    orgEntity.AssertFound($"Cannot queue transcription because the organization (route:{org.Route}) settings were not found.");
                    // Default to transcribing unless opt out
                    OrgSessionSettings settings = orgEntity.GetSessionSettings();
                    if (settings == null || settings?.TranscriptionEnabled == true)
                    {
                        TranscriptionRequest request = await db.CreateTranscriptionRequest(org.Route, clip.Route);
                        await SbTranscriptionService.QueueTranscriptionRequest(request);
                    }
                }
            }
        }


        #endregion
    }
}
