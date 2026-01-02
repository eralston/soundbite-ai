using AutoMapper;
using Masticore;
using Soundbite.Entity;
using Soundbite.Models;
using System.Data;
using System.Linq;

namespace Soundbite.Services
{
    /// <summary>
    /// Static methods supporting <see cref="ISessionFeedService"/>
    /// </summary>
    public static class SessionFeedExtensions
    {
        /// <summary>
        /// Builds a query for <see cref="SessionPreview"/> objects relevant to the given user in the given org
        /// </summary>
        /// <remarks>This code is repetitious; however, statically declared LINQ is better for performance management</remarks>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <returns></returns>
        public static IQueryable<SessionPreview> QueryPendingOrgSessions(this SbDb db, string orgRoute, string userUniversalId, IMapper mapper)
        {
            IQueryable<SessionPreview> query =
                            from session in db.Sessions

                            let hasParticipantAccess =
                                session.Participants.Any(p =>
                                    p.DeletedUtc == null &&
                                    p.Person.DeletedUtc == null &&
                                    p.Person.User.DeletedUtc == null &&
                                    p.Person.Organization.DeletedUtc == null &&
                                    p.Person.Organization.Tenant.DeletedUtc == null &&
                                     p.Person.User.UniversalId == userUniversalId)

                            let hasOrgAccess =
                                 // Has access as org admin
                                 (from person in db.People
                                  where
                                    person.DeletedUtc == null &&
                                    person.User.DeletedUtc == null &&
                                    person.Organization.DeletedUtc == null &&
                                    person.Organization.Tenant.DeletedUtc == null &&
                                    person.PersonRole >= PersonRole.Admin &&
                                    person.User.UniversalId == userUniversalId &&
                                    person.Organization.Route == orgRoute
                                  select person).Any()

                            let hasSystemAccess =
                                 // Has access as system admin
                                 (from user in db.Users
                                  where
                                    user.DeletedUtc == null &&
                                    user.UserRole >= UserRole.God &&
                                    user.UniversalId == userUniversalId
                                  select user).Any()

                            where
                                session.DeletedUtc == null &&
                                session.IsTemplate == false &&
                                session.Organization.DeletedUtc == null &&
                                session.Organization.Route == orgRoute &&
                                session.Organization.Tenant.DeletedUtc == null &&
                                (hasParticipantAccess || hasOrgAccess || hasSystemAccess) &&
                                // Pending
                                session.Publish != null &&
                                session.PublishSent == null
                            // Most recent on top
                            orderby session.Id descending

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
                                MyParticipants = session.Participants
                                      .Where(p =>
                                          p.DeletedUtc == null &&
                                          p.Person.DeletedUtc == null &&
                                          p.Person.User.DeletedUtc == null &&
                                          p.Person.User.UniversalId == userUniversalId &&
                                          p.Person.Organization.DeletedUtc == null &&
                                          p.Person.Organization.Tenant.DeletedUtc == null &&
                                          p.SessionId == session.Id
                                      )
                                      .Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User))
                            };
            return query;
        }

        /// <summary>
        /// Builds a query for <see cref="SessionPreview"/> objects relevant to the given user in the given group in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <returns></returns>
        public static IQueryable<SessionPreview> QueryPendingGroupSessions(this SbDb db, string orgRoute, string groupRoute, string userUniversalId, IMapper mapper)
        {
            IQueryable<SessionPreview> query =
                            from session in db.Sessions

                            let userIsOwner =
                                 session.Groups.Any(pg =>
                                     pg.DeletedUtc == null &&
                                     pg.Group.DeletedUtc == null &&
                                     pg.Group.Route == groupRoute &&
                                     pg.Group.Members.Any(m =>
                                         m.DeletedUtc == null &&
                                         m.MemberRole >= MemberRole.Owner &&
                                         m.Person.DeletedUtc == null &&
                                         m.Person.User.DeletedUtc == null &&
                                         m.Person.User.UniversalId == userUniversalId
                                        )
                                 )

                            let userIsAdmin =
                                 // Has access as org admin
                                 (from person in db.People
                                  where
                                    person.DeletedUtc == null &&
                                    person.User.DeletedUtc == null &&
                                    person.Organization.DeletedUtc == null &&
                                    person.Organization.Tenant.DeletedUtc == null &&
                                    person.PersonRole >= PersonRole.Admin &&
                                    person.User.UniversalId == userUniversalId &&
                                    person.Organization.Route == orgRoute
                                  select person).Any()

                            let userIsGod =
                                 // Has access as system admin
                                 (from user in db.Users
                                  where
                                    user.DeletedUtc == null &&
                                    user.UserRole >= UserRole.God &&
                                    user.UniversalId == userUniversalId
                                  select user).Any()


                            let sessionHasGroup =
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
                                sessionHasGroup &&
                                (userIsOwner || userIsAdmin || userIsGod) &&
                                // Pending
                                session.Publish != null &&
                                session.PublishSent == null
                            // Most recent on top
                            orderby session.Id descending

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
                                MyParticipants = session.Participants
                                      .Where(p =>
                                          p.DeletedUtc == null &&
                                          p.Person.DeletedUtc == null &&
                                          p.Person.User.DeletedUtc == null &&
                                          p.Person.User.UniversalId == userUniversalId &&
                                          p.Person.Organization.DeletedUtc == null &&
                                          p.Person.Organization.Tenant.DeletedUtc == null &&
                                          p.SessionId == session.Id
                                      )
                                      .Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User))
                            };
            return query;
        }

        /// <summary>
        /// <see cref="SessionPreview"/> objects that were published in the given org relevant to the given user
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static IQueryable<SessionPreview> QueryPastOrgSessions(this SbDb db, string orgRoute, string userUniversalId, IMapper mapper)
        {
            IQueryable<SessionPreview> query =
                            from session in db.Sessions

                            let hasParticipantAccess =
                                session.Participants.Any(p =>
                                    p.DeletedUtc == null &&
                                    p.Person.DeletedUtc == null &&
                                    p.Person.User.DeletedUtc == null &&
                                    p.Person.Organization.DeletedUtc == null &&
                                    p.Person.Organization.Tenant.DeletedUtc == null &&
                                     p.Person.User.UniversalId == userUniversalId)

                            let hasOrgAccess =
                                 // Has access as org admin
                                 (from person in db.People
                                  where
                                    person.DeletedUtc == null &&
                                    person.User.DeletedUtc == null &&
                                    person.Organization.DeletedUtc == null &&
                                    person.Organization.Tenant.DeletedUtc == null &&
                                    person.PersonRole >= PersonRole.Admin &&
                                    person.User.UniversalId == userUniversalId &&
                                    person.Organization.Route == orgRoute
                                  select person).Any()

                            let hasSystemAccess =
                                 // Has access as system admin
                                 (from user in db.Users
                                  where
                                    user.DeletedUtc == null &&
                                    user.UserRole >= UserRole.God &&
                                    user.UniversalId == userUniversalId
                                  select user).Any()

                            where
                                session.DeletedUtc == null &&
                                session.IsTemplate == false &&
                                session.Organization.DeletedUtc == null &&
                                session.Organization.Route == orgRoute &&
                                session.Organization.Tenant.DeletedUtc == null &&
                                (hasParticipantAccess || hasOrgAccess || hasSystemAccess) &&
                                // Past
                                session.PublishSent != null
                            // Most recent on top
                            orderby session.Id descending

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
                                MyParticipants = session.Participants
                                      .Where(p =>
                                          p.DeletedUtc == null &&
                                          p.Person.DeletedUtc == null &&
                                          p.Person.User.DeletedUtc == null &&
                                          p.Person.User.UniversalId == userUniversalId &&
                                          p.Person.Organization.DeletedUtc == null &&
                                          p.Person.Organization.Tenant.DeletedUtc == null &&
                                          p.SessionId == session.Id
                                      )
                                      .Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User))
                            };
            return query;
        }

        /// <summary>
        /// Builds a query for <see cref="SessionPreview"/> objects relevant to the given user in the given group in the given org
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="userUniversalId"></param>
        /// <returns></returns>
        public static IQueryable<SessionPreview> QueryPublishedGroupSessions(this SbDb db, string orgRoute, string groupRoute, string userUniversalId, IMapper mapper)
        {
            IQueryable<SessionPreview> query =
                            from session in db.Sessions

                            let userIsMember =
                                 session.Groups.Any(pg =>
                                     pg.DeletedUtc == null &&
                                     pg.Group.DeletedUtc == null &&
                                     pg.Group.Route == groupRoute &&
                                     pg.Group.Members.Any(m =>
                                         m.DeletedUtc == null &&
                                         m.MemberRole >= MemberRole.Member &&
                                         m.Person.DeletedUtc == null &&
                                         m.Person.User.DeletedUtc == null &&
                                         m.Person.User.UniversalId == userUniversalId
                                        )
                                 )

                            let userIsAdmin =
                                 // Has access as org admin
                                 (from person in db.People
                                  where
                                    person.DeletedUtc == null &&
                                    person.User.DeletedUtc == null &&
                                    person.Organization.DeletedUtc == null &&
                                    person.Organization.Tenant.DeletedUtc == null &&
                                    person.PersonRole >= PersonRole.Admin &&
                                    person.User.UniversalId == userUniversalId &&
                                    person.Organization.Route == orgRoute
                                  select person).Any()

                            let userIsGod =
                                 // Has access as system admin
                                 (from user in db.Users
                                  where
                                    user.DeletedUtc == null &&
                                    user.UserRole >= UserRole.God &&
                                    user.UniversalId == userUniversalId
                                  select user).Any()

                            let sessionHasGroup =
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
                                sessionHasGroup &&
                                (userIsMember || userIsAdmin || userIsGod) &&
                                // Past
                                session.PublishSent != null
                            // Most recent on top
                            orderby session.Id descending

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
                                MyParticipants = session.Participants
                                      .Where(p =>
                                          p.DeletedUtc == null &&
                                          p.Person.DeletedUtc == null &&
                                          p.Person.User.DeletedUtc == null &&
                                          p.Person.User.UniversalId == userUniversalId &&
                                          p.Person.Organization.DeletedUtc == null &&
                                          p.Person.Organization.Tenant.DeletedUtc == null &&
                                          p.SessionId == session.Id
                                      )
                                      .Select(p => mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User))
                            };
            return query;
        }
    }
}
