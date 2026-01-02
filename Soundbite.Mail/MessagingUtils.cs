using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Soundbite.Entity;
using Soundbite.Models;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// Extension methods for <see cref="TeamsNotificationService"/>
    /// </summary>
    public static class MessagingUtils
    {
        /// <summary>
        /// Returns true if the given <see cref="OrgSettings"/> has MS teams enabled
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static bool IsMsTeamsEnabled(this OrgSettings settings)
        {
            return settings?.Azure?.EnableTeamsNotifications ?? false;
        }

        /// <summary>
        /// Determine if the given <see cref="OrganizationEntity"/> has MS Teams enabled
        /// </summary>
        /// <remarks>Must have a fully loaded <see cref="OrganizationEntity.ConfigJson"/> field</remarks>
        /// <param name="orgEnt"></param>
        /// <returns></returns>
        public static bool IsMsTeamsEnabled(this OrganizationEntity orgEnt)
        {
            OrgSettings settings = OrgSettingsExtensions.GetSettings(orgEnt.ConfigJson) ?? new OrgSettings();
            // Teams must be explicitly enabled by an org; so default to false
            return IsMsTeamsEnabled(settings);
        }

        /// <summary>
        /// Async determine if the given org has MS Teams enabled
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        public static async Task<bool> IsMsTeamsEnabled(this IIdentityDb db, string orgRoute)
        {
            if (string.IsNullOrEmpty(orgRoute))
            {
                return false;
            }

            OrganizationEntity orgEnt = await db.OrgWhereRoute(orgRoute);
            orgEnt.AssertFound($"Could not find org '{orgRoute}'");
            return orgEnt.IsMsTeamsEnabled();
        }

        /// <summary>
        /// Generates a string that deep links into the given <paramref name="session"/>.
        /// </summary>
        /// <param name="settings">TeamsAppAzureSettings containing the base URL for building the link.</param>
        /// <param name="session">Session for which the link is being built.</param>
        /// <returns>a string containing a URL for deep linking directly to the session in MS Teams.</returns>
        public static string DeepLinkForPlay(this TeamsAppAzureSettings settings, SessionEntity session)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            if (session == null)
            {
                return settings.BaseUrl;
            }

            return settings.BaseUrl + $"#sbplay={session.Route}";
        }

        /// <summary>
        /// Generates a string that deep links into the given <paramref name="sessionRoute"/>.
        /// </summary>
        /// <param name="settings">TeamsAppAzureSettings containing the base URL for building the link.</param>
        /// <param name="sessionRoute">Route of the session for which the link is being built.</param>
        /// <returns>a string containing a URL for deep linking directly to the session in MS Teams.</returns>
        public static string DeepLinkForPlay(this TeamsAppAzureSettings settings, string sessionRoute)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            if (string.IsNullOrEmpty(sessionRoute))
            {
                return settings.BaseUrl;
            }

            return settings.BaseUrl + $"#sbplay={sessionRoute}";
        }

        /// <summary>
        /// Generates a string that deep links into the given <see cref="Session"/>
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string DeepLinkForPlay(this TeamsAppAzureSettings settings, Session session)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            if (session == null)
            {
                return settings.BaseUrl;
            }

            return settings.BaseUrl + $"#sbplay={session.Route}";
        }

        public static string DeepLinkForRecord(this TeamsAppAzureSettings settings, SessionEntity session)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            if (session == null)
            {
                return settings.BaseUrl;
            }

            return settings.BaseUrl + $"#sbrecord={session.Route}";
        }

        public static string DeepLinkForRecord(this TeamsAppAzureSettings settings, Session session)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            if (session == null)
            {
                return settings.BaseUrl;
            }

            return settings.BaseUrl + $"#sbrecord={session.Route}";
        }
    }
}
