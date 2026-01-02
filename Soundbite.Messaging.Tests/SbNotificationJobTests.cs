using Masticore.Entity.Tests;
using Masticore.Graph;
using Masticore.Models;
using Masticore.Queue;
using Masticore.Security;
using Masticore.Services;
using Masticore.Services.Tests;
using Masticore.Tests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Soundbite.Entity;
using Soundbite.Messaging.Jobs;
using Soundbite.Services;
using Soundbite.Services.Tests;
using Soundbite.Services.Tests.Mock;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MockGraph = Masticore.Services.Tests.MockGraph;

namespace Soundbite.Messaging.Tests
{

    public class AzQueueTests : ServiceTestBase
    {
        /// <summary>
        /// This is here just to match Masticore.Storage.Tests and make it possible to turn-key unit test w/o figuring out Azurite in the pipeline or requiring devs to install it locally
        /// </summary>
        /// <remarks>
        /// TODO Swap for local Azurite;
        /// In theory two devs running unit tests at the same time could trample each other since this is global;
        /// One may also get old messages in the queue in event of failure, so consider opening up the Azure console and clearing the relevant queue from time-to-time if you're having trouble
        /// </remarks>
        private const string ConnectionString = "[SB STORAGE CONNECTION STRING]";

        IGraph Graph { get; } = new MockGraph();
        MockNotifications Notifications { get; } = new MockNotifications();
        AzQueue Queue { get; } = new AzQueue(ConnectionString);
        MockEmailTemplates EmailTemplates { get; } = new MockEmailTemplates();
        MockMailbox Mailbox { get; } = new MockMailbox();
        MockSmsGateway SmsGateway { get; } = new MockSmsGateway();

        ThrowIfErrorLogger<SbNotificationJobWorker> WorkerLogger { get; } = new ThrowIfErrorLogger<SbNotificationJobWorker>();
        ThrowIfErrorLogger<UserService> UsersLogger { get; } = new ThrowIfErrorLogger<UserService>();
        ThrowIfErrorLogger<OrganizationService> OrganizationsLogger { get; } = new ThrowIfErrorLogger<OrganizationService>();
        ThrowIfErrorLogger<GroupService> GroupsLogger { get; } = new ThrowIfErrorLogger<GroupService>();
        ThrowIfErrorLogger<EmailNotificationService> EmailLogger { get; } = new ThrowIfErrorLogger<EmailNotificationService>();
        ThrowIfErrorLogger<SmsNotificationService> SmsLogger { get; } = new ThrowIfErrorLogger<SmsNotificationService>();
        ThrowIfErrorLogger<NotificationComposite> NotificationsLogger { get; } = new ThrowIfErrorLogger<NotificationComposite>();

        private Task<ISecurityContext> CreateSecurity(bool isRandomUser)
        {
            ISecurityContext security;

            if (isRandomUser)
            {
                security = new MockSecurityContext(true);
            }
            else
            {
                SbDb db = Builder.DbContext.Value;
                security = MockSecurityContext.CreateAndLoad(db);
            }

            return Task.FromResult(security);
        }

        protected IUserService CreateUserService()
        {
            IUserService users = new UserService(
                Builder.SecurityContext.Value,
                Builder.Rbac.Value,
                Builder.ImageService.Value,
                Graph,
                Notifications,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                UsersLogger);
            return users;
        }

        private IOrganizationService CreateOrganizationService()
        {
            return new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);
        }

        private IGroupService CreateGroupService()
        {
            return new GroupService(
                Builder.Rbac.Value,
                Notifications,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                GroupsLogger);
        }

        private SbNotificationJobWorker CreateNotificationWorker(ISecurityContext security)
        {
            IUserService users = CreateUserService();
            IOrganizationService organizationService = CreateOrganizationService();
            IGroupService groups = CreateGroupService();

            GlobalNotificationSettings.SetAllFlags();

            EmailNotificationService emails = new EmailNotificationService(
                MockMapper.Instance,
                EmailTemplates,
                Mailbox,
                Builder.Infrastructure.Value,
                EmailLogger,
                new TeamsAppAzureSettings { BaseUrl = "https://google.com" },
                Builder.ImageService.Value);

            SmsNotificationService sms = new SmsNotificationService(
                SmsGateway,
                Builder.Infrastructure.Value,
                SmsLogger,
                new TeamsAppAzureSettings { BaseUrl = "https://google.com" });
            Mock<ICombinedNotificationService> teamsMock = new Mock<ICombinedNotificationService>();

            NotificationComposite notifications = new NotificationComposite(
                NotificationsLogger,
                emails,
                sms,
                teamsMock.Object
            );

            SbNotificationJobWorker worker = new SbNotificationJobWorker(
                Builder.Infrastructure.Value,
                WorkerLogger,
                users,
                organizationService,
                groups,
                notifications);
            return worker;
        }

        private async Task<SbNotificationJob> ProcessJobWithType(SbNotificationJobWorker worker, string typeName, SbNotificationJobType notType, string receiverRoute)
        {
            SbNotificationJob sbNotifyJob = await Queue.ProcessNext(worker, typeName) as SbNotificationJob;
            Assert.NotNull(sbNotifyJob);
            Assert.Equal(notType, sbNotifyJob.NotificationType);
            Assert.Equal(receiverRoute, sbNotifyJob.ReceiverUserRoute);
            return sbNotifyJob;
        }

        [Fact]
        public async Task QueuedNotificationTests()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();
            SbNotificationJobWorker worker = CreateNotificationWorker(Builder.SecurityContext.Value);
            QueuedNotifications notifications = new QueuedNotifications(NullLogger<QueuedNotifications>.Instance, Builder.Infrastructure.Value, Queue);

            User receiver = await UserWhereRoute();
            User sender = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            // ACT
            // INotificationService
            await notifications.WelcomeAsync(receiver);
            await notifications.UserInviteAsync(receiver, sender);
            await notifications.PersonInviteAsync(receiver, sender, org, true);
            await notifications.MemberInviteAsync(receiver, sender, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(receiver, org, session);
            await notifications.SessionPublishAsync(receiver, org, session);
            await notifications.SessionHostPublishAsync(receiver, org, session);

            snap.AssertAdd<SessionNotificationEntity>(0);
            string typeName = TypeNameUtils.TypeNameForClass<SbNotificationJob>();

            // INotificationService
            // RECEIVING ERRORS? Make sure the queue is empty before running this test
            SbNotificationJob welcomeAsyncJob = await ProcessJobWithType(worker, typeName, SbNotificationJobType.UserWelcome, receiver.Route);

            SbNotificationJob userInviteAsync = await ProcessJobWithType(worker, typeName, SbNotificationJobType.UserInvite, receiver.Route);
            Assert.Equal(sender.Route, userInviteAsync.SenderUserRoute);

            SbNotificationJob personInviteAsync = await ProcessJobWithType(worker, typeName, SbNotificationJobType.PersonInvite, receiver.Route);
            Assert.Equal(sender.Route, personInviteAsync.SenderUserRoute);
            Assert.Equal(org.Route, personInviteAsync.OrgRoute);
            Assert.True(personInviteAsync.IsAppInvite);

            SbNotificationJob memberInviteAsync = await ProcessJobWithType(worker, typeName, SbNotificationJobType.MemberInvite, receiver.Route);
            Assert.Equal(sender.Route, memberInviteAsync.SenderUserRoute);
            Assert.Equal(org.Route, memberInviteAsync.OrgRoute);
            Assert.Equal(grp.Route, memberInviteAsync.GroupRoute);
            Assert.True(memberInviteAsync.IsAppInvite);

            // INotificationService
            SbNotificationJob sessionReminderAsync = await ProcessJobWithType(worker, typeName, SbNotificationJobType.SessionReminder, receiver.Route);
            Assert.Equal(org.Route, sessionReminderAsync.OrgRoute);
            Assert.Equal(session.Route, sessionReminderAsync.SessionRoute);

            SbNotificationJob sessionPublishAsync = await ProcessJobWithType(worker, typeName, SbNotificationJobType.SessionPublish, receiver.Route);
            Assert.Equal(org.Route, sessionPublishAsync.OrgRoute);
            Assert.Equal(session.Route, sessionPublishAsync.SessionRoute);

            SbNotificationJob sessionHostPublishAsync = await ProcessJobWithType(worker, typeName, SbNotificationJobType.SessionHostPublish, receiver.Route);
            Assert.Equal(org.Route, sessionHostPublishAsync.OrgRoute);
            Assert.Equal(session.Route, sessionHostPublishAsync.SessionRoute);

            // 3 sessions times 2 channels
            snap.AssertAdd<SessionNotificationEntity>(6);
            SessionNotificationEntity[] sessionNotifications = snap.NewEntities<SessionNotificationEntity>();
            Assert.Equal(3, sessionNotifications.Where(n => n.Channel == NotificationChannel.Email).Count());
            Assert.Equal(3, sessionNotifications.Where(n => n.Channel == NotificationChannel.SMS).Count());

            Assert.Equal(7, EmailTemplates.Count);
            Assert.Equal(7, Mailbox.Count);
            // User invite never has a phone number
            Assert.Equal(6, SmsGateway.Count);
            WorkerLogger.AssertNoThrow();
            UsersLogger.AssertNoThrow();
            OrganizationsLogger.AssertNoThrow();
            GroupsLogger.AssertNoThrow();
            EmailLogger.AssertNoThrow();
            SmsLogger.AssertNoThrow();
            NotificationsLogger.AssertNoThrow();
        }
    }
}
