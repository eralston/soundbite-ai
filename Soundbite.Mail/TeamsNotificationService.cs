using Masticore;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Services;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Implementation of INotificationService for sending Teams Messages
    /// </summary>
    public class TeamsNotificationService : InfrastructureServiceBase<ISbInfrastructure>, ICombinedNotificationService
    {
        #region Static Fields

        /// <summary>
        /// Stores a value indicating whether Teams Notiications are turned on in the environment.
        /// </summary>
        private static readonly bool IsTeamsNotificationEnabledForSystem = bool.TryParse(
            Environment.GetEnvironmentVariable("SB_TEAMS_NOTIFICATIONS_ENABLED"),
            out bool isEnabled) ? isEnabled : true;

        #endregion

        #region Fields

        protected readonly ITeamsGraphService _teamsGraphService;
        protected readonly TeamsAppAzureSettings _teamsAzureAppSettings;
        protected Dictionary<string, bool> OrgFlags { get; } = new Dictionary<string, bool>();

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="TeamsNotificationService"/> instance.
        /// </summary>
        /// <param name="infrastructure">DI reference.</param>
        /// <param name="logger">DI reference.</param>
        /// <param name="teamsAzureAppSettings">DI reference.</param>
        /// <param name="teamsGraphService">DI reference.</param>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are <c>null</c>.</exception>
        public TeamsNotificationService(
            TeamsAppAzureSettings teamsAzureAppSettings,
            ISbInfrastructure infrastructure,
            ILogger<TeamsNotificationService> logger)
            : base(infrastructure, logger)
        {
            Validator.ArgNotNull(nameof(teamsAzureAppSettings), teamsAzureAppSettings);
            _teamsAzureAppSettings = teamsAzureAppSettings;
        }



        #endregion

        #region INotificationService Implementation

        /// <inheritdoc />
        public async Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session)
        {

            if (!IsTeamsNotificationEnabledForSystem)
            {
                Logger.LogDebug($"Skipping Teams notification for user '{receiver.Route}' in org '{org.Route}' for session '{session.Route}'; not enabled in system");
                return false;
            }

            bool isTeamsEnabledForOrg = await IsEnabledForOrg(org);

            if (!isTeamsEnabledForOrg)
            {
                Logger.LogDebug($"Skipping Teams notification for user '{receiver.Route}' in org '{org.Route}' for session '{session.Route}'; not enabled for org");
                return false;
            }

            bool isRegistered = false;
            try
            {
                await CreatePendingSessionNotification(receiver, session, null);
                isRegistered = true;
                Logger.LogDebug($"Registered teams notification for user '{receiver.Route}' in org '{org.Route}' for session '{session.Route}'");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to send teams notification to user '{receiver?.Route}' in org '{org?.Route}' with error: {ex.Message}");
                if (!isRegistered)
                {
                    await CreateFailedSessionNotification(receiver, session, ex);
                }
            }

            return true;
        }

        #endregion

        #region INotificationService Implementation - Unused

        /// <inheritdoc />
        public Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> WelcomeAsync(User receiver)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> UserInviteAsync(User receiver, User sender)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isAppInvite)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group group, bool isAppInvite)
        {
            return Task.FromResult(false);
        }

        #endregion

        #region Methods

        protected async Task<bool> IsMsTeamsEnabled(string orgRoute)
        {
            if (string.IsNullOrEmpty(orgRoute))
            {
                return false;
            }

            // Org settings may override system settings to block
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            return await db.IsMsTeamsEnabled(orgRoute);
        }

        protected async Task<bool> IsEnabledForOrg(Organization org)
        {
            string orgRoute = org?.Route;
            if (string.IsNullOrEmpty(orgRoute))
            {
                // Default to disabled
                return false;
            }

            if (!OrgFlags.ContainsKey(orgRoute))
            {
                OrgFlags[orgRoute] = await IsMsTeamsEnabled(orgRoute);
            }
            return OrgFlags[orgRoute];
        }

        private async Task CreatePendingSessionNotification(User receiver, SessionEntity session, string details)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                await db.CreateSessionNotification(receiver.Route, session.Route, SessionNotificationType.Publish, NotificationChannel.Teams, details, NotificationStatus.Pending);
                await db.SaveChangesAsync();
            }
            catch (Exception innerEx)
            {
                Logger.LogCritical($"Unable to create successful session notification record for Teams with receiver '{receiver?.Route}' for session '{session?.Route}' with error: {innerEx.Message}");
            }
        }

        private async Task CreateFailedSessionNotification(User receiver, SessionEntity session, Exception ex)
        {
            try
            {
                SbDb db = await Infrastructure.DbAsync();
                await db.CreateSessionNotification(receiver.Route, session.Route, SessionNotificationType.Publish, NotificationChannel.Teams, ex.Message, NotificationStatus.Failed);
                await db.SaveChangesAsync();
            }
            catch (Exception innerEx)
            {
                Logger.LogCritical($"Unable to create failed session notification record for Teams with receiver '{receiver?.Route}' for session '{session?.Route}' with error: {innerEx.Message}");
            }
        }

        #endregion
    }
}
