using Masticore.Models;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// <see href="https://en.wikipedia.org/wiki/Composite_pattern">Composite Pattern</see> around instances of <see cref="ICombinedNotificationService"/>
    /// </summary>
    /// <remarks>This must be chained into the DI system via a factory</remarks>
    public class NotificationComposite : ICombinedNotificationService
    {
        #region Properties

        protected ICombinedNotificationService[] Inner { get; }
        protected ILogger Logger { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emails"></param>
        /// <param name="sms"></param>
        /// <param name="infrastructure"></param>
        /// <param name="rbac"></param>
        public NotificationComposite(
            ILogger<NotificationComposite> logger,
            params ICombinedNotificationService[] inner)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        private async Task<bool> NotifyAsync(User receiver, string methodName, Func<ICombinedNotificationService, Task<bool>> operation)
        {
            bool ret = false;

            foreach (ICombinedNotificationService service in Inner)
            {
                try
                {
                    bool didSend = await operation(service);
                    ret = ret || didSend;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error trying to {methodName} to receiver '{receiver?.Route}' with {nameof(ICombinedNotificationService)} type {service.GetType().Name} message '{ex.Message}' with trace: {ex.StackTrace}");
                }
            }

            return ret;
        }

        #endregion

        #region INotificationService

        public async Task<bool> WelcomeAsync(User receiver)
        {
            return await NotifyAsync(receiver, nameof(WelcomeAsync), async (n) => { return await n.WelcomeAsync(receiver); });
        }

        public async Task<bool> UserInviteAsync(User receiver, User sender)
        {
            return await NotifyAsync(receiver, nameof(UserInviteAsync), async (n) => { return await n.UserInviteAsync(receiver, sender); });
        }

        public async Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isAppInvite)
        {
            return await NotifyAsync(receiver, nameof(PersonInviteAsync), async (n) => { return await n.PersonInviteAsync(receiver, sender, org, isAppInvite); });
        }


        public async Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group group, bool isAppInvite)
        {
            return await NotifyAsync(receiver, nameof(MemberInviteAsync), async (n) => { return await n.MemberInviteAsync(receiver, sender, org, group, isAppInvite); });
        }

        #endregion

        #region ISbNotificationService

        public async Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session)
        {
            return await NotifyAsync(receiver, nameof(SessionReminderAsync), async (n) => { return await n.SessionReminderAsync(receiver, org, session); });
        }

        public async Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            return await NotifyAsync(receiver, nameof(SessionPublishAsync), async (n) => { return await n.SessionPublishAsync(receiver, org, session); });
        }

        public async Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            return await NotifyAsync(receiver, nameof(SessionHostPublishAsync), async (n) => { return await n.SessionHostPublishAsync(receiver, org, session); });
        }

        #endregion
    }
}
