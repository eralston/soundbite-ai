using AutoMapper;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// A service for managing tokens and related token data.
    /// </summary>
    public class TokenDataService : InfrastructureServiceBase<IIdentityInfrastructure>, ITokenDataService
    {
        #region Properties

        protected IRbac Rbac { get; set; }

        #endregion

        #region Constructor

        public TokenDataService(
            ISecurityContext securityContext,
            IIdentityInfrastructure infrastructure,
            IRbac rbac,
            ILogger<TokenDataService> logger,
            IMapper mapper
            ) : base(infrastructure, logger, securityContext, mapper)
        {
            Rbac = rbac;
        }

        #endregion

        #region ITokenDataService Implementation

        /// <summary>
        /// Responsible for issuing a user and refresh token in response to a successful login.
        /// </summary>
        /// <param name="orgRoute">Route of the organization for which a token is being issued.</param>
        /// <returns>a <see cref="TokenInfo" /> instance containing a user token and a refresh token.</returns>
        public async Task<TokenInfo> CreateUserToken(string orgRoute)
        {
            ITokenServiceSettings settings = orgRoute == Constants.UserOnlyOrgRoute
                ? TokenServiceSettings.DefaultTokenServiceSettings
                : await GetTokenSettingsByOrgRoute(orgRoute);
            TokenServiceFactory factory = new TokenServiceFactory(Logger);
            ITokenService tokenService = factory.GetTokenService(settings);
            string token = tokenService.GenerateToken(
                new Claim(Constants.Tokens.UserRouteClaim, SecurityContext.CurrentUser.Route),
                new Claim(Constants.Tokens.OrgRouteClaim, orgRoute),
                new Claim(Constants.Tokens.AudienceClaim, "sbuser")
                );

            TokenInfo result = new TokenInfo()
            {
                Token = token,
                RefreshToken = RefreshToken.Create(settings, SecurityContext.CurrentUser.Route)
            };

            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            UserEntity user = await db.UserWhereRoute(SecurityContext.CurrentUser.Route);

            //======================================================================================
            //TODO: this GEM of a piece of code is handling a very specific situation. When an OKTA
            //  user has been synchronized but has not yet logged in, the UID that is set by the
            //  login mechanism is somehow persisted to the database but not picked up by entity
            //  framework in the UserWhereRoute call above. This results in the UID being cleared
            //  out when we save below and screwing up RBAC security checks in future calls. So we
            //  set it here because it works, but it would be nice to have it work more betterer.
            if (user.UniversalId == null && !string.IsNullOrEmpty(SecurityContext.CurrentUser.UniversalId))
            {
                user.UniversalId = SecurityContext.CurrentUser.UniversalId;
            }
            //======================================================================================

            user.RefreshToken = result.RefreshToken;
            db.Users.Update(user);
            await db.SaveChangesAsync();


            return result;
        }

        /// <summary>
        /// Deletes the token settings for an organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose settings are being deleted.</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        public async Task DeleteOrgTokenSettings(string orgRoute)
        {
            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Admin);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            // Acquire all active items (there should only be one but we're being careful here)
            IList<TokenSettingsEntity> activeItems = await db.TokenSettings
                .Where(i => i.Organization.Route == orgRoute && i.DeletedUtc == null)
                .ToListAsync();

            // Mark any active items as deleted (soft)
            activeItems.ForEach(i =>
            {
                i.DeletedUtc = DateTime.UtcNow;
                db.TokenSettings.Update(i);
            });
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the token settings for a tenant.
        /// </summary>
        /// <param name="tenantRoute">Route of the organization whose settings are being persisted.</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        public Task DeleteTenantTokenSettings(string tenantRoute, ITokenServiceSettings settings)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Responsible for validating the incoming refresh token and, if the refresh token is 
        /// valid, issuing a new user and refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token used to acquire the new token.</param>
        /// <param name="orgRoute">Route of the organization associated with the token.</param>
        /// <returns>a <see cref="TokenInfo"/> instance containing a user token and a refresh token.</returns>
        public async Task<TokenInfo> RefreshUserToken(string refreshToken, string orgRoute)
        {
            Validator.ArgNotNull(nameof(refreshToken), refreshToken);

            TokenInfo result = null;
            RefreshToken refreshTokenInfo = RefreshToken.Parse(refreshToken);

            if (!refreshTokenInfo.IsExpired)
            {
                IIdentityDb db = await Infrastructure.IdentityDbAsync();
                UserEntity currentUser = await db.UserWhereRoute(SecurityContext.CurrentUser.Route);

                // Token is not expired so verify that the refresh token matches expected value
                if (currentUser.RefreshToken == refreshToken)
                {
                    result = await CreateUserToken(orgRoute);
                }
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// Sets the token settings for an organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose settings are being persisted.</param>
        /// <param name="settings">Settings to persist.</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        public async Task SetOrgTokenSettings(string orgRoute, ITokenServiceSettings settings)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);
            Validator.ArgNotNull(nameof(settings), settings);
            Validator.SecurityContextHasTenant(SecurityContext);

            // Ensure the level is set appropriately
            settings.Level = TokenSecurityLevel.Organization;

            // Validate Current User
            await Rbac.AssertCurrentUser();

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            // validate
            OrganizationEntity org = await db.Organizations.Where(i =>
                i.Route == orgRoute
                && i.DeletedUtc == null
                && i.TenantId == SecurityContext.CurrentTenantId
                && i.Tenant.DeletedUtc == null
                ).FirstOrDefaultAsync();
            Validator.NotNull(org, $"Organization '{orgRoute}' was not found.");

            // Acquire existing settings if possible 
            TokenSettingsEntity tokenSettings = await db.TokenSettings.Where(i =>
                i.DeletedUtc == null
                && i.TenantId == SecurityContext.CurrentTenantId
                && i.OrganizationId == org.Id).FirstOrDefaultAsync();

            // Create a new entity if necessary
            if (tokenSettings == null)
            {
                tokenSettings = new TokenSettingsEntity();
                tokenSettings.NewRoute();
                tokenSettings.TenantId = SecurityContext.CurrentTenantId;
                tokenSettings.OrganizationId = org.Id;
                db.TokenSettings.Add(tokenSettings);
            }
            else
            {
                tokenSettings.SetUpdatedFields(SecurityContext);
            }

            // Populate and save
            tokenSettings.Config = JsonUtils.ToLowerCamelJson(settings);

            if (tokenSettings.Id > 0)
            {
                db.Update(tokenSettings);
            }

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Sets the token settings for a tenant.
        /// </summary>
        /// <param name="tenantRoute">Route of the organization whose settings are being persisted.</param>
        /// <param name="settings">Settings to persist.</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        public async Task SetTenantTokenSettings(string tenantRoute, ITokenServiceSettings settings)
        {
            Validator.ArgNotNull(nameof(tenantRoute), tenantRoute);
            Validator.ArgNotNull(nameof(settings), settings);
            Validator.SecurityContextHasTenant(SecurityContext);

            // Ensure the level is set appropriately
            settings.Level = TokenSecurityLevel.Tenant;

            await Rbac.AssertCurrentUserInRole(UserRole.God);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings);
            IIdentityDb db = await Infrastructure.IdentityDbAsync();

            // validate
            TenantEntity tenant = await db.Tenants.Where(i => i.Route == tenantRoute && i.DeletedUtc == null).FirstOrDefaultAsync();
            Validator.NotNull(tenant, $"Tenant '{tenantRoute}' was not found.");

            // Acquire existing settings if possible 
            TokenSettingsEntity tokenSettings = await db.TokenSettings.Where(i =>
                i.DeletedUtc == null
                && i.TenantId == tenant.Id
                && i.OrganizationId == null).FirstOrDefaultAsync();

            // Create a new entity if necessary
            if (tokenSettings == null)
            {
                tokenSettings = new TokenSettingsEntity();
                tokenSettings.NewRoute();
                tokenSettings.TenantId = SecurityContext.CurrentTenantId;
                tokenSettings.OrganizationId = null;
                db.TokenSettings.Add(tokenSettings);
            }
            else
            {
                tokenSettings.SetUpdatedFields(SecurityContext);
            }

            // Populate and save
            tokenSettings.Config = JsonUtils.ToLowerCamelJson(settings);

            if (tokenSettings.Id > 0)
            {
                db.Update(tokenSettings);
            }

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the settings for the specified organization / tenant.  If the organization does not 
        /// have explicit settings defined, this method falls back to the settings defined on the
        /// tenant.  If tenant has no settings, defaults for the application are returned.
        /// </summary>
        /// <param name="tenantRoute">Route of the tenant.</param>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <returns></returns>
        public async Task<TokenServiceSettings> GetTokenSettingsByOrgRoute(string orgRoute)
        {
            Validator.ArgNotNull(nameof(orgRoute), orgRoute);

            ITokenServiceSettings innerResult = null;
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.OrgWhereRoute(orgRoute);
            Validator.NotNull(org, $"Organization '{orgRoute}' was not found.");

            TokenSettingsEntity settings = await db.TokenSettings.Where(i =>
                i.TenantId == org.TenantId && i.Tenant.DeletedUtc == null
                &&
                (
                    i.OrganizationId == null
                    || (i.Organization.Route == orgRoute && i.Organization.DeletedUtc == null && i.TenantId == org.TenantId)
                )).OrderByDescending(i => i.OrganizationId).FirstOrDefaultAsync();

            if (settings != null)
            {
                // Deserialize the setting and make sure the level value is set correctly (it should be but better to ensure it)
                innerResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenServiceSettings>(settings.Config);
                innerResult.Level = settings.OrganizationId >= 0 ? TokenSecurityLevel.Organization : TokenSecurityLevel.Tenant;
            }

            TokenServiceSettings result = Mapper.MapSafe<TokenServiceSettings>(innerResult)
                ?? TokenServiceSettings.DefaultTokenServiceSettings;

            return result;
        }

        /// <summary>
        /// Gets the settings for the specified organization / tenant.  If the organization does not 
        /// have explicit settings defined, this method falls back to the settings defined on the
        /// tenant.  If tenant has no settings, defaults for the application are returned.
        /// </summary>
        /// <param name="orgUniversalId">Universal ID of the organization.</param>
        /// <returns>an <see cref="ITokenServiceSettings"/> implementation populated with settings applicable to the organization.</returns>
        public async Task<TokenServiceSettings> GetTokenSettingsByOrgUniversalId(string orgUniversalId)
        {
            Validator.ArgNotNull(nameof(orgUniversalId), orgUniversalId);
            ITokenServiceSettings innerResult = null;
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            OrganizationEntity org = await db.Organizations.Where(i => i.UniversalId == orgUniversalId).FirstOrDefaultAsync();
            Validator.NotNull(org, $"Organization with Universal ID '{orgUniversalId}' was not found.");

            TokenSettingsEntity settings = await db.TokenSettings.Where(i =>
                i.TenantId == org.TenantId && i.Tenant.DeletedUtc == null
                &&
                (
                    i.OrganizationId == null
                    || (i.Organization.UniversalId == orgUniversalId && i.Organization.DeletedUtc == null && i.TenantId == org.TenantId)
                )).OrderByDescending(i => i.OrganizationId).FirstOrDefaultAsync();

            if (settings != null)
            {
                // Deserialize the setting and make sure the level value is set correctly (it should be but better to ensure it)
                innerResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenServiceSettings>(settings.Config);
                innerResult.Level = settings.OrganizationId >= 0 ? TokenSecurityLevel.Organization : TokenSecurityLevel.Tenant;
            }

            TokenServiceSettings result = Mapper.MapSafe<TokenServiceSettings>(innerResult)
                ?? TokenServiceSettings.DefaultTokenServiceSettings;

            return result;
        }

        #endregion
    }
}