using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Sends notifications to users via channels like email, text, and bots
    /// NOTE: This does NOT implement RBAC
    /// </summary>
    public interface INotificationService : IService
    {
        /// <summary>
        /// Sends a friendly greeting for the user once they activate their Soundbite account, joining the platform
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        Task<bool> WelcomeAsync(User receiver);

        /// <summary>
        /// Sends an invite to the receiver from the sender, telling them about Soundbite on the notion the new user will be joining in their own organization
        /// AKA Referral
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        Task<bool> UserInviteAsync(User receiver, User sender);

        /// <summary>
        /// Sends an invite to the receiver from the sender, telling them about Soundbite with the intention of them joining the given organization
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="sender"></param>
        /// <param name="org"></param>
        /// <param name="isAppInvite"></param>
        /// <returns></returns>
        Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isAppInvite);

        /// <summary>
        /// Sends an invite to the receiver from the sender, telling them about Soundbite and inviting them into the given team in the given org
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="sender"></param>
        /// <param name="org"></param>
        /// <param name="group"></param>
        /// <param name="isAppInvite"></param>
        /// <returns></returns>
        Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group group, bool isAppInvite);
    }
}
