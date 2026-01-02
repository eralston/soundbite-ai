using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.EntityFrameworkCore;
using Soundbite.Entity;
using Soundbite.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Extensions class for querying into <see cref="SbDb"/>
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class SbQueryExtensions
    {
        /// <summary>
        /// Builds a <see cref="IQueryable{Participant}"/> for the given session in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// 
        /// <returns></returns>
        public static IQueryable<ParticipantEntity> QueryDirectParticipants(this SbDb db, string orgRoute, string sessionRoute)
        {
            // TODO: Paging
            Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ParticipantEntity, UserEntity> query =
                db.Participants
                    .Where(p =>
                        p.DeletedUtc == null &&
                        p.IsDirectParticipant == true &&
                        p.Person.DeletedUtc == null &&
                        p.Person.User.DeletedUtc == null &&
                        p.Person.Organization.DeletedUtc == null &&
                        p.Person.Organization.Tenant.DeletedUtc == null &&
                        p.Session.DeletedUtc == null &&
                        p.Session.Route == sessionRoute &&
                        p.Session.Organization.DeletedUtc == null &&
                        p.Session.Organization.Route == orgRoute &&
                        p.Session.Organization.Tenant.DeletedUtc == null)
                    .Include(p => p.Person)
                    .ThenInclude(p => p.User);

            return query;
        }

        /// <summary>
        /// For the given session in the given org, emit a summary of the reactions as a colledction of <see cref="ReactionSummary"/> records
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        public static IQueryable<ReactionSummary> QueryReactions(this SbDb db, string orgRoute, string sessionRoute)
        {
            IQueryable<ReactionSummary> query =
                db.Participants
                    .Where(p =>
                        p.DeletedUtc == null &&
                        p.Person.DeletedUtc == null &&
                        p.Person.User.DeletedUtc == null &&
                        p.Person.Organization.DeletedUtc == null &&
                        p.Person.Organization.Tenant.DeletedUtc == null &&
                        p.Session.DeletedUtc == null &&
                        p.Session.Route == sessionRoute &&
                        p.Session.Organization.DeletedUtc == null &&
                        p.Session.Organization.Route == orgRoute &&
                        p.Session.Organization.Tenant.DeletedUtc == null &&
                        (p.ParticipantRole == ParticipantRole.Audience || p.ParticipantRole == ParticipantRole.Participant))
                    .GroupBy(p => p.ReactionType)
                    .Select(g => new ReactionSummary
                    {
                        ReactionType = g.Key,
                        Count = g.Count()
                    });

            return query;
        }

        /// <summary>
        /// Builds a query for all session participant records for the specified user.
        /// </summary>
        /// <param name="db">Reference to the database.</param>
        /// <param name="orgRoute">Route of the organization that owns the session.</param>
        /// <param name="sessionRoute">Route of the session.</param>
        /// <param name="userUniversalId">Universal ID of the user whose participation records are being sought.</param>
        /// <returns>a query for the participant records for the specified user.</returns>
        public static IQueryable<ParticipantEntity> QueryUserParticipation(this SbDb db, string orgRoute, string sessionRoute, string userUniversalId)
        {
            // TODO: Paging
            Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ParticipantEntity, UserEntity> query =
                db.Participants
                    .Where(p =>
                        p.DeletedUtc == null &&
                        p.Person.DeletedUtc == null &&
                        p.Person.User.DeletedUtc == null &&
                        p.Person.User.UniversalId == userUniversalId &&
                        p.Person.Organization.DeletedUtc == null &&
                        p.Person.Organization.Tenant.DeletedUtc == null &&
                        p.Session.DeletedUtc == null &&
                        p.Session.Route == sessionRoute &&
                        p.Session.Organization.DeletedUtc == null &&
                        p.Session.Organization.Route == orgRoute &&
                        p.Session.Organization.Tenant.DeletedUtc == null)
                    .Include(p => p.Person)
                    .ThenInclude(p => p.User);

            return query;
        }

        /// <summary>
        /// Gets a paged resultset containing the participants of a session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the session resides.</param>
        /// <param name="sessionRoute">Route of the session with which the group is associated.</param>
        /// <param name="includeRoles">Specifies which roles to include in the query.  A null or empty array returns all roles.</param>
        /// <param name="page">Specifies which page of data to retrieve.</param>
        /// <returns>a page of participant information</returns>
        public static IQueryable<ParticipantEntity> QueryDirectParticipants(this SbDb db, string orgRoute, string sessionRoute, ParticipantRole[] includeRoles, string userUniversalId)
        {
            IQueryable<ParticipantEntity> query = from session in db.Sessions

                                                  let hasGroupAccess = session.Groups.Any(i => i.Group.Members.Any(i => i.Person.User.UniversalId == userUniversalId))

                                                  let hasParticipantAccess =
                                                          // Linked via participant
                                                          session.Participants.Any(
                                                              (p) =>
                                                                  p.DeletedUtc == null &&
                                                                  p.Person.DeletedUtc == null &&
                                                                  p.Person.User.DeletedUtc == null &&
                                                                  p.Person.Organization.DeletedUtc == null &&
                                                                  p.Person.Organization.Tenant.DeletedUtc == null &&
                                                                  p.Person.User.UniversalId == userUniversalId
                                                              )

                                                  let hasOrgAccess =
                                                              // Has access as org admin
                                                              (from
                                                                  person in db.People
                                                               where
                                                                   person.DeletedUtc == null &&
                                                                   person.PersonRole >= PersonRole.Admin &&
                                                                   person.User.DeletedUtc == null &&
                                                                   person.User.UniversalId == userUniversalId &&
                                                                   person.Organization.DeletedUtc == null &&
                                                                   person.Organization.Route == orgRoute &&
                                                                   person.Organization.Tenant.DeletedUtc == null
                                                               select
                                                                  person
                                                              ).Any()

                                                  let hasSystemAccess =
                                                              // Has access as system admin
                                                              (from
                                                                  user in db.Users
                                                               where
                                                                   user.DeletedUtc == null &&
                                                                   user.UniversalId == userUniversalId &&
                                                                   user.UserRole >= UserRole.God
                                                               select
                                                                  user
                                                              ).Any()

                                                  where
                                                      session.Route == sessionRoute &&
                                                      session.DeletedUtc == null &&
                                                      session.IsTemplate == false &&
                                                      session.Organization.DeletedUtc == null &&
                                                      session.Organization.Route == orgRoute &&
                                                      session.Organization.Tenant.DeletedUtc == null &&
                                                      (session.SessionSecurity == SessionSecurityType.Public || hasGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)

                                                  from p in session.Participants
                                                  where (includeRoles == null || includeRoles.Contains(p.ParticipantRole))
                                                    && !p.ParticipantGroupMembers.Any()
                                                  select p;

            return query;
        }

        /// <summary>
        /// Gets a paged resultset containing the participants of a session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the session resides.</param>
        /// <param name="sessionRoute">Route of the session with which the group is associated.</param>
        /// <param name="includeRoles">Specifies which roles to include in the query.  A null or empty array returns all roles.</param>
        /// <param name="page">Specifies which page of data to retrieve.</param>
        /// <returns>a page of participant information</returns>
        public static IQueryable<ParticipantEntity> QueryParticipants(this SbDb db, string orgRoute, string sessionRoute, ParticipantRole[] includeRoles, string userUniversalId)
        {
            IQueryable<ParticipantEntity> query = from session in db.Sessions

                                                  let hasGroupAccess = session.Groups.Any(i => i.Group.Members.Any(i => i.Person.User.UniversalId == userUniversalId))

                                                  /*
                                                  let hasGroupAccess =
                                                          // linked via participantgroup
                                                          session.Groups.Any((pg) => groupIds.Contains(pg.Group.Id))
                                                  */

                                                  let hasParticipantAccess =
                                                          // Linked via participant
                                                          session.Participants.Any(
                                                              (p) =>
                                                                  p.DeletedUtc == null &&
                                                                  p.Person.DeletedUtc == null &&
                                                                  p.Person.User.DeletedUtc == null &&
                                                                  p.Person.Organization.DeletedUtc == null &&
                                                                  p.Person.Organization.Tenant.DeletedUtc == null &&
                                                                  p.Person.User.UniversalId == userUniversalId
                                                              )

                                                  let hasOrgAccess =
                                                              // Has access as org admin
                                                              (from
                                                                  person in db.People
                                                               where
                                                                   person.DeletedUtc == null &&
                                                                   person.PersonRole >= PersonRole.Admin &&
                                                                   person.User.DeletedUtc == null &&
                                                                   person.User.UniversalId == userUniversalId &&
                                                                   person.Organization.DeletedUtc == null &&
                                                                   person.Organization.Route == orgRoute &&
                                                                   person.Organization.Tenant.DeletedUtc == null
                                                               select
                                                                  person
                                                              ).Any()

                                                  let hasSystemAccess =
                                                              // Has access as system admin
                                                              (from
                                                                  user in db.Users
                                                               where
                                                                   user.DeletedUtc == null &&
                                                                   user.UniversalId == userUniversalId &&
                                                                   user.UserRole >= UserRole.God
                                                               select
                                                                  user
                                                              ).Any()

                                                  where
                                                      session.Route == sessionRoute &&
                                                      session.DeletedUtc == null &&
                                                      session.IsTemplate == false &&
                                                      session.Organization.DeletedUtc == null &&
                                                      session.Organization.Route == orgRoute &&
                                                      session.Organization.Tenant.DeletedUtc == null &&
                                                      (session.SessionSecurity == SessionSecurityType.Public || hasGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)

                                                  from p in session.Participants
                                                  where includeRoles == null || includeRoles.Contains(p.ParticipantRole)
                                                  select p;

            return query;
        }

        /// <summary>
        /// Gets a paged resultset containing the participants of a session.
        /// </summary>
        /// <param name="db">Reference to the SB database context.</param>
        /// <param name="orgRoute">Route of the organization in which the session resides.</param>
        /// <param name="sessionRoute">Route of the session with which the group is associated.</param>
        /// <param name="participantGroupRoute">Route of the participant group whose members are being sought.</param>
        /// <param name="includeRoles">Specifies which roles to include in the query.  A null or empty array returns all roles.</param>
        /// <param name="userUniversalId">Universal ID of the user accessing the session/participant data.</param>
        /// <returns>a page of participant information</returns>
        public static IQueryable<ParticipantEntity> QueryParticipantGroupMembers(this SbDb db, string orgRoute, string sessionRoute, string participantGroupRoute, ParticipantRole[] includeRoles, string userUniversalId)
        {
            // Empty role array needs to be nulled out for the query
            includeRoles = includeRoles != null && includeRoles.Length == 0 ? null : includeRoles;

            IQueryable<ParticipantEntity> query = from p in db.Participants

                                                  let sessionIsValid = p.Session.Route == sessionRoute
                                                    && p.Session.DeletedUtc == null
                                                    && p.Session.IsTemplate == false
                                                    && p.Session.Organization.DeletedUtc == null
                                                    && p.Session.Organization.Route == orgRoute
                                                    && p.Session.Organization.Tenant.DeletedUtc == null

                                                  let hasGroupAccess = p.Session.Groups.Any(i => i.Group.Members.Any(i => i.Person.User.UniversalId == userUniversalId))

                                                  let hasParticipantAccess =
                                                      // Linked via participant
                                                      p.Session.Participants.Any(
                                                          (p) =>
                                                              p.DeletedUtc == null &&
                                                              p.Person.DeletedUtc == null &&
                                                              p.Person.User.DeletedUtc == null &&
                                                              p.Person.Organization.DeletedUtc == null &&
                                                              p.Person.Organization.Tenant.DeletedUtc == null &&
                                                              p.Person.User.UniversalId == userUniversalId
                                                          )

                                                  let hasOrgAccess =
                                                      // Has access as org admin
                                                      (from
                                                          person in db.People
                                                       where
                                                          person.DeletedUtc == null &&
                                                          person.PersonRole >= PersonRole.Admin &&
                                                          person.User.DeletedUtc == null &&
                                                          person.User.UniversalId == userUniversalId &&
                                                          person.Organization.DeletedUtc == null &&
                                                          person.Organization.Route == orgRoute &&
                                                          person.Organization.Tenant.DeletedUtc == null
                                                       select
                                                      person
                                                      ).Any()

                                                  let hasSystemAccess =
                                                      // Has access as system admin
                                                      (from
                                                          user in db.Users
                                                       where
                                                          user.DeletedUtc == null &&
                                                          user.UniversalId == userUniversalId &&
                                                          user.UserRole >= UserRole.God
                                                       select
                                                      user).Any()

                                                  from pgm in p.ParticipantGroupMembers
                                                  where pgm.ParticipantGroup.Route == participantGroupRoute
                                                         && sessionIsValid
                                                         && (p.Session.SessionSecurity == SessionSecurityType.Public || hasGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)
                                                         && (includeRoles == null || includeRoles.Contains(p.ParticipantRole))
                                                  select p;

            return query;
        }

        /// <summary>
        /// Builds an <see cref="IQueryable{Participant}"/> for the given session in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// 
        /// <returns></returns>
        public static IQueryable<ParticipantGroupEntity> QueryParticipantGroups(this SbDb db, string orgRoute, string sessionRoute)
        {
            // TODO: Paging
            Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ParticipantGroupEntity, GroupEntity> query =
                db.ParticipantGroups
                .Where(pg =>
                    pg.DeletedUtc == null &&
                    pg.Group.DeletedUtc == null &&
                    pg.Group.Organization.DeletedUtc == null &&
                    pg.Group.Organization.Tenant.DeletedUtc == null &&
                    pg.Session.DeletedUtc == null &&
                    pg.Session.Route == sessionRoute &&
                    pg.Session.Organization.DeletedUtc == null &&
                    pg.Session.Organization.Route == orgRoute &&
                    pg.Session.Organization.Tenant.DeletedUtc == null
                    )
                .Include(pg => pg.Group);
            return query;
        }

        /// <summary>
        /// Builds an <see cref="IQueryable"/> for <see cref="PromptWithClips"/> object associated with the given session in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IQueryable<PromptWithClips> QueryPromptsWithClips(this SbDb db, string orgRoute, string sessionRoute)
        {
            IQueryable<PromptWithClips> query =
                from
                    prompt in db.Prompts

                let clips = from clip in db.Clips
                                // NOTE: Contributor has no check for deleted right now
                                // If you want an automatic "right to be forgotten" feature with regard to past podcasts, you can implement that filter here
                            let contributor =
                                            (
                                            from
                                                u in db.Users
                                            where
                                                u.Id == clip.CreatedById
                                            select
                                                new User
                                                {
                                                    // ResourceBase
                                                    Route = u.Route,
                                                    CreatedUtc = u.CreatedUtc,
                                                    UpdatedUtc = u.UpdatedUtc,
                                                    DeletedUtc = u.DeletedUtc,
                                                    // User
                                                    UniversalId = u.UniversalId,
                                                    Email = u.Email,
                                                    GivenName = u.GivenName,
                                                    FamilyName = u.FamilyName,
                                                    Phone = u.Phone,
                                                    Title = u.Title,
                                                }
                                            ).Single()

                            where
                                clip.DeletedUtc == null &&
                                clip.Prompt.Id == prompt.Id
                            select new ClipWithContributor
                            {
                                // Fields
                                Route = clip.Route,
                                CreatedUtc = clip.CreatedUtc,
                                UpdatedUtc = clip.UpdatedUtc,
                                DeletedUtc = clip.DeletedUtc,
                                ClipType = clip.ClipType,
                                FileType = clip.FileType,
                                Seconds = clip.DisplaySeconds,
                                TranscriptState = clip.TranscriptState,
                                HostingType = clip.HostingType,
                                HostingData = clip.HostingData,
                                // Contributor
                                Contributor = contributor
                            }

                where
                    prompt.DeletedUtc == null &&
                    prompt.Session.DeletedUtc == null &&
                    prompt.Session.Route == sessionRoute &&
                    prompt.Session.Organization.DeletedUtc == null &&
                    prompt.Session.Organization.Route == orgRoute &&
                    prompt.Session.Organization.Tenant.DeletedUtc == null

                select new PromptWithClips
                {
                    // Fields
                    Route = prompt.Route,
                    CreatedUtc = prompt.CreatedUtc,
                    UpdatedUtc = prompt.UpdatedUtc,
                    Text = prompt.Text,
                    // Relations
                    Clips = clips
                };

            return query;
        }

        /// <summary>
        /// Determines whether a user has at least read access to a session.
        /// </summary>
        /// <param name="db">Reference to the SBDB</param>
        /// <param name="userUniversalId">Universal ID of the user whose access is being determined. May be null if the <paramref name="isPublic"/> flag is <c>true</c>.</param>
        /// <param name="orgRoute">Route of the organization that owns the session.</param>
        /// <param name="sessionRoute">Route of the session.</param>
        /// <param name="isPublic">Flag indicating whether the session is public.</param>
        /// <returns><c>true</c> if the user has access to the session, otherwise <c>false</c>.</returns>
        public static async Task<bool> UserHasSessionAccess(this SbDb db, string userUniversalId, string orgRoute, string sessionRoute, bool isPublic = false)
        {
            int[] groupIds = isPublic && userUniversalId == null
                ? new int[0]
                : await db.GroupIdsAsync(orgRoute, userUniversalId);
            IQueryable<SessionDetails> query = QuerySessionDetails(db, userUniversalId, orgRoute, sessionRoute, false, groupIds, true).AsNoTracking();
            SessionDetails sessionDetails = (await query.FirstOrDefaultAsync());
            return sessionDetails != null;
        }

        /// <summary>
        /// Async query for the given session details
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="isTemplate"></param>
        /// 
        /// <returns></returns>
        public static async Task<SessionDetails> ReadOnlySessionDetailsAsync(this SbDb db, string userUniversalId, string orgRoute, string sessionRoute, bool isPublic, IMapper mapper, bool isTemplate = false)
        {

            int[] groupIds = isPublic && userUniversalId == null
                ? new int[0]
                : await db.GroupIdsAsync(orgRoute, userUniversalId);

            string msg = $"Could not find session '{sessionRoute}' in org '{orgRoute}'";

            IQueryable<SessionDetails> query = QuerySessionDetails(db, userUniversalId, orgRoute, sessionRoute, isTemplate, groupIds, true).AsNoTracking();
            SessionDetails sessionDetails = (await query.FirstOrDefaultAsync()).AssertFound(msg);
            sessionDetails.AssertNotGone($"Session '{sessionRoute}' in org '{orgRoute}' has been removed");

            // Check for participant records
            await ReadOrCreateMyParticipant(db, userUniversalId, orgRoute, sessionRoute, mapper, msg, sessionDetails);

            IQueryable<ParticipantEntity> queryParticipants = db.QueryDirectParticipants(orgRoute, sessionRoute).AsNoTracking();
            IEnumerable<ParticipantEntity> partEntities = (await queryParticipants.ToArrayAsync()).AssertFound(msg);
            sessionDetails.Participants = partEntities.Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User));

            IQueryable<ParticipantGroupEntity> queryGroups = QueryParticipantGroups(db, orgRoute, sessionRoute).AsNoTracking();
            IEnumerable<ParticipantGroupEntity> groupEntities = (await queryGroups.ToArrayAsync()).AssertFound(msg);
            sessionDetails.Groups = groupEntities.Select(p => mapper.MapSafeWith<ParticipantGroup>(p, p.Group));

            IQueryable<PromptWithClips> promptQuery = db.QueryPromptsWithClips(orgRoute, sessionRoute).AsNoTracking();
            sessionDetails.Prompts = (await promptQuery.ToArrayAsync()).AssertFound(msg);

            ReactionSummary[] ret = await ReadReactionsAsync(db, orgRoute, sessionRoute);
            sessionDetails.Reactions = ret;

            return sessionDetails;
        }

        /// <summary>
        /// Read reaction data for the given session in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        public static async Task<ReactionSummary[]> ReadReactionsAsync(this SbDb db, string orgRoute, string sessionRoute)
        {
            IQueryable<ReactionSummary> reactionsQuery = db.QueryReactions(orgRoute, sessionRoute).AsNoTracking();
            ReactionSummary[] ret = await reactionsQuery.ToArrayAsync();
            return ret;
        }

        private static async Task ReadOrCreateMyParticipant(SbDb db, string userUniversalId, string orgRoute, string sessionRoute, IMapper mapper, string msg, SessionDetails sessionDetails)
        {
            IQueryable<ParticipantEntity> queryMyParticipation = db.QueryUserParticipation(orgRoute, sessionRoute, userUniversalId).AsNoTracking();
            IEnumerable<ParticipantEntity> myPartEntities = (await queryMyParticipation.ToArrayAsync()).AssertFound(msg);

            // If we didn't get any in our query, but we weren't locked out of the session itself, then we must need to generate a participant record for the current user
            if (myPartEntities.Count() == 0 && sessionDetails.SessionSecurity == SessionSecurityType.Protected)
            {
                // If they're a real person in the org, then we can load them
                PersonEntity personEntity = await db.PersonWhereUid(orgRoute, userUniversalId);
                if (personEntity != null)
                {
                    // Create ParticipantEntity records for the current user, attach them to the current session, then save
                    ParticipantEntity participant = db.Participants.CreateResource();
                    participant.Person = personEntity;
                    SessionEntity session = await db.SessionAsync(sessionRoute);
                    participant.Session = session;
                    participant.ParticipantRole = ParticipantRole.Participant;
                    participant.ParticipantState = ParticipantState.ConsumptionRequested;
                    participant.IsDirectParticipant = false; // If we're dynamically adding them, they're definitely not direct
                    await db.SaveChangesAsync();
                    // save to myPartEntities as an array
                    myPartEntities = new ParticipantEntity[] { participant };
                }
            }

            // Whether we read or created them, we need to conver them to DTOs
            IEnumerable<Participant> myParticipants = myPartEntities.Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User));
            sessionDetails.MyParticipation = myParticipants;
        }

        /// <summary>
        /// Builds an <see cref="IQueryable{SessionDetails}"/> for the given session in the given org
        /// </summary>
        /// <remarks>
        /// This only returns the field values for <see cref="SessionDetails"/> the relations must be queried separately; this will return a soft-deleted session
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="isTemplate"></param>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        public static IQueryable<SessionDetails> QuerySessionDetails(
            this SbDb db,
            string userUniversalId,
            string orgRoute,
            string sessionRoute,
            bool isTemplate,
            int[] groupIds,
            bool allowDeleted = false)
        {
            IQueryable<SessionDetails> query =
                from session in db.Sessions

                let hasGroupAccess =
                        // linked via participantgroup
                        session.Groups.Any((pg) => groupIds.Contains(pg.Group.Id))

                let hasParticipantAccess =
                        // Linked via participant
                        session.Participants.Any(
                            (p) =>
                                p.DeletedUtc == null &&
                                p.Person.DeletedUtc == null &&
                                p.Person.User.DeletedUtc == null &&
                                p.Person.Organization.DeletedUtc == null &&
                                p.Person.Organization.Tenant.DeletedUtc == null &&
                                p.Person.User.UniversalId == userUniversalId
                            )

                let hasOrgAccess =
                            // Has access as org admin
                            (from
                                person in db.People
                             where
                                 person.DeletedUtc == null &&
                                 person.PersonRole >= PersonRole.Admin &&
                                 person.User.DeletedUtc == null &&
                                 person.User.UniversalId == userUniversalId &&
                                 person.Organization.DeletedUtc == null &&
                                 person.Organization.Route == orgRoute &&
                                 person.Organization.Tenant.DeletedUtc == null
                             select
                                person
                            ).Any()

                let hasSystemAccess =
                            // Has access as system admin
                            (from
                                user in db.Users
                             where
                                 user.DeletedUtc == null &&
                                 user.UniversalId == userUniversalId &&
                                 user.UserRole >= UserRole.God
                             select
                                user
                            ).Any()

                where
                    session.Route == sessionRoute &&
                    session.IsTemplate == isTemplate &&
                    session.Organization.DeletedUtc == null &&
                    session.Organization.Route == orgRoute &&
                    session.Organization.Tenant.DeletedUtc == null &&
                    (session.SessionSecurity == SessionSecurityType.Public || hasGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)

                select new SessionDetails
                {
                    // IResource
                    Route = session.Route,
                    CreatedUtc = session.CreatedUtc,
                    UpdatedUtc = session.UpdatedUtc,
                    DeletedUtc = session.DeletedUtc,
                    // Session
                    Reminder = session.Reminder,
                    ReminderSent = session.ReminderSent,
                    PublishSent = session.PublishSent,
                    Publish = session.Publish,
                    Name = session.Name,
                    Limit = session.Limit,
                    SessionType = session.SessionType,
                    SessionSecurity = session.SessionSecurity,
                    SessionCommentPolicy = session.SessionCommentPolicy,
                    ReminderCalEventId = session.ReminderCalEventId,
                };

            if (!allowDeleted)
            {
                query = query.Where(s => s.DeletedUtc == null);
            }

            return query;
        }

        /// <summary>
        /// Builds an <see cref="IQueryable{Session}"/> for objects the given user has access to in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <param name="groupIds"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static Task<Session[]> SessionsAsync(
            this SbDb db,
            string userUniversalId,
            int[] groupIds,
            string orgRoute)
        {
            // TODO: Paging
            IQueryable<Session> query =
                from session in db.Sessions

                let hasParticipantGroupAccess =
                    session.Groups.Any(pg => groupIds.Contains(pg.Group.Id))

                let hasParticipantAccess =
                    session.Participants.Any(p =>
                        p.DeletedUtc == null
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
                    session.DeletedUtc == null &&
                    session.IsTemplate == false &&
                    session.Organization.DeletedUtc == null &&
                    session.Organization.Route == orgRoute &&
                    session.Organization.Tenant.DeletedUtc == null &&
                    (hasParticipantGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)

                select new Session
                {
                    // Fields
                    Route = session.Route,
                    CreatedUtc = session.CreatedUtc,
                    UpdatedUtc = session.UpdatedUtc,
                    DeletedUtc = session.DeletedUtc,
                    Name = session.Name,
                    Reminder = session.Reminder,
                    ReminderSent = session.ReminderSent,
                    Publish = session.Publish,
                    PublishSent = session.PublishSent,
                    Limit = session.Limit,
                    SessionType = session.SessionType,
                    SessionSecurity = session.SessionSecurity,
                    SessionCommentPolicy = session.SessionCommentPolicy,
                    ReminderCalEventId = session.ReminderCalEventId
                };

            return query.ReadOnly();
        }

        /// <summary>
        /// Async gets the <see cref="Session"/> records for the given group in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <param name="groupIds"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        public static Task<Session[]> GroupSessionsAsync(
            this SbDb db,
            IClaims claims,
            int[] groupIds,
            string orgRoute,
            string groupRoute)
        {
            // TODO: Paging
            IQueryable<Session> query =
                from session in db.Sessions

                let hasParticipantGroupAccess =
                    session.Groups.Any(pg => groupIds.Contains(pg.Group.Id))

                let hasParticipantAccess =
                    session.Participants.Any(p =>
                        p.DeletedUtc == null
                        && p.Person.DeletedUtc == null
                        && p.Person.User.DeletedUtc == null
                        && p.Person.Organization.DeletedUtc == null
                        && p.Person.Organization.Tenant.DeletedUtc == null
                        && p.Person.User.UniversalId == claims.UniversalId)

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

                // This is added to QueryMySessions to make this exclusion
                let sessionHasParticipantGroup =
                    session.Groups.Any(pg =>
                        pg.DeletedUtc == null &&
                        pg.Group.DeletedUtc == null &&
                        pg.Group.Route == groupRoute &&
                        pg.SessionId == session.Id
                    )

                where
                    session.DeletedUtc == null &&
                    session.IsTemplate == false &&
                    session.Organization.DeletedUtc == null &&
                    session.Organization.Route == orgRoute &&
                    session.Organization.Tenant.DeletedUtc == null &&
                    sessionHasParticipantGroup &&
                    (hasParticipantGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)

                select new Session
                {
                    // Fields
                    Route = session.Route,
                    CreatedUtc = session.CreatedUtc,
                    UpdatedUtc = session.UpdatedUtc,
                    DeletedUtc = session.DeletedUtc,
                    Name = session.Name,
                    Reminder = session.Reminder,
                    ReminderSent = session.ReminderSent,
                    Publish = session.Publish,
                    PublishSent = session.PublishSent,
                    Limit = session.Limit,
                    SessionType = session.SessionType,
                    SessionSecurity = session.SessionSecurity,
                    SessionCommentPolicy = session.SessionCommentPolicy,
                    ReminderCalEventId = session.ReminderCalEventId
                };

            return query.ReadOnly();
        }

        /// <summary>
        /// Creates an <see cref="IQueryable{SessionEntity}"/> instance containing an unexecuted 
        /// query for the specified criteria defined in the method parameters.
        /// </summary>
        /// <param name="db">Reference to the Soundbite database context.</param>
        /// <param name="orgRoute">Route of the organization in which to search for sessions.</param>
        /// <param name="groupRoute">Route of the group/team in which to search for sessions.</param>
        /// <param name="participantQueries">Enumeration of partipant queries used to narrow down sessions by participant data (leave null to avoid filtering by this criteria).</param>
        /// <param name="securityType">Filters by the specified session security value (leave null to avoid filtering by this criteria).</param>
        /// <param name="isTemplate">Filters by whether the session is a template for a series (leave null to avoid filtering by this criteria).</param>
        /// <param name="isDeleted">Filters by whether the session is active or deleted (leave null to avoid filtering by this criteria).</param>        
        /// <returns>an unexecuted query that can be further manipulated or executed.</returns>        
        public static IQueryable<SessionEntity> QuerySessions(
            this SbDb db,
            string orgRoute,
            string groupRoute,
            ParticipantQuery participants,
            SessionSecurityType[] securityTypes,
            bool? isTemplate,
            bool? isDeleted)
        {
            // Validation
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);

            // Build Session Query
            IQueryable<SessionEntity> sessionQuery = db.Sessions.Where(i =>
                (
                    isDeleted == null
                    || (isDeleted == false && i.DeletedUtc == null)
                    || (isDeleted == true && i.DeletedUtc != null)
                )
                && (isTemplate == null || (i.IsTemplate == isTemplate))
                && i.Organization.Route == orgRoute // Mandatory Condition
                && i.Organization.DeletedUtc == null // Mandatory Condition
                && i.Organization.Tenant.DeletedUtc == null // Mandatory Condition
            );

            // Append group route condition if applicable
            if (!string.IsNullOrEmpty(groupRoute))
            {
                sessionQuery = sessionQuery.Where(i => i.Groups.Any(i => i.Group.Route == groupRoute));
            }

            // Append the security type if applicable
            if (securityTypes?.Any() == true)
            {
                sessionQuery = sessionQuery.Where(i => securityTypes.Contains(i.SessionSecurity));
            }

            // Append the pariticpant query if applicable
            if (participants != null && participants.Criteria?.Any() == true)
            {
                // Storage variable for the overall participant query to append to the session query
                Expression<Func<SessionEntity, bool>> participantsQuery = null;

                // Iterate over each participant query
                participants.Criteria.ForEach(q =>
                {
                    // Create a predicate to use for the query.  We do this to avoid doubling up
                    // the predciate code when we need to create vs. add a predicate to the query.
                    Expression<Func<SessionEntity, bool>> predicate = (s) => s.Participants.Any(p =>
                            (q.UserId == null || p.Person.UserId == q.UserId)
                            && (q.UserRoute == null || p.Person.User.Route == q.UserRoute)
                            && (q.UserUniversalId == null || p.Person.User.UniversalId == q.UserUniversalId)
                            && (q.Roles.Count == 0 || q.Roles.Contains(p.ParticipantRole))
                            && (q.States.Count == 0 || q.States.Contains(p.ParticipantState))
                        );

                    participantsQuery = participantsQuery == null
                        ? PredicateBuilder.Create(predicate)
                        : participants.IsOrQuery
                            ? participantsQuery.Or(predicate)
                            : participantsQuery.And(predicate);
                });

                // Append the participant query to the session query
                sessionQuery = sessionQuery.Where(participantsQuery);
            }

            return sessionQuery;
        }

        /// <summary>
        /// Creates an <see cref="IQueryable{SessionEntity}"/> instance containing an unexecuted 
        /// query for the specified criteria defined in the method parameters.
        /// </summary>
        /// <param name="db">Reference to the Soundbite database context.</param>
        /// <param name="mapper">Reference to the mapping utility.</param>
        /// <param name="orgRoute">Route of the organization in which to search for sessions.</param>
        /// <param name="groupRoute">Route of the group/team in which to search for sessions.</param>
        /// <param name="participantQueries">Enumeration of partipant queries used to narrow down sessions by participant data (leave null to avoid filtering by this criteria).</param>
        /// <param name="securityType">Filters by the specified session security value (leave null to avoid filtering by this criteria).</param>
        /// <param name="isTemplate">Filters by whether the session is a template for a series (leave null to avoid filtering by this criteria).</param>
        /// <param name="isDeleted">Filters by whether the session is active or deleted (leave null to avoid filtering by this criteria).</param>        
        /// <returns>an unexecuted query that can be further manipulated or executed.</returns>        
        public static async Task<IEnumerable<SessionPreview>> QuerySessionPreview(
            this SbDb db,
            IMapper mapper,
            string uid,
            string orgRoute,
            string groupRoute,
            ParticipantQuery participants,
            SessionSecurityType[] securityTypes,
            bool? isTemplate,
            bool? isDeleted)
        {
            IQueryable<SessionEntity> sessionQuery = QuerySessions(db, orgRoute, groupRoute, participants, securityTypes, isTemplate, isDeleted);

            IQueryable<SessionEntity> sQuery = sessionQuery.OrderBy(session => session.CreatedUtc);

            sQuery = sQuery.Include(s => s.Participants).ThenInclude(sp => sp.Person).ThenInclude(p => p.User);

            SessionEntity[] result = await sQuery.ReadOnly();

            IEnumerable<SessionPreview> ret = result.Select(session =>
                   new SessionPreview
                   {
                       // Fields
                       Route = session.Route,
                       CreatedUtc = session.CreatedUtc,
                       UpdatedUtc = session.UpdatedUtc,
                       DeletedUtc = session.DeletedUtc,
                       Name = session.Name,
                       Reminder = session.Reminder,
                       ReminderSent = session.ReminderSent,
                       Publish = session.Publish,
                       PublishSent = session.PublishSent,
                       Limit = session.Limit,
                       SessionType = session.SessionType,
                       SessionSecurity = session.SessionSecurity,
                       SessionCommentPolicy = session.SessionCommentPolicy,
                       ReminderCalEventId = session.ReminderCalEventId,

                       MyParticipants =
                         session.Participants
                         .Where(p => p.Person.User.UniversalId == uid)
                         .Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User))
                         .ToArray(),

                       HostParticipants =
                          session.Participants
                         .Where(p => p.ParticipantRole >= ParticipantRole.Host)
                         .Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User))
                         .ToArray(),
                   }).ToArray();
            return ret;
        }

        /// <summary>
        /// <see cref="IQueryable{SessionForFeed}"/> for the given user and groups in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="uid"></param>
        /// <param name="orgRoute"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Task<IEnumerable<SessionPreview>> SessionPreviewAsync(
            this SbDb db,
            IMapper mapper,
            string uid,
            string orgRoute)
        {
            return QuerySessionPreview(
                db,
                mapper,
                uid,
                orgRoute,
                null,
                ParticipantQuery.Single(i => i
                  .UserUid(uid)
                  .States(ParticipantState.ConsumptionRequested, ParticipantState.ContributionRequested)),
                new[] {
                    SessionSecurityType.Protected,
                    SessionSecurityType.Public
                },
                false,
                false);
        }

        /// <summary>
        /// Builds an <see cref="IQueryable"/> for <see cref="SessionPreview"/> for the given group in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mapper"></param>
        /// <param name="uid"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        public static Task<IEnumerable<SessionPreview>> SessionPreviewAsync(
            this SbDb db,
            IMapper mapper,
            string uid,
            string orgRoute,
            string groupRoute)
        {
            return QuerySessionPreview(
                db,
                mapper,
                uid,
                orgRoute,
                groupRoute,
                ParticipantQuery.Single(i => i
                  .UserUid(uid)
                  .States(
                    ParticipantState.ConsumptionRequested,
                    ParticipantState.ContributionRequested)),
                new[] {
                    SessionSecurityType.Public,
                    SessionSecurityType.Protected
                },
                false,
                false);
        }

        /// <summary>
        /// Loads the most recent N (not deleted) sessions in the given org orderd by <see cref="Session.PublishSent"/> with newest on top
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SessionPreview>> RecentlyPublishedSessionsAsync(
            this SbDb db,
            string orgRoute,
            IndexPageRequest page = null)
        {
            IQueryable<SessionPreview> query =
                from session in db.Sessions

                where
                    session.DeletedUtc == null &&
                    session.IsTemplate == false &&
                    session.PublishSent != null &&
                    session.Organization.DeletedUtc == null &&
                    session.Organization.Route == orgRoute &&
                    session.Organization.Tenant.DeletedUtc == null

                orderby session.PublishSent descending

                select new SessionPreview
                {
                    // Fields
                    Route = session.Route,
                    CreatedUtc = session.CreatedUtc,
                    UpdatedUtc = session.UpdatedUtc,
                    DeletedUtc = session.DeletedUtc,
                    Name = session.Name,
                    Reminder = session.Reminder,
                    ReminderSent = session.ReminderSent,
                    Publish = session.Publish,
                    PublishSent = session.PublishSent,
                    Limit = session.Limit,
                    SessionType = session.SessionType,
                    SessionSecurity = session.SessionSecurity,
                    SessionCommentPolicy = session.SessionCommentPolicy,
                    ReminderCalEventId = session.ReminderCalEventId,
                };

            // Paging
            PageResponseBase<SessionPreview> response = await query.ReadPageAsync(page ?? new IndexPageRequest { Skip = 0, Take = 10 });

            return response.Result;
        }

        /// <summary>
        /// Builds an <see cref="IQueryable"/> for the <see cref="SeriesDetailsWithMetadata"/> associated with the given series in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <param name="groupIds"></param>
        /// <param name="orgRoute"></param>
        /// <param name="seriesRoute"></param>
        /// <param name="minRole"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IQueryable<SeriesDetailsWithMetadata> QuerySeriesDetails(
            this SbDb db,
            IClaims claims,
            int[] groupIds,
            string orgRoute,
            string seriesRoute,
            ParticipantRole minRole,
            IMapper mapper)
        {
            IQueryable<SeriesDetailsWithMetadata> query =
                from
                    series in db.Series

                let sessions = from
                                session in db.Sessions

                               let myParticipants =
                                   // The full list of people directly linked to the session
                                   from
                                       p in db.Participants
                                   where
                                       p.DeletedUtc == null
                                       && p.Person.DeletedUtc == null
                                       && p.Person.User.DeletedUtc == null
                                       && p.Person.Organization.DeletedUtc == null
                                       && p.Person.Organization.Tenant.DeletedUtc == null
                                       && p.SessionId == session.Id
                                   // TODO: If Person or Person.User needed, then this has to convert to NOT use MapSafe since it's called AsNoTracking
                                   select mapper.MapSafe<Participant>(p)

                               where
                                   session.DeletedUtc == null
                                    && session.Organization.DeletedUtc == null
                                    && session.Organization.Tenant.DeletedUtc == null
                                    && session.Organization.Route == orgRoute
                                    && session.SeriesId == series.Id
                                    && session.IsTemplate == false
                                    && (session.Reminder > Time.UtcNow || session.Reminder == null)
                                    && session.Publish > Time.UtcNow

                               select new SessionPreview
                               {
                                   // Fields
                                   ReminderSent = session.ReminderSent,
                                   PublishSent = session.PublishSent,
                                   Route = session.Route,
                                   CreatedUtc = session.CreatedUtc,
                                   UpdatedUtc = session.UpdatedUtc,
                                   Name = session.Name,
                                   Limit = session.Limit,
                                   SessionType = session.SessionType,
                                   SessionSecurity = session.SessionSecurity,
                                   SessionCommentPolicy = session.SessionCommentPolicy,
                                   Reminder = session.Reminder,
                                   Publish = session.Publish,
                                   // Relationships
                                   MyParticipants = myParticipants
                               }

                let hasGroupAccess =
                    (from
                        grp in series.Template.Groups
                     where
                         grp.DeletedUtc == null
                         && grp.Group.DeletedUtc == null
                         && grp.Group.Organization.DeletedUtc == null
                         && grp.Group.Organization.Tenant.DeletedUtc == null
                         && grp.ParticipantRole >= minRole
                     select grp.Id)
                     .Any(grpId => groupIds.Contains(grpId))

                let hasParticipantAccess =
                    // Linked via participant
                    series.Template.Participants.Any(p =>
                        p.DeletedUtc == null &&
                        p.ParticipantRole >= minRole &&
                        p.Person.DeletedUtc == null &&
                        p.Person.User.DeletedUtc == null &&
                        p.Person.User.UniversalId == claims.UniversalId &&
                        p.Person.OrganizationId == series.OrganizationId
                        )

                let hasOrgAccess =
                    // Has access as org admin
                    (from
                        person in db.People
                     where
                        person.DeletedUtc == null &&
                        person.PersonRole >= PersonRole.Admin &&
                        person.User.DeletedUtc == null &&
                        person.User.UniversalId == claims.UniversalId &&
                        person.Organization.Id == series.OrganizationId
                     select person).Any()

                let hasSystemAccess =
                    // Has access as system admin
                    (from user in db.Users
                     where user.DeletedUtc == null
                         && user.UserRole >= UserRole.God
                         && user.UniversalId == claims.UniversalId
                     select user).Any()

                where
                    series.DeletedUtc == null
                    && series.Organization.DeletedUtc == null
                    && series.Organization.Tenant.DeletedUtc == null
                    && series.Organization.Route == orgRoute
                    && series.Route == seriesRoute
                    && (hasGroupAccess || hasParticipantAccess || hasOrgAccess || hasSystemAccess)

                select new SeriesDetailsWithMetadata
                {
                    SeriesDetails = new SeriesDetails
                    {
                        Route = series.Route,
                        CreatedUtc = series.CreatedUtc,
                        UpdatedUtc = series.UpdatedUtc,
                        Name = series.Name,
                        Recurrence = series.Recurrence,
                        RecurrenceData = series.RecurrenceData,
                        Sessions = sessions
                    },
                    TemplateSessionRoute = series.Template.Route
                };

            return query;
        }

        /// <summary>
        /// Gets the <see cref="SessionPreview"/> associated with the given series in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="seriesRoute"></param>
        /// <returns></returns>
        public static Task<SessionPreview> SessionPreviewForSeries(
            this SbDb db,
            IClaims claims,
            IMapper mapper,
            string orgRoute,
            string seriesRoute)
        {
            IQueryable<SessionPreview> query =
                from
                    s in db.Series

                let myParticipants =
                    s.Template.Participants
                        .Where(p =>
                           p.DeletedUtc == null &&
                           p.Person.DeletedUtc == null &&
                           p.Person.Organization.DeletedUtc == null &&
                           p.Person.Organization.Tenant.DeletedUtc == null &&
                           p.Person.User.DeletedUtc == null &&
                           p.Person.User.UniversalId == claims.UniversalId
                        )

                where
                    s.DeletedUtc == null &&
                    s.Route == seriesRoute &&
                    s.Organization.DeletedUtc == null &&
                    s.Organization.Route == orgRoute &&
                    s.Organization.Tenant.DeletedUtc == null

                select new SessionPreview
                {
                    // Resource
                    Route = s.Template.Route,
                    CreatedUtc = s.Template.CreatedUtc,
                    UpdatedUtc = s.Template.UpdatedUtc,
                    DeletedUtc = s.Template.DeletedUtc,
                    // Fields
                    Reminder = s.Template.Reminder,
                    ReminderSent = s.Template.ReminderSent,
                    Publish = s.Template.Publish,
                    PublishSent = s.Template.PublishSent,
                    Name = s.Name,
                    Limit = s.Template.Limit,
                    SessionType = s.Template.SessionType,
                    SessionSecurity = s.Template.SessionSecurity,
                    SessionCommentPolicy = s.Template.SessionCommentPolicy,
                    MyParticipants = myParticipants.Select(p => mapper.MapSafe<Participant>(p))
                };

            return query.ReadOnlyOne();
        }

        /// <summary>
        /// Gets the <see cref="SeriesPreview"/> objects for the given user in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="claims"></param>
        /// <param name="groupIds"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<SeriesPreview>> SeriesPreviewAsync(
            this SbDb db,
            IClaims claims,
            int[] groupIds,
            IMapper mapper,
            string orgRoute)
        {
            // TODO: Paging
            IQueryable<SeriesPreview> query =
                from
                    series in db.Series

                let hasGroupAccess =
                          (from
                             grp in series.Template.Groups
                           where
                               grp.DeletedUtc == null &&
                               grp.Group.DeletedUtc == null &&
                               grp.Group.Organization.DeletedUtc == null &&
                               grp.Group.Organization.Tenant.DeletedUtc == null &&
                               grp.ParticipantRole >= ParticipantRole.Audience
                           select
                             grp)
                           .Any(grp => groupIds.Contains(grp.Group.Id))

                let hasParticipantAccess =
                     series.Template.Participants.Any(participant =>
                         participant.DeletedUtc == null &&
                         participant.ParticipantRole >= ParticipantRole.Audience &&
                         participant.Person.DeletedUtc == null &&
                         participant.Person.User.DeletedUtc == null &&
                         participant.Person.Organization.DeletedUtc == null &&
                         participant.Person.Organization.Tenant.DeletedUtc == null &&
                         participant.Person.User.UniversalId == claims.UniversalId)

                let hasOrgAccess =
                    (from
                        person in db.People
                     where
                        person.DeletedUtc == null &&
                        person.User.DeletedUtc == null &&
                        person.Organization.DeletedUtc == null &&
                        person.Organization.Tenant.DeletedUtc == null &&
                        person.PersonRole >= PersonRole.Admin &&
                        person.User.UniversalId == claims.UniversalId &&
                        person.Organization.Route == orgRoute
                     select person)
                     .Any()

                let hasSystemAccess =
                    (from
                        user in db.Users
                     where
                         user.DeletedUtc == null &&
                         user.UserRole >= UserRole.God &&
                         user.UniversalId == claims.UniversalId
                     select
                         user)
                    .Any()

                where
                    series.DeletedUtc == null &&
                    series.Organization.DeletedUtc == null &&
                    series.Organization.Route == orgRoute &&
                    series.Organization.Tenant.DeletedUtc == null &&
                    (hasParticipantAccess || hasGroupAccess || hasOrgAccess || hasSystemAccess)
                select new SeriesPreview
                {
                    // Fields
                    Route = series.Route,
                    CreatedUtc = series.CreatedUtc,
                    UpdatedUtc = series.UpdatedUtc,
                    Name = series.Name,
                    Recurrence = series.Recurrence,
                    RecurrenceData = series.RecurrenceData
                };

            IEnumerable<SeriesPreview> ret = await query.ReadOnly();

            foreach (SeriesPreview s in ret)
            {
                s.Template = await db.SessionPreviewForSeries(claims, mapper, orgRoute, s.Route);
            }

            return ret;
        }

        public static async Task<IEnumerable<SeriesPreview>> SeriesPreviewAsync(
            this SbDb db,
            IClaims claims,
            int[] groupIds,
            IMapper mapper,
            string orgRoute,
            string groupRoute)
        {
            // TODO: Paging
            IQueryable<SeriesPreview> query =
                from
                    series in db.Series

                let isLinkedToGroup =
                    (from
                        grp in series.Template.Groups
                     where
                         grp.DeletedUtc == null &&
                         grp.Group.DeletedUtc == null &&
                         grp.Group.Organization.DeletedUtc == null &&
                         grp.Group.Organization.Tenant.DeletedUtc == null &&
                         grp.Group.Route == groupRoute
                     select
                        grp)
                     .Any()

                let hasGroupAccess =
                          (from
                             grp in series.Template.Groups
                           where
                               grp.DeletedUtc == null &&
                               grp.Group.DeletedUtc == null &&
                               grp.Group.Organization.DeletedUtc == null &&
                               grp.Group.Organization.Tenant.DeletedUtc == null &&
                               grp.ParticipantRole >= ParticipantRole.Audience
                           select
                             grp)
                           .Any(grp => groupIds.Contains(grp.Group.Id))

                let hasParticipantAccess =
                     series.Template.Participants.Any(participant =>
                         participant.DeletedUtc == null &&
                         participant.ParticipantRole >= ParticipantRole.Audience &&
                         participant.Person.DeletedUtc == null &&
                         participant.Person.User.DeletedUtc == null &&
                         participant.Person.Organization.DeletedUtc == null &&
                         participant.Person.Organization.Tenant.DeletedUtc == null &&
                         participant.Person.User.UniversalId == claims.UniversalId)

                let hasOrgAccess =
                    (from
                        person in db.People
                     where
                        person.DeletedUtc == null &&
                        person.User.DeletedUtc == null &&
                        person.Organization.DeletedUtc == null &&
                        person.Organization.Tenant.DeletedUtc == null &&
                        person.PersonRole >= PersonRole.Admin &&
                        person.User.UniversalId == claims.UniversalId &&
                        person.Organization.Route == orgRoute
                     select person)
                     .Any()

                let hasSystemAccess =
                    (from
                        user in db.Users
                     where
                         user.DeletedUtc == null &&
                         user.UserRole >= UserRole.God &&
                         user.UniversalId == claims.UniversalId
                     select
                         user)
                    .Any()

                where
                    series.DeletedUtc == null &&
                    series.Organization.DeletedUtc == null &&
                    series.Organization.Route == orgRoute &&
                    series.Organization.Tenant.DeletedUtc == null &&
                    isLinkedToGroup &&
                    (hasParticipantAccess || hasGroupAccess || hasOrgAccess || hasSystemAccess)
                select new SeriesPreview
                {
                    // Fields
                    Route = series.Route,
                    CreatedUtc = series.CreatedUtc,
                    UpdatedUtc = series.UpdatedUtc,
                    Name = series.Name,
                    Recurrence = series.Recurrence,
                    RecurrenceData = series.RecurrenceData
                };

            IEnumerable<SeriesPreview> ret = await query.ReadOnly();

            foreach (SeriesPreview s in ret)
            {
                s.Template = await db.SessionPreviewForSeries(claims, mapper, orgRoute, s.Route);
            }

            return ret;
        }

        /// <summary>
        /// Async query for <see cref="SbGroupDetails"/> for the given group in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mapper"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        public static async Task<Group> GroupAsync(
            this IIdentityDb db,
            string orgRoute,
            string groupRoute)
        {
            IQueryable<Group> query = from
                                          grp in db.Groups

                                      where
                                          grp.Route == groupRoute &&
                                          grp.DeletedUtc == null &&
                                          grp.Organization.DeletedUtc == null &&
                                          grp.Organization.Route == orgRoute &&
                                          grp.Organization.Tenant.DeletedUtc == null

                                      select
                                          new Group
                                          {
                                              CreatedUtc = grp.CreatedUtc,
                                              DeletedUtc = grp.DeletedUtc,
                                              Description = grp.Description,
                                              Name = grp.Name,
                                              Route = grp.Route,
                                              UpdatedUtc = grp.UpdatedUtc,
                                          };

            Group group = (await query.ReadOnlyOne()).AssertFound($"Could not find group '{groupRoute}' in org '{orgRoute}'");
            return group;
        }

        /// <summary>
        /// Async query for <see cref="Member"/> records associated with the given group in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mapper"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        public static Task<Member[]> MembersAsync(
            this IIdentityDb db,
            IMapper mapper,
            string orgRoute,
            string groupRoute)
        {
            // TODO: Paging
            IQueryable<Member> query = db.Members
                                       .Where(m =>
                                          m.DeletedUtc == null &&
                                          m.Group.DeletedUtc == null &&
                                          m.Group.Route == groupRoute &&
                                          m.Group.Organization.DeletedUtc == null &&
                                          m.Group.Organization.Route == orgRoute &&
                                          m.Group.Organization.Tenant.DeletedUtc == null &&
                                          m.Person.DeletedUtc == null &&
                                          m.Person.User.DeletedUtc == null
                                          )
                                       .OrderBy(m => m.Person.User.GivenName)
                                       .Include(m => m.Person)
                                       .ThenInclude(p => p.User)
                                       .Select(m => mapper.MapSafeWith<Member>(m, m.Person, m.Person.User));

            return query.ReadOnly();
        }

        public static IQueryable<ClipEntity> QueryClips(this SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            return db.Clips
                    .Where(c =>
                        // NOTE: This purposefully ignores deletion
                        c.Prompt.Session.Organization.Route == orgRoute &&
                        startUtc <= c.CreatedUtc &&
                        c.CreatedUtc <= endUtc);
        }

        /// <summary>
        /// Async gets the clip for the given route
        /// </summary>
        /// <remarks>
        /// WARNING: This is a raw utility and does NOT try to prove the validity of the clip, but it does prove the tree of the clip is not deleted
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        public static Task<ClipEntity> ClipAsync(this SbDb db, string orgRoute, string clipRoute)
        {
            IQueryable<ClipEntity> query =
                from
                    c in db.Clips

                where
                    c.Prompt.Session.Organization.Route == orgRoute &&
                    c.DeletedUtc == null &&
                    c.Route == clipRoute &&
                    c.Prompt.DeletedUtc == null &&
                    c.Prompt.Session.DeletedUtc == null &&
                    c.Prompt.Session.Organization.DeletedUtc == null &&
                    c.Prompt.Session.Organization.Tenant.DeletedUtc == null

                select
                    c;

            return query.FirstOrDefaultAsync();
        }

        public static IQueryable<ClipEventEntity> QueryClipEvents(this SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            return db.ClipEvents
                    .Where(c =>
                        c.DeletedUtc == null &&
                        // NOTE: This purposefully ignores deletion for all other entities
                        c.Target.Prompt.Session.Organization.Route == orgRoute &&
                        startUtc <= c.CreatedUtc &&
                        c.CreatedUtc <= endUtc);
        }

        public static IQueryable<SessionEntity> QuerySessions(this SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            return db.Sessions
                    .Where(s =>
                        // NOTE: This purposefully ignores deletion
                        s.Organization.Route == orgRoute &&
                        startUtc <= s.CreatedUtc &&
                        s.CreatedUtc <= endUtc);
        }

        public static IQueryable<ParticipantEntity> QueryParticipants(this SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            return db.Sessions
                    .Where(s =>
                        // NOTE: This purposefully ignores deletion
                        s.Organization.Route == orgRoute &&
                        startUtc <= s.CreatedUtc &&
                        s.CreatedUtc <= endUtc)
                    .SelectMany(s => s.Participants);
        }

        public static IQueryable<SeriesEntity> QuerySeries(this SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            return db.Series
                    .Where(s =>
                        // NOTE: This purposefully ignores deletion
                        s.Organization.Route == orgRoute &&
                        startUtc <= s.CreatedUtc &&
                        s.CreatedUtc <= endUtc);
        }

        /// <summary>
        /// The most common type of <see cref="ClipEventEntity"/>: Capturing the duration in display seconds; for user-visible views
        /// </summary>
        /// <param name="db"></param>
        /// <param name="clipRoute"></param>
        /// <param name="clipEventType"></param>
        /// <param name="createdById"></param>
        /// <param name="autoCommit"></param>
        /// <returns></returns>
        public static async Task<ClipEventEntity> CreateClipEventWithDisplaySeconds(this SbDb db, string clipRoute, ClipEventType clipEventType, int? createdById)
        {
            ClipEventEntity clipEvent = await db.CreateClipEvent(clipRoute, EventType.Read, clipEventType, createdById);
            clipEvent.Duration = clipEvent.Target.DisplaySeconds;
            return clipEvent;
        }

        /// <summary>
        /// The most common type of <see cref="ClipEventEntity"/>: Capturing the duration in billing seconds; for invoice-driven views
        /// </summary>
        /// <param name="db"></param>
        /// <param name="clipRoute"></param>
        /// <param name="clipEventType"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static async Task<ClipEventEntity> CreateClipEventWithBillingSeconds(this SbDb db, string clipRoute, ClipEventType clipEventType, int? createdById)
        {
            ClipEventEntity clipEvent = await db.CreateClipEvent(clipRoute, EventType.Read, clipEventType, createdById);
            clipEvent.Duration = clipEvent.Target.BillingSeconds;
            return clipEvent;
        }
    }
}
