using AutoMapper;
using Masticore;
using Masticore.Acs;
using Masticore.Azure.MediaServices;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Masticore.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Settings;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Implements <see cref="ISbOrganizationService"/> over Entity Framework
    /// </summary>
    public class SbOrganizationService : InfrastructureServiceBase<ISbInfrastructure>, ISbOrganizationService
    {
        #region Properties

        protected IOrganizationService Organizations { get; }
        protected ISessionService Sessions { get; }
        protected IRbac Rbac { get; }
        protected IEmailTemplates Templates { get; }
        protected ITokenDataService TokenDataService { get; }
        protected IAzureMediaServiceConfig AzureMediaServicesConfig { get; }
        protected IAzureMediaService AzMediaService { get; }

        #endregion

        #region Constructor & Support Methods

        /// <summary>
        /// Instantiates a new <see cref="SbOrganizationService"/> instance.
        /// </summary>
        public SbOrganizationService(
           IOrganizationService organizations,
           ISessionService sessions,
           ISbInfrastructure infrastructure,
           ILogger<SbOrganizationService> logger,
           IRbac rbac,
           IEmailTemplates templates,
           IMapper mapper,
           ITokenDataService tokenDataService,
           IAzureMediaServiceConfig azureMediaServicesConfig,
           IAzureMediaService azureMediaService
           )
           : base(infrastructure, logger, null, mapper)
        {
            Organizations = organizations ?? throw new ArgumentNullException(nameof(organizations));
            Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
            TokenDataService = tokenDataService;
            AzureMediaServicesConfig = azureMediaServicesConfig ?? throw new ArgumentNullException(nameof(rbac));
            AzMediaService = azureMediaService ?? throw new ArgumentNullException(nameof(AzMediaService));
            Templates = templates ?? throw new ArgumentNullException(nameof(templates));
        }

        private async Task SendTestEmailIfNeeded(OrgAzureSettings newSettings, SbDb db)
        {
            // If we're being offered new settings, then we have to confirm we can send e-mail
            if (string.IsNullOrEmpty(newSettings.AcsConnectionString))
            {
                return;
            }

            if (string.IsNullOrEmpty(newSettings.AcsFromEmail))
            {
                throw new Exception("Cannot send with empty ACS E-mail");
            }

            if (!newSettings.AcsFromEmail.IsEmail())
            {
                throw new Exception($"Cannot use given e-mail; {newSettings.AcsFromEmail} does not look like an e-mail address");
            }

            if (string.IsNullOrEmpty(newSettings.AcsFromName))
            {
                throw new Exception("Cannot send with empty ACS From Name");
            }

            UserEntity currentUser = await db.UserWhereUid(Rbac.UniversalIdForCurrentUser);
            Body welcomeTemplate = await Templates.WelcomeAsync(Mapper.MapSafe<User>(currentUser));
            MailMessage testMsg = new MailMessage
            {
                Subject = "Soundbite ACS Test E-Mail",
                Body = welcomeTemplate,
                From = new Address(newSettings.AcsFromEmail, newSettings.AcsFromName),
                To = new Address(currentUser.Email, currentUser.DisplayName())
            };
            AcsSettings acsSettings = new AcsSettings
            {
                ConnectionString = newSettings.AcsConnectionString,
                FromEmail = newSettings.AcsFromEmail,
                FromName = newSettings.AcsFromName
            };

            AcsMailbox mailbox = new AcsMailbox(acsSettings);
            await mailbox.SendAsync(testMsg);
        }

        #endregion

        #region ISbOrganizationService Implementation

        /// <inheritdoc />
        public async Task<OrganizationWithSettings> ReadAsync(string orgRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Guest);

            OrganizationWithSettings result = new OrganizationWithSettings
            {
                Details = await Organizations.ReadAsync(orgRoute)
            };
            result.Details.AssertFound($"Could not find org '{orgRoute}'");

            result.HasPublicNotifications = await Sessions.HasPublicNotifications(orgRoute);
            LoadSettings(result);
            return result;
        }

        /// <inheritdoc />
        [Obsolete("Deprecated for performance reasons; use ReadAsync")]
        public async Task<OrganizationWithPermissions_Obsolete> ReadAsync_Obsolete(string orgRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Guest);

            OrganizationWithPermissions_Obsolete ret = new OrganizationWithPermissions_Obsolete
            {
                Details = await Organizations.ReadAsync_Obsolete(orgRoute)
            };
            ret.Details.AssertFound($"Could not find org '{orgRoute}'");

            OrgPermissions perms = OrgSettingsExtensions.GetSettings(ret.Details.ConfigJson)?.Permissions ?? new OrgPermissions();
            ret.Permissions = perms;

            return ret;
        }

        /// <inheritdoc />
        public async Task<OrgNotificationSettings> ReadNotificationSettings(string orgRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            OrgSettings settings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            return settings.Notifications;
        }

        public async Task<OrgNotificationSettings> UpdateNotificationSettings(string orgRoute, OrgNotificationSettings settings)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");
            Validator.ArgNotNull(nameof(settings), settings, $"'{nameof(settings)}' cannot be null");

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            OrgSettings currentSettings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            currentSettings.Notifications = settings;
            org.ConfigJson = currentSettings.ToJson();
            await db.SaveChangesAsync();

            return await ReadNotificationSettings(orgRoute);
        }

        /// <inheritdoc />
        public async Task<OrgAzureSettings> ReadAzureSettings(string orgRoute)
        {
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);
            return await ReadAzureSettingsInternal(orgRoute);
        }

        /// <inheritdoc />
        public async Task<OrgAzureSettings> UpdateAzureSettings(string orgRoute, OrgAzureSettings newSettings)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");
            Validator.ArgNotNull(nameof(newSettings), newSettings, $"'{nameof(newSettings)}' cannot be null");

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            // Acquire the current settings and import new settings into the current settings
            OrgSettings currentSettings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            currentSettings.Azure.ImportFrom(newSettings);

            // Ensure authorization entry with curren tenantId
            await UpdateAzureAuth(org, currentSettings.Azure);

            // Verify ACS Settings
            await SendTestEmailIfNeeded(currentSettings.Azure, db);

            // Save off current settings (which contains updated values)
            currentSettings.Azure.AllowSecureDataJsonExport = true; // Must set this here or JSON will contain sentinel values
            org.ConfigJson = currentSettings.ToJson();
            await db.SaveChangesAsync();

            return await ReadAzureSettings(orgRoute);
        }

        private async Task UpdateAzureAuth(OrganizationEntity org, OrgAzureSettings settings)
        {
            SbDb db = await Infrastructure.DbAsync();
            OrganizationAuthProviderEntity entry = await db.OrganizationAuthProviders
                .FirstOrDefaultAsync(i => i.OrganizationId == org.Id && i.ProviderType == AuthProviderType.Azure);

            // Determine whether the tenant ID is set
            if (!string.IsNullOrEmpty(settings.TenantId))
            {
                // Tenant ID exists so the entry should exist and have the tenant ID
                if (entry == null)
                {
                    entry = new OrganizationAuthProviderEntity();
                    entry.NewRoute();
                    entry.OrganizationId = org.Id;
                    entry.ProviderType = AuthProviderType.Azure;
                    entry.ProviderUid = settings.TenantId;
                    entry.SetCreatedFields(null);
                    entry.SetUpdatedFields(SecurityContext);
                    db.OrganizationAuthProviders.Add(entry);
                }
                else
                {
                    // Entry exists so update it if necessary
                    if (entry.ProviderUid != settings.TenantId.Trim() || entry.DeletedUtc != null)
                    {
                        entry.ProviderUid = settings.TenantId.Trim();
                        entry.DeletedUtc = null;
                        db.OrganizationAuthProviders.Update(entry);
                    }
                }
            }
            else
            {
                // No Tenant ID so soft delete the entry if it exists
                if (entry != null)
                {
                    entry.SoftDelete();
                    db.OrganizationAuthProviders.Update(entry);
                }
            }
        }

        /// <inheritdoc />
        public async Task<OrgPermissions> ReadPermissions(string orgRoute, bool ignoreSecurity = false)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");

            if (!ignoreSecurity)
            {
                await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);
            }

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            OrgSettings settings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            return settings.Permissions;
        }

        /// <inheritdoc />
        public async Task<OrgPermissions> UpdatePermissions(string orgRoute, OrgPermissions settings)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");
            Validator.ArgNotNull(nameof(settings), settings, $"'{nameof(settings)}' cannot be null");

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            OrgSettings currentSettings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            currentSettings.Permissions = settings;
            org.ConfigJson = currentSettings.ToJson();
            await db.SaveChangesAsync();

            return await ReadPermissions(orgRoute);
        }

        /// <inheritdoc />
        public async Task<Theme> UpdateTheme(string orgRoute, Theme theme)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");
            // null theme object is allowed

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            if (theme != null)
            {
                theme.PublishedUtc = Time.UtcNow.ToString("s");
            }

            OrgSettings currentSettings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            currentSettings.Theme = theme;
            org.ConfigJson = currentSettings.ToJson();
            await db.SaveChangesAsync();

            return theme;
        }

        public async Task<OrgSessionSettings> ReadSessionSettings(string orgRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);
            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");
            OrgSettings settings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            return settings.Sessions;
        }

        public async Task<OrgSessionSettings> UpdateSessionSettings(string orgRoute, OrgSessionSettings settings)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");
            Validator.ArgNotNull(nameof(settings), settings, $"'{nameof(settings)}' cannot be null");
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);
            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");
            OrgSettings currentSettings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            currentSettings.Sessions = settings;
            org.ConfigJson = currentSettings.ToJson();
            await db.SaveChangesAsync();
            return await ReadSessionSettings(orgRoute);
        }

        /// <inheritdoc />
        public async Task SetSecurityTokenValue(string orgRoute, string tokenValue)
        {
            // Only allow global administrators to perform this operation
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            await Rbac.AssertCurrentUserInRole(UserRole.God);

            // Acquire the settings to ensure we have defaults, delete the settings, then add them back with new token
            ITokenServiceSettings tokenSettings = await TokenDataService.GetTokenSettingsByOrgRoute(orgRoute);
            await TokenDataService.DeleteOrgTokenSettings(orgRoute);
            await TokenDataService.SetOrgTokenSettings(orgRoute, new TokenServiceSettings()
            {
                ClockSkewInSeconds = tokenSettings.ClockSkewInSeconds,
                Level = TokenSecurityLevel.Organization,
                RefreshTokenTimeoutInMinutes = tokenSettings.RefreshTokenTimeoutInMinutes,
                SecurityType = TokenSecurityType.SecretKey,
                TokenTimeoutInMinutes = tokenSettings.TokenTimeoutInMinutes,
                Config = tokenValue
            });

            await EnsureAmsContentKeyPolicy(orgRoute, tokenValue);
        }

        /// <inheritdoc />
        public async Task<string> SyncOrgTokenWithAmsContentKeyPolicy(string orgRoute)
        {
            Validator.NotNullOrEmpty(nameof(orgRoute), orgRoute);
            ITokenServiceSettings tokenSettings = await TokenDataService.GetTokenSettingsByOrgRoute(orgRoute);
            Validator.NotNull("Token Settings", tokenSettings, $"Cannot ensure the AMS content key policy because the token settings for the organization (route='{orgRoute}') was not found.");
            string result = await EnsureAmsContentKeyPolicy(orgRoute, tokenSettings.Config);
            return result;
        }

        /// <inheritdoc />
        public async Task<string> EnsureAmsContentKeyPolicy(string orgRoute, string tokenValue)
        {
            string result;

            if (string.IsNullOrEmpty(tokenValue))
            {
                ITokenServiceSettings tokenSettings = await TokenDataService.GetTokenSettingsByOrgRoute(orgRoute);
                tokenValue = tokenSettings.Config;
            }

            Validator.ArgNotNullOrEmpty(nameof(tokenValue), tokenValue);

            // Update the azure media service content key policy with the new token setting (if applicable)
            OrgAzureSettings azureSettings = await ReadAzureSettings(orgRoute);

            if (azureSettings != null
                && !string.IsNullOrEmpty(azureSettings.MediaServiceResourceGroupName)
                && !string.IsNullOrEmpty(azureSettings.MediaServiceAccountName))
            {
                // Acquire the org
                SbDb db = await Infrastructure.DbAsync();
                OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
                org.AssertFound();

                // Soundbite stores the token keys as base64 encoded text.
                byte[] keyValue = Encoding.ASCII.GetBytes(Encoding.UTF8.GetString(Convert.FromBase64String(tokenValue)));

                // Create or Update the Content Key Policy
                string name = AzureMediaService.BuildOrgContentPolicyKeyName(org.UniversalId);

                AzMediaService.SetConnectionInfo(AzureMediaServicesConfig.SubscriptionId, AzureMediaServicesConfig.ResourceGroupName, azureSettings.MediaServiceAccountName);
                await AzMediaService.AddOrUpdateOrgContentKeyPolicy(
                    name,
                    $"Org Content Key - {org.Name}",
                    AzureMediaServicesConfig.MediaTokenIssuer,
                    AzureMediaServicesConfig.MediaTokenAudience,
                    keyValue);

                result = "SUCCESS";
            }
            else
            {
                result = "NO ATTEMPT";
            }

            return result;
        }

        /// <summary>
        /// Reads the Azure Media Services (AMS) streaming URL prefix for the organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose AMS streaming URL prefix is being sought.</param>
        /// <returns>a string containing the AMS streaming ULR prefix or <c>null</c> if the value is not set.</returns>
        public async Task<string> ReadAmsStreamingUrl(string orgRoute)
        {
            OrgAzureSettings settings = await ReadAzureSettingsInternal(orgRoute);
            return settings?.StreamingEndpointUrlPrefix;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        private async Task<OrgAzureSettings> ReadAzureSettingsInternal(string orgRoute)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute, $"'{nameof(orgRoute)}' cannot be null or empty");

            SbDb db = await Infrastructure.DbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            org.AssertFound($"Could not find org '{orgRoute}'");

            OrgSettings settings = OrgSettingsExtensions.GetSettings(org.ConfigJson) ?? new OrgSettings();
            OrgAzureSettings result = settings.Azure;

            return result;
        }

        /// <summary>
        /// Unpacks the <see cref="OrgSettings"/> object for the given <see cref="OrganizationWithSettings"/>
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="result"></param>
        private static void LoadSettings(OrganizationWithSettings result)
        {
            OrgSettings settings = OrgSettingsExtensions.GetSettings(result.Details.ConfigJson) ?? new OrgSettings();
            result.Settings = settings;
            result.Permissions = settings.Permissions; // Set here for legacy support
        }

        #endregion
    }
}
