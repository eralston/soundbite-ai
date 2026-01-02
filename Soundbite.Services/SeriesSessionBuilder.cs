using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Microsoft.EntityFrameworkCore;
using Soundbite.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Given information on a series, maintains the set of 
    /// </summary>
    public class SeriesSessionBuilder
    {
        private const int MaxIterations = 90;

        private SbDb Db { get; }
        private UserEntity CurrentUser { get; }
        private IMapper Mapper { get; }

        public SeriesSessionBuilder(IMapper mapper, SbDb db, UserEntity currentUser = null)
        {
            Validator.ArgNotNull(nameof(mapper), mapper);
            Validator.ArgNotNull(nameof(db), db);

            Db = db;
            CurrentUser = currentUser;
            Mapper = mapper;
        }

        /// <summary>
        /// Finds the next closest future occurrence based on the given initial state of the series and the current DateTime
        /// </summary>
        /// <param name="recurrence">The pattern for repeating the series</param>
        /// <param name="current">The latest dateTime, which is the bases for skipping forward to the next</param>
        /// <returns></returns>
        protected DateTime? NextDateTimeInSeries(Recurrence recurrence, DateTime? startOrNull, int increments)
        {
            if (startOrNull == null)
            {
                return null;
            }

            DateTime start = startOrNull.Value;

            if (recurrence == Recurrence.NoRepeat)
            {
                throw new InvalidOperationException("Cannot calculate next time for a no repeat recurrence");
            }
            else if (recurrence == Recurrence.Daily)
            {
                return start.AddDays(1 * increments);
            }
            else if (recurrence == Recurrence.Weekday)
            {
                return start.AddWeekdays(increments);
            }
            else if (recurrence == Recurrence.Weekly)
            {
                return start.AddDays(7 * increments);
            }
            else if (recurrence == Recurrence.Monthly)
            {
                return start.AddMonths(1 * increments);
            }
            else
            {
                string recurrenceTypeName = Enum.GetName(typeof(Recurrence), recurrence);
                throw new InvalidOperationException($"Cannot find recurrence type {recurrenceTypeName}");
            }
        }

        /// <summary>
        /// Builds the set of sessions for this series from the end of the current session history OR the Template
        /// </summary>
        /// <param name="series">series.Organization, series.Session.Participant.Person, series.Session.ParticipantGroup.Group Required</param>
        /// <param name="strategy"></param>
        /// <param name="isRebuild"></param>
        /// <returns></returns>
        public async Task<SessionEntity[]> BuildAsync(SeriesEntity series, bool isRebuild = false)
        {
            // If we somehow have a series w/o recurrence, then we skip it
            if (series.Recurrence == Recurrence.NoRepeat)
            {
                return null;
            }

            // Optionally clear pending sessions
            // If this is a rebuild, then the series.Template session better have the new starting point
            SessionEntity[] pendingSessions = await Db.FutureSessionsAsync(series.Id);
            if (isRebuild)
            {
                pendingSessions.ForEach(s => s.SoftDelete());
                pendingSessions = new SessionEntity[] { };
            }

            // Check how many we need and abort if we don't need any more
            int desiredFinalCount = MinRecurrence.DesiredMinCount(series);
            int activeCount = pendingSessions.Where(s => s.DeletedUtc == null).Count();
            if (activeCount >= desiredFinalCount)
            {
                return null;
            }

            // NOTE: If this is new or being rebuilt, then we dropped the previous sessions already and we're generating starting at Template
            SessionEntity latest = pendingSessions.LastOrDefault() ?? series.Template;
            return await PopulateAsync(series, latest, desiredFinalCount, activeCount);
        }

        protected async Task<SessionEntity[]> PopulateAsync(SeriesEntity series, SessionEntity latest, int desiredCount, int activeCount)
        {
            // TODO: For now we're going to assume one cannot change the date of a series session
            // We will trust that the latest one is the starting point for the next place in the series
            // In the future, one could most easily implement "editing" series sessions by simply
            // cloning then deleting the session that one wants to clone, but that operation
            // would most likely go into the SessionService
            SessionEntity templateSession = series.Template;

            Recurrence recurrence = series.Recurrence;
            DateTime? reminder = latest.Reminder;
            DateTime? publish = latest.Publish;

            // Generate active sessions based on the template and iterate timing
            OrganizationEntity org = await Db.Organizations.Where(o => o.Route == series.Organization.Route && o.DeletedUtc == null && o.Tenant.DeletedUtc == null).AsNoTracking().FirstOrDefaultAsync();
            org.AssertFound();

            SessionEntityBuilder sessionBuilder = new SessionEntityBuilder(Db, CurrentUser, org.GetSettings());
            List<SessionEntity> newSessions = new List<SessionEntity>();
            int iteration = 0; // Mild paranoia about perhaps running this endlessly due to active and desired being off
            while (activeCount < desiredCount && iteration < MaxIterations)
            {
                DateTime? nextReminder = NextDateTimeInSeries(recurrence, reminder, iteration + 1);
                DateTime? nextPublish = NextDateTimeInSeries(recurrence, publish, iteration + 1);
                ++iteration;

                if (nextPublish < Time.UtcNow)
                {
                    continue;
                }

                // Clone and modify time
                SessionEntity newSession = await sessionBuilder.BuildAsync(Mapper, series.Organization, templateSession);
                newSession.Reminder = nextReminder;
                newSession.Publish = nextPublish;
                newSession.Series = series;

                // Prep for insertion
                newSessions.Add(newSession);

                ++activeCount;
            }

            return newSessions.ToArray();
        }
    }
}
