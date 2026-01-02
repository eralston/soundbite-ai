using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class EnvSettingsTests : EntityTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        /// <summary>
        /// Populates a consistent set of environment variables.
        /// </summary>
        /// <param name="db">Reference to the identity DB in which to populate environment variables.</param>
        private async Task PopulateEnvSettings(MockIdentityDb db)
        {
            await db.SaveEnvSetting(new EnvSetting()
            {
                Name = "Setting A",
                Description = "Description of Setting A",
                GroupKey = "Group1",
                IsSecure = true,
                Value = "Setting A Value"
            });
            await db.SaveEnvSetting(new EnvSetting()
            {
                Name = "Setting B",
                Description = "Description of Setting B",
                GroupKey = "Group1",
                IsSecure = false,
                Value = "Setting B Value"
            });
            await db.SaveEnvSetting(new EnvSetting()
            {
                Name = "Setting C",
                Description = "Description of Setting C",
                GroupKey = null,
                IsSecure = false,
                Value = "Setting C Value"
            });
            await db.SaveEnvSetting(new EnvSetting()
            {
                Name = "Setting D",
                Description = "Description of Setting D",
                GroupKey = "Group2",
                IsSecure = false,
                Value = "Setting D Value"
            });
            await db.SaveEnvSetting(new EnvSetting()
            {
                Name = "Setting E",
                Description = "Description of Setting E",
                GroupKey = "Group2",
                IsSecure = false,
                Value = "Setting E Value"
            });
            await db.SaveEnvSetting(new EnvSetting()
            {
                Name = "Setting F",
                Description = "Description of Setting F",
                GroupKey = "Group2",
                IsSecure = false,
                Value = "Setting F Value"
            });
        }

        /// <summary>
        /// Test to ensure that settings can be retrieved and are populated propertly.
        /// </summary>
        [Fact]
        public async Task ReadEnvSettingByName()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            await PopulateEnvSettings(db);

            // ACT
            EnvSetting settingA = await db.ReadEnvSettingByName("Setting A");

            // ASSERT
            Assert.NotNull(settingA);
            Assert.Equal("Setting A", settingA.Name);
            Assert.Equal("Description of Setting A", settingA.Description);
            Assert.Equal("Group1", settingA.GroupKey);
            Assert.True(settingA.IsSecure);
            Assert.Equal("Setting A Value", settingA.Value);
        }

        /// <summary>
        /// Test to ensure environment variables can be reliably retrieved by group key.
        /// </summary>
        [Fact]
        public async Task ReadEnvSettingByGroup()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            await PopulateEnvSettings(db);

            // ACT
            IList<EnvSetting> settingsGrp1 = await db.ReadEnvSettingsByGroupKey("Group1");
            IList<EnvSetting> settingsGrp2 = await db.ReadEnvSettingsByGroupKey("Group2");
            IList<EnvSetting> settingsGrp3 = await db.ReadEnvSettingsByGroupKey(null);

            // ASSERT
            Assert.NotNull(settingsGrp1);
            Assert.NotNull(settingsGrp2);
            Assert.NotNull(settingsGrp3);
            Assert.Equal(2, settingsGrp1.Count);
            Assert.Equal(3, settingsGrp2.Count);
            Assert.Equal(1, settingsGrp3.Count);
            Assert.Equal("Setting A", settingsGrp1[0].Name);
            Assert.Equal("Setting D", settingsGrp2[0].Name);
            Assert.Equal("Setting C", settingsGrp3[0].Name);
        }

        /// <summary>
        /// Test to ensure the value defaults to the null group key when multiples found.
        /// </summary>
        [Fact]
        public async Task PickupNullGroupKeyOnMultipleFound()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            await PopulateEnvSettings(db);
            await db.SaveEnvSetting(new EnvSetting()
            {
                Name = "Setting A",
                Description = "Description of Setting A - Null Group Key",
                GroupKey = null,
                IsSecure = true,
                Value = "Setting A Value - Null Group Key"
            });

            // ACT
            EnvSetting settingA = await db.ReadEnvSettingByName("Setting A");

            // ASSERT
            Assert.NotNull(settingA);
            Assert.Equal("Setting A", settingA.Name);
            Assert.Null(settingA.GroupKey);
        }

        /// <summary>
        /// Test to ensure an exception is thrown if requested and value not found.
        /// </summary>
        [Fact]
        public async Task ThrowOnNotFound()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            await PopulateEnvSettings(db);

            // ACT
            EnvSetting settingZ = await db.ReadEnvSettingByName("Setting Z", null, false);
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                settingZ = await db.ReadEnvSettingByName("Setting Z");
            });

            // ASSERT
            // By virtue of getting here the test was successful

        }
    }
}
