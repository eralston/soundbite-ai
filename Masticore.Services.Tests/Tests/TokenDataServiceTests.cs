using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Token;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class TokenDataServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="TokenDataServiceTests"/> instance.
        /// </summary>
        public TokenDataServiceTests()
        {
            // Setup the default "constructor" for the TokenDataService used in these tests
            Builder.TokenDataService.Use(i => new TokenDataService(
                i.SecurityContext.Value,
                i.Infrastructure.Value,
                i.Rbac.Value,
                i.Logger<TokenDataService>(),
                i.Mapper.Value
            ));
        }

        #endregion

        [Fact]
        public async Task SetTenantTokenSettings_AsAppAdmin()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            MockIdentityDb db = Builder.DbContext.Value;
            ITokenDataService service = Builder.TokenDataService.Value;
            Assert.Empty(db.TokenSettings);

            // ACT
            await service.SetTenantTokenSettings(IdentityDbSeed.Tenant.Route, new TokenServiceSettings()
            {
                ClockSkewInSeconds = 100,
                Config = "Test",
                RefreshTokenTimeoutInMinutes = 10,
                SecurityType = TokenSecurityType.SecretKey,
                TokenTimeoutInMinutes = 60
            });

            // ASSERT
            TokenSettingsEntity item = await db.TokenSettings.Where(i =>
                i.TenantId == IdentityDbSeed.Tenant.Id
                && i.OrganizationId == null).FirstOrDefaultAsync();

            Assert.NotNull(item);
            Assert.Equal(IdentityDbSeed.Tenant.Id, item.TenantId);
            Assert.Null(item.OrganizationId);
            Assert.NotNull(item.Config);
            Assert.NotNull(item.Route);
        }

        [Fact]
        public async Task SetOrgTokenSettings_AsAppAdmin()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            ITokenDataService service = Builder.TokenDataService.Value;
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Empty(db.TokenSettings);

            // ACT
            await service.SetOrgTokenSettings(IdentityDbSeed.OrgPrimary.Route, new TokenServiceSettings()
            {
                ClockSkewInSeconds = 100,
                Config = "Test",
                RefreshTokenTimeoutInMinutes = 10,
                SecurityType = TokenSecurityType.SecretKey,
                TokenTimeoutInMinutes = 60
            });

            // ASSERT
            TokenSettingsEntity item = await db.TokenSettings.Where(i =>
                i.TenantId == IdentityDbSeed.Tenant.Id
                && i.OrganizationId == IdentityDbSeed.OrgPrimary.Id
            ).FirstOrDefaultAsync();

            Assert.NotNull(item);
            Assert.Equal(IdentityDbSeed.Tenant.Id, item.TenantId);
            Assert.Equal(IdentityDbSeed.OrgPrimary.Id, item.OrganizationId);
            Assert.NotNull(item.Config);
            Assert.NotNull(item.Route);
        }


        [Fact]
        public async Task SetTokenSettings_PropogatesCorrectly()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            ITokenDataService service = Builder.TokenDataService.Value;
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Empty(db.TokenSettings);
            OrganizationEntity orgSecondary = db.MockOrg("SecondaryOrg");
            await db.SaveChangesAsync();

            // ACT
            await service.SetTenantTokenSettings(IdentityDbSeed.Tenant.Route, new TokenServiceSettings()
            {
                ClockSkewInSeconds = 100,
                Config = "Tenant",
                RefreshTokenTimeoutInMinutes = 10,
                SecurityType = TokenSecurityType.SecretKey,
                TokenTimeoutInMinutes = 60
            });

            await service.SetOrgTokenSettings(IdentityDbSeed.OrgPrimary.Route, new TokenServiceSettings()
            {
                ClockSkewInSeconds = 100,
                Config = "Org",
                RefreshTokenTimeoutInMinutes = 10,
                SecurityType = TokenSecurityType.SecretKey,
                TokenTimeoutInMinutes = 60
            });

            ITokenServiceSettings settingsForOrg = await service.GetTokenSettingsByOrgRoute(IdentityDbSeed.OrgPrimary.Route);
            ITokenServiceSettings settingsForOtherOrg = await service.GetTokenSettingsByOrgRoute(orgSecondary.Route);
            ITokenServiceSettings settingsForOtherOrgInOtherTenant = await service.GetTokenSettingsByOrgRoute(IdentityDbSeed.OrgOutside.Route);

            // ASSERT

            Assert.Equal("Org", settingsForOrg.Config);                     // Org should have it's own settings
            Assert.Equal("Tenant", settingsForOtherOrg.Config);             // Other org should have the tenant settings
            Assert.Equal(TokenServiceSettings.DefaultTokenServiceSettings,  // Other org in other tenant should have default
                    settingsForOtherOrgInOtherTenant);
        }
    }
}