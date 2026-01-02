using Masticore;
using Masticore.Entity;

namespace Soundbite.Entity
{
    /// <summary>
    /// Extension helpers for handling settings objects
    /// </summary>
    public static class OrgSettingsExtensions
    {
        /// <summary>
        /// Gets the <see cref="OrgSettings"/> for the given <see cref="OrganizationEntity"/>
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public static OrgSettings GetSettings(this OrganizationEntity org)
        {
            if (org is null)
            {
                throw new System.ArgumentNullException(nameof(org));
            }

            string configJson = org.ConfigJson;

            return GetSettings(configJson);
        }

        /// <summary>
        /// Parses the given JSON string and optionally returns a new <see cref="OrgSettings"/> object
        /// </summary>
        /// <param name="configJson"></param>
        /// <returns></returns>
        public static OrgSettings GetSettings(string configJson)
        {
            if (configJson == null)
            {
                return null;
            }

            return JsonUtils.FromLowerCamelJson<OrgSettings>(configJson);
        }

        /// <summary>
        /// Gets the json stringified version of the given orgsettings; returning null if the object itself is null
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string ToJson(this OrgSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return settings.ToLowerCamelJson();
        }

        /// <summary>
        /// Sets the <see cref="OrgSettings"/> object for the given <see cref="OrganizationEntity"/>; accepting null as clearing the settings
        /// </summary>
        /// <param name="org"></param>
        /// <param name="orgSettings"></param>
        public static void SetSettings(this OrganizationEntity org, OrgSettings orgSettings)
        {
            if (org is null)
            {
                throw new System.ArgumentNullException(nameof(org));
            }

            if (orgSettings == null)
            {
                org.ConfigJson = null;
            }
            else
            {
                string json = orgSettings.ToJson();
                org.ConfigJson = json;
            }
        }

        /// <summary>
        /// Sets the <see cref="OrgNotificationSettings"/> for the given <see cref="OrganizationEntity"/>; this will create the root <see cref="OrgSettings"/> object for the given org if necessary
        /// </summary>
        /// <param name="org"></param>
        /// <param name="notificationSettings"></param>
        public static void SetSettings(this OrganizationEntity org, OrgNotificationSettings notificationSettings)
        {
            if (org is null)
            {
                throw new System.ArgumentNullException(nameof(org));
            }

            OrgSettings existingSettings = org.GetSettings() ?? new OrgSettings();

            if (notificationSettings == null)
            {
                existingSettings.Notifications = null;
            }
            else
            {
                existingSettings.Notifications = notificationSettings;
            }

            org.SetSettings(existingSettings);
        }

        /// <summary>
        /// Gets the <see cref="OrgNotificationSettings"/> object for the given <see cref="OrganizationEntity"/>; returning null if not found
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public static OrgNotificationSettings GetNotificationSettings(this OrganizationEntity org)
        {
            OrgSettings settings = org.GetSettings();
            if (settings == null)
            {
                return null;
            }

            return settings.Notifications;
        }

        /// <summary>
        /// Gets the <see cref="OrgPermissions"/> configuration for the given org; returning null if none found
        /// </summary>
        /// <param name="org"></param>
        /// <returns></returns>
        public static OrgPermissions GetPermissionSettings(this OrganizationEntity org)
        {
            OrgSettings settings = org.GetSettings();
            if (settings == null)
            {
                return null;
            }

            return settings.Permissions;
        }

        /// <summary>
        /// Gets the <see cref="OrgSessionSettings"/> configuration for the given org; returning null if none found
        /// </summary>
        /// <param name="org">Organization whose session settings are being sought.</param>
        /// <returns>a reference to the settings if found, otherwise <c>null</c>.</returns>
        public static OrgSessionSettings GetSessionSettings(this OrganizationEntity org)
        {
            OrgSettings settings = org.GetSettings();
            if (settings == null)
            {
                return null;
            }

            return settings.Sessions;
        }

        /// <summary>
        /// Sets the <see cref="OrgPermissions"/> for the given org; clearing them if <paramref name="permissions"/> is null
        /// </summary>
        /// <param name="org"></param>
        /// <param name="permissions"></param>
        public static void SetSettings(this OrganizationEntity org, OrgPermissions permissions)
        {
            if (org is null)
            {
                throw new System.ArgumentNullException(nameof(org));
            }

            OrgSettings existingSettings = org.GetSettings() ?? new OrgSettings();

            if (permissions == null)
            {
                existingSettings.Permissions = null;
            }
            else
            {
                existingSettings.Permissions = permissions;
            }

            org.SetSettings(existingSettings);
        }
    }
}
