using Masticore;
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
    public class BillingService : InfrastructureServiceBase<ISbInfrastructure>, IBillingService
    {
        /// <summary>
        /// Gets the <see cref="IRbac"/> object for this service, which controls access
        /// </summary>
        protected IRbac Rbac { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BillingService(
            ISbInfrastructure infrastructure,
            ILogger<BillingService> logger,
            IRbac rbac)
            : base(infrastructure, logger)
        {
            Rbac = rbac;
        }

        #region Methods

        protected Task<int> ConsumerCountAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            // TODO: Triangulate this count by checking for consumed prompts as well
            return db.QueryClipEvents(orgRoute, startUtc, endUtc)
                        .AsNoTracking()
                        .Where(c => c.ClipEventType == ClipEventType.ServerShareLinkPublic ||
                                    c.ClipEventType == ClipEventType.ServerShareLinkSecure)
                        .Select(c => c.CreatedById)
                        .Distinct()
                        .CountAsync();
        }

        protected async Task<long> UnitsConsumedAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            // TODO: Triangulate this count by checking for consumed prompts as well
            long consumedSeconds = await db.QueryClipEvents(orgRoute, startUtc, endUtc)
                        .AsNoTracking()
                        .Where(c => c.ClipEventType == ClipEventType.ServerShareLinkPublic ||
                                    c.ClipEventType == ClipEventType.ServerShareLinkSecure)
                        .Select(c => c.Duration)
                        .SumAsync();
            long consumedMinutes = (long)Math.Ceiling(consumedSeconds / 60.0);
            return consumedMinutes;
        }

        protected static Task<int> ProducerCountAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            return db.QueryClips(orgRoute, startUtc, endUtc)
                    .AsNoTracking()
                    .Select(c => c.CreatedById)
                    .Distinct()
                    .CountAsync();
        }

        protected static async Task<long> UnitsProducedAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            long producedSeconds = await db.QueryClips(orgRoute, startUtc, endUtc)
                .AsNoTracking()
                .SumAsync(c => c.BillingSeconds);
            long producedMinutes = (long)Math.Ceiling(producedSeconds / 60.0);
            return producedMinutes;
        }

        protected static Task<ActivityHighlight[]> HighlightsAsync(SbDb db, string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            // TODO
            return Task.FromResult(new ActivityHighlight[] { });
        }


        #endregion

        #region IBillingService

        /// <summary>
        /// Generates a <see cref="ActivityReport"/> for the given org in the given time period
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <returns></returns>
        public async Task<ActivityReport> ActivityAsync(string orgRoute, DateTime startUtc, DateTime endUtc)
        {
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();
            ActivityReport ret = new ActivityReport
            {
                // Headers
                Name = "Activity",
                StartUtc = startUtc,
                EndUtc = endUtc,

                // Production
                UnitsProduced = await UnitsProducedAsync(db, orgRoute, startUtc, endUtc),
                ProducerCount = await ProducerCountAsync(db, orgRoute, startUtc, endUtc),

                // Consumption
                UnitsConsumed = await UnitsConsumedAsync(db, orgRoute, startUtc, endUtc),
                ConsumerCount = await ConsumerCountAsync(db, orgRoute, startUtc, endUtc),

                // Modular highlights potentially unique to this org
                Highlights = await HighlightsAsync(db, orgRoute, startUtc, endUtc)
            };
            return ret;
        }

        #endregion
    }
}
