using Masticore.Jobs;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Provides fully async processing of notifications via the <see cref="IJob"/> system
    /// </summary>
    public class QueuedNotifications : ICombinedNotificationService
    {
        protected ISbInfrastructure Infrastructure { get; }
        protected IJobQueue JobQueue { get; }
        protected ILogger<QueuedNotifications> Logger { get; }

        public QueuedNotifications(ILogger<QueuedNotifications> logger, ISbInfrastructure infrastructure, IJobQueue jobQueue)
        {
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            JobQueue = jobQueue ?? throw new ArgumentNullException(nameof(jobQueue));
        }

        #region INotificationService

        public async Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group group, bool isAppInvite)
        {
            SbNotificationJob job = new SbNotificationJob
            {
                NotificationType = SbNotificationJobType.MemberInvite,
                ReceiverUserRoute = receiver.Route,
                SenderUserRoute = sender.Route,
                OrgRoute = org.Route,
                GroupRoute = group.Route,
                IsAppInvite = isAppInvite,
            };
            await JobQueue.Add(job);
            return true;
        }

        public async Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isAppInvite)
        {
            SbNotificationJob job = new SbNotificationJob
            {
                NotificationType = SbNotificationJobType.PersonInvite,
                ReceiverUserRoute = receiver.Route,
                SenderUserRoute = sender.Route,
                OrgRoute = org.Route,
                IsAppInvite = isAppInvite,
            };
            await JobQueue.Add(job);
            return true;
        }

        public async Task<bool> UserInviteAsync(User receiver, User sender)
        {
            SbNotificationJob job = new SbNotificationJob
            {
                NotificationType = SbNotificationJobType.UserInvite,
                ReceiverUserRoute = receiver.Route,
                SenderUserRoute = sender.Route,
            };
            await JobQueue.Add(job);
            return true;
        }

        public async Task<bool> WelcomeAsync(User receiver)
        {
            SbNotificationJob job = new SbNotificationJob
            {
                NotificationType = SbNotificationJobType.UserWelcome,
                ReceiverUserRoute = receiver.Route,
            };
            await JobQueue.Add(job);
            return true;
        }

        #endregion

        #region ISbNotificationService

        public async Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            SbNotificationJob job = new SbNotificationJob
            {
                NotificationType = SbNotificationJobType.SessionHostPublish,
                ReceiverUserRoute = receiver.Route,
                OrgRoute = org.Route,
                SessionRoute = session.Route,
            };
            await JobQueue.Add(job);
            return true;
        }

        public async Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            SbNotificationJob job = new SbNotificationJob
            {
                NotificationType = SbNotificationJobType.SessionPublish,
                ReceiverUserRoute = receiver.Route,
                OrgRoute = org.Route,
                SessionRoute = session.Route,
            };
            await JobQueue.Add(job);
            return true;
        }

        public async Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session)
        {
            SbNotificationJob job = new SbNotificationJob
            {
                NotificationType = SbNotificationJobType.SessionReminder,
                ReceiverUserRoute = receiver.Route,
                OrgRoute = org.Route,
                SessionRoute = session.Route,
            };
            await JobQueue.Add(job);
            return true;
        }

        #endregion
    }
}
