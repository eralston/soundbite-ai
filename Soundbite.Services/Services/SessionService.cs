using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Providers;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Services.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// EF Implementation of <see cref="ISessionService"/>
    /// </summary>
    public class SessionService : InfrastructureServiceBase<ISbInfrastructure>, ISessionService
    {
        #region Properties

        protected ILifecycleFactory LifecycleFactory { get; }
        protected IClipFileService ClipFileService { get; }
        protected IImageService Images { get; }
        protected ISeriesService Series { get; }
        protected IProviderFactory ProviderFactory { get; }
        protected IRbac Rbac { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="series"></param>
        /// <param name="images"></param>
        /// <param name="clipFileService"></param>
        /// <param name="lifecycleFactory"></param>
        /// <param name="infrastructure"></param>
        /// <param name="providerFactory"></param>
        /// <param name="mapper"></param>
        /// <param name="logger"></param>
        public SessionService(
            IRbac rbac,
            ISeriesService series,
            IImageService images,
            IClipFileService clipFileService,
            ILifecycleFactory lifecycleFactory,
            ISbInfrastructure infrastructure,
            IProviderFactory providerFactory,
            IMapper mapper,
            ILogger<SessionService> logger)
            : base(infrastructure, logger, null, mapper)
        {
            Rbac = rbac;
            Series = series;
            Images = images;
            ClipFileService = clipFileService;
            LifecycleFactory = lifecycleFactory;
            ProviderFactory = providerFactory;
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Loads to Url to the media in each IClip instance in the given session
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sessionDetails"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        private async Task HydrateClipsAsync(SbDb db, SessionDetails sessionDetails, string orgRoute, string sessionRoute)
        {
            OrganizationEntity orgEntity = await db.OrgWhereRoute(orgRoute);
            Organization org = Mapper.MapSafe<Organization>(orgEntity);

            await LoadClipUrls(db, sessionDetails, sessionRoute, org);

            await LoadImageUrlsForContributors(sessionDetails);
        }

        private async Task LoadClipUrls(SbDb db, SessionDetails sessionDetails, string sessionRoute, Organization org)
        {
            foreach (PromptWithClips prompt in sessionDetails.Prompts)
            {
                foreach (ClipWithContributor clip in prompt.Clips)
                {
                    // Clup URLs must be pulled one at a time because they depend on using the DbContext
                    clip.Url = await ClipFileService.DownloadUrlAsync(org, sessionRoute, prompt.Route, clip.Route, clip.FileType);
                }
            }

            // Saving here ensures the multiple load events from above persist in the database
            await db.SaveChangesAsync();
        }

        private async Task LoadImageUrlsForContributors(SessionDetails sessionDetails)
        {
            Dictionary<string, Task<string>> contributorTasks = new Dictionary<string, Task<string>>();

            // We can pull images in parallel, so let's wrap up a dictionary from contribute route => URL
            foreach (PromptWithClips prompt in sessionDetails.Prompts)
            {
                foreach (ClipWithContributor clip in prompt.Clips)
                {
                    if (clip?.Contributor?.Route == null || contributorTasks.ContainsKey(clip.Contributor.Route))
                    {
                        continue;
                    }
                    // Contributor image URL
                    contributorTasks[clip.Contributor.Route] = Images.UserImageAsync(clip.Contributor);
                }
            }

            // Wait on the tasks to finish querying storage in parallel
            await Task.WhenAll(contributorTasks.Values.ToArray());

            // Loads up the clips using the results
            foreach (PromptWithClips prompt in sessionDetails.Prompts)
            {
                foreach (ClipWithContributor clip in prompt.Clips)
                {
                    // Clup URLs must be pulled one at a time because they depend on using the DbContext
                    if (clip?.Contributor?.Route != null)
                    {
                        clip.Contributor.ImageSrc = contributorTasks[clip.Contributor.Route].Result;
                    }
                }
            }
        }

        private async Task LoadHostImages(IEnumerable<SessionPreview> sessions)
        {
            Dictionary<string, Task<string>> tasks = new Dictionary<string, Task<string>>();

            foreach (User usr in sessions.SelectMany(s => s.HostParticipants.Select(hp => hp.Person.User)).Distinct())
            {
                string key = usr.Route;
                if (tasks.ContainsKey(key) || usr.ImageSrc != null)
                {
                    continue;
                }
                Task<string> task = Images.UserImageAsync(usr);
                tasks.Add(key, task);
            }

            await Task.WhenAll(tasks.Values);

            // Write back nestered because LINQ projections interfere with persistence
            foreach (SessionPreview sess in sessions)
            {
                foreach (Participant host in sess.HostParticipants)
                {
                    string key = host.Person.User.Route;
                    if (!tasks.ContainsKey(key) || host.Person.User.ImageSrc != null)
                    {
                        continue;
                    }
                    string imageSrc = tasks[key].Result;
                    host.Person.User.ImageSrc = imageSrc;
                }
            }
        }

        /// <summary>
        /// Acknowledges a public session for the specified user.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the public session belongs.</param>
        /// <param name="sessionRoute">Route of the public session being acknowledged.</param>
        /// <param name="userRoute">Route of the user acknowledging the public session.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        public async Task AcknowledgePublicSession(string orgRoute, string sessionRoute, string userRoute)
        {
            SbDb db = await Infrastructure.DbAsync();

            // Verify session exists
            bool sessionExists = db.Sessions
                .Where(s => s.Route == sessionRoute
                    && s.SessionSecurity == SessionSecurityType.Public
                    && s.DeletedUtc == null
                    && s.Organization.Route == orgRoute
                    && s.Organization.DeletedUtc == null)
                .Select(s => true)
                .FirstOrDefault();

            // Verify that the user exists
            bool userExists = sessionExists && db.Users
                .Where(u => u.Route == userRoute && u.DeletedUtc == null)
                .Select(u => true)
                .FirstOrDefault();

            // Only continue if the session and the user exist
            if (sessionExists && userExists)
            {
                List<ParticipantEntity> participantEntries = db.Participants.Where(p =>
                    p.Session.Route == sessionRoute
                    // && (p.ParticipantRole == ParticipantRole.Audience || p.ParticipantRole == ParticipantRole.Participant)
                    && (p.ParticipantState == ParticipantState.ConsumptionRequested)
                    && p.Session.DeletedUtc == null
                    && p.Session.Organization.Route == orgRoute
                    && p.Person.User.Route == userRoute
                    && p.Person.DeletedUtc == null
                    && p.Person.User.DeletedUtc == null
                    ).ToList();

                if (participantEntries?.Any() == true)
                {
                    foreach (ParticipantEntity p in participantEntries)
                    {
                        if (p.ParticipantState != ParticipantState.Consumed)
                        {
                            p.ParticipantState = ParticipantState.Consumed;
                            db.Participants.Update(p);
                        }
                    }

                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<SessionDetails> UpdateAsync(string orgRoute, NewSession newSession)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(newSession), newSession);

            // Check access
            SbDb db = await Infrastructure.DbAsync();

            UserEntity currentUser = await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person);
            currentUser.AssertFound();

            // Pull session and update
            int[] groupIds = await db.GroupIdsAsync(orgRoute, Rbac.UniversalIdForCurrentUser);
            SessionEntity session = await db.SessionAsync(Rbac.UniversalIdForCurrentUser, orgRoute, newSession.Route, groupIds, ParticipantRole.Host, true);
            session.AssertFound();

            // Session
            session.Name = newSession.Name;
            session.Limit = newSession.Limit;
            if (session.Reminder != newSession.Reminder)
            {
                session.ReminderSent = null;
            }

            session.Reminder = newSession.Reminder;
            if (session.Publish != newSession.Publish)
            {
                session.PublishSent = null;
            }

            session.Publish = newSession.Publish;

            // Prompt
            PromptEntity prompt = session.Prompts.First();
            prompt.Text = newSession.FirstPrompt;

            // Participants
            bool hasGroupHost = await MergeParticipantGroups(newSession, db, session, orgRoute, currentUser);
            bool hasParticipantHost = await MergeParticipants(newSession, db, session, orgRoute, currentUser);

            // TODO: Series

            // Validate
            if (!hasGroupHost && !hasParticipantHost)
            {
                throw new ArgumentException("Must have at least one host participant or group");
            }

            await db.SaveChangesAsync();

            return await ReadAsync(orgRoute, newSession.Route);
        }

        private static async Task<bool> MergeParticipantGroups(NewSession newSession, SbDb db, SessionEntity session, string orgRoute, UserEntity currentUser)
        {
            bool hasHost = false;

            if (newSession.Groups == null || newSession.Groups.Count() == 0)
            {
                return hasHost;
            }

            // Delete everything, then resurrect what's on the update
            session.Groups.Select(pg => pg.SoftDelete());
            foreach (NewParticipantGroup newGrp in newSession.Groups)
            {
                if (newGrp.ParticipantRole >= ParticipantRole.Host)
                {
                    hasHost = true;
                }

                // Try from the session
                ParticipantGroupEntity pGrp = session.Groups.Where(pg =>
                                                            pg.Group.Route == newGrp.GroupRoute
                                                            && pg.ParticipantRole == newGrp.ParticipantRole)
                                                            .FirstOrDefault();

                // Try from the database
                if (pGrp == null)
                {
                    pGrp = await db.ParticipantGroups.Where(pg =>
                                                        pg.Group.Route == newGrp.GroupRoute
                                                        && pg.Session.Route == session.Route
                                                        && pg.ParticipantRole == newGrp.ParticipantRole)
                                                            .FirstOrDefaultAsync();
                }

                // If all else fails, create a new one
                if (pGrp == null)
                {
                    pGrp = db.ParticipantGroups.CreateResource(currentUser);
                    pGrp.Session = session;
                    GroupEntity grp = await db.GroupWhereRoute(orgRoute, newGrp.GroupRoute);
                    pGrp.Group = grp ?? throw new ArgumentException($"No group {newGrp.GroupRoute} found");
                    pGrp.ParticipantRole = newGrp.ParticipantRole;
                }

                pGrp.Restore();
            }

            return hasHost;
        }

        private static async Task<bool> MergeParticipants(NewSession newSession, SbDb db, SessionEntity session, string orgRoute, UserEntity currentUser)
        {
            bool hasHost = false;

            if (newSession.Participants == null || newSession.Participants.Count() == 0)
            {
                return hasHost;
            }

            // Delete everything, then resurrect what's on the update
            session.Participants.Select(p => p.SoftDelete());
            foreach (NewParticipant newPart in newSession.Participants)
            {
                if (newPart.ParticipantRole >= ParticipantRole.Host)
                {
                    hasHost = true;
                }

                // Try from the session
                ParticipantEntity participant = session.Participants.Where(p =>
                                                            p.Person.Route == newPart.PersonRoute
                                                            && p.ParticipantRole == newPart.ParticipantRole)
                                                            .FirstOrDefault();

                // Try from the database
                if (participant == null)
                {
                    participant = await db.Participants.Where(p =>
                                                        p.Person.Route == newPart.PersonRoute
                                                        && p.Session.Route == session.Route
                                                        && p.ParticipantRole == newPart.ParticipantRole)
                                                            .FirstOrDefaultAsync();
                }

                // If all else fails, create a new one
                if (participant == null)
                {
                    participant = db.Participants.CreateResource(currentUser);
                    participant.Session = session;
                    PersonEntity person = await db.PersonWhereRoute(orgRoute, newPart.PersonRoute);
                    participant.Person = person ?? throw new ArgumentException($"No Person {newPart.PersonRoute} found");
                    participant.ParticipantRole = newPart.ParticipantRole;
                }

                participant.Restore();
            }

            return hasHost;
        }

        private async Task<SessionEntity> CreateSessionAsync(string orgRoute, NewSession newSession, SbDb db, UserEntity currentUser)
        {
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            SessionEntityBuilder builder = new SessionEntityBuilder(db, currentUser);
            SessionEntity session = await builder.BuildAsync(org, newSession);
            ILifecycleStrategy strategy = LifecycleFactory.StrategyForSession(session.SessionType);

            await strategy.BeforeCreateSessionAsync(db, session);
            await db.SaveChangesAsync();
            await strategy.AfterCreateSessionAsync(db, session);
            return session;
        }

        #endregion

        #region ISessionService_Obosolete

        [Obsolete("Need to convert to a paging version")]
        public async Task<IEnumerable<Session>> ReadAllAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            SbDb db = await Infrastructure.DbAsync();

            UserEntity currentUser = await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person);
            currentUser.AssertFound("No User for Claims");

            int[] groupIds = await db.GroupIdsAsync(orgRoute, Rbac.UniversalIdForCurrentUser);
            Session[] result = await db.SessionsAsync(Rbac.UniversalIdForCurrentUser, groupIds, orgRoute);

            return result;
        }

        [Obsolete("Need to convert to a paging version")]
        public async Task<IEnumerable<Session>> ReadAllAsync_Obsolete(string orgRoute, string groupRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(groupRoute), groupRoute);

            SbDb db = await Infrastructure.DbAsync();

            (await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person)).AssertFound();

            int[] groupIds = await db.GroupIdsAsync(orgRoute, Rbac.UniversalIdForCurrentUser);
            Session[] result = await db.GroupSessionsAsync(SecurityContext, groupIds, orgRoute, groupRoute);
            return result;
        }

        [Obsolete("Need to convert to a paging version")]
        public async Task<IEnumerable<SessionPreview>> ReadFeedAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            SbDb db = await Infrastructure.DbAsync();
            IEnumerable<SessionPreview> ret = await db.SessionPreviewAsync(Mapper, Rbac.UniversalIdForCurrentUser, orgRoute);

            await LoadHostImages(ret);

            return ret;
        }

        [Obsolete("Need to convert to a paging version")]
        public async Task<IEnumerable<SessionPreview>> ReadFeedAsync_Obsolete(string orgRoute, string grpRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(grpRoute), grpRoute);

            SbDb db = await Infrastructure.DbAsync();
            (await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person)).AssertFound();
            IEnumerable<SessionPreview> ret = await db.SessionPreviewAsync(Mapper, Rbac.UniversalIdForCurrentUser, orgRoute, grpRoute);

            await LoadHostImages(ret);

            return ret;
        }

        [Obsolete("Need to convert to a paging version")]
        public async Task<IEnumerable<SessionPreview>> ReadPublicFeedAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            SbDb db = await Infrastructure.DbAsync();
            IEnumerable<SessionPreview> ret = await db.QuerySessionPreview(Mapper, Rbac.UniversalIdForCurrentUser, orgRoute, null, null, new[] { SessionSecurityType.Public }, false, false);

            await LoadHostImages(ret);

            return ret;
        }

        [Obsolete("Need to convert to a paging version")]
        public async Task<IEnumerable<SessionPreview>> ReadPublicFeedAsync_Obsolete(string orgRoute, string groupRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            SbDb db = await Infrastructure.DbAsync();
            IEnumerable<SessionPreview> ret = await db.QuerySessionPreview(Mapper, Rbac.UniversalIdForCurrentUser, orgRoute, groupRoute, null, new[] { SessionSecurityType.Public }, false, false);
            await LoadHostImages(ret);
            return ret;
        }

        [Obsolete("Need to convert to a paging version")]
        public async Task<IEnumerable<SessionPreview>> ReadRecentlyPublishedAsync_Obsolete(string orgRoute, int skip = 0, int take = 10)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            SbDb db = await Infrastructure.DbAsync();
            (await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Admin)).AssertFound();
            IEnumerable<SessionPreview> ret = await db.RecentlyPublishedSessionsAsync(orgRoute, new IndexPageRequest { Skip = skip, Take = take });
            return ret;
        }

        #endregion

        #region ISessionService

        public async Task SetReminderCalendarEntryId(string organizationRoute, string sessionRoute, string reminderCalendarEventId)
        {
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            Validator.ArgNotNull(nameof(reminderCalendarEventId), reminderCalendarEventId);

            await Rbac.AssertCurrentUser();

            SbDb db = await Infrastructure.DbAsync();
            SessionEntity session = await db.Sessions.Where(i => i.Organization.Route == organizationRoute && i.Route == sessionRoute).SingleOrDefaultAsync();
            session.ReminderCalEventId = reminderCalendarEventId;
            db.Sessions.Update(session);
            await db.SaveChangesAsync();
        }

        public async Task<SessionDetails> CreateAsync(string orgRoute, NewSession newSession)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(newSession), newSession);

            if ((newSession.Groups == null || !newSession.Groups.Any()) && (newSession.Participants == null || !newSession.Participants.Any()))
            {
                throw new ArgumentException("New session must contain an audience");
            }

            SbDb db = await Infrastructure.DbAsync();

            UserEntity currentUser = await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person);
            currentUser.AssertFound();

            SessionEntity session = await CreateSessionAsync(orgRoute, newSession, db, currentUser);

            if (newSession.Recurrence > Recurrence.NoRepeat)
            {
                SeriesDetails result = await Series.CreateAsync(orgRoute, newSession);
                session.Series = await db.Series.Where(s => s.Route == result.Route && s.DeletedUtc == null).FirstOrDefaultAsync();
            }

            SessionDetails sessionDetails = await ReadAsync(orgRoute, session.Route);

            return sessionDetails;
        }

        public async Task<SessionDetails> ReadAsync(string orgRoute, string sessionRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);

            SbDb db = await Infrastructure.DbAsync();

            // Enforce RBAC
            (await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person)).AssertFound();

            SessionDetails sessionDetails = await db.ReadOnlySessionDetailsAsync(Rbac.UniversalIdForCurrentUser, orgRoute, sessionRoute, false, Mapper);

            await HydrateClipsAsync(db, sessionDetails, orgRoute, sessionRoute);

            return sessionDetails;
        }

        public async Task<ReactionSummary[]> ReadReactionsAsync(string orgRoute, string sessionRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);

            SbDb db = await Infrastructure.DbAsync();

            // Enforce RBAC
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            ReactionSummary[] ret = await db.ReadReactionsAsync(orgRoute, sessionRoute);

            return ret;
        }

        public async Task<SessionDetails> ReadPublicAsync(string orgRoute, string sessionRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);
            SbDb db = await Infrastructure.DbAsync();
            SessionDetails sessionDetails = await db.ReadOnlySessionDetailsAsync(Rbac.UniversalIdForCurrentUser, orgRoute, sessionRoute, true, Mapper);
            await HydrateClipsAsync(db, sessionDetails, orgRoute, sessionRoute);
            return sessionDetails;
        }

        public async Task DeleteAsync(string orgRoute, string sessionRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);

            SbDb db = await Infrastructure.DbAsync();

            (await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person)).AssertFound();

            int[] groupIds = await db.GroupIdsAsync(orgRoute, Rbac.UniversalIdForCurrentUser);
            SessionEntity ret = await db.SessionAsync(Rbac.UniversalIdForCurrentUser, orgRoute, sessionRoute, groupIds, ParticipantRole.Host, false);
            ret.AssertFound();
            ret.SoftDelete();

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a flag indicating whether the organization has any active public notifications.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to check for active public notifications.</param>
        /// <returns><c>true</c> if the organization has active public notifications, otherwise <c>false</c>.</returns>
        public async Task<bool> HasPublicNotifications(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            SbDb db = await Infrastructure.DbAsync();
            IQueryable<SessionEntity> query = db.QuerySessions(orgRoute, null, null, new[] { SessionSecurityType.Public }, false, false);
            bool result = await query.AnyAsync();
            return result;
        }

        public async Task UpdateStateAsync(string orgRoute, string sessionRoute, ParticipantState state, ParticipantRole role = ParticipantRole.Unknown)
        {
            //NOTE: Keep logic in this method up-to-date with SessionStoreClass.updateParticipantState on client-side

            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(sessionRoute), sessionRoute);

            // Check access
            SbDb db = await Infrastructure.DbAsync();

            // TODO: Permissions for sessions
            UserEntity currentUser = await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person);
            currentUser.AssertFound();

            // Pull session and update
            int[] groupIds = await db.GroupIdsAsync(orgRoute, Rbac.UniversalIdForCurrentUser);
            SessionEntity session = await db.SessionAsync(Rbac.UniversalIdForCurrentUser, orgRoute, sessionRoute, groupIds, ParticipantRole.Audience, true);
            session.AssertFound();

            // All active participant records for the current user
            IEnumerable<ParticipantEntity> participants = session.Participants.Where(p =>
                p.Person.UserId == currentUser.Id &&
                p.DeletedUtc == null &&
                p.Person.DeletedUtc == null &&
                p.Person.User.DeletedUtc == null
            );

            // Optionally filter by a specified role
            if (role > ParticipantRole.Unknown)
            {
                participants = participants.Where(p => p.ParticipantRole == role);
            }

            // Shift participant records to the given state
            participants.ForEach(p => { p.ParticipantState = state; p.UpdatedUtc = Time.UtcNow; });

            await db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IndexPageResponse<Participant>> ReadParticipants(
            string orgRoute,
            string sessionRoute,
            ParticipantRole[] includeRoles = null,
            IndexPageRequest page = null)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            includeRoles ??= new[] { ParticipantRole.Participant };
            page ??= new IndexPageRequest() { Skip = 0, Take = 100 };

            SbDb db = await Infrastructure.DbAsync();

            IQueryable<ParticipantEntity> query = db
                .QueryDirectParticipants(orgRoute, sessionRoute, includeRoles, Rbac.UniversalIdForCurrentUser)
                .Include(i => i.Person)
                .ThenInclude(i => i.User);
            IndexPageQuery pagedQuery = new IndexPageQuery(page).SetMaxResults(int.MaxValue);
            IndexPageResponse<ParticipantEntity> entityPage = await query.ReadPageAsync(pagedQuery); //TODO: filter here?
            IndexPageResponse<Participant> ret = entityPage.Map((p) => Mapper.MapSafeWith<Participant>(p));
            ret.BasedOn(pagedQuery);
            return ret;
        }

        /// <inheritdoc />
        public async Task<IndexPageResponse<Participant>> ReadParticipants(
            string orgRoute,
            string sessionRoute,
            string participantGroupRoute,
            ParticipantRole[] includeRoles = null,
            IndexPageRequest page = null)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);
            includeRoles ??= new[] { ParticipantRole.Participant };
            page ??= new IndexPageRequest() { Skip = 0, Take = 100 };

            SbDb db = await Infrastructure.DbAsync();
            IQueryable<ParticipantEntity> query = db
                .QueryParticipantGroupMembers(orgRoute, sessionRoute, participantGroupRoute, includeRoles, Rbac.UniversalIdForCurrentUser)
                .Include(p => p.Person)
                .ThenInclude(p => p.User);
            IndexPageQuery pagedQuery = new IndexPageQuery(page).SetMaxResults(int.MaxValue);
            IndexPageResponse<ParticipantEntity> entityPage = await query.ReadPageAsync(pagedQuery); //TODO: filter here?
            IndexPageResponse<Participant> ret = entityPage.Map((p) => Mapper.MapSafeWith<Participant>(p));
            ret.BasedOn(pagedQuery);
            return ret;
        }

        public async Task UpdateReactionAsync(string orgRoute, string sessionRoute, ParticipantReactionType reaction)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);

            // Guests in orgs can react to sessions where they have access
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Guest);

            SbDb db = await Infrastructure.DbAsync();
            IQueryable<ParticipantEntity> queryMyParticipation = db.QueryUserParticipation(orgRoute, sessionRoute, Rbac.UniversalIdForCurrentUser);
            ParticipantEntity[] participants = await queryMyParticipation.ToArrayAsync();
            foreach (ParticipantEntity p in participants)
            {
                p.ReactionType = reaction;
            }
            await db.SaveChangesAsync();
        }

        #endregion
    }
}