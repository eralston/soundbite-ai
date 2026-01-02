using Masticore.Services;
using Soundbite.Models;
using System;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// General-purpose reporting service
    /// </summary>
    public interface IReportService : IService
    {
        /// <summary>
        /// Reporting on the content for an org, such as the amount of content created and consumed, etc
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <returns></returns>
        Task<OrgContentReport> OrgContentReportAsync(string orgRoute, DateTime startUtc, DateTime endUtc);

        /// <summary>
        /// Reporting on a single session's engagement
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        Task<SessionContentReport> SessionContentReportAsync(string orgRoute, string sessionRoute);

        /// <summary>
        /// Report on a single sessions' detailed engagement data
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        Task<SessionContentDetailsReport> SessionContentDetailsReportAsync(string orgRoute, string sessionRoute);
    }
}
