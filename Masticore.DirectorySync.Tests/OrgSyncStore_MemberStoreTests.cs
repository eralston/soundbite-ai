using Masticore.DirectorySync.Tests.Mocks;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Tests
{
    public class OrgSyncStore_MemberStoreTests
    {
        private readonly ILogger<OrgSyncStore> StoreLogger = NullLogger<OrgSyncStore>.Instance;

        [Fact]
        public async Task AddMember_ExistingGroup_ExistingPerson_SyncAll()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Existing Person
            MockSyncDb db = dbProvider.Db;
            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonOrgAdmin.Route);

            // Non-Member Group
            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupMemberOwned.Route);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddMember(existingGroup, new UserWithAliases(existingPerson.User), MemberSyncOpType.SyncAllMembers);

            await orgStore.EndSync();

            // ASSERT - A new member was added with valid mapping
            Assert.Equal(1, orgResult.MembersAdded);
            dbProvider.AssertAdd(0, 0, 0, 1);
            db.AssertMemberActive(IdentityDbSeed.OrgPrimary.Route, existingGroup.UniversalId, existingPerson.User.Email);
        }

        [Fact]
        public async Task AddMember_ExistingOtherGroup_ExistingPerson_SyncExisting()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Existing Person
            MockSyncDb db = dbProvider.Db;
            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonOrgAdmin.Route);

            // Non-Member Group
            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupMemberOwned.Route);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddMember(existingGroup, new UserWithAliases(existingPerson.User), MemberSyncOpType.SyncExistingMembers);

            await orgStore.EndSync();

            // ASSERT - A new member was added with valid mapping
            Assert.Equal(0, orgResult.MembersAdded);
            dbProvider.AssertAdd(0, 0, 0, 0);
            dbProvider.Db.AssertMemberMissing(IdentityDbSeed.OrgPrimary.Route, existingGroup.UniversalId, existingPerson.User.Email);
        }

        [Fact]
        public async Task AddMember_ExistingGroup_ExistingPerson_SyncNoMembers()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Existing Person
            MockSyncDb db = dbProvider.Db;
            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonOrgAdmin.Route);

            // Non-Member Group
            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupMemberOwned.Route);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.AddMember(existingGroup, new UserWithAliases(existingPerson.User), MemberSyncOpType.SyncNoMembers);

            await orgStore.EndSync();

            // ASSERT - Nothing was added
            Assert.Equal(0, orgResult.MembersAdded);
            dbProvider.AssertAdd(0, 0, 0, 0);
            dbProvider.Db.AssertMemberMissing(IdentityDbSeed.OrgPrimary.Route, existingGroup.UniversalId, existingPerson.User.Email);
        }

        [Fact]
        public async Task RemoveMember_ExistingMember()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Existing Person
            MockSyncDb db = dbProvider.Db;
            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonOrgAdmin.Route);

            // Existing Member Group
            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupOrgAdminOwned.Route);

            // Assert existing member record
            db.AssertMemberActive(IdentityDbSeed.OrgPrimary.Route, existingGroup.UniversalId, existingPerson.User.Email);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.RemoveMember(existingGroup.UniversalId, existingPerson.User.Email);

            await orgStore.EndSync();

            // ASSERT - The member record was deleted
            Assert.Equal(1, orgResult.MembersRemoved);
            dbProvider.AssertAdd(0, 0, 0, 0);
            db.AssertMemberDeleted(IdentityDbSeed.OrgPrimary.Route, existingGroup.UniversalId, existingPerson.User.Email);
        }

        [Fact]
        public async Task RemoveMember_NoMember()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult orgResult = new OrgSyncResult();

            // Existing Person
            MockSyncDb db = dbProvider.Db;
            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonOrgAdmin.Route);

            // Non-Member Group
            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupMemberOwned.Route);

            // Double-check there is no existing member record
            MemberEntity existingMember = dbProvider.Db.Members.Where(m => m.PersonId == existingPerson.Id && m.GroupId == existingGroup.Id).FirstOrDefault();
            Assert.Null(existingMember);

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            await orgStore.RemoveMember(existingGroup.UniversalId, existingPerson.User.Email);

            await orgStore.EndSync();

            // ASSERT - The member record still doesn't exist
            Assert.Equal(0, orgResult.MembersRemoved);
            dbProvider.AssertAdd(0, 0, 0, 0);
            dbProvider.Db.AssertMemberMissing(IdentityDbSeed.OrgPrimary.Route, existingGroup.UniversalId, existingPerson.User.Email);
        }

        /// <summary>
        /// Tests if <see cref="MemberSyncStore"/> can properly add a generic person in the org to a group successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SyncMember_NewMember()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new()
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult syncResult = new();
            ISyncDb db = dbProvider.Db;

            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupOrgAdminOwned.Route);
            string groupId = existingGroup.UniversalId;

            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonNoGroupMembership.Route);
            string userEmail = existingPerson.User.Email;
            MemberSyncOpType opType = MemberSyncOpType.SyncAllMembers;

            OrgSyncStore orgStore = new(StoreLogger, dbProvider)
            {
                Config = orgConfig,
                Result = syncResult
            };
            orgStore.PeopleStore.Config = orgConfig;
            orgStore.GroupStore.Config = orgConfig;
            orgStore.SetProviderType(ProviderType.AAD);
            MemberSyncStore memberStore = orgStore.MemberStore;
            memberStore.Config = orgConfig;
            memberStore.Result = syncResult;

            // ACT
            await memberStore.SyncMember(dbProvider.Db, groupId, userEmail, opType);

            // ASSERT - New member record should have been added
            // Must save DB to see changes in the assert
            dbProvider.Db.SaveChanges();
            dbProvider.AssertAdd(0, 0, 0, 1);
        }

        /// <summary>
        /// Tests if <see cref="MemberSyncStore"/> can properly add a generic person in the org to a group successfully even if they're only given an alias email
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SyncMember_NewMemberWithAlias()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new()
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            OrgSyncResult syncResult = new();
            ISyncDb db = dbProvider.Db;

            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupOrgAdminOwned.Route);
            string groupId = existingGroup.UniversalId;

            // Add an alias for the non-group member to be synced
            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonNoGroupMembership.Route);
            string aliasEmail = "alias@somealias";
            UserIdentityEntity identity = db.UserIdentities.CreateResource(null);
            identity.Identifier = aliasEmail;
            identity.User = existingPerson.User;
            identity.ProviderType = ProviderType.AAD;
            await db.SaveChangesAsync();

            string userEmail = existingPerson.User.Email;
            MemberSyncOpType opType = MemberSyncOpType.SyncAllMembers;

            OrgSyncStore orgStore = new(StoreLogger, dbProvider)
            {
                Config = orgConfig,
                Result = syncResult
            };
            orgStore.PeopleStore.Config = orgConfig;
            orgStore.GroupStore.Config = orgConfig;
            orgStore.SetProviderType(ProviderType.AAD);
            MemberSyncStore memberStore = orgStore.MemberStore;
            memberStore.Config = orgConfig;
            memberStore.Result = syncResult;

            // ACT
            // Try to sync with the alias email instead of the main
            await memberStore.SyncMember(dbProvider.Db, groupId, aliasEmail, opType);

            // ASSERT - New member record should have been added
            // Must save DB to see changes in the assert
            dbProvider.Db.SaveChanges();
            dbProvider.AssertAdd(0, 0, 0, 1);
        }

        /// <summary>
        /// Tests if <see cref="MemberSyncStore"/> can properly add a generic person in the org to a group successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SyncMember_Existing()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            OrgSyncConfig orgConfig = new()
            {
                OrgRoute = IdentityDbSeed.OrgPrimary.Route
            };
            ISyncDb db = dbProvider.Db;

            GroupEntity existingGroup = await db.ValidGroup(IdentityDbSeed.GroupOrgAdminOwned.Route);
            string groupId = existingGroup.UniversalId;

            PersonEntity existingPerson = await db.ValidPerson(IdentityDbSeed.PersonOrgAdmin.Route);
            string userEmail = existingPerson.User.Email;
            MemberSyncOpType opType = MemberSyncOpType.SyncAllMembers;

            OrgSyncStore orgStore = new(StoreLogger, dbProvider);
            orgStore.Config = orgConfig;
            orgStore.SetProviderType(ProviderType.AAD);
            MemberSyncStore memberStore = orgStore.MemberStore;
            memberStore.Config = orgConfig;

            // ACT
            await memberStore.SyncMember(dbProvider.Db, groupId, userEmail, opType);

            // ASSERT - The member record still doesn't exist
            // Assert nothing was added, because the member already existed in the group
            dbProvider.AssertAdd();
        }
    }
}
