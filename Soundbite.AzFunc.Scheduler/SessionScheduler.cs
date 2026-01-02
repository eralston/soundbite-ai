using AutoMapper;
using Masticore;
using Masticore.Models;
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
    /// Manages session lifecycle
    /// </summary>
    public class SessionScheduler
    {
        private ILifecycleFactory LifecycleFactory { get; }
        private ISbInfrastructure Infrastructure { get; }
        public ILogger Logger { get; set; }
        private IMapper Mapper { get; }

        public SessionScheduler(
            ILifecycleFactory lifecycleFactory,
            ISbInfrastructure infrastructure,
            IMapper mapper,
            ILogger<SessionScheduler> logger)
        {
            LifecycleFactory = lifecycleFactory ?? throw new ArgumentNullException(nameof(lifecycleFactory));
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Queries the database and processes sessions awaiting reminder and deadline
        /// </summary>
        /// <param name="log"></param>
        /// <param name="infrastructure"></param>
        /// <returns></returns>
        public async Task ProcessAsync()
        {
            SbDb db = await Infrastructure.DbAsync();

            SessionEntity[] sessions = await QueryActiveSessionsAsync(db);
            Logger.LogInformation("Processing {count} sessions...", sessions.Length);
            foreach (SessionEntity s in sessions)
            {
                await ProcessSessionAsync(db, s);
            }
        }

        /// <summary>
        /// Processes the given session, applying 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="db"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private async Task ProcessSessionAsync(SbDb db, SessionEntity s)
        {
            try
            {
                Logger.LogInformation("Processing {type} session {route}", Enum.GetName(typeof(SessionType), s.SessionType), s.Route);
                Organization org = s?.Organization == null ? null : Mapper.Map<Organization>(s.Organization);
                ILifecycleStrategy strategy = LifecycleFactory.StrategyForSession(s.SessionType);
                strategy.Logger = Logger;
                await strategy.RemindAsync(db, org, s);
                await strategy.PublishAsync(db, org, s);
                Logger.LogInformation("Successfully processed session {route}", s.Route);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Exception from processing session {s.Route}");
                Logger.LogError(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Queries the sessions pending reminder and/or deadline right now
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private async Task<SessionEntity[]> QueryActiveSessionsAsync(SbDb db)
        {
            DateTime now = Time.UtcNow.AddMinutes(5); // Look slightly ahead
            Logger.LogInformation("Querying for sessions before {now}", now.ToString());
            IQueryable<SessionEntity> query = from
                                            s in db.Sessions
                                              where
                                                  // Active sessions
                                                  s.DeletedUtc == null
                                                  && s.Organization.DeletedUtc == null
                                                  && s.Organization.Tenant.DeletedUtc == null
                                                  // Ignore the Series templates
                                                  && s.IsTemplate == false
                                                  // Ready for reminder or deadline
                                                  && ((s.Reminder != null && s.Reminder <= now && s.ReminderSent == null)
                                                      || (s.Publish != null && s.Publish <= now && s.PublishSent == null))
                                              select s;

            query = SbDbExtensions.IncludeChildren(query);

            SessionEntity[] sessions = await query.ToArrayAsync();
            return sessions;
        }
    }
}
