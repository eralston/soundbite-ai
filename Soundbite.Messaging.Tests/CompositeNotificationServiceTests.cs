using Masticore.Azure.MediaServices;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Soundbite.Entity;
using Soundbite.Services;
using Soundbite.Services.Tests;
using Soundbite.Services.Tests.Mock;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Messaging.Tests
{
    public class CompositeNotificationServiceTests : ServiceTestBase
    {
        #region Constructor

        public CompositeNotificationServiceTests()
        {
            Builder.OrganizationService.Use(i => new OrganizationService(
                i.SecurityContext.Value,
                i.Infrastructure.Value,
                i.ImageService.Value,
                i.Mapper.Value,
                i.Logger<OrganizationService>(),
                i.Rbac.Value));

            Builder.SessionService.Use(i => new SessionService(
                i.Rbac.Value,
                i.SeriesService.Value,
                i.ImageService.Value,
                i.ClipFileService.Value,
                i.LifeCycleFactory.Value,
                i.Infrastructure.Value,
                i.ProviderFactory.Value,
                i.Mapper.Value,
                i.Logger<SessionService>()));

            AzureMediaServiceConfig config = new AzureMediaServiceConfig(); //TODO: make part of builder
            Builder.SbOrganizationService.Use(i => new SbOrganizationService(
                i.OrganizationService.Value,
                i.SessionService.Value,
                i.Infrastructure.Value,
                i.Logger<SbOrganizationService>(),
                i.Rbac.Value,
                new MockEmailTemplates(),
                i.Mapper.Value,
                Builder.TokenDataService.Value,
                config, //TODO: make part of builder
                new AzureMediaService())); //TODO: make part of builder
        }

        #endregion

        private void CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications)
        {
            GlobalNotificationSettings.SetAllFlags();
            emailTemplates = new MockEmailTemplates();
            mailbox = new MockMailbox();
            EmailNotificationService emails = new EmailNotificationService(
                MockMapper.Instance,
                emailTemplates,
                mailbox,
                Builder.Infrastructure.Value,
                new NullLogger<EmailNotificationService>(),
                new TeamsAppAzureSettings { BaseUrl = "https://google.com" },
                Builder.ImageService.Value);

            smsGateway = new MockSmsGateway();
            SmsNotificationService sms = new SmsNotificationService(
                smsGateway,
                Builder.Infrastructure.Value,
                new NullLogger<SmsNotificationService>(),
                new TeamsAppAzureSettings { BaseUrl = "https://google.com" });
            teamsMock = new Mock<ICombinedNotificationService>();
            NotificationComposite composite = new NotificationComposite(NullLogger<NotificationComposite>.Instance, emails, sms, teamsMock.Object);
            notifications = new NotificationPermissionDecorator(NullLogger<NotificationPermissionDecorator>.Instance, composite, Builder.Infrastructure.Value, Builder.Rbac.Value);
        }

        [Fact]
        public async Task SendAll()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications);

            // Turns off flags for the user just to make sure their settings do NOT effect sending
            UserEntity otherUserEnt = await UserEntityAsync(IdentityDbSeed.UserOutside.Route);
            otherUserEnt.AllowEmail = false;
            otherUserEnt.AllowSms = false;
            await SaveDbAsync();

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            User user = await UserWhereRoute();
            User otherUser = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            // ACT - Fire each notification and check for expected activity

            // INotificationService
            await notifications.WelcomeAsync(user);
            await notifications.UserInviteAsync(user, otherUser);
            await notifications.PersonInviteAsync(user, otherUser, org, true);
            await notifications.MemberInviteAsync(user, otherUser, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(user, org, session);
            await notifications.SessionPublishAsync(user, org, session);
            await notifications.SessionHostPublishAsync(user, org, session);

            // ASSERT - Ensure all notifications fired for both email and SMS
            Assert.Equal(7, emailTemplates.Count);
            Assert.Equal(7, mailbox.Count);
            // SMS is one less because the invite does NOT send an SMS (impossible to have a phone number for the target user as of this writing)
            Assert.Equal(6, smsGateway.Count);
            teamsMock.Verify(x => x.SessionPublishAsync(user, org, session), Times.Once);

            // 3 types times 2 channels = 6
            snap.AssertAdd<SessionNotificationEntity>(6);
        }

        [Fact]
        public async Task SendSome_EmailDisabledForOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ISbOrganizationService orgs = Builder.SbOrganizationService.Value;

            CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications);

            User user = await UserWhereRoute();
            User otherUser = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            // Disable e-mail
            OrgNotificationSettings settings = await orgs.ReadNotificationSettings(org.Route);
            settings.Channels.IsEmailEnabled = false;
            settings.Channels.IsSmsEnabled = true;
            await orgs.UpdateNotificationSettings(org.Route, settings);

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // ACT - Fire each notification and check for expected activity

            // INotificationService
            await notifications.WelcomeAsync(user);
            await notifications.UserInviteAsync(user, otherUser);
            await notifications.PersonInviteAsync(user, otherUser, org, true);
            await notifications.MemberInviteAsync(user, otherUser, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(user, org, session);
            await notifications.SessionPublishAsync(user, org, session);
            await notifications.SessionHostPublishAsync(user, org, session);

            // ASSERT - Check no notifications were sent
            // The templates should render
            Assert.Equal(7, emailTemplates.Count);
            // There are 2 e-mails that are not tied to an org, so they do send
            Assert.Equal(2, mailbox.Count);
            // The user invite cannot fire via SMS, so it does not count
            Assert.Equal(6, smsGateway.Count);
            // 3 types times 1 channel = 3
            snap.AssertAdd<SessionNotificationEntity>(3);
        }

        [Fact]
        public async Task SendSome_SmsDisabledForOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ISbOrganizationService orgs = Builder.SbOrganizationService.Value;

            CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications);

            User user = await UserWhereRoute();
            User otherUser = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            // Disable e-mail
            OrgNotificationSettings settings = await orgs.ReadNotificationSettings(org.Route);
            settings.Channels.IsSmsEnabled = false;
            await orgs.UpdateNotificationSettings(org.Route, settings);

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // ACT - Fire each notification and check for expected activity

            // INotificationService
            await notifications.WelcomeAsync(user);
            await notifications.UserInviteAsync(user, otherUser);
            await notifications.PersonInviteAsync(user, otherUser, org, true);
            await notifications.MemberInviteAsync(user, otherUser, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(user, org, session);
            await notifications.SessionPublishAsync(user, org, session);
            await notifications.SessionHostPublishAsync(user, org, session);

            // ASSERT - Check no notifications were sent
            // All e-mails should render and fire
            Assert.Equal(7, emailTemplates.Count);
            Assert.Equal(7, mailbox.Count);
            // 2 messages are not tied to an org, so they fire
            Assert.Equal(2, smsGateway.Count);
            // 3 types times 1 channel = 3
            snap.AssertAdd<SessionNotificationEntity>(3);
        }

        [Fact]
        public async Task SendSome_EmailAndSmsDisabledForUser()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications);


            // Turn off flags for target user; this should disabled sending notifications to them
            UserEntity userEnt = await UserEntityAsync();
            userEnt.AllowEmail = false;
            userEnt.AllowSms = false;
            await SaveDbAsync();

            User user = await UserWhereRoute();
            User otherUser = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // ACT - Fire each notification and check for expected activity

            // INotificationService
            await notifications.WelcomeAsync(user);
            await notifications.UserInviteAsync(user, otherUser);
            await notifications.PersonInviteAsync(user, otherUser, org, true);
            await notifications.MemberInviteAsync(user, otherUser, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(user, org, session);
            await notifications.SessionPublishAsync(user, org, session);
            await notifications.SessionHostPublishAsync(user, org, session);

            // ASSERT - Check no notifications were sent
            // The templates should render
            Assert.Equal(7, emailTemplates.Count);
            // But nothing is actually sent
            Assert.Equal(0, mailbox.Count);
            Assert.Equal(0, smsGateway.Count);
            snap.AssertAdd<SessionNotificationEntity>(0);
        }

        [Fact]
        public async Task SendNone_GlobalSettingsDisabled()
        {
            // ARRANGE
            // Disable global flags
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications);
            GlobalNotificationSettings.SetAllFlags(false);
            User user = await UserWhereRoute();
            User otherUser = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // ACT - Fire each notification and check for expected activity

            // INotificationService
            await notifications.WelcomeAsync(user);
            await notifications.UserInviteAsync(user, otherUser);
            await notifications.PersonInviteAsync(user, otherUser, org, true);
            await notifications.MemberInviteAsync(user, otherUser, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(user, org, session);
            await notifications.SessionPublishAsync(user, org, session);
            await notifications.SessionHostPublishAsync(user, org, session);

            // ASSERT - Check no notifications were sent
            Assert.Equal(0, emailTemplates.Count);
            Assert.Equal(0, mailbox.Count);
            Assert.Equal(0, smsGateway.Count);
            snap.AssertAdd<SessionNotificationEntity>(0);
        }

        [Fact]
        public async Task SendSome_OrgSettingsDisabled_TwoOrgs()
        {
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications);

            // Set the notification settings to disable everything
            OrgNotificationSettings notificationSettings = new OrgNotificationSettings
            {
                // Orgs cannot disable the user invite notification
                IsWelcomeEnabled = false,
                IsPersonInviteEnabled = false,
                IsMemberInvitedEnabled = false,
                IsSessionReminderEnabled = false,
                IsSessionPublishEnabled = false,
                IsSessionHostPublishEnabled = false,
            };
            OrgSettings orgSettings = new OrgSettings
            {
                Notifications = notificationSettings
            };
            OrganizationEntity orgEnt = await OrgEntityAsync();
            orgEnt.SetSettings(orgSettings);
            await SaveDbAsync();

            User user = await UserWhereRoute();
            User otherUser = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            SbDb db = Builder.DbContext.Value;
            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // ACT - Fire each notification and check for expected activity

            // INotificationService
            await notifications.WelcomeAsync(user);
            await notifications.UserInviteAsync(user, otherUser);
            await notifications.PersonInviteAsync(user, otherUser, org, true);
            await notifications.MemberInviteAsync(user, otherUser, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(user, org, session);
            await notifications.SessionPublishAsync(user, org, session);
            await notifications.SessionHostPublishAsync(user, org, session);

            // ASSERT - Check restricted notifications
            // The welcome should be sent because the user is in two orgs
            Assert.Equal(2, emailTemplates.Count);
            Assert.Equal(2, mailbox.Count);
            // SMS is sent only once because invite never has a phone number
            Assert.Equal(1, smsGateway.Count);
            // None of the session notifications fired
            snap.AssertAdd<SessionNotificationEntity>(0);
        }

        [Fact]
        public async Task SendSome_OrgSettingsDisabled_OneOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ISbOrganizationService orgs = Builder.SbOrganizationService.Value;
            CreateServices(out MockEmailTemplates emailTemplates, out MockMailbox mailbox, out MockSmsGateway smsGateway, out Mock<ICombinedNotificationService> teamsMock, out ICombinedNotificationService notifications);


            // Set the notification settings to disable everything
            OrgNotificationSettings notificationSettings = new OrgNotificationSettings
            {
                // Orgs cannot disable the user invite notification
                IsWelcomeEnabled = false,
                IsPersonInviteEnabled = false,
                IsMemberInvitedEnabled = false,
                IsSessionReminderEnabled = false,
                IsSessionPublishEnabled = false,
                IsSessionHostPublishEnabled = false,
            };
            OrgSettings orgSettings = new OrgSettings
            {
                Notifications = notificationSettings
            };
            OrganizationEntity orgEnt = await OrgEntityAsync();
            orgEnt.SetSettings(orgSettings);

            // Delete all other orgs
            SbDb db = Builder.DbContext.Value;
            foreach (OrganizationEntity o in db.Organizations.Where(o => o.Route != IdentityDbSeed.OrgPrimary.Route))
            {
                o.SoftDeleteNow();
            }
            await SaveDbAsync();

            User user = await UserWhereRoute();
            User otherUser = await UserWhereRoute(IdentityDbSeed.UserOutside.Route);
            Organization org = await OrgAsync();
            Group grp = await GroupAsync();
            SessionEntity session = await SessionEntityAsync();

            DbSnapshot snap = new DbSnapshot(db).Snap<SessionNotificationEntity>();

            // ACT - Fire each notification and check for expected activity

            // INotificationService
            await notifications.WelcomeAsync(user);
            await notifications.UserInviteAsync(user, otherUser);
            await notifications.PersonInviteAsync(user, otherUser, org, true);
            await notifications.MemberInviteAsync(user, otherUser, org, grp, true);

            // INotificationService
            await notifications.SessionReminderAsync(user, org, session);
            await notifications.SessionPublishAsync(user, org, session);
            await notifications.SessionHostPublishAsync(user, org, session);

            // ASSERT - Check restricted notifications
            // Making it so the user only has one org with completely deactivated notifications means only user invite works
            Assert.Equal(1, emailTemplates.Count);
            Assert.Equal(1, mailbox.Count);
            // User invite never has a phone number
            Assert.Equal(0, smsGateway.Count);
            // None of the session notifications fired
            snap.AssertAdd<SessionNotificationEntity>(0);
        }
    }
}
