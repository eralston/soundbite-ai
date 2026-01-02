using Masticore.Models;
using Masticore.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Soundbite.Entity;
using Soundbite.Queue;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Carries a union of all notification data to rehydrate and send the notification from the job worker
    /// </summary>
    /// <remarks>Does not implement RBAC</remarks>
    public class SbNotificationJob : SbJobBase
    {
        /// <summary>
        /// In theory <see cref="SbNotificationJob"/> should not know its own queue name since that is a detail in the job worker;
        /// however, the Azure Function needs a constant to initialize based on when listening, so this is here for now
        /// </summary>
        public const string QueueName = "q-sbnotificationjob";

        // Consider breaking up this class into one per notification type if it ever gets bigger than a couple hundred lines

        // Carries a union of all possible notification data
        public SbNotificationJobType NotificationType { get; set; }
        public string ReceiverUserRoute { get; set; }
        public string SenderUserRoute { get; set; }
        public string OrgRoute { set; get; }
        public bool IsAppInvite { get; set; }
        public string GroupRoute { get; set; }
        public string SessionRoute { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public ICombinedNotificationService Notifications { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IUserService Users { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IOrganizationService Organizations { get; set; }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IGroupService Groups { get; set; }

        private async Task<SessionEntity> ReadSession()
        {
            SbDb db = await Infrastructure.DbAsync();
            SessionEntity session = await db.SessionAsync(SessionRoute);
            return session;
        }

        private async Task SendSessionHostPublish()
        {
            User receiver = await Users.ReadAsync(ReceiverUserRoute, false);
            Organization org = await Organizations.ReadAsync(OrgRoute);
            SessionEntity session = await ReadSession();
            await Notifications.SessionHostPublishAsync(receiver, org, session);
        }

        private async Task SendSessionPublish()
        {
            User receiver = await Users.ReadAsync(ReceiverUserRoute, false);
            Organization org = await Organizations.ReadAsync(OrgRoute);
            SessionEntity session = await ReadSession();
            await Notifications.SessionPublishAsync(receiver, org, session);
        }

        private async Task SendSessionReminder()
        {
            User receiver = await Users.ReadAsync(ReceiverUserRoute, false);
            Organization org = await Organizations.ReadAsync(OrgRoute);
            SessionEntity session = await ReadSession();
            await Notifications.SessionReminderAsync(receiver, org, session);
        }

        private async Task SendMemberInvite()
        {
            User receiver = await Users.ReadAsync(ReceiverUserRoute, false);
            User sender = await Users.ReadAsync(SenderUserRoute);
            Organization org = await Organizations.ReadAsync(OrgRoute);
            Group group = await Groups.ReadAsync(OrgRoute, GroupRoute);
            await Notifications.MemberInviteAsync(receiver, sender, org, group, IsAppInvite);
        }

        private async Task SendPersonInvite()
        {
            User receiver = await Users.ReadAsync(ReceiverUserRoute, false);
            Organization org = await Organizations.ReadAsync(OrgRoute);
            User sender = await Users.ReadAsync(SenderUserRoute);
            await Notifications.PersonInviteAsync(receiver, sender, org, IsAppInvite);
        }

        private async Task SendUserInviteAsync()
        {
            User receiver = await Users.ReadAsync(ReceiverUserRoute, false);
            User sender = await Users.ReadAsync(SenderUserRoute);
            await Notifications.UserInviteAsync(receiver, sender);
        }

        private async Task SendUserWelcomeAsync()
        {
            User receiver = await Users.ReadAsync(ReceiverUserRoute, false);
            await Notifications.WelcomeAsync(receiver);
        }

        public override async Task Process()
        {
            if (Notifications == null)
            {
                throw new Exception($"Cannot process {nameof(SbNotificationJob)}; no {nameof(ICombinedNotificationService)} set to support sending messages");
            }

            if (Users == null)
            {
                throw new Exception($"Cannot process {nameof(SbNotificationJob)}; no {nameof(IUserService)} set to support sending messages");
            }

            if (Organizations == null)
            {
                throw new Exception($"Cannot process {nameof(SbNotificationJob)}; no {nameof(IOrganizationService)} set to support sending messages");
            }

            if (Groups == null)
            {
                throw new Exception($"Cannot process {nameof(SbNotificationJob)}; no {nameof(IGroupService)} set to support sending messages");
            }

            Logger.LogDebug($"Sending notification type {NotificationType} to user {ReceiverUserRoute}");

            // INotificationService
            if (NotificationType == SbNotificationJobType.UserWelcome)
            {
                await SendUserWelcomeAsync();
            }
            else if (NotificationType == SbNotificationJobType.UserInvite)
            {
                await SendUserInviteAsync();
            }
            else if (NotificationType == SbNotificationJobType.PersonInvite)
            {
                await SendPersonInvite();
            }
            else if (NotificationType == SbNotificationJobType.MemberInvite)
            {
                await SendMemberInvite();
            }
            // ISbNotificationService
            else if (NotificationType == SbNotificationJobType.SessionReminder)
            {
                await SendSessionReminder();
            }
            else if (NotificationType == SbNotificationJobType.SessionPublish)
            {
                await SendSessionPublish();
            }
            else if (NotificationType == SbNotificationJobType.SessionHostPublish)
            {
                await SendSessionHostPublish();
            }
            else
            {
                throw new Exception($"Cannot process {nameof(SbNotificationJob)}; unknown notification type {NotificationType}");
            }
        }
    }
}
