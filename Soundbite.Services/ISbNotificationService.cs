using Masticore.Models;
using Masticore.Services;
using Soundbite.Entity;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Soundbite-specific notifications
    /// </summary>
    public interface ISbNotificationService : IService
    {
        /// <summary>
        /// Sends an invite to the given
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="org"></param>
        /// <param name="session"></param>
        /// <returns>Task indicating completion or failure of the operation.</returns>
        Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session);

        /// <summary>
        /// Sends a publish notification to the given person
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="org"></param>
        /// <param name="session"></param>
        /// <returns>Task indicating completion or failure of the operation.</returns>
        Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session);

        /// <summary>
        /// A notification for the host(s) of the session
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="org"></param>
        /// <param name="session"></param>
        /// <returns>Task indicating completion or failure of the operation.</returns>
        Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session);
    }
}
