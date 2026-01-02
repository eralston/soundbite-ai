using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Services;
using Masticore.Sms;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Resources;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Notification service for SMS messaging via Masticore
    /// </summary>
    public class SmsNotificationService : InfrastructureServiceBase<ISbInfrastructure>, ICombinedNotificationService
    {
        protected class OrgSmsSettings
        {
            public bool IsEnabled { get; set; }
            public bool IsMsTeamsEnabled { get; set; }
        }

        private static readonly string HomeUrl = Environment.GetEnvironmentVariable("SB_HOME_URL");

        private static bool? _isSmsEnabled = null;

        private static bool IsSmsEnabledForSystem
        {
            get
            {
                if (!_isSmsEnabled.HasValue)
                {
                    bool wasParsed = bool.TryParse(Environment.GetEnvironmentVariable("SB_SMS_ENABLED"), out bool isEnabled);
                    _isSmsEnabled = !wasParsed || isEnabled;
                }
                return _isSmsEnabled.Value;
            }
        }

        protected ISmsGateway Gateway { get; }
        protected TeamsAppAzureSettings TeamsAzureAppSettings { get; }

        protected Dictionary<string, OrgSmsSettings> OrgFlags { get; } = new Dictionary<string, OrgSmsSettings>();

        public SmsNotificationService(
            ISmsGateway gateway,
            ISbInfrastructure infrastructure,
            ILogger<SmsNotificationService> logger,
            TeamsAppAzureSettings teamsAzureAppSettings)
            : base(infrastructure, logger)
        {
            Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            TeamsAzureAppSettings = teamsAzureAppSettings ?? throw new ArgumentNullException(nameof(teamsAzureAppSettings));
        }

        protected async Task<OrgSmsSettings> ReadNotificationSettings(string orgRoute)
        {
            if (orgRoute is null)
            {
                throw new ArgumentNullException(nameof(orgRoute));
            }

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");
            OrgSettings settings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            OrgSmsSettings ret = new OrgSmsSettings
            {
                IsEnabled = settings?.Notifications.Channels?.IsSmsEnabled ?? true,
                IsMsTeamsEnabled = settings.IsMsTeamsEnabled(),
            };
            return ret;
        }

        protected async Task<OrgSmsSettings> SmsSettingsForOrg(string orgRoute)
        {
            // If it's not associated with an org, then we allow it
            if (string.IsNullOrEmpty(orgRoute))
            {
                return new OrgSmsSettings
                {
                    IsMsTeamsEnabled = false,
                    IsEnabled = false
                };
            }

            if (!OrgFlags.ContainsKey(orgRoute))
            {
                OrgSmsSettings orgSettings = await ReadNotificationSettings(orgRoute);
                OrgFlags[orgRoute] = orgSettings;
            }

            return OrgFlags[orgRoute];
        }

        protected async Task<bool> IsSmsEnabledForOrg(string orgRoute)
        {
            OrgSmsSettings settings = await SmsSettingsForOrg(orgRoute);
            return settings.IsEnabled;
        }

        protected async Task<bool> IsMsTeamsEnabledForOrg(string orgRoute)
        {
            OrgSmsSettings settings = await SmsSettingsForOrg(orgRoute);
            return settings.IsMsTeamsEnabled;
        }

        protected async Task<bool> SendAsync(
            User receiver,
            string message,
            string orgRoute = null,
            string sessionRoute = null,
            SessionNotificationType type = SessionNotificationType.Unknown)
        {
            if (!IsSmsEnabledForSystem)
            {
                Logger.LogInformation($"Skipping SMS notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{orgRoute}' for session '{sessionRoute}'; SMS disabled system-wide");
                return false;
            }

            if (!receiver.AllowSms)
            {
                Logger.LogInformation($"Skipping SMS notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{orgRoute}' for session '{sessionRoute}'; SMS disabled for user");
                return false;
            }

            if (string.IsNullOrWhiteSpace(receiver.Phone))
            {
                Logger.LogInformation($"Skipping SMS notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{orgRoute}' for session '{sessionRoute}'; user does not have a phone number");
                return false;
            }

            if (orgRoute != null)
            {
                bool isSmsEnabledForOrg = await IsSmsEnabledForOrg(orgRoute);

                // We need sms enabled on the app and user, plus a valid phone number
                if (!isSmsEnabledForOrg)
                {
                    Logger.LogInformation($"Skipping SMS notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{orgRoute}' for session '{sessionRoute}'; SMS disabled for org");
                    return false;
                }
            }

            PhoneNumber toNumber = new PhoneNumber(receiver);
            if (!toNumber.IsValid)
            {
                Logger.LogInformation($"Skipping SMS notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{orgRoute}' for session '{sessionRoute}'; phone number '{toNumber.Last4Numbers}' does not seem valid");
            }

            bool isSent = false;

            try
            {

                Logger.LogInformation($"Sending SMS notification to {toNumber.Last4Numbers}");
                Message msg = new Message
                {
                    To = toNumber,
                    Body = message,
                };
                string id = await Gateway.SendAsync(msg);
                isSent = true;
                if (type != SessionNotificationType.Unknown)
                {
                    await CreateSuccessfulNotificationEntity(receiver, sessionRoute, type, $"{toNumber.Last4Numbers} - {id}");
                }
                Logger.LogInformation($"Successfully sent SMS notification {Enum.GetName(typeof(SessionNotificationType), type)} for user '{receiver.Route}' in org '{orgRoute}' for session '{sessionRoute}'");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error trying to send SMS in {nameof(SmsNotificationService)}: {ex.Message}");

                if (!isSent && type != SessionNotificationType.Unknown)
                {
                    await CreateFailedNotificationEntity(receiver, sessionRoute, type, ex);
                }

                return false;
            }
        }

        private async Task CreateFailedNotificationEntity(User receiver, string sessionRoute, SessionNotificationType type, Exception ex)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                await db.CreateSessionNotification(receiver.Route, sessionRoute, type, NotificationChannel.SMS, ex.Message, NotificationStatus.Failed);
                await db.SaveChangesAsync();
            }
            catch (Exception innerEx)
            {
                Logger.LogCritical($"Unable to create failed sms notification entity for user '{receiver?.Route}' and session '{sessionRoute}' with error: {innerEx.Message}");
            }
        }

        private async Task CreateSuccessfulNotificationEntity(User receiver, string sessionRoute, SessionNotificationType type, string details)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                await db.CreateSessionNotification(receiver.Route, sessionRoute, type, NotificationChannel.SMS, details);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogCritical($"Unable to create successful sms notification entity for user '{receiver?.Route}' and session '{sessionRoute}' with error: {ex.Message}");
            }
        }

        private async Task<string> GetUrlForSession(SessionEntity session, Organization org, User receiver = null)
        {
            bool isPublic = session.SessionSecurity == Masticore.SessionSecurityType.Public;

            if (isPublic)
            {
                return GetPublicLink(session, org, receiver);
            }
            else
            {
                bool isMsTeamsEnabled = await IsMsTeamsEnabledForOrg(org.Route);
                if (isMsTeamsEnabled)
                {
                    return TeamsAzureAppSettings.DeepLinkForPlay(session);
                }
                else
                {
                    return $"{HomeUrl}/organizations/{org.Route}/feed#sbplay={session.Route}";
                }
            }
        }

        private static string GetPublicLink(SessionEntity session, Organization org, User receiver)
        {
            string userQueryString = receiver != null ? $"#ur={receiver.Route}" : "";
            return $"{HomeUrl}/public/organizations/{org.Route}/sessions/{session.Route}{userQueryString}";
        }

        #region ICombinedNotificationService

        public Task<bool> UserInviteAsync(User receiver, User sender)
        {
            // Cannot send an sms on user univite because receiver will never have a phone number
            return Task.FromResult(false);
        }

        public async Task<bool> WelcomeAsync(User receiver)
        {
            // NOTE: This is not guaranteed to have a valid phone number

            string message = $"{receiver.GivenName}, welcome to Soundbite. Login any time: {HomeUrl}";

            return await SendAsync(receiver, message);
        }

        public async Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group group, bool isAppInvite)
        {
            string message = $"{sender.GivenName} has invited you to team {group.Name} in the {org.Name} organization on Soundbite: {HomeUrl}/organizations/{org.Route}/teams/{group.Route}";

            return await SendAsync(receiver, message);
        }

        public async Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isAppInvite)
        {
            string message = $"{sender.GivenName} has invited you to the {org.Name} organization on Soundbite: {HomeUrl}/organizations/{org.Route}/";

            return await SendAsync(receiver, message, org.Route);
        }

        public async Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            string url = await GetUrlForSession(session, org, receiver);
            string typeName = session.SessionType.Name();
            string message = $"{receiver.GivenName}, listen to {typeName}: {session.Name} in the {org.Name} organization on Soundbite: {url}";
            return await SendAsync(receiver, message, org.Route, session.Route, SessionNotificationType.Publish);
        }

        public async Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session)
        {
            string typeName = session.SessionType.Name();
            string message = $"{receiver.GivenName}, {typeName}: {session.Name} needs your recording in the {org.Name} organization on Soundbite: {HomeUrl}/organizations/{org.Route}/feed#sbrecord={session.Route}";

            return await SendAsync(receiver, message, org.Route, session.Route, SessionNotificationType.Reminder);
        }

        public async Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            string typeName = session.SessionType.Name();
            string message = $"{receiver.GivenName}, you successfully published {typeName}: {session.Name} in the {org.Name} organization on Soundbite: {HomeUrl}/organizations/{org.Route}/feed#sbplay={session.Route}";

            return await SendAsync(receiver, message, org.Route, session.Route, SessionNotificationType.HostPublish);
        }

        #endregion
    }
}
