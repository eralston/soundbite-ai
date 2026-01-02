using Masticore.DirectorySync.Aad;
using Masticore.DirectorySync.Tests.Mocks;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Tests
{
    public class OrgSyncStore_PersonStoreTests
    {
        private readonly ILogger<OrgSyncStore> StoreLogger = NullLogger<OrgSyncStore>.Instance;

        private static OrgSyncConfig CreateOrgSyncConfig()
        {
            SyncStrategyConfigReader syncStrategyConfigReader = new SyncStrategyConfigReader();
            syncStrategyConfigReader.ImportPhoneNumbers = true;
            syncStrategyConfigReader.SyncType = "Test";
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route,
                SyncConfigJson = syncStrategyConfigReader.ToLowerCamelJson(),
                SyncType = syncStrategyConfigReader.SyncType,
            };
            return orgConfig;
        }

        [Fact]
        public async Task AddPerson_AddOrUpdate_New()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            MockSyncUser addOrUpdateUser = new MockSyncUser(nameof(addOrUpdateUser));
            await orgStore.AddPerson(addOrUpdateUser, SyncOpType.AddOrUpdate);

            await orgStore.EndSync();

            // ASSERT - A new user and person for the new user
            Assert.Equal(1, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(1, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);

            dbProvider.AssertAdd(1, 1);
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, addOrUpdateUser);

            UserEntity newUserEntity = dbProvider.Db.Users.Where(u => u.Email == addOrUpdateUser.Email.ToEmail(true)).Single();
            Assert.Null(newUserEntity.UniversalId);
            Assert.Equal(UserRole.User, newUserEntity.UserRole);
        }

        [Fact]
        public async Task AddPerson_AddOrUpdate_ExistingUserAndPerson()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();
            UserEntity existingUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserRegular.Route);
            existingUser.Phone = null; // Ensure they actually restore it
            MockSyncUser addOrUpdateUser = new MockSyncUser(nameof(addOrUpdateUser), existingUser);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddPerson(addOrUpdateUser, SyncOpType.AddOrUpdate);

            await orgStore.EndSync();

            // ASSERT - Existing user is updated and live
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, addOrUpdateUser);
        }

        [Fact]
        public async Task AddPerson_AddOrUpdate_DeletedUser()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();
            UserEntity existingUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserRegular.Route);
            existingUser.SoftDeleteNow();
            dbProvider.Db.SaveChanges();
            dbProvider.Snap();
            MockSyncUser addOrUpdateUser = new MockSyncUser(nameof(addOrUpdateUser), existingUser);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddPerson(addOrUpdateUser, SyncOpType.AddOrUpdate);

            await orgStore.EndSync();

            // ASSERT - User should be updated and no longer deleted
            Assert.Equal(1, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, addOrUpdateUser);
        }

        [Fact]
        public async Task AddPerson_AddOrUpdate_DeletedPerson()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();


            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();
            string userRoute = IdentityDbSeed.UserRegular.Route;
            string orgRoute = IdentityDbSeed.OrgPrimary.Route;
            UserEntity existingUser = await dbProvider.Db.UserWhereRoute(userRoute);
            PersonEntity existingPerson = await dbProvider.Db.PersonWhereUserRoute(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.UserRegular.Route);
            existingPerson.SoftDeleteNow();
            dbProvider.Db.SaveChanges();
            dbProvider.Snap();
            MockSyncUser addOrUpdateUser = new MockSyncUser(nameof(addOrUpdateUser), existingUser);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddPerson(addOrUpdateUser, SyncOpType.AddOrUpdate);

            await orgStore.EndSync();

            // ASSERT - User should be updated and no longer deleted
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(1, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, addOrUpdateUser);
        }

        [Fact]
        public async Task AddPerson_UpdateOnly_ExistingUser()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();
            UserEntity existingUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserRegular.Route);
            MockSyncUser updateOnlyUser = new MockSyncUser(nameof(updateOnlyUser), existingUser);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddPerson(updateOnlyUser, SyncOpType.UpdateOnly);

            await orgStore.EndSync();

            // ASSERT - Updated happened w/o new records
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, updateOnlyUser);
        }

        [Fact]
        public async Task AddPerson_UpdateOnly_NoExisting()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            MockSyncUser updateOnlyUser = new MockSyncUser(nameof(updateOnlyUser));
            await orgStore.AddPerson(updateOnlyUser, SyncOpType.UpdateOnly);

            await orgStore.EndSync();

            // ASSERT - Since we didn't find a user to update, no new records and the email will not be in there
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(0, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertUserForEmailDeleted(updateOnlyUser.Email);
        }

        [Fact]
        public async Task AddPerson_UpdateOnly_DeletedUser()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();
            UserEntity existingUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserRegular.Route);
            existingUser.SoftDeleteNow();
            dbProvider.Db.SaveChanges();
            dbProvider.Snap();

            // Model an update 
            MockSyncUser updateOnlyUser = new MockSyncUser(nameof(updateOnlyUser), existingUser);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddPerson(updateOnlyUser, SyncOpType.UpdateOnly);

            await orgStore.EndSync();

            // ASSERT - The user's fields should be updated, but it will NOT be resurrected
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, updateOnlyUser, false, true);
        }

        [Fact]
        public async Task AddPerson_NoPhone_ExplicitFalse()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            AadOrgConfig strategyOrgConfigBase = new AadOrgConfig
            {
                ImportPhoneNumbers = false
            };
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route,
                SyncConfigJson = strategyOrgConfigBase.ToLowerCamelJson(),
                SyncType = AadOrgSyncStrategyBase.Name
            };
            OrgSyncResult orgResult = new OrgSyncResult();
            UserEntity existingUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserRegular.Route);
            existingUser.Phone = null; // Clear phone, we want to evaluate if it will sync
            existingUser.SoftDeleteNow();
            dbProvider.Db.SaveChanges();
            dbProvider.Snap();

            // Model an update 
            MockSyncUser updateOnlyUser = new MockSyncUser(nameof(updateOnlyUser), existingUser);
            updateOnlyUser.Phone = null;

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddPerson(updateOnlyUser, SyncOpType.UpdateOnly);

            await orgStore.EndSync();

            // ASSERT - The user's fields should be updated, but it will NOT be resurrected
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, updateOnlyUser, false, true, false);
        }

        [Fact]
        public async Task AddPerson_NoPhone_NotSet()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            AadOrgConfig strategyOrgConfigBase = new AadOrgConfig
            {
                // Default config
            };
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route,
                SyncConfigJson = strategyOrgConfigBase.ToLowerCamelJson(),
                SyncType = AadOrgSyncStrategyBase.Name
            };
            OrgSyncResult orgResult = new OrgSyncResult();
            UserEntity existingUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserRegular.Route);
            existingUser.Phone = null; // Clear phone, we want to evaluate if it will sync
            existingUser.SoftDeleteNow();
            dbProvider.Db.SaveChanges();
            dbProvider.Snap();

            // Model an update 
            MockSyncUser updateOnlyUser = new MockSyncUser(nameof(updateOnlyUser), existingUser);
            updateOnlyUser.Phone = null;

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddPerson(updateOnlyUser, SyncOpType.UpdateOnly);

            await orgStore.EndSync();

            // ASSERT - The user's fields should be updated, but it will NOT be resurrected
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(1, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            dbProvider.Db.AssertPersonForEmail(IdentityDbSeed.OrgPrimary.Route, updateOnlyUser, false, true, false);
        }

        [Fact]
        public async Task RemovePerson_ExistingPerson()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();
            PersonEntity existingPerson = await dbProvider.Db.PersonWhereUserRoute(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.UserRegular.Route, false, true);
            Assert.Null(existingPerson.DeletedUtc);
            Assert.Null(existingPerson.User.DeletedUtc);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.RemovePerson(existingPerson.User.Email);

            await orgStore.EndSync();

            // ASSERT - The Person should be deleted, but not the user
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(0, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(1, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            PersonEntity deletedPerson = await dbProvider.Db.PersonWhereUserRoute(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.UserRegular.Route, true, true);
            Assert.NotNull(deletedPerson.DeletedUtc);
            Assert.Null(deletedPerson.User.DeletedUtc);
        }

        [Fact]
        public async Task RemovePerson_NoPerson()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();

            string nonExistentEmail = "EMAIL_DOES_NOT_EXIST@NOPE.COM";
            UserEntity existingUser = await dbProvider.Db.UserWhereEmail(nonExistentEmail, allowDeleted: true);
            Assert.Null(existingUser);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.RemovePerson(nonExistentEmail);

            await orgStore.EndSync();

            // ASSERT - Nothing was created; user who never existed should still NOT exist
            Assert.Equal(0, orgResult.UsersAdded);
            Assert.Equal(0, orgResult.UsersUpdated);
            Assert.Equal(0, orgResult.PeopleAdded);
            Assert.Equal(0, orgResult.PeopleRemoved);
            dbProvider.AssertAdd();
            UserEntity stillNonExistentUser = await dbProvider.Db.UserWhereEmail(nonExistentEmail, allowDeleted: true);
            Assert.Null(stillNonExistentUser);
        }
    }
}