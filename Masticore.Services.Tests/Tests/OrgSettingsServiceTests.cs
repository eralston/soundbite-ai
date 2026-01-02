using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class OrgSettingsServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        #region Classes

        private class MockOrgSettings : IOrgSettings
        {
            public int SettingA { get; set; } = 100;
        }

        #endregion

        [Fact]
        public async Task ReadConfig()
        {
            // Arrange
            ILogger<OrganizationService> logger = new NullLogger<OrganizationService>();
            ISecurityContext securityContext = new MockSecurityContext();
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), Builder.Infrastructure.Value, Builder.SecurityContext.Value);
            OrgSettingsService oss = new OrgSettingsService(securityContext, Builder.Infrastructure.Value, MockMapper.Instance, logger, dbRbac);

            MockOrgSettings orgSettings = new MockOrgSettings() { SettingA = 1000 };
            await oss.SaveOrgSettingsAsync(OrgRoute, orgSettings);
            orgSettings = await oss.ReadOrgSettingsAsync<MockOrgSettings>(OrgRoute);
            Assert.NotNull(orgSettings);
            Assert.Equal(1000, orgSettings.SettingA);
        }

        [Fact]
        public async Task WriteConfig()
        {
            // Arrange
            ILogger<OrganizationService> logger = new NullLogger<OrganizationService>();
            ISecurityContext securityContext = new MockSecurityContext();
            IRbac dbRbac = new CurrentUserDbRbac(Builder.Logger<CurrentUserDbRbac>(), Builder.Infrastructure.Value, Builder.SecurityContext.Value);
            OrgSettingsService oss = new OrgSettingsService(securityContext, Builder.Infrastructure.Value, MockMapper.Instance, logger, dbRbac);

            MockOrgSettings orgSettings = new MockOrgSettings() { SettingA = 1000 };
            await oss.SaveOrgSettingsAsync(OrgRoute, orgSettings);
            orgSettings = await oss.ReadOrgSettingsAsync<MockOrgSettings>(OrgRoute);
            Assert.NotNull(orgSettings);
            Assert.Equal(1000, orgSettings.SettingA);
        }
    }

}