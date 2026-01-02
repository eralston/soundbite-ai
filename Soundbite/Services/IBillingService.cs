using Masticore.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Provides reporting and actions relevant to the purchace and maintenance of the billing system, including reporting back consumption and calculating costs
    /// </summary>
    public interface IBillingService : IService
    {
        /// <summary>
        /// Analyze the activity for the given org in the given time period and summarize it as a <see cref="ActivityReport"/> object
        /// </summary>
        /// <remarks>
        /// This is an *indication* of billing, but not the same as an invoice since it is just a raw dump
        /// </remarks>
        /// <param name="orgRoute"></param>
        /// <param name="startUtc"></param>
        /// <param name="endUtc"></param>
        /// <returns></returns>
        Task<ActivityReport> ActivityAsync(string orgRoute, DateTime startUtc, DateTime endUtc);
    }
}
