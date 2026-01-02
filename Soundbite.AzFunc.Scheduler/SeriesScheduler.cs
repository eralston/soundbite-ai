using AutoMapper;
using Masticore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using Soundbite.Services.Lifecycle;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Manages populating future sessions of a series
    /// </summary>
    public class SeriesScheduler
    {
        private ILifecycleFactory LifecycleFactory { get; }
        private ISbInfrastructure Infrastructure { get; }
        private ILogger<SessionScheduler> Logger { get; }
        private IMapper Mapper { get; }

        public bool ReThrowExceptions { get; set; } = false;

        public SeriesScheduler(IMapper mapper, ILifecycleFactory lifecycleFactory, ISbInfrastructure infrastructure, ILogger<SessionScheduler> logger)
        {
            Mapper = mapper;
            LifecycleFactory = lifecycleFactory;
            Infrastructure = infrastructure;
            Logger = logger;
        }

        /// <summary>
        /// Queries the database and processes sessions awaiting reminder and deadline
        /// </summary>
        /// <param name="log"></param>
        /// <param name="infrastructure"></param>
        /// <returns></returns>
        public async Task ProcessAsync()
        {
            // Query the db for series that need more sessions
            SbDb db = await Infrastructure.DbAsync();
            SeriesEntity[] series = await QueryActiveSeriesAsync(db);
            Logger.LogInformation($"Processing {series.Length} series...");

            // Populare sessions for each series
            SeriesSessionBuilder builder = new SeriesSessionBuilder(Mapper, db);
            foreach (SeriesEntity s in series)
            {
                await ProcessSeriesAsync(db, builder, s);
            }
        }

        /// <summary>
        /// Processes the given session, applying 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="db"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        private async Task ProcessSeriesAsync(SbDb db, SeriesSessionBuilder builder, SeriesEntity series)
        {
            try
            {
                Logger.LogInformation("Processing {type} series {route}", Enum.GetName(typeof(SessionType), series.Template.SessionType), series.Route);

                // Populate future sessions and save
                SessionEntity[] newSessions = await builder.BuildAsync(series);
                if (newSessions == null)
                {
                    Logger.LogInformation("No sessions built for series {route}", series.Route);
                    return;
                }
                ILifecycleStrategy strategy = LifecycleFactory.StrategyForSession(series.Template.SessionType);
                await LifecycleAndSaveChangesAsync(db, strategy, series, newSessions);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception from processing series {series.Route}: {e.Message}");
                if (ReThrowExceptions)
                {
                    throw;
                }
            }
        }

        private static async Task LifecycleAndSaveChangesAsync(SbDb db, ILifecycleStrategy strategy, SeriesEntity series, SessionEntity[] newSessions)
        {
            await strategy.BeforeProcessSeriesAsync(db, series);

            foreach (SessionEntity s in newSessions)
            {
                await strategy.BeforeCreateSessionAsync(db, s);
            }

            foreach (SessionEntity s in newSessions)
            {
                await strategy.AfterCreateSessionAsync(db, s);
            }

            await strategy.AfterProcessSeriesAsync(db, series);

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Queries the sessions that are active right now
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private async Task<SeriesEntity[]> QueryActiveSeriesAsync(SbDb db)
        {
            DateTime now = Time.UtcNow;
            Logger.LogInformation("Querying for series needing more sessions before {now}", now.ToString());
            IQueryable<SeriesEntity> query = from
                                            series in db.Series
                                             let sessCount = (from sess in series.Sessions
                                                              where
                                                                 sess.Reminder > now // Anything in the future, including deleted
                                                              select sess).Count()

                                             let needsMoreSessions = (series.Recurrence == Recurrence.Daily && sessCount < MinRecurrence.Daily) ||
                                                                     (series.Recurrence == Recurrence.Weekday && sessCount < MinRecurrence.Weekday) ||
                                                                     (series.Recurrence == Recurrence.Weekly && sessCount < MinRecurrence.Weekly) ||
                                                                     (series.Recurrence == Recurrence.Monthly && sessCount < MinRecurrence.Monthly)
                                             where
                                                 // Active series
                                                 series.DeletedUtc == null
                                                 && series.Organization.DeletedUtc == null
                                                 && series.Organization.Tenant.DeletedUtc == null
                                                 && series.Template != null
                                                 && series.Template.DeletedUtc == null
                                                 && needsMoreSessions
                                             select series;

            // Load up everything
            query = query
                    .Include(s => s.Template).ThenInclude(t => t.Participants).ThenInclude(p => p.Person).ThenInclude(p => p.User)
                    .Include(s => s.Template).ThenInclude(t => t.Groups).ThenInclude(g => g.Group)
                    .Include(s => s.Template).ThenInclude(t => t.Prompts)
                    .Include(s => s.Organization);

            SeriesEntity[] ret = await query.ToArrayAsync();
            return ret;
        }
    }
}
