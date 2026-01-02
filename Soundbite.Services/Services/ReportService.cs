using AutoMapper;
using Masticore;
using Masticore.Exceptions;
using Masticore.Security;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Implement <see cref="IBillingService"/> over EF Core
    /// </summary>
    public class ReportService : InfrastructureServiceBase<ISbInfrastructure>, IReportService
    {
        public const string SessionCountTitle = "Sessions";
        public const string SeriesCountTitle = "Series";
        public const string ConsumeCountTitle = "Plays";
        public const string AcknowledgeCountTitle = "Acknowledgements";

        /// <summary>
        /// Gets the <see cref="IRbac"/> object for this service, which controls access
        /// </summary>
        protected IRbac Rbac { get; }

        protected ISessionService Sessions { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReportService(
            ISbInfrastructure infrastructure,
            ILogger<ReportService> logger,
            IRbac rbac,
            ISessionService sessions,
            IMapper mapper)
            : base(infrastructure, logger, null, mapper)
        {
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        }

        #region Methods

        private static IQueryable<ClipEventEntity> QueryConsumedClipEvents(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc, string sessionRoute = null)
        {
            IQueryable<ClipEventEntity> query = db.QueryClipEvents(orgRoute, startUtc, endUtc)
                        .AsNoTracking()
                        .Where(c => c.ClipEventType == ClipEventType.ServerShareLinkPublic ||
                                    c.ClipEventType == ClipEventType.ServerShareLinkSecure);

            if (sessionRoute != null)
            {
                query = query.Where(c => c.Target.Prompt.Session.Route == sessionRoute);
            }

            return query;
        }

        protected static Task<ActivityHighlight[]> OrgHighlights(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            // TODO: Think about some dynamic highlights
            return Task.FromResult(new ActivityHighlight[] { });
        }

        protected static async Task<ActivityHighlight> AcknowledgementCountHighlightAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc, string sessionRoute = null)
        {
            IQueryable<ParticipantEntity> query = ParticipantsQuery(db, orgRoute, startUtc, endUtc, sessionRoute);
            query = query.Where(p => p.ParticipantState == ParticipantState.Consumed);

            return await AcknowledgementsHighlight(query);
        }

        private static async Task<ActivityHighlight> AcknowledgementsHighlight(IQueryable<ParticipantEntity> query)
        {
            long count = await query
                                            .CountAsync();
            ActivityHighlight ret = new ActivityHighlight
            {
                Title = AcknowledgeCountTitle,
                Subtitle = "Number of session acknowledgements",
                Value = count.ToString()
            };
            return ret;
        }

        private static IQueryable<ParticipantEntity> ParticipantsQuery(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc, string sessionRoute)
        {
            IQueryable<ParticipantEntity> query = db.Participants
                                                        .Where(p =>
                                                            // NOTE: This purposefully ignores deleted sessions
                                                            p.Session.Organization.Route == orgRoute &&
                                                            startUtc <= p.UpdatedUtc &&
                                                            p.UpdatedUtc <= endUtc
                                                        );

            if (sessionRoute != null)
            {
                query = query.Where(p => p.Session.Route == sessionRoute);
            }

            return query;
        }

        private async Task<ParticipantGroup[]> ParticipantGoups(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc, string sessionRoute)
        {
            IQueryable<ParticipantGroupEntity> query = db.ParticipantGroups
                                                        .AsNoTracking()
                                                        .Where(p =>
                                                            // NOTE: This purposefully ignores deleted sessions
                                                            p.Session.Organization.Route == orgRoute &&
                                                            startUtc <= p.UpdatedUtc &&
                                                            p.UpdatedUtc <= endUtc
                                                        )
                                                        .Include(pg => pg.Group);

            if (sessionRoute != null)
            {
                query = query.Where(p => p.Session.Route == sessionRoute);
            }

            ParticipantGroupEntity[] groups = await query.ToArrayAsync();
            return groups.Select(pg => Mapper.MapSafeWith<ParticipantGroup>(pg, pg.Group)).ToArray();
        }

        protected static async Task<ActivityHighlight> SeriesCountHighlightAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            long count = await QueryConsumedClipEvents(db, orgRoute, startUtc, endUtc)
                    .Select(c => c.Target.Prompt.Session.SeriesId)
                    .Distinct()
                    .CountAsync();

            ActivityHighlight ret = new ActivityHighlight
            {
                Title = SeriesCountTitle,
                Subtitle = "Number of series consumed",
                Value = count.ToString()
            };
            return ret;
        }

        protected static async Task<ActivityHighlight> SessionCountHighlightAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            long count = await QueryConsumedClipEvents(db, orgRoute, startUtc, endUtc)
                    .Select(c => c.Target.Prompt.SessionId)
                    .Distinct()
                    .CountAsync();
            ActivityHighlight ret = new ActivityHighlight
            {
                Title = SessionCountTitle,
                Subtitle = "The number of sessions consumed",
                Value = count.ToString()
            };
            return ret;
        }

        protected static async Task<ActivityRange> SessionCountOverTimeAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            long count = await QueryConsumedClipEvents(db, orgRoute, startUtc, endUtc)
                    .Select(c => c.Target.Prompt.SessionId)
                    .Distinct()
                    .CountAsync();

            var query = await db.Sessions
                            .Where(s =>
                                s.DeletedUtc == null &&
                                s.Organization.DeletedUtc == null &&
                                s.Organization.Route == orgRoute &&
                                s.PublishSent != null &&
                                startUtc <= s.PublishSent &&
                                s.PublishSent <= endUtc)
                            .GroupBy(s => s.PublishSent.Value.Date)
                            .Select(c => new
                            {
                                Date = c.Key,
                                Value = c.Count()
                            })
                            .OrderBy(c => c.Date)
                            .ToArrayAsync();
            ActivityRange ret = new ActivityRange
            {
                Items = query.Select(i => new ActivityRangeItem { Label = i.Date.ToString("o"), Value = i.Value }),
                Title = "Sessions Over Time"
            };
            return ret;
        }

        protected static async Task<ActivityHighlight> ConsumeCountHighlightAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc, string sessionRoute = null)
        {
            int sessionCount = await QueryConsumedClipEvents(db, orgRoute, startUtc, endUtc, sessionRoute)
                                    .AsNoTracking()
                                    .CountAsync();
            ActivityHighlight ret = new ActivityHighlight
            {
                Title = ConsumeCountTitle,
                Subtitle = "The number of listens",
                Value = sessionCount.ToString()
            };
            return ret;
        }

        protected static async Task<ActivityRange> ConsumeCountOverTime(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc, string sessionRoute = null)
        {
            var query = await QueryConsumedClipEvents(db, orgRoute, startUtc, endUtc, sessionRoute)
                                   .GroupBy(c => c.CreatedUtc.Date)
                                   .Select(c => new
                                   {
                                       Date = c.Key,
                                       Value = c.Count()
                                   })
                                   .OrderBy(c => c.Date)
                                   .ToArrayAsync();
            ActivityRange ret = new ActivityRange
            {
                Items = query.Select(i => new ActivityRangeItem { Label = i.Date.ToString("o"), Value = i.Value }),
                Title = "Listens Over Time"
            };
            return ret;
        }

        /// <summary>
        /// Convert the given query to a collection of <see cref="SessionContentPersonEvent"/>
        /// </summary>
        /// <param name="acknowledgersQuery"></param>
        /// <returns></returns>
        /// <remarks>The value for <see cref="SessionContentPersonEvent.DateTimeUtc"/> is not guaranteed to be accurate</remarks>
        private async Task<SessionContentPersonEvent[]> AcknowledgerPeople(IQueryable<ParticipantEntity> acknowledgersQuery)
        {
            IQueryable<ParticipantEntity> query = acknowledgersQuery.Include(p => p.Person).ThenInclude(p => p.User);
            ParticipantEntity[] result = await query.ToArrayAsync();
            SessionContentPersonEvent[] ret = result.Select(p => new SessionContentPersonEvent
            {
                UserRoute = p.Person.User.Route,
                Email = p.Person.User.Email,
                FamilyName = p.Person.User.FamilyName,
                GivenName = p.Person.User.GivenName,
                Phone = p.Person.User.Phone,
                Title = p.Person.User.Title,
                // No UniversalId
                // We're guessing on the DateTime, might not be accurate
                DateTimeUtc = p.UpdatedUtc
            }).ToArray();

            return ret;
        }

        private async Task<SessionContentPersonEvent[]> PlaysAsync(IQueryable<ClipEventEntity> clipQuery)
        {
            IQueryable<ClipEventEntity> query = clipQuery.Include(e => e.CreatedBy);
            ClipEventEntity[] result = await query.ToArrayAsync();
            SessionContentPersonEvent[] ret = result
                .Where(e => e != null && e.CreatedBy != null)
                .Select(e => new SessionContentPersonEvent
                {
                    UserRoute = e.CreatedBy.Route,
                    Email = e.CreatedBy.Email,
                    FamilyName = e.CreatedBy.FamilyName,
                    GivenName = e.CreatedBy.GivenName,
                    Phone = e.CreatedBy.Phone,
                    Title = e.CreatedBy.Title,
                    // No UniversalId
                    DateTimeUtc = e.CreatedUtc
                }).ToArray();

            return ret;
        }

        private async Task<SessionContentNotificiation[]> NotificationsAsync(SbDb db, string orgRoute, string sessionRoute)
        {
            IQueryable<SessionContentNotificiation> query = db.SessionNotifications
                                    .Where(sn =>
                                        sn.DeletedUtc == null &&
                                        sn.Session.DeletedUtc == null &&
                                        sn.Session.Route == sessionRoute &&
                                        sn.Session.Organization.DeletedUtc == null &&
                                        sn.Session.Organization.Route == orgRoute &&
                                        sn.Session.Organization.Tenant.DeletedUtc == null)
                                    .AsNoTracking()
                                    .Include(sn => sn.CreatedBy)
                                    .Select(sn => new SessionContentNotificiation
                                    {
                                        UserRoute = sn.User.Route,
                                        Email = sn.User.Email,
                                        FamilyName = sn.User.FamilyName,
                                        GivenName = sn.User.GivenName,
                                        Phone = sn.User.Phone,
                                        Title = sn.User.Title,
                                        // No UniversalId or UPN
                                        DateTimeUtc = sn.CreatedUtc,
                                        // Types
                                        NotificationType = sn.NotificationType,
                                        Channel = sn.Channel,
                                        Status = sn.Status,
                                        Details = sn.Details,
                                    });

            SessionContentNotificiation[] ret = await query.ToArrayAsync();
            return ret;
        }

        #endregion

        #region IReportService

        /// <summary>
        /// Reporting on the key content for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <returns></returns>
        public async Task<OrgContentReport> OrgContentReportAsync(string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();
            OrgContentReport ret = new OrgContentReport
            {
                // Headers
                OrgRoute = orgRoute,
                Name = "Content",
                StartUtc = startUtc,
                EndUtc = endUtc,

                // Metrics
                SessionsCount = await SessionCountHighlightAsync(db, orgRoute, startUtc, endUtc),
                SessionsCountOverTime = await SessionCountOverTimeAsync(db, orgRoute, startUtc, endUtc),
                SeriesCount = await SeriesCountHighlightAsync(db, orgRoute, startUtc, endUtc),

                ConsumeCount = await ConsumeCountHighlightAsync(db, orgRoute, startUtc, endUtc),
                ConsumeCountOverTime = await ConsumeCountOverTime(db, orgRoute, startUtc, endUtc),
                AcknowledgeCount = await AcknowledgementCountHighlightAsync(db, orgRoute, startUtc, endUtc),

                // Modular highlights potentially unique to this org
                Highlights = await OrgHighlights(db, orgRoute, startUtc, endUtc),

                // Key Content
                // TODO: Convert arguments to standardized paging version with a reasonable take limit
                RecentSessions = await Sessions.ReadRecentlyPublishedAsync_Obsolete(orgRoute),
            };
            return ret;
        }

        public async Task<SessionContentReport> SessionContentReportAsync(string orgRoute, string sessionRoute)
        {
            SbDb db = await Infrastructure.DbAsync();

            SessionParticipantSummary summary = await db.SessionParticipantSummary(orgRoute, sessionRoute);
            if (summary == null)
            {
                throw new NotFoundException($"Could not find session '{sessionRoute}' in org '{orgRoute}'");
            }

            SessionContentReport ret = new SessionContentReport
            {
                // Headers
                OrgRoute = orgRoute,
                Name = $"{summary.Session.Name} Engagement",
                StartUtc = DateTime.MinValue,
                EndUtc = DateTime.MaxValue,

                SessionRoute = sessionRoute,

                // Metrics
                AcknowledgeCount = await AcknowledgementCountHighlightAsync(db, orgRoute, DateTime.MinValue, DateTime.MaxValue, sessionRoute),
                AudienceSize = new ActivityHighlight { Title = "Audience", Value = summary.AudienceCount.ToString() },
                ConsumeCount = new ActivityHighlight { Title = "Plays", Value = summary.ListensCount.ToString() },
                ConsumeCountOverTime = await ConsumeCountOverTime(db, orgRoute, DateTime.MinValue, DateTime.MaxValue, sessionRoute),
                ConsumerCount = new ActivityHighlight { Title = "Consumers", Value = summary.ListenersCount.ToString() },
                Highlights = new ActivityHighlight[] { },
            };
            return ret;
        }

        public async Task<SessionContentDetailsReport> SessionContentDetailsReportAsync(string orgRoute, string sessionRoute)
        {
            // Require admin, because this has PII in the result
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();

            SessionParticipantSummary summary = await db.SessionParticipantSummary(orgRoute, sessionRoute);
            if (summary == null)
            {
                throw new NotFoundException($"Could not find session '{sessionRoute}' in org '{orgRoute}'");
            }

            IQueryable<ParticipantEntity> participantsQuery = ParticipantsQuery(db, orgRoute, DateTime.MinValue, DateTime.MaxValue, sessionRoute);
            IQueryable<ParticipantEntity> audienceQuery = participantsQuery.Include(p => p.Person).ThenInclude(p => p.User);
            ParticipantEntity[] audience = await audienceQuery.ToArrayAsync();
            IQueryable<ParticipantEntity> acknowledgersQuery = participantsQuery.Where(p => p.ParticipantState == ParticipantState.Consumed);
            SessionContentPersonEvent[] acknowledgers = await AcknowledgerPeople(acknowledgersQuery);
            ActivityHighlight activityHighlight = await AcknowledgementsHighlight(acknowledgersQuery);
            IQueryable<ClipEventEntity> clipQuery = QueryConsumedClipEvents(db, orgRoute, DateTime.MinValue, DateTime.MaxValue, sessionRoute);
            SessionContentPersonEvent[] plays = await PlaysAsync(clipQuery);
            SessionContentDetailsReport ret = new SessionContentDetailsReport
            {
                // Headers
                OrgRoute = orgRoute,
                Name = $"{summary.Session.Name} Engagement",
                StartUtc = DateTime.MinValue,
                EndUtc = DateTime.MaxValue,

                SessionRoute = sessionRoute,

                // Metrics
                AcknowledgeCount = activityHighlight,
                AudienceSize = new ActivityHighlight { Title = "Audience", Value = summary.AudienceCount.ToString() },
                ConsumeCount = new ActivityHighlight { Title = "Plays", Value = summary.ListensCount.ToString() },
                ConsumeCountOverTime = await ConsumeCountOverTime(db, orgRoute, DateTime.MinValue, DateTime.MaxValue, sessionRoute),
                ConsumerCount = new ActivityHighlight { Title = "Consumers", Value = summary.ListenersCount.ToString() },
                Highlights = new ActivityHighlight[] { },
                Notifications = await NotificationsAsync(db, orgRoute, sessionRoute),

                // People
                Acknowledgers = acknowledgers,
                Plays = plays,
                AudienceGroups = await ParticipantGoups(db, orgRoute, DateTime.MinValue, DateTime.MaxValue, sessionRoute),
                Audience = audience.Select(p => Mapper.MapSafeWith<Participant>(p, p.Person, p.Person.User)).ToArray(),

            };
            return ret;
        }

        #endregion
    }
}
