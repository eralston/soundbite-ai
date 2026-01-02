using Masticore.Entity;
using Masticore.Models;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// <see href="https://en.wikipedia.org/wiki/Decorator_pattern">Decorator pattern</see> over the <see cref="ICombinedNotificationService"/>
    /// </summary>
    public class NotificationPermissionDecorator : ICombinedNotificationService
    {
        #region Properties

        protected ICombinedNotificationService Inner { get; }
        protected IIdentityInfrastructure Infrastructure { get; }
        protected IRbac Rbac { get; }
        protected ILogger Logger { get; }

        /// <summary>
        /// This cache ensures we only parse org settings once per org per request. Handy, but this should not ever need to graduate to a centralized cache since cache invalidation is hard and parsing is cheap
        /// </summary>
        protected Dictionary<string, OrgNotificationSettings> OrgSettingsCache { get; } = new Dictionary<string, OrgNotificationSettings>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emails"></param>
        /// <param name="sms"></param>
        /// <param name="infrastructure"></param>
        /// <param name="rbac"></param>
        public NotificationPermissionDecorator(
            ILogger<NotificationPermissionDecorator> logger,
            ICombinedNotificationService inner,
            IIdentityInfrastructure infrastructure,
            IRbac rbac)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
        }

        #endregion

        #region Methods        

        /// <summary>
        /// Checks the given predicate versus the given org; this returns true if the org does not specify a restriction
        /// </summary>
        /// <param name="org"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected bool IsFlagEnabled(OrganizationEntity org, Func<OrgNotificationSettings, bool> predicate)
        {
            OrgNotificationSettings settings = SettingsForOrg(org);
            if (settings == null)
            {
                return true; // By default, we assume all notifications are enabled
            }

            return predicate.Invoke(settings);
        }

        private OrgNotificationSettings SettingsForOrg(OrganizationEntity org)
        {
            string key = org.Route;
            if (!OrgSettingsCache.ContainsKey(key))
            {
                OrgSettingsCache[key] = org.GetNotificationSettings();
            }
            return OrgSettingsCache[key];
        }

        /// <summary>
        /// Async return the set of orgs associated with the given user; excluding deleted ones
        /// </summary>
        /// <returns></returns>
        protected async Task<OrganizationEntity[]> OrgsAsync()
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            if (Rbac.UniversalIdForCurrentUser == null)
            {
                return null;
            }
            OrganizationEntity[] orgs = await db.QueryOrgsForUser(Rbac.UniversalIdForCurrentUser).ReadOnly();
            return orgs;
        }

        /// <summary>
        /// Returns true if at least one org for the current user fails the given predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected async Task<bool> IsRestrictedByAnyOrg(Func<OrgNotificationSettings, bool> predicate)
        {
            IEnumerable<OrganizationEntity> orgs = await OrgsAsync();
            if (orgs == null)
            {
                return false;
            }
            int countWhereRestricted = orgs.Where(o => !IsFlagEnabled(o, predicate)).Count();
            return countWhereRestricted > 0;
        }

        /// <summary>
        /// Returns true if at least one org for the current user has the given flag enabled
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected async Task<bool> IsAllowedByAnyOrg(Func<OrgNotificationSettings, bool> predicate)
        {
            IEnumerable<OrganizationEntity> orgs = await OrgsAsync();
            if (orgs == null)
            {
                return true;
            }
            int countWhereAllowed = orgs.Where(o => IsFlagEnabled(o, predicate)).Count();
            return countWhereAllowed > 0;
        }

        /// <summary>
        /// Async get the given org; still returns deleted orgs
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        protected async Task<OrganizationEntity> OrgAsync(string orgRoute)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            return await db.OrgWhereRoute(orgRoute, true);
        }

        /// <summary>
        /// Returns true if the given org fails the given predicate
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected async Task<bool> IsRestrictedByOrgAsync(string orgRoute, Func<OrgNotificationSettings, bool> predicate)
        {
            OrganizationEntity org = await OrgAsync(orgRoute);
            if (org == null)
            {
                return false;
            }
            return !IsFlagEnabled(org, predicate);
        }

        #endregion

        #region INotificationService

        // Look at INotificationService for descriptions

        public async Task<bool> WelcomeAsync(User receiver)
        {
            if (!GlobalNotificationSettings.Instance.IsWelcomeEnabled || !await IsAllowedByAnyOrg(s => s.IsWelcomeEnabled))
            {
                return false;
            }

            return await Inner.WelcomeAsync(receiver);
        }

        public async Task<bool> UserInviteAsync(User receiver, User sender)
        {
            // User invite e-mails are never restricted at the org-level; though they can disable the UI for it when the user is logged into their org
            if (!GlobalNotificationSettings.Instance.IsUserInvitedEnabled)
            {
                return false;
            }

            return await Inner.UserInviteAsync(receiver, sender);
        }

        public async Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isAppInvite)
        {
            if (!GlobalNotificationSettings.Instance.IsPersonInviteEnabled || await IsRestrictedByOrgAsync(org.Route, o => o.IsPersonInviteEnabled))
            {
                return false;
            }

            return await Inner.PersonInviteAsync(receiver, sender, org, isAppInvite);
        }


        public async Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group team, bool isAppInvite)
        {
            if (!GlobalNotificationSettings.Instance.IsMemberInvitedEnabled || await IsRestrictedByOrgAsync(org.Route, o => o.IsMemberInvitedEnabled))
            {
                return false;
            }

            return await Inner.MemberInviteAsync(receiver, sender, org, team, isAppInvite);
        }

        #endregion

        #region ISbNotificationService

        // Look at ISbNotificationService for descriptions

        public async Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session)
        {
            if (!GlobalNotificationSettings.Instance.IsSessionReminderEnabled || await IsRestrictedByOrgAsync(org.Route, o => o.IsSessionReminderEnabled))
            {
                return false;
            }

            return await Inner.SessionReminderAsync(receiver, org, session);
        }

        public async Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            if (!GlobalNotificationSettings.Instance.IsSessionPublishEnabled || await IsRestrictedByOrgAsync(org.Route, o => o.IsSessionPublishEnabled))
            {
                return false;
            }

            return await Inner.SessionPublishAsync(receiver, org, session);
        }

        public async Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            if (!GlobalNotificationSettings.Instance.IsSessionHostPublishEnabled || await IsRestrictedByOrgAsync(org.Route, o => o.IsSessionHostPublishEnabled))
            {
                return false;
            }

            return await Inner.SessionHostPublishAsync(receiver, org, session);
        }

        #endregion
    }
}
