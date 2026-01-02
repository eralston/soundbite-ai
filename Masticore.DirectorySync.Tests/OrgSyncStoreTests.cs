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
    /// <summary>
    /// These are a mixture of operations from
    /// <see cref="OrgSyncStore_PersonStoreTests"/>,
    /// <see cref="OrgSyncStore_GroupStoreTests"/>, and
    /// <see cref="OrgSyncStore_MemberStoreTests"/>
    /// arranged to be semi-realistic batches for <see cref="OrgSyncStore"/>
    /// </summary>
    public class OrgSyncStoreTests
    {
        private readonly ILogger<OrgSyncStore> StoreLogger = NullLogger<OrgSyncStore>.Instance;

        [Fact]
        public async Task HappyPath()
        {
            // ARRANGE
            using MockSyncInfrastructure dbProvider = new MockSyncInfrastructure();
            ISyncDb db = dbProvider.Db;
            OrgSyncConfig orgConfig = CreateOrgSyncConfig();
            OrgSyncResult orgResult = new OrgSyncResult();

            // Arrange data changes
            PersonEntity myPerson = await db.ValidPerson(IdentityDbSeed.PersonOrgAdmin.Route);
            GroupEntity memberGroup = await db.ValidGroup(IdentityDbSeed.GroupOrgAdminOwned.Route);
            MockSyncGroup existingSyncGroup = new MockSyncGroup(memberGroup);
            GroupEntity nonMemberGroup = await db.ValidGroup(IdentityDbSeed.GroupMemberOwned.Route);

            // Mock data for deleting
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).First();

            GroupEntity deleteGroup = db.MockGroup(org);
            GroupEntity updateGrpEnt = db.MockGroup(org);
            PersonEntity removePerson = db.MockPerson(org);
            PersonEntity removePersonMember = db.MockPerson(org);

            MockSyncUser newUser = new MockSyncUser(nameof(newUser));

            UserEntity myUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserOrgAdmin.Route);
            MockSyncUser mySyncUser = new MockSyncUser(nameof(myUser), myUser);
            UserEntity otherUser = await dbProvider.Db.UserWhereRoute(IdentityDbSeed.UserRegular.Route);
            MockSyncUser otherSyncUser = new MockSyncUser(nameof(otherSyncUser), otherUser);

            dbProvider.Db.SaveChanges();
            dbProvider.Snap();

            // ACT
            OrgSyncStore orgStore = new OrgSyncStore(StoreLogger, dbProvider);
            orgStore.SetProviderType(ProviderType.AAD);
            await orgStore.StartSync(orgConfig, orgResult);

            // USERS ==============================

            // Add a new user
            await orgStore.AddPerson(mySyncUser, SyncOpType.AddOrUpdate); // Do nothing, already exists
            await orgStore.AddPerson(newUser, SyncOpType.AddOrUpdate); // Add 1 User & 1 Person

            // Remove a user
            await orgStore.RemovePerson(removePerson.User.Email);

            // Update an existing user
            await orgStore.AddPerson(otherSyncUser, SyncOpType.UpdateOnly);

            // GROUPS & MEMBERS ==============================

            // Add a new group
            const string newGrpName = nameof(newGrpName);
            string newGrpDesc = $"Description for {nameof(newGrpName)}";
            MockSyncGroup newGrp = new MockSyncGroup
            {
                Name = newGrpName,
                Description = newGrpDesc
            };
            await orgStore.AddGroup(newGrp, SyncOpType.AddOrUpdate); // Add 1 Group
            // Add new user in this batch
            await orgStore.AddMember(newGrp, newUser, MemberSyncOpType.SyncAllMembers); // Add 1 Member
            // Try to remove existing user from before this batch, but not in this new group
            await orgStore.RemoveMember(newGrp.UniversalId, otherSyncUser.Email);
            // Remove user who is NOT in this group
            await orgStore.RemoveMember(newGrp.UniversalId, removePersonMember.User.Email);

            // Remove existing member
            await orgStore.RemoveMember(existingSyncGroup.UniversalId, otherSyncUser.Email);

            // Try to update a non-existent group
            const string updateGrpName = nameof(updateGrpName);
            string updateGrpDesc = $"Description for {nameof(updateGrpName)}";
            MockSyncGroup newUpdateGrp_shouldSkip = new MockSyncGroup
            {
                Name = updateGrpName,
                Description = updateGrpDesc
            };
            await orgStore.AddGroup(newUpdateGrp_shouldSkip, SyncOpType.UpdateOnly);

            // Update an existing group, add a member, and remove a member
            string updatedGrpName = nameof(updatedGrpName);
            string updatedGrpDesc = nameof(updatedGrpDesc);
            MockSyncGroup updateOnlyGrp = new MockSyncGroup(updateGrpEnt)
            {
                Name = updatedGrpName,
                Description = updatedGrpDesc
            };
            await orgStore.AddGroup(updateOnlyGrp, SyncOpType.UpdateOnly);
            // Add new user in this batch
            await orgStore.AddMember(updateOnlyGrp, mySyncUser, MemberSyncOpType.SyncAllMembers); // Add 1 Member
            await orgStore.AddMember(updateOnlyGrp, newUser, MemberSyncOpType.SyncAllMembers); // Add 1 Member

            // Remove a group
            await orgStore.RemoveGroup(deleteGroup.UniversalId);
            // Remove a non-existent group
            string missingGrpUid = "THIS GROUP UNIVERSAL ID DOES NOT EXIST!";
            await orgStore.RemoveGroup(missingGrpUid);

            await orgStore.EndSync();

            // ASSERT - Check all updates
            Assert.Equal(1, orgResult.UsersAdded);
            Assert.Equal(3, orgResult.UsersUpdated);
            Assert.Equal(1, orgResult.PeopleAdded);
            Assert.Equal(1, orgResult.PeopleRemoved);
            Assert.Equal(1, orgResult.GroupsAdded);
            Assert.Equal(3, orgResult.GroupsUpdated);
            Assert.Equal(1, orgResult.GroupsRemoved);
            Assert.Equal(3, orgResult.MembersAdded);
            Assert.Equal(1, orgResult.MembersRemoved);

            dbProvider.AssertAdd(1, 1, 1, 3);

            db.AssertPersonForEmail(org.Route, newUser);
            MemberEntity[] membersForGroup = db.Members.Where(m => m.Group.UniversalId == newGrp.UniversalId).ToArray();
            db.AssertMemberActive(org.Route, newGrp.UniversalId, newUser.Email);
            db.AssertMemberMissing(org.Route, newGrp.UniversalId, otherSyncUser.Email);
            db.AssertMemberMissing(org.Route, newGrp.UniversalId, removePersonMember.User.Email);
            db.AssertPersonDeleted(org.Route, removePerson.Route);
            db.AssertPersonForEmail(org.Route, otherSyncUser);
            db.AssertGroupMissing(newUpdateGrp_shouldSkip.UniversalId);
            GroupEntity updatedGrp = db.AssertGroupActive(org.Route, updateGrpEnt);
            Assert.Equal(updatedGrpName, updatedGrp.Name);
            Assert.Equal(updatedGrpDesc, updatedGrp.Description);
            db.AssertMemberActive(org.Route, updatedGrp.UniversalId, newUser.Email);
            db.AssertGroupDeleted(deleteGroup, org.Route);
            db.AssertGroupMissing(missingGrpUid);
        }

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
    }
}
