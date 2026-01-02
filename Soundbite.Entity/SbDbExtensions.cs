using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Soundbite.Models;
using Soundbite.Models.JobRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Entity
{
    /// <summary>
    /// Extensions for the SbDb dbcontext
    /// </summary>
    public static class SbDbExtensions
    {
        /// <summary>
        /// Creates a transcription request for the specified clip.
        /// </summary>
        /// <param name="db">Reference to the Soundbite database.</param>
        /// <param name="orgRoute">Route of the org containing the session associated with the clip.</param>
        /// <param name="clipRoute">Route of the clip for which the transcription request is being built.</param>
        /// <returns>a transcript request for the specified clip if the clip is found, otherwise <c>null</c>.</returns>
        public static async Task<TranscriptionRequest> CreateTranscriptionRequest(this SbDb db, string orgRoute, string clipRoute)
        {
            TranscriptionRequest request = await db.Clips.Where(i => i.Prompt.Session.Organization.Route == orgRoute && i.Route == clipRoute)
                .Select(i => new TranscriptionRequest()
                {
                    ClipRoute = clipRoute,
                    OrgRoute = orgRoute,
                    SessionRoute = i.Prompt.Session.Route,
                    PromptRoute = i.Prompt.Route,
                    ClipFileType = i.FileType
                })
                .FirstOrDefaultAsync();
            return request;
        }

        /// <summary>
        /// Creates a transcription request for the specified clip.
        /// </summary>
        /// <param name="db">Reference to the Soundbite database.</param>
        /// <param name="orgRoute">Route of the org containing the session associated with the clip.</param>
        /// <param name="clipRoute">Route of the clip for which the transcription request is being built.</param>
        /// <returns>a transcript request for the specified clip if the clip is found, otherwise <c>null</c>.</returns>
        public static async Task<MediaOperationRequest> CreateMediaRequest(this SbDb db, string orgRoute, string clipRoute)
        {
            MediaOperationRequest request = await db.Clips.Where(i => i.Prompt.Session.Organization.Route == orgRoute && i.Route == clipRoute)
                .Select(i => new MediaOperationRequest()
                {
                    ClipRoute = clipRoute,
                    OrgRoute = orgRoute,
                    SessionRoute = i.Prompt.Session.Route,
                    PromptRoute = i.Prompt.Route,
                    ClipFileType = i.FileType
                })
                .FirstOrDefaultAsync();
            return request;
        }

        /// <summary>
        /// Ensures that a record exists associating a participant with the specified participant
        /// group. If the record exists, a reference to the record is returned.  If the record does
        /// not exist, a new entry is created.
        /// </summary>
        /// <param name="db">Reference to the Soundbite database context.</param>
        /// <param name="participantGroupId">ID of the participant group.</param>
        /// <param name="participantId">ID of the participant.</param>
        /// <returns>a reference to the existing or newly created entry.</returns>
        public static async Task<ParticipantGroupMemberEntity> EnsureParticipantGroupMemberEntry(this SbDb db, int participantGroupId, int participantId, bool commit = false)
        {
            ParticipantGroupMemberEntity item = await db.ParticipantGroupMembers
                        .Where(i => i.ParticipantGroupId == participantGroupId && i.ParticipantId == participantId)
                        .FirstOrDefaultAsync();

            if (item == null)
            {
                item = await AddParticipantGroupMemberEntry(db, participantGroupId, participantId, commit);
            }

            return item;
        }

        /// <summary>
        /// Adds record associating a participant with the specified participant group. 
        /// </summary>
        /// <param name="db">Reference to the Soundbite database context.</param>
        /// <param name="participantGroupId">ID of the participant group.</param>
        /// <param name="participantId">ID of the participant.</param>
        /// <returns>a reference to the newly created entry.</returns>
        public static async Task<ParticipantGroupMemberEntity> AddParticipantGroupMemberEntry(this SbDb db, int participantGroupId, int participantId, bool commit = false)
        {
            ParticipantGroupMemberEntity newItem = new ParticipantGroupMemberEntity()
            {
                ParticipantGroupId = participantGroupId,
                ParticipantId = participantId
            };
            db.ParticipantGroupMembers.Add(newItem);
            if (commit)
            {
                await db.SaveChangesAsync();
            }
            return newItem;
        }

        /// <summary>
        /// Returns a set of members for all of the people inside of the groups associated with the given sessionRoute
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<PersonEntity>> PeopleInSessionGroupsAsync(this SbDb db, string sessionRoute, ParticipantRole role = ParticipantRole.Unknown)
        {
            IQueryable<IEnumerable<PersonEntity>> query = from
                                            grp in db.ParticipantGroups
                                                              // Filter down to the people who are living members of the sessions' group(s)
                                                          let people = from
                                                                          m in db.Members
                                                                       where
                                                                          m.DeletedUtc == null
                                                                          && m.Person.DeletedUtc == null
                                                                          && m.Person.User.DeletedUtc == null
                                                                          && m.Person.Organization.DeletedUtc == null
                                                                          && m.Person.Organization.Tenant.DeletedUtc == null
                                                                          && m.GroupId == grp.Group.Id
                                                                       select m.Person
                                                          where
                                                             // Pull groups for the session where their role is >= requested role
                                                             grp.ParticipantRole >= role
                                                             && grp.DeletedUtc == null
                                                             && grp.Group.DeletedUtc == null
                                                             && grp.Group.Organization.DeletedUtc == null
                                                             && grp.Group.Organization.Tenant.DeletedUtc == null
                                                             && grp.Session.DeletedUtc == null
                                                             && grp.Session.Route == sessionRoute
                                                             && grp.Session.Organization.DeletedUtc == null
                                                             && grp.Session.Organization.Tenant.DeletedUtc == null
                                                          select people;

            PersonEntity[] ret = await query.SelectMany(i => i).Include((p) => p.User).Distinct().ToArrayAsync();
            return ret;
        }

        public static async Task<IEnumerable<ParticipantGroupAndPeople>> PeopleInSessionByGroupAsync(this SbDb db, string sessionRoute, ParticipantRole role = ParticipantRole.Unknown)
        {
            IQueryable<ParticipantGroupAndPeople> query = from grp in db.ParticipantGroups
                                                          let people = from m in db.Members
                                                                       where
                                                                          m.DeletedUtc == null
                                                                          && m.Person.DeletedUtc == null
                                                                          && m.Person.User.DeletedUtc == null
                                                                          && m.Person.Organization.DeletedUtc == null
                                                                          && m.Person.Organization.Tenant.DeletedUtc == null
                                                                          && m.GroupId == grp.Group.Id
                                                                       select new PersonWithUser { Person = m.Person, User = m.Person.User }
                                                          where
                                                                           grp.ParticipantRole >= role
                                                                           && grp.DeletedUtc == null
                                                                           && grp.Group.DeletedUtc == null
                                                                           && grp.Group.Organization.DeletedUtc == null
                                                                           && grp.Group.Organization.Tenant.DeletedUtc == null
                                                                           && grp.Session.DeletedUtc == null
                                                                           && grp.Session.Route == sessionRoute
                                                                           && grp.Session.Organization.DeletedUtc == null
                                                                           && grp.Session.Organization.Tenant.DeletedUtc == null
                                                          select new ParticipantGroupAndPeople() { ParticipantGroup = grp, People = people };

            ParticipantGroupAndPeople[] ret = await query.ToArrayAsync();

            return ret;
        }

        public static async Task<SessionParticipantSummary> SessionParticipantSummary(this SbDb db, string orgRoute, string sessionRoute)
        {
            IQueryable<SessionParticipantSummary> query =
                        from
                            ses in db.Sessions

                            // Count of unique participants (distinct by user id)
                        let audienceCount = (from
                                                p in db.Participants
                                             where
                                                 p.DeletedUtc == null &&
                                                 p.SessionId == ses.Id &&
                                                 p.Person.DeletedUtc == null &&
                                                 p.Person.User.DeletedUtc == null
                                             select p.Person.User.Id).Distinct().Count()

                        let listensCount = (from
                                                ce in db.ClipEvents
                                            where
                                                ce.DeletedUtc == null &&
                                                // In the future, we may want to count something in more detail than whether or not the file location was shared
                                                (ce.ClipEventType == ClipEventType.ServerShareLinkSecure || ce.ClipEventType == ClipEventType.ServerShareLinkPublic) &&
                                                ce.Target.DeletedUtc == null &&
                                                ce.Target.Prompt.DeletedUtc == null &&
                                                ce.Target.Prompt.Session.Id == ses.Id
                                            select ce.CreatedById).Count()


                        // Count of unique people who have had the clip shared to them (distinct by user id)
                        let listenerCount = (from
                                                ce in db.ClipEvents
                                             where
                                                 ce.DeletedUtc == null &&
                                                 // In the future, we may want to count something in more detail than whether or not the file location was shared
                                                 (ce.ClipEventType == ClipEventType.ServerShareLinkSecure || ce.ClipEventType == ClipEventType.ServerShareLinkPublic) &&
                                                 ce.Target.DeletedUtc == null &&
                                                 ce.Target.Prompt.DeletedUtc == null &&
                                                 ce.Target.Prompt.Session.Id == ses.Id
                                             select ce.CreatedById).Distinct().Count()

                        where
                            ses.DeletedUtc == null &&
                            ses.IsTemplate == false &&
                            ses.Route == sessionRoute &&
                            ses.Organization.DeletedUtc == null &&
                            ses.Organization.Route == orgRoute &&
                            ses.Organization.Tenant.DeletedUtc == null
                        select new SessionParticipantSummary
                        {
                            AudienceCount = audienceCount,
                            ListenersCount = listenerCount,
                            ListensCount = listensCount,
                            Session = ses,
                        };

            SessionParticipantSummary ret = await query.AsNoTracking().FirstOrDefaultAsync();
            return ret;
        }

        public static async Task<IEnumerable<PersonEntity>> PeopleInSessionParticipantsAsync(this SbDb db, string sessionRoute, ParticipantRole role)
        {
            IQueryable<ParticipantEntity> query = from
                                            part in db.Participants.AsNoTracking()
                                                  where
                                                     part.ParticipantRole >= role
                                                     && part.DeletedUtc == null
                                                     && part.Person.DeletedUtc == null
                                                     && part.Person.Organization.DeletedUtc == null
                                                     && part.Person.Organization.Tenant.DeletedUtc == null
                                                     && part.Session.DeletedUtc == null
                                                     && part.Session.Route == sessionRoute
                                                     && part.Session.Organization.DeletedUtc == null
                                                     && part.Session.Organization.Tenant.DeletedUtc == null
                                                     && part.Session.Route == sessionRoute
                                                  select part;

            PersonEntity[] people = await query.Include((p) => p.Person).ThenInclude(p => p.User).Select(s => s.Person).ToArrayAsync();
            return people;
        }

        /// <summary>
        /// Queries for sessions allowed by the given claims, optionally including children
        /// WARNING: The children are NOT filtered by them being deleted
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="groupIds"></param>
        /// <param name="includeChildRecords"></param>
        /// <returns></returns>
        public static async Task<SessionEntity> SessionAsync(
            this SbDb db,
            string userUniversalId,
            string orgRoute,
            string sessionRoute,
            int[] groupIds,
            ParticipantRole role = ParticipantRole.Audience,
            bool includeChildRecords = false)
        {
            IQueryable<SessionEntity> query = from session in db.Sessions

                                              let hasPublicAccess =
                                                session.SessionSecurity == SessionSecurityType.Public

                                              let hasGroupAccess =
                                                     // linked via participantgroup
                                                     session.Groups.Any(pg =>
                                                          groupIds.Contains(pg.Group.Id)
                                                          && pg.ParticipantRole >= role)

                                              let hasParticipantAccess =
                                                    // Linked via participant
                                                    session.Participants.Any(p =>
                                                         p.DeletedUtc == null
                                                         && p.Person.DeletedUtc == null
                                                         && p.Person.User.DeletedUtc == null
                                                         && p.Person.Organization.DeletedUtc == null
                                                         && p.Person.Organization.Tenant.DeletedUtc == null
                                                         && p.Person.User.UniversalId == userUniversalId
                                                         && p.ParticipantRole >= role)

                                              let hasOrgAccess =
                                                          // Has access as org admin
                                                          (from person in db.People
                                                           where person.DeletedUtc == null
                                                               && person.User.DeletedUtc == null
                                                               && person.Organization.DeletedUtc == null
                                                               && person.Organization.Tenant.DeletedUtc == null
                                                               && person.PersonRole >= PersonRole.Admin
                                                               && person.User.UniversalId == userUniversalId
                                                               && person.Organization.Route == orgRoute
                                                           select person).Any()

                                              let hasSystemAccess =
                                                         // Has access as system admin
                                                         (from user in db.Users
                                                          where user.DeletedUtc == null
                                                              && user.UserRole >= UserRole.God
                                                              && user.UniversalId == userUniversalId
                                                          select user).Any()

                                              where
                                                session.DeletedUtc == null
                                                && session.Organization.DeletedUtc == null
                                                && session.Organization.Tenant.DeletedUtc == null
                                                && session.Organization.Route == orgRoute
                                                && session.Route == sessionRoute
                                                && (hasPublicAccess ||
                                                    hasGroupAccess ||
                                                    hasParticipantAccess ||
                                                    hasOrgAccess ||
                                                    hasSystemAccess)

                                              select session;

            if (includeChildRecords)
            {
                // TODO: Split query
                query = IncludeChildren(query);
            }

            SessionEntity ret = await query.FirstOrDefaultAsync();

            return ret;
        }

        /// <summary>
        /// Simplistic lookup of session by route with no RBAC; returns null if not found
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        public static Task<SessionEntity> SessionAsync(this SbDb db, string sessionRoute, bool allowDeleted = false)
        {
            if (allowDeleted)
            {
                return db.Sessions.Where(s => s.Route == sessionRoute).FirstOrDefaultAsync();
            }
            else
            {
                return db.Sessions.Where(s =>
                            s.Route == sessionRoute &&
                            s.DeletedUtc == null &&
                            s.Organization.DeletedUtc == null &&
                            s.Organization.Tenant.DeletedUtc == null).FirstOrDefaultAsync();
            }
        }

        public static async Task<SessionEntity> SessionAsync(this SbDb db, IClaims claims, string orgRoute, string sessionRoute, int[] groupIds)
        {
            return await SessionAsync(db, claims, orgRoute, sessionRoute, groupIds, null);
        }

        public static IQueryable<SessionEntity> IncludeChildren(IQueryable<SessionEntity> query)
        {
            return query
                    .Include(s => s.Organization)
                    .Include(s => s.Prompts)
                    .ThenInclude(p => p.Clips)
                    .Include(s => s.Participants)
                    .ThenInclude(p => p.Person)
                    .ThenInclude(p => p.User)
                    .Include(s => s.Groups)
                    .ThenInclude(s => s.Group);
        }

        /// <summary>
        /// Async retrives the given series with an RBAC query
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="orgRoute"></param>
        /// <param name="seriesRoute"></param>
        /// <param name="minRole"></param>
        /// <param name="includeChildren"></param>
        /// <returns></returns>
        public static async Task<SeriesEntity> SeriesAsync(this SbDb db, string userUniversalId, string orgRoute, string seriesRoute, ParticipantRole minRole = ParticipantRole.Unknown, bool includeChildren = false)
        {
            int[] groupIds = await db.GroupIdsAsync(orgRoute, userUniversalId);

            IQueryable<SeriesEntity> query = from
                                        series in db.Series

                                             let hasGroupAccess =
                                                // linked via participantgroup
                                                series.Template.Groups.Where(g => g.ParticipantRole >= minRole).Any(pg => groupIds.Contains(pg.Group.Id))

                                             let hasParticipantAccess =
                                                  // Linked via participant
                                                  series.Template.Participants.Any(p =>
                                                      p.DeletedUtc == null
                                                      && p.ParticipantRole >= minRole
                                                      && p.Person.DeletedUtc == null
                                                      && p.Person.User.DeletedUtc == null
                                                      && p.Person.Organization.DeletedUtc == null
                                                      && p.Person.Organization.Tenant.DeletedUtc == null
                                                      && p.Person.User.UniversalId == userUniversalId)

                                             let hasOrgAccess =
                                                  // Has access as org admin
                                                  (from person in db.People
                                                   where person.DeletedUtc == null
                                                       && person.User.DeletedUtc == null
                                                       && person.Organization.DeletedUtc == null
                                                       && person.Organization.Tenant.DeletedUtc == null
                                                       && person.PersonRole >= PersonRole.Admin
                                                       && person.User.UniversalId == userUniversalId
                                                       && person.Organization.Route == orgRoute
                                                   select person).Any()

                                             let hasSystemAccess =
                                                  // Has access as system admin
                                                  (from user in db.Users
                                                   where user.DeletedUtc == null
                                                       && user.UserRole >= UserRole.God
                                                       && user.UniversalId == userUniversalId
                                                   select user).Any()
                                             where
                                                 series.DeletedUtc == null
                                                 && series.Organization.DeletedUtc == null
                                                 && series.Organization.Tenant.DeletedUtc == null
                                                 && series.Organization.Route == orgRoute
                                                 && series.Route == seriesRoute
                                                 && (hasGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)

                                             select series;

            if (includeChildren)
            {
                // TODO: Split query
                query = query
                        .Include(s => s.Template).ThenInclude(t => t.Participants).ThenInclude(p => p.Person).ThenInclude(p => p.User)
                        .Include(s => s.Template).ThenInclude(t => t.Groups).ThenInclude(g => g.Group)
                        .Include(s => s.Template).ThenInclude(t => t.Prompts)
                        .Include(s => s.Sessions)
                        .Include(s => s.Organization);
            }

            SeriesEntity ret = await query.FirstOrDefaultAsync();
            ret.AssertFound($"Could no find series {seriesRoute} in org {orgRoute}");

            return ret;
        }

        /// <summary>
        /// Gets the set of sessions in the future for the series
        /// </summary>
        /// <param name="db"></param>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public static async Task<SessionEntity[]> FutureSessionsAsync(this SbDb db, int seriesId)
        {
            DateTime now = Time.UtcNow;
            IQueryable<SessionEntity> query = from
                                            session in db.Sessions
                                              where
                                                  // Allow deleted sessions since we want to always resume AFTER deleted ones
                                                  session.Organization.DeletedUtc == null
                                                  && session.Organization.Tenant.DeletedUtc == null
                                                  && session.SeriesId == seriesId
                                                  && session.IsTemplate == false
                                                  // Not started yet
                                                  && session.ReminderSent == null
                                                  && session.PublishSent == null
                                                  // In the future
                                                  && session.Reminder > now
                                                  && session.Publish > now
                                              orderby session.Publish // Furthest in the future will be last
                                              select session;
            SessionEntity[] futureSessions = await query.ToArrayAsync();
            return futureSessions;
        }

        /// <summary>
        /// <see cref="SessionEntity"/> that was never acted upon by the user; ordered oldest on the bottom
        /// </summary>
        /// <param name="db"></param>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public static async Task<SessionEntity[]> IgnoredPastSessions(this SbDb db, int seriesId)
        {
            DateTime now = Time.UtcNow;
            IQueryable<SessionEntity> query = from
                                                session in db.Sessions
                                              where
                                                // Never deleted
                                                session.DeletedUtc == null &&
                                                session.Organization.DeletedUtc == null &&
                                                session.Organization.Tenant.DeletedUtc == null &&
                                                session.SeriesId == seriesId &&
                                                session.IsTemplate == false &&
                                                // Would have published in the past, but never published
                                                session.Publish < now &&
                                                session.PublishSent == null
                                              orderby session.Publish descending // Newest on top
                                              select session;
            SessionEntity[] pastIgnoredSessions = await query.ToArrayAsync();
            return pastIgnoredSessions;
        }

        /// <summary>
        /// RBAC-enabled query checking if the current user has access to the given session in the given org, returing th associated <see cref="SessionEntity"/> if found
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        public static async Task<SessionEntity> SessionAsync(this SbDb db, IClaims claims, string orgRoute, string sessionRoute, int[] groupIds, IRbac rbac)
        {
            IQueryable<SessionEntity> query = from session in db.Sessions

                                              let hasGroupAccess =
                                                     // linked via participantgroup
                                                     session.Groups.Any(pg =>
                                                          groupIds.Contains(pg.Group.Id)
                                                          && pg.ParticipantRole >= ParticipantRole.Host)

                                              let hasParticipantAccess =
                                                    // Linked via participant
                                                    session.Participants.Any(p =>
                                                         p.DeletedUtc == null
                                                         && p.Person.DeletedUtc == null
                                                         && p.Person.User.DeletedUtc == null
                                                         && p.Person.Organization.DeletedUtc == null
                                                         && p.Person.Organization.Tenant.DeletedUtc == null
                                                         && p.Person.User.UniversalId == claims.UniversalId
                                                         && p.ParticipantRole >= ParticipantRole.Host)

                                              let hasOrgAccess =
                                                          // Has access as org admin
                                                          (from person in db.People
                                                           where person.DeletedUtc == null
                                                               && person.User.DeletedUtc == null
                                                               && person.Organization.DeletedUtc == null
                                                               && person.Organization.Tenant.DeletedUtc == null
                                                               && person.PersonRole >= PersonRole.Admin
                                                               && person.User.UniversalId == claims.UniversalId
                                                               && person.Organization.Route == orgRoute
                                                           select person).Any()

                                              let hasSystemAccess =
                                                         // Has access as system admin
                                                         (from user in db.Users
                                                          where user.DeletedUtc == null
                                                              && user.UserRole >= UserRole.God
                                                              && user.UniversalId == claims.UniversalId
                                                          select user).Any()

                                              let hasDbRbac = (rbac is SystemDbRbac)

                                              where
                                                session.DeletedUtc == null
                                                && session.Organization.DeletedUtc == null
                                                && session.Organization.Tenant.DeletedUtc == null
                                                && session.Organization.Route == orgRoute
                                                && session.Route == sessionRoute
                                                && (hasGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess || hasDbRbac)

                                              select session;

            // TODO: Split query
            query = query
                        .Include(s => s.Prompts)
                        .ThenInclude(p => p.Clips)
                        .Include(s => s.Participants)
                        .ThenInclude(p => p.Person)
                        .ThenInclude(p => p.User)
                        .Include(s => s.Groups);

            SessionEntity ret = await query.FirstOrDefaultAsync();
            return ret;
        }

        public static async Task<SessionType> ReadSessionTypeByClipOpRoute(this SbDb db, string clipOpRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(clipOpRoute), clipOpRoute);
            SessionType result = await db.ClipOperations
                .Where(i =>
                    i.Route == clipOpRoute
                    && i.DeletedUtc == null
                    && i.Clip.DeletedUtc == null
                    && i.Clip.Prompt.DeletedUtc == null
                    && i.Clip.Prompt.Session.DeletedUtc == null
                    && i.Clip.Prompt.Session.Organization.DeletedUtc == null
                    && i.Clip.Prompt.Session.Organization.Tenant.DeletedUtc == null
                )
                .Select(i => i.Clip.Prompt.Session.SessionType)
                .FirstAsync();
            return result;
        }

        /// <summary>
        /// Retrieve the full clip object, with optional children for the given route
        /// </summary>
        /// <param name="db"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        public static async Task<ClipEntity> ClipByRouteAsync(this SbDb db, string clipRoute, bool includeChildren = true)
        {
            IQueryable<ClipEntity> query = db.Clips
                .Where(c =>
                    c.DeletedUtc == null
                    && c.Route == clipRoute
                    && c.Prompt.DeletedUtc == null
                    && c.Prompt.Session.DeletedUtc == null
                    && c.Prompt.Session.Organization.DeletedUtc == null
                );

            if (includeChildren)
            {
                query = query.Include(i => i)
                    .Include(i => i.Prompt)
                    .Include(i => i.Prompt.Session)
                    .Include(i => i.Prompt.Session.Organization);
            }

            ClipEntity clip = await query.FirstOrDefaultAsync();
            return clip;
        }
    }
}

