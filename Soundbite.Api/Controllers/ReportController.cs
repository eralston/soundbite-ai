using Masticore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Models;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// General-purpose reporting controler
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        #region Properties

        /// <summary>
        /// <see cref="IReportService"/> for this current request
        /// </summary>
        protected IReportService ReportService { get; }

        /// <summary>
        /// <see cref="IBillingService"/> for the current request
        /// </summary>
        protected IBillingService BillingService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reportService"></param>
        /// <param name="billingService"></param>
        public ReportController(IReportService reportService, IBillingService billingService)
        {
            ReportService = reportService;
            BillingService = billingService;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Returns a <see cref="ActivityReport"/> loaded for the given org and timespan
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/reports/activity")]
        [HttpPost]
        public async Task<ActivityReport> ActivityReportAsync(
            string orgRoute,
            [CodeGenField(IsNullable = true)]
            DateTime? startUtc = null,
            [CodeGenField(IsNullable = true)]
            DateTime? endUtc = null)
        {
            if (!startUtc.HasValue)
            {
                startUtc = Time.UtcNow.AddDays(-30);
            }

            if (!endUtc.HasValue)
            {
                endUtc = Time.UtcNow;
            }

            ActivityReport report = await BillingService.ActivityAsync(orgRoute, startUtc.Value, endUtc.Value);
            return report;
        }

        /// <summary>
        /// Async gets the <see cref="OrgContentReport"/> for the given org and time period
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/reports/content")]
        [HttpPost]
        public async Task<OrgContentReport> ContentReportAsync(
            string orgRoute,
            [CodeGenField(IsNullable = true)]
            DateTime? startUtc = null,
            [CodeGenField(IsNullable = true)]
            DateTime? endUtc = null)
        {
            if (!startUtc.HasValue)
            {
                startUtc = Time.UtcNow.AddDays(-30);
            }

            if (!endUtc.HasValue)
            {
                endUtc = Time.UtcNow;
            }

            OrgContentReport report = await ReportService.OrgContentReportAsync(orgRoute, startUtc.Value, endUtc.Value);
            return report;
        }

        /// <summary>
        /// Async get the <see cref="SessionContentReport"/> for the given session in the given org
        /// </summary>
        /// <remarks>This is ALWAYS for all time on the report; we may implement time limitation or group by in the future</remarks>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sessions/{sessionRoute}/reports/content")]
        [HttpPost]
        public async Task<SessionContentReport> SessionReportAsync(
            string orgRoute,
            string sessionRoute)
        {
            SessionContentReport report = await ReportService.SessionContentReportAsync(orgRoute, sessionRoute);
            return report;
        }

        /// <summary>
        /// Async get the <see cref="SessionContentDetailsReport"/> for the given session in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/sessions/{sessionRoute}/reports/content/details")]
        [HttpPost]
        public async Task<SessionContentDetailsReport> SessionDetailsReportAsync(
            string orgRoute,
            string sessionRoute)
        {
            SessionContentDetailsReport report = await ReportService.SessionContentDetailsReportAsync(orgRoute, sessionRoute);
            return report;
        }

        #endregion
    }
}