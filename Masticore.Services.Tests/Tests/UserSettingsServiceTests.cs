using Masticore.Entity.Tests;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class UserSettingsServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        #region Classes

        private class MockUserSettings : IUserSettings
        {
            public int SettingA { get; set; } = 100;
        }

        #endregion

        [Fact]
        public async Task ReadConfig()
        {
            // Arrange
            ILogger<UserSettingsService> logger = new NullLogger<UserSettingsService>();
            ISecurityContext securityContext = new MockSecurityContext();
            UserSettingsService uss = new UserSettingsService(securityContext, Builder.Infrastructure.Value, MockMapper.Instance, logger);
            MockUserSettings orgSettings = new MockUserSettings() { SettingA = 1000 };
            await uss.SaveConfigAsync(IdentityDbSeed.UserRegular.Route, orgSettings);
            orgSettings = await uss.ReadConfigAsync<MockUserSettings>(IdentityDbSeed.UserRegular.Route);
            Assert.NotNull(orgSettings);
            Assert.Equal(1000, orgSettings.SettingA);
        }

        [Fact]
        public async Task WriteConfig()
        {
            // Arrange
            ILogger<UserSettingsService> logger = new NullLogger<UserSettingsService>();
            ISecurityContext securityContext = new MockSecurityContext();
            UserSettingsService oss = new UserSettingsService(securityContext, Builder.Infrastructure.Value, MockMapper.Instance, logger);
            MockUserSettings orgSettings = new MockUserSettings() { SettingA = 1000 };
            await oss.SaveConfigAsync(IdentityDbSeed.UserRegular.Route, orgSettings);
            orgSettings = await oss.ReadConfigAsync<MockUserSettings>(IdentityDbSeed.UserRegular.Route);
            Assert.NotNull(orgSettings);
            Assert.Equal(1000, orgSettings.SettingA);
        }
    }

}