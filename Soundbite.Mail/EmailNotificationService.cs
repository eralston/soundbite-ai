using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Services;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Resources;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Implementation of INotificationService for sending Email
    /// </summary>
    public class EmailNotificationService : InfrastructureServiceBase<ISbInfrastructure>, ICombinedNotificationService
    {
        protected class OrgEmailSettings : IOrgEmailOptions
        {
            public bool IsEnabled { get; set; }
            public bool IsMsTeamsEnabled { get; set; }
            public string OrgImageUrl { get; set; }
        }

        private static bool? _isEmailEnabled = null;

        private static bool IsEmailEnabledForSystem
        {
            get
            {
                if (!_isEmailEnabled.HasValue)
                {
                    bool wasParsed = bool.TryParse(Environment.GetEnvironmentVariable("SB_EMAIL_ENABLED"), out bool isEnabled);
                    _isEmailEnabled = !wasParsed || isEnabled;
                }
                return _isEmailEnabled.Value;
            }
        }

        private static readonly string HomeUrl = Environment.GetEnvironmentVariable("SB_HOME_URL");

        private static Address SenderEmail => new Address("NoReply@Soundbite.AI", "Soundbite");

        private IEmailTemplates Templates { get; }
        private IMailbox Mailbox { get; }
        protected Dictionary<string, OrgEmailSettings> OrgFlags { get; } = new Dictionary<string, OrgEmailSettings>();
        protected Dictionary<string, User> SenderForSessions { get; } = new Dictionary<string, User>();
        protected TeamsAppAzureSettings TeamsAzureAppSettings { get; }
        public IImageService Images { get; }

        public EmailNotificationService(
            IMapper mapper,
            IEmailTemplates templates,
            IMailbox mailbox,
            ISbInfrastructure infrastructure,
            ILogger<EmailNotificationService> logger,
            TeamsAppAzureSettings teamsAzureAppSettings,
            IImageService images)
            : base(infrastructure, logger, null, mapper)
        {
            Templates = templates ?? throw new ArgumentNullException(nameof(templates));
            Mailbox = mailbox ?? throw new ArgumentNullException(nameof(mailbox));
            TeamsAzureAppSettings = teamsAzureAppSettings ?? throw new ArgumentNullException(nameof(teamsAzureAppSettings));
            Images = images ?? throw new ArgumentNullException(nameof(images));
        }

        private async Task<User> GetSender(SessionEntity session)
        {
            string sessionRoute = session.Route;

            if (!SenderForSessions.ContainsKey(sessionRoute))
            {
                SbDb db = await Infrastructure.DbAsync();
                IEnumerable<PersonEntity> participants = await db.PeopleInSessionParticipantsAsync(session.Route, ParticipantRole.Host);
                UserEntity senderEnt = participants.FirstOrDefault()?.User;
                User sender = senderEnt != null ? Mapper.MapSafe<User>(senderEnt) : null;
                SenderForSessions[sessionRoute] = sender;
            }

            return SenderForSessions[sessionRoute];
        }

        protected async Task<OrgEmailSettings> ReadNotificationSettings(string orgRoute)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity orgEnt = await db.OrgWhereRoute(orgRoute);
            orgEnt.AssertFound($"Could not find org '{orgRoute}'");
            Organization org = Mapper.MapSafe<Organization>(orgEnt);
            string imgUrl = await Images.OrgImageAsync(org);
            OrgSettings settings = OrgSettingsExtensions.GetSettings(orgEnt.ConfigJson) ?? new OrgSettings();
            OrgEmailSettings ret = new OrgEmailSettings
            {
                IsEnabled = settings?.Notifications.Channels?.IsEmailEnabled ?? true,
                IsMsTeamsEnabled = settings.IsMsTeamsEnabled(),
                OrgImageUrl = imgUrl,
            };
            return ret;
        }

        protected async Task<OrgEmailSettings> EmailSettingsForOrg(string orgRoute)
        {
            // If it's not associated with an org, then we allow it
            if (string.IsNullOrEmpty(orgRoute))
            {
                return new OrgEmailSettings
                {
                    IsMsTeamsEnabled = false,
                    IsEnabled = false
                };
            }

            if (!OrgFlags.ContainsKey(orgRoute))
            {
                OrgEmailSettings orgSettings = await ReadNotificationSettings(orgRoute);
                OrgFlags[orgRoute] = orgSettings;
            }

            return OrgFlags[orgRoute];
        }

        protected async Task<bool> IsEnabledForOrg(Organization org)
        {
            string orgRoute = org?.Route;
            // If it's not associated with an org, then we allow it
            if (string.IsNullOrEmpty(orgRoute))
            {
                return true;
            }

            OrgEmailSettings flags = await EmailSettingsForOrg(org.Route);
            return flags.IsEnabled;
        }

        protected async Task<bool> IsTeamsEnabledForOrg(Organization org)
        {
            string orgRoute = org?.Route;
            // If it's not associated with an org, then we allow it
            if (string.IsNullOrEmpty(orgRoute))
            {
                return true;
            }

            OrgEmailSettings flags = await EmailSettingsForOrg(org.Route);
            return flags.IsMsTeamsEnabled;
        }

        private async Task<bool> SendMailAsync(MailMessage mail, User receiver, Organization org = null, string sessionRoute = null, SessionNotificationType type = SessionNotificationType.Unknown)
        {
            if (!IsEmailEnabledForSystem)
            {
                Logger.LogDebug($"Skipping email notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{org?.Route}' for session '{sessionRoute}'; email disabled system-wide");
                return false;
            }

            if (!receiver.AllowEmail)
            {
                Logger.LogDebug($"Skipping email notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{org?.Route}' for session '{sessionRoute}'; user disabled email");
                return false;
            }

            if (string.IsNullOrWhiteSpace(receiver.Email))
            {
                Logger.LogDebug($"Skipping email notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{org?.Route}' for session '{sessionRoute}'; user missing email");
                return false;
            }

            bool isEnabledForOrg = await IsEnabledForOrg(org);

            if (!isEnabledForOrg)
            {
                Logger.LogDebug($"Skipping email notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{org?.Route}' for session '{sessionRoute}'; email disabled for org");
                return false;
            }

            bool isSent = false;
            try
            {
                if (org != null)
                {
                    await Mailbox.SendForOrgAsync(mail, org.Route);
                }
                else
                {
                    await Mailbox.SendAsync(mail);
                }

                isSent = true;
                if (type != SessionNotificationType.Unknown)
                {
                    await CreateSuccessfulNotificationEntity(receiver, sessionRoute, type, mail.To.Email.RedactEmail());
                }
                Logger.LogDebug($"Successfully sent email notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{org?.Route}' for session '{sessionRoute}'");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error trying to send email in {nameof(EmailNotificationService)}: {ex.Message}");

                if (type != SessionNotificationType.Unknown)
                {
                    await CreateFailedNotificationEntity(receiver, sessionRoute, type, ex);
                }
            }

            return isSent;
        }

        private async Task CreateFailedNotificationEntity(User receiver, string sessionRoute, SessionNotificationType type, Exception ex)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                await db.CreateSessionNotification(receiver.Route, sessionRoute, type, NotificationChannel.Email, ex.Message, NotificationStatus.Failed);
                await db.SaveChangesAsync();
            }
            catch (Exception innerEx)
            {
                Logger.LogCritical($"Unable to create failed email notification entity for user '{receiver?.Route}' and session '{sessionRoute}' with error: {innerEx.Message}");
            }
        }

        private async Task CreateSuccessfulNotificationEntity(User receiver, string sessionRoute, SessionNotificationType type, string details)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                await db.CreateSessionNotification(receiver.Route, sessionRoute, type, NotificationChannel.Email, details);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogCritical($"Unable to create successful email notification entity for user '{receiver?.Route}' and session '{sessionRoute}' with error: {ex.Message}");
            }
        }

        private static string GetPublicLink(SessionEntity session, Organization org, User receiver)
        {
            string userQueryString = receiver != null ? $"#ur={receiver.Route}" : "";
            return $"{HomeUrl}/public/organizations/{org.Route}/sessions/{session.Route}{userQueryString}";
        }

        #region ICombinedNotificationService

        public async Task<bool> WelcomeAsync(User receiver)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            // Welcome to User
            MailMessage mail = new MailMessage
            {
                From = SenderEmail,
                To = new Address(receiver),
                Subject = $"Welcome to Soundbite.AI, {receiver.GivenName}",
                Body = await Templates.WelcomeAsync(receiver)
            };

            return await SendMailAsync(mail, receiver);
        }

        public async Task<bool> UserInviteAsync(User receiver, User sender)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }


            UserInviteModel model = new UserInviteModel
            {
                Sender = sender,
                Receiver = receiver,
            };

            MailMessage mail = new MailMessage
            {
                From = SenderEmail,
                CC_1 = new Address(sender),
                To = new Address(receiver),
                Subject = $"{sender.DisplayName()} just invited you to Soundbite.ai",
                Body = await Templates.UserInviteAsync(model)
            };

            return await SendMailAsync(mail, receiver);
        }

        public async Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isNewUser)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            OrgEmailSettings settings = await EmailSettingsForOrg(org.Route);

            PersonInviteModel model = new PersonInviteModel
            {
                IsAppInvite = isNewUser,
                IsMsTeamsEnabled = settings.IsMsTeamsEnabled,
                MsTeamsInfoLink = TeamsAzureAppSettings.BaseUrl,
                Org = org,
                Receiver = receiver,
                Sender = sender,
                Options = settings,
            };

            MailMessage mail = new MailMessage
            {
                From = SenderEmail,
                CC_1 = new Address(sender),
                To = new Address(receiver),
                Subject = $"{sender.GivenName} just invited you the organization '{org.Name}' on Soundbite.ai",
                Body = await Templates.PersonInviteAsync(model)
            };

            return await SendMailAsync(mail, receiver, org);

        }

        public async Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group group, bool isNewUser)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (group is null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            OrgEmailSettings settings = await EmailSettingsForOrg(org.Route);

            MemberInviteModel model = new MemberInviteModel
            {
                IsAppInvite = isNewUser,
                IsMsTeamsEnabled = settings.IsMsTeamsEnabled,
                MsTeamsInfoLink = TeamsAzureAppSettings.BaseUrl,
                Org = org,
                Receiver = receiver,
                Sender = sender,
                Group = group,
                Options = settings,
            };

            MailMessage mail = new MailMessage
            {
                From = SenderEmail,
                CC_1 = new Address(sender),
                To = new Address(receiver),
                Subject = $"{sender.GivenName} just invited you to the team '{group.Name}' on Soundbite.ai",
                Body = await Templates.MemberInviteAsync(model)
            };

            return await SendMailAsync(mail, receiver, org);
        }

        public async Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            string sessionTypeString = session.SessionType.Name();

            OrgEmailSettings settings = await EmailSettingsForOrg(org.Route);

            SessionModel sessionModel = new SessionModel
            {
                IsMsTeamsEnabled = settings.IsMsTeamsEnabled,
                MsTeamsActionLink = TeamsAzureAppSettings.DeepLinkForRecord(session),
                MsTeamsInfoLink = TeamsAzureAppSettings.BaseUrl,
                Org = org,
                Receiver = receiver,
                Session = Mapper.MapSafe<Session>(session),
                Options = settings,
            };

            MailMessage mail = new MailMessage
            {
                From = SenderEmail,
                To = new Address(receiver),
                Subject = $"Reminder for {sessionTypeString}: {session.Name}",
                Body = await Templates.SessionReminderAsync(sessionModel)
            };

            return await SendMailAsync(mail, receiver, org, session.Route, SessionNotificationType.Reminder);
        }

        public async Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            string sessionTypeString = session.SessionType.Name();
            OrgEmailSettings settings = await EmailSettingsForOrg(org.Route);
            User sender = await GetSender(session);

            SessionPublishModel sessionModel = new SessionPublishModel
            {
                IsMsTeamsEnabled = settings.IsMsTeamsEnabled,
                MsTeamsActionLink = TeamsAzureAppSettings.DeepLinkForPlay(session),
                MsTeamsInfoLink = TeamsAzureAppSettings.BaseUrl,
                Org = org,
                PublicLink = GetPublicLink(session, org, receiver),
                Receiver = receiver,
                Sender = sender,
                Session = Mapper.MapSafe<Session>(session),
                Options = settings,
            };

            string subjectFrom = sender != null ? $" from {sender.DisplayName}" : "";

            MailMessage mail = new MailMessage
            {
                From = SenderEmail,
                To = new Address(receiver),
                Subject = $"{session.Name}{subjectFrom}",
                Body = await Templates.SessionPublishAsync(sessionModel)
            };

            return await SendMailAsync(mail, receiver, org, session.Route, SessionNotificationType.Publish);
        }

        public async Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (org is null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            OrgEmailSettings settings = await EmailSettingsForOrg(org.Route);
            string sessionTypeString = session.SessionType.Name();

            SessionPublishModel sessionModel = new SessionPublishModel
            {
                IsMsTeamsEnabled = settings.IsMsTeamsEnabled,
                MsTeamsActionLink = TeamsAzureAppSettings.DeepLinkForPlay(session),
                MsTeamsInfoLink = TeamsAzureAppSettings.BaseUrl,
                Org = org,
                PublicLink = GetPublicLink(session, org, receiver),
                Receiver = receiver,
                Session = Mapper.MapSafe<Session>(session),
                Options = settings,
            };

            MailMessage mail = new MailMessage
            {
                From = SenderEmail,
                To = new Address(receiver),
                Subject = $"Successfully Published {sessionTypeString}: {session.Name}",
                Body = await Templates.SessionHostPublishAsync(sessionModel)
            };

            return await SendMailAsync(mail, receiver, org, session.Route, SessionNotificationType.HostPublish);
        }

        #endregion
    }
}
