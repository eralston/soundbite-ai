using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Exceptions;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Services.Lifecycle;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Services for interacting with recurring sessions AKA Series
    /// </summary>
    public class SeriesService : InfrastructureServiceBase<ISbInfrastructure>, ISeriesService
    {
        protected ILifecycleFactory LifecycleFactory { get; }

        public SeriesService(
            ISecurityContext securityContext,
            ILifecycleFactory lifecycleFactory,
            ISbInfrastructure infrastructure,
            IMapper mapper,
            ILogger<SeriesService> logger)
            : base(infrastructure, logger, securityContext, mapper)
        {
            LifecycleFactory = lifecycleFactory;
        }

        /// <summary>
        /// Static method for creating sessions, which is called from outsiders
        /// </summary>
        /// <param name="db"></param>
        public async Task<SeriesDetails> CreateAsync(string orgRoute, NewSession newSession)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(newSession), newSession);

            // Setup calling
            SbDb db = await Infrastructure.DbAsync();

            // TODO: This allows anyone to create a series, perhaps this should be configurable
            UserEntity currentUser = await db.UserWhereUid(orgRoute, SecurityContext.UniversalId, PersonRole.Person);
            currentUser.AssertFound();

            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);

            // Generate template session
            SessionEntityBuilder builder = new SessionEntityBuilder(db, currentUser);
            SessionEntity templateSession = await builder.BuildAsync(org, newSession);
            templateSession.IsTemplate = true;
            // NOTE: Template session does NOT have a backlink to series
            // This is due to restrictions on circular references

            // Generate series
            SeriesEntity series = db.Series.CreateResource(currentUser);
            // Fields
            series.Name = newSession.Name;
            series.Recurrence = newSession.Recurrence;
            series.RecurrenceData = newSession.RecurrenceData;
            // Relationships
            series.Template = templateSession;
            series.Organization = org;

            // Populate future sessions
            SeriesSessionBuilder sessionBuilder = new SeriesSessionBuilder(Mapper, db, currentUser);
            SessionEntity[] newSessions = await sessionBuilder.BuildAsync(series);

            // Run the lifecycle and save it
            ILifecycleStrategy strategy = LifecycleFactory.StrategyForSession(newSession.SessionType);
            await LifecycleAndSaveAsync(db, series, strategy, newSessions);

            // Return the full series information
            SeriesDetails result = await QuerySeriesDetails(db, false, orgRoute, series.Route);
            return result;
        }

        private static async Task LifecycleAndSaveAsync(SbDb db, SeriesEntity series, ILifecycleStrategy strategy, SessionEntity[] newSessions)
        {
            ClientOpCollection clientOps = new ClientOpCollection();
            foreach (SessionEntity s in newSessions)
            {
                await strategy.BeforeCreateSessionAsync(db, s);
            }

            await strategy.BeforeCreateSeriesAsync(db, series);

            await db.SaveChangesAsync();

            await strategy.AfterCreateSeriesAsync(db, series);

            foreach (SessionEntity s in newSessions)
            {
                await strategy.AfterCreateSessionAsync(db, s);
            }
        }

        public async Task<IEnumerable<SeriesPreview>> ReadAllAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            // Setup calling
            SbDb db = await Infrastructure.DbAsync();

            // Confirm they are in the org
            UserEntity currentUser = await db.UserWhereUid(orgRoute, SecurityContext.UniversalId, PersonRole.Person);
            currentUser.AssertFound();

            int[] groupIds = await db.GroupIdsAsync(orgRoute, SecurityContext.UniversalId);
            return await db.SeriesPreviewAsync(SecurityContext, groupIds, Mapper, orgRoute);
        }

        public async Task<IEnumerable<SeriesPreview>> ReadAllAsync_Obsolete(string orgRoute, string grpRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(grpRoute), grpRoute);

            // Setup calling
            SbDb db = await Infrastructure.DbAsync();

            // Confirm they are in the org
            (await db.UserWhereUid(orgRoute, SecurityContext.UniversalId, PersonRole.Person)).AssertFound();

            int[] groupIds = await db.GroupIdsAsync(orgRoute, SecurityContext.UniversalId);
            return await db.SeriesPreviewAsync(SecurityContext, groupIds, Mapper, orgRoute, grpRoute);
        }

        public async Task<SeriesDetails> ReadAsync(string orgRoute, string seriesRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(seriesRoute), seriesRoute);

            // Setup calling
            SbDb db = await Infrastructure.DbAsync();

            // Confirm they are in the org
            (await db.UserWhereUid(orgRoute, SecurityContext.UniversalId, PersonRole.Person)).AssertFound();

            // Read out the series
            // NOTE: This does NOT check if they have host/admin privileges right now
            return await QuerySeriesDetails(db, false, orgRoute, seriesRoute);
        }

        private async Task<SeriesDetails> QuerySeriesDetails(SbDb db, bool isPublic, string orgRoute, string seriesRoute, ParticipantRole minRole = ParticipantRole.Unknown)
        {
            int[] groupIds = await db.GroupIdsAsync(orgRoute, SecurityContext.UniversalId);
            IQueryable<SeriesDetailsWithMetadata> query = db.QuerySeriesDetails(SecurityContext, groupIds, orgRoute, seriesRoute, minRole, Mapper).AsNoTracking();
            SeriesDetailsWithMetadata details = await query.FirstOrDefaultAsync();
            if (details is null)
            {
                throw new NotFoundException($"Could no find series {seriesRoute} in org {orgRoute}");
            }

            SeriesDetails ret = details.SeriesDetails;

            // TODO: Consider recycling groupIds here
            ret.Template = await db.ReadOnlySessionDetailsAsync(SecurityContext.UniversalId, orgRoute, details.TemplateSessionRoute, isPublic, Mapper, true);
            ret.Template.AssertFound($"Could not find template session for series {seriesRoute} in org {orgRoute}");

            return ret;
        }

        public async Task<SeriesDetails> UpdateAsync(string orgRoute, string seriesRoute, NewSession newSession)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(seriesRoute), seriesRoute);
            Validator.ArgNotNull(nameof(newSession), newSession);

            // Setup calling
            SbDb db = await Infrastructure.DbAsync();

            // Confirm they are in the org
            UserEntity currentUser = await db.UserWhereUid(orgRoute, SecurityContext.UniversalId, PersonRole.Person);
            currentUser.AssertFound();

            // Pull the series with children, confirming they're host or equivalent
            SeriesEntity series = await db.SeriesAsync(SecurityContext.UniversalId, orgRoute, seriesRoute, ParticipantRole.Host, true);

            // Series update
            series.Name = newSession.Name;
            series.Recurrence = newSession.Recurrence;
            series.RecurrenceData = newSession.RecurrenceData;

            // Template session update
            series.Template.Name = newSession.Name;

            // TODO: Merge participants

            SeriesSessionBuilder sessionBuilder = new SeriesSessionBuilder(Mapper, db, currentUser);
            SessionEntity[] newSessions = await sessionBuilder.BuildAsync(series, true);

            // Run the lifecycle and save it
            ILifecycleStrategy strategy = LifecycleFactory.StrategyForSession(newSession.SessionType);
            await LifecycleAndSaveAsync(db, series, strategy, newSessions);

            // Return the complete details now in the database
            return await ReadAsync(orgRoute, seriesRoute);
        }

        public async Task DeleteAsync(string orgRoute, string seriesRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(seriesRoute), seriesRoute);

            // Setup calling
            SbDb db = await Infrastructure.DbAsync();

            // Confirm they are in the org
            UserEntity currentUser = await db.UserWhereUid(orgRoute, SecurityContext.UniversalId, PersonRole.Person);
            currentUser.AssertFound();

            // Do a query with RBAC; if not found, then it will abort with exception
            SeriesEntity series = await db.SeriesAsync(SecurityContext.UniversalId, orgRoute, seriesRoute, ParticipantRole.Host);
            // If we go here, then it's good to pull all the active sessions and delete them as well
            SessionEntity[] sessions = await db.FutureSessionsAsync(series.Id);
            sessions.ForEach(s => s.SoftDelete());
            series.SoftDelete();
            await db.SaveChangesAsync();
        }
    }
}
