using Masticore.Ad;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Services.Tests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Aad.Tests
{
    /// <summary>
    /// Tests for AAD provider
    /// </summary>
    public class AadOrgSyncStrategyTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        #region Tests

        private ILogger<AadOrgSyncStrategyTests> Logger { get; } = NullLogger<AadOrgSyncStrategyTests>.Instance;

        public AadOrgSyncStrategyTests()
        {
            // Pre-setup for all tests
            AdAppSettings.Init(
                new AdAppSettings
                {
                    AppId = "123",
                    AppSecret = "SUPER_SECRET"
                }, false);
        }

        [Fact]
        public async Task Sync_Empty()
        {
            // NOTE: This should never happen because we shouldn't do syncs with no values

            // ARRANGE

            // One custom user

            GroupSyncConfig[] groups = null;
            bool allGroups = false;
            bool allUsers = false;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient { };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.False(orgStore.IsConfigSaved);
            Assert.Empty(orgStore.AddGroupIds);
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Empty(orgStore.AddMemberEmails);
            Assert.Empty(orgStore.RemoveMemberEmails);
            Assert.Empty(orgStore.AddPersonEmails);
            Assert.Empty(orgStore.RemovePersonEmails);

            // Graph client
            Assert.Equal(0, graphClient.GetCount);
            Assert.Equal(0, graphClient.PostCount);
        }

        /// <summary>
        /// Add a single user while finding a single user in graph
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Sync_Users_HappyPath()
        {
            // ARRANGE

            // One custom user
            string customUserId = nameof(customUserId);
            GroupSyncConfig[] groups = null;
            bool allGroups = false;
            bool allUsers = false;
            UserSyncConfig[] users = new UserSyncConfig[]
                {
                    new UserSyncConfig
                    {
                        // Only one custom user ID
                        Id = customUserId
                    }
                };

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            string firstUserEmail = nameof(firstUserEmail);
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    // Custom User ID that is in graph
                    new GraphUser { Email = firstUserEmail, UniversalId = customUserId, Name="NOT EMPTY"}
                }
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.Empty(orgStore.AddGroupIds);
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Empty(orgStore.AddMemberEmails);
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Single(orgStore.AddPersonEmails); // Added a single user
            Assert.Equal(firstUserEmail, orgStore.AddPersonEmails[0]); // The single user is the one for which we asked
            Assert.Empty(orgStore.RemovePersonEmails);

            // Graph client
            Assert.Equal(0, graphClient.GetCount);
            Assert.Equal(1, graphClient.PostCount);
        }


        /// <summary>
        /// Try to find two users, but only find one in graph
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Sync_Users_FoundOnlyOneUser()
        {
            // ARRANGE

            // One custom user
            string firstUserEmail = nameof(firstUserEmail);
            string missingUserId = nameof(missingUserId);
            GroupSyncConfig[] groups = null;
            bool allGroups = false;
            bool allUsers = false;
            UserSyncConfig[] users = new UserSyncConfig[]
                {
                    new UserSyncConfig{ Id = firstUserEmail },
                    new UserSyncConfig{ Id = missingUserId },
                };

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    // Custom User ID that is in graph
                    new GraphUser { Email = firstUserEmail, UniversalId = firstUserEmail, Name="NOT EMPTY"}
                }
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.Empty(orgStore.AddGroupIds);
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Empty(orgStore.AddMemberEmails);
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Single(orgStore.AddPersonEmails); // Added a single user
            Assert.Equal(firstUserEmail, orgStore.AddPersonEmails[0]); // The single user is the one for which we asked
            Assert.Empty(orgStore.RemovePersonEmails); // TODO: Missing someone in the user sync does NOT remove them from SB right now

            // Graph client
            Assert.Equal(0, graphClient.GetCount);
            Assert.Equal(1, graphClient.PostCount);
        }

        [Fact]
        public async Task Sync_AllUsers_HappyPath()
        {
            // ARRANGE

            Utils.ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult orgResult, out MockGraphClient graphClient);

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            Utils.AssertAllUsers(orgStore);
        }

        [Fact]
        public async Task Sync_Groups_HappyPath()
        {
            // ARRANGE

            // One custom user
            string groupId = nameof(groupId);
            string rename = nameof(rename);
            string memberUserEmail = "member@user.com";
            GraphUser memberUser = new GraphUser { Email = memberUserEmail, UniversalId = "NotEmpty", Name = "Not Empty" };

            GroupSyncConfig[] groups = new GroupSyncConfig[]
            {
                new GroupSyncConfig { GroupId = groupId, RenameTo = rename, MemberSyncType = MemberSyncOpType.SyncAllMembers }
            };
            bool allGroups = false;
            bool allUsers = false;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    // Custom User ID that is in graph
                    memberUser
                },
                Groups = new GraphGroup[]
                {
                    new GraphGroup { UniversalId = groupId, Name = "Test Group 1", Description = "Test Group 1 Description"}
                },
                Members = Utils.CreateMembers(
                    Utils.CreateMember(groupId, memberUser)
                    )
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.Single(orgStore.AddGroupIds); // One group
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Single(orgStore.AddMemberEmails); // One member
            Assert.Empty(orgStore.RemoveMemberEmails);
            Assert.Empty(orgStore.AddPersonEmails);
            Assert.Empty(orgStore.RemovePersonEmails);

            Assert.Equal(memberUserEmail, orgStore.AddMemberEmails[0]); // We are adding the member we expect
            Assert.Equal(groupId, orgStore.AddGroupIds[0]); // We are adding the member we expect

            // Graph client
            Assert.Equal(1, graphClient.GetCount); // One to pull members
            Assert.Equal(1, graphClient.PostCount); // One to pull the group
        }

        [Fact]
        public async Task Sync_Groups_MissingGroup()
        {
            // ARRANGE

            // One custom user
            string groupId = nameof(groupId);
            string missingGroupId = nameof(missingGroupId);
            string rename = nameof(rename);
            string memberEmail = nameof(memberEmail);
            GraphUser memberUser = new GraphUser { Email = memberEmail, Name = $"Name: {memberEmail}", UniversalId = $"ID:{memberEmail}" };

            GroupSyncConfig[] groups = new GroupSyncConfig[]
            {
                new GroupSyncConfig { GroupId = groupId, RenameTo = rename, MemberSyncType = MemberSyncOpType.SyncAllMembers },
                new GroupSyncConfig { GroupId = missingGroupId, MemberSyncType = MemberSyncOpType.SyncAllMembers }
            };
            bool allGroups = false;
            bool allUsers = false;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    // Custom User ID that is in graph
                    memberUser
                },
                Groups = new GraphGroup[]
                {
                    new GraphGroup { UniversalId = groupId, Name = "Test Group 1", Description = "Test Group 1 Description"},
                },
                Members = Utils.CreateMembers(
                    Utils.CreateMember(groupId, memberUser)
                    )
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.Single(orgStore.AddGroupIds);
            Assert.Single(orgStore.RemoveGroupIds);
            Assert.Single(orgStore.AddMemberEmails);
            Assert.Empty(orgStore.RemoveMemberEmails);
            Assert.Empty(orgStore.AddPersonEmails);
            Assert.Empty(orgStore.RemovePersonEmails);

            Assert.Equal(memberEmail, orgStore.AddMemberEmails[0]); // We are adding the member we expect
            Assert.Equal(groupId, orgStore.AddGroupIds[0]); // We are adding the member we expect
            Assert.Equal(missingGroupId, orgStore.RemoveGroupIds[0]); // We are adding the member we expect

            // Graph client
            Assert.Equal(1, graphClient.GetCount); // One to pull members
            Assert.Equal(1, graphClient.PostCount); // One to pull the group
        }

        [Fact]
        public async Task Sync_AllGroups_HappyPath()
        {
            // ARRANGE

            // One custom user
            string firstGroupId = nameof(firstGroupId);
            string secondGroupId = nameof(secondGroupId);

            string firstUserEmail = nameof(firstUserEmail);
            string secondUserEmail = nameof(secondUserEmail);
            string thirdUserEmail = nameof(thirdUserEmail);
            GraphUser userInFirstGroup = new GraphUser { Email = firstUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };
            GraphUser userInSecondGroup = new GraphUser { Email = secondUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };
            GraphUser userInBoth = new GraphUser { Email = thirdUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };

            GroupSyncConfig[] groups = null;
            bool allGroups = true;
            bool allUsers = false;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    userInFirstGroup,
                    userInSecondGroup,
                    userInBoth
                },
                Groups = new GraphGroup[]
                {
                    Utils.CreateGroup(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateGroup(secondGroupId, userInSecondGroup, userInBoth)
                },
                Members = Utils.CreateMembers(
                    Utils.CreateMember(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateMember(secondGroupId, userInSecondGroup, userInBoth)
                    )
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.Equal(2, orgStore.AddGroupIds.Count); // 2 groups
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Equal(4, orgStore.AddMemberEmails.Count); // 2 members in 2 groups
            Assert.Empty(orgStore.RemoveMemberEmails);
            Assert.Empty(orgStore.AddPersonEmails);
            Assert.Empty(orgStore.RemovePersonEmails);

            // Graph client
            Assert.Equal(3, graphClient.GetCount); // All groups + 2 member calls
            Assert.Equal(0, graphClient.PostCount);
        }

        [Fact]
        public async Task Sync_AllUsersAllGroups_HappyPath()
        {
            // ARRANGE
            string firstGroupId = nameof(firstGroupId);
            string secondGroupId = nameof(secondGroupId);

            string firstUserEmail = nameof(firstUserEmail);
            string secondUserEmail = nameof(secondUserEmail);
            string thirdUserEmail = nameof(thirdUserEmail);
            GraphUser userInFirstGroup = new GraphUser { Email = firstUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };
            GraphUser userInSecondGroup = new GraphUser { Email = secondUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };
            GraphUser userInBoth = new GraphUser { Email = thirdUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };

            GroupSyncConfig[] groups = null;
            bool allGroups = true;
            bool allUsers = true;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    userInFirstGroup,
                    userInSecondGroup,
                    userInBoth
                },
                Groups = new GraphGroup[]
                {
                    Utils.CreateGroup(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateGroup(secondGroupId, userInSecondGroup, userInBoth)
                },
                Members = Utils.CreateMembers(
                    Utils.CreateMember(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateMember(secondGroupId, userInSecondGroup, userInBoth)
                    )
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            await provider.Sync(orgResult);

            // ASSERT

            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.Equal(2, orgStore.AddGroupIds.Count); // 2 groups
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Equal(4, orgStore.AddMemberEmails.Count); // 2 members in 2 groups
            Assert.Empty(orgStore.RemoveMemberEmails);
            Assert.Equal(3, orgStore.AddPersonEmails.Count);
            Assert.Empty(orgStore.RemovePersonEmails);

            // Graph client
            Assert.Equal(4, graphClient.GetCount); // 1 users + 1 groups + 1 for each group (2)
            Assert.Equal(0, graphClient.PostCount);
        }

        [Fact]
        public async Task Get_Users()
        {
            // ARRANGE
            string firstUserUid = nameof(firstUserUid);
            string secondUserUid = nameof(secondUserUid);
            string thirdUserUid = nameof(thirdUserUid);
            GraphUser firstUser = new GraphUser { UniversalId = firstUserUid, Email = "first@email.com" };
            GraphUser secondUser = new GraphUser { UniversalId = secondUserUid, Email = "second@email.com" };
            GraphUser thirdUser = new GraphUser { UniversalId = thirdUserUid, Email = "third@email.com" };

            GroupSyncConfig[] groups = null;
            bool allGroups = true;
            bool allUsers = true;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    firstUser,
                    secondUser,
                    thirdUser
                }
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            //IEnumerable<SyncTarget> usersResponse = await provider.AllUsersAsync_Obsolete();
            IEnumerable<SyncTarget> usersResponse = (await provider.UsersAsync()).Result;

            // ASSERT
            Assert.Equal(3, usersResponse.Count());
            Assert.Single(usersResponse.Where(u => u.Id == firstUserUid));
            Assert.Single(usersResponse.Where(u => u.Id == secondUserUid));
            Assert.Single(usersResponse.Where(u => u.Id == thirdUserUid));
        }

        [Fact]
        public async Task Get_Groups()
        {
            // ARRANGE

            // ARRANGE
            string firstGroupUid = nameof(firstGroupUid);
            string secondGroupUid = nameof(secondGroupUid);

            string firstUserUid = nameof(firstUserUid);
            string secondUserUid = nameof(secondUserUid);
            string thirdUserUid = nameof(thirdUserUid);
            GraphUser userInFirstGroup = new GraphUser { UniversalId = firstUserUid };
            GraphUser userInSecondGroup = new GraphUser { UniversalId = secondUserUid };
            GraphUser userInBoth = new GraphUser { UniversalId = thirdUserUid };

            GroupSyncConfig[] groups = null;
            bool allGroups = true;
            bool allUsers = true;
            UserSyncConfig[] users = null;

            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    userInFirstGroup,
                    userInSecondGroup,
                    userInBoth
                },
                Groups = new GraphGroup[]
                {
                    Utils.CreateGroup(firstGroupUid, userInFirstGroup, userInBoth),
                    Utils.CreateGroup(secondGroupUid, userInSecondGroup, userInBoth)
                }
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT
            TokenPageResponse<SyncTarget> usersResponse = await provider.GroupsAsync();

            // ASSERT
            Assert.Equal(2, usersResponse.Result.Count());
            Assert.Single(usersResponse.Result.Where(u => u.Id == firstGroupUid));
            Assert.Single(usersResponse.Result.Where(u => u.Id == secondGroupUid));
        }

        [Fact]
        public async Task ValidateConfig()
        {
            //ARRANGE
            string firstGroupId = nameof(firstGroupId);
            string secondGroupId = nameof(secondGroupId);

            string firstUserEmail = nameof(firstUserEmail);
            string secondUserEmail = nameof(secondUserEmail);
            string thirdUserEmail = nameof(thirdUserEmail);
            GraphUser userInFirstGroup = new GraphUser { Email = firstUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };
            GraphUser userInSecondGroup = new GraphUser { Email = secondUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };
            GraphUser userInBoth = new GraphUser { Email = thirdUserEmail, UniversalId = "UID", Name = "NOT EMPTY" };

            GroupSyncConfig[] groups = null;
            bool allGroups = true;
            bool allUsers = true;
            UserSyncConfig[] users = null;
            Utils.SetupAadConfigAndResult(allGroups, allUsers, users, groups, out OrgSyncConfig orgConfig, out _);

            // Load up a graph client that has the user in the config
            MockGraphClient graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    userInFirstGroup,
                    userInSecondGroup,
                    userInBoth
                },
                Groups = new GraphGroup[]
                {
                    Utils.CreateGroup(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateGroup(secondGroupId, userInSecondGroup, userInBoth)
                },
                Members = Utils.CreateMembers(
                    Utils.CreateMember(firstGroupId, userInFirstGroup, userInBoth),
                    Utils.CreateMember(secondGroupId, userInSecondGroup, userInBoth)
                    )
            };

            MockOrgStore orgStore = new MockOrgStore();
            IOrgSyncStrategy provider = new AadOrgSyncStrategy(Logger, orgConfig, new MockGraphFactory(graphClient), orgStore, Builder.Infrastructure.Value);

            // ACT & ASSERT

            // null is a valid reset of the values
            string msg = await provider.ValidationMessagesAsync(null);
            Assert.Null(msg);
            // If we get past here, then we're good

            // No provider type
            OrgSyncConfig nullProvider = Utils.NoTypeConfig();
            msg = await provider.ValidationMessagesAsync(nullProvider);
            Assert.Equal(Constants.SyncConfigErrorMessages.MissingType, msg);

            // Invalid 365 json
            OrgSyncConfig invalidProviderJson = Utils.InvalidJsonConfig();
            msg = await provider.ValidationMessagesAsync(invalidProviderJson);
            Assert.Equal(Constants.SyncConfigErrorMessages.InvalidJson, msg);

            // Wrong provider type
            OrgSyncConfig wrongProvider = Utils.WrongTypeConfig();
            msg = await provider.ValidationMessagesAsync(wrongProvider);
            Assert.Equal(Constants.SyncConfigErrorMessages.WrongType, msg);

            // Missing aad
            OrgSyncConfig missingProviderConfig = Utils.MissingJsonConfig();
            msg = await provider.ValidationMessagesAsync(missingProviderConfig);
            Assert.Equal(Constants.SyncConfigErrorMessages.MissingConfig, msg);

            // Missing tenant ID
            OrgSyncConfig nullTenant = Utils.MissingTenantIdConfig();
            msg = await provider.ValidationMessagesAsync(nullTenant);
            Assert.Equal(AadOrgSyncStrategyBase.MissingTenantId, msg);

            // The JSON equivalent of configs from SyncScenarios.sql in the Masticore.DirectorySync.Aad project

            // Good Users
            OrgSyncConfig userConfig = Utils.ValidUserConfig();
            msg = await provider.ValidationMessagesAsync(userConfig);
            Assert.Null(msg);

            // Good Groups
            OrgSyncConfig groupsConfig = Utils.ValidGroupsConfig();
            msg = await provider.ValidationMessagesAsync(groupsConfig);
            Assert.Null(msg);

            // Good all users, all groups
            OrgSyncConfig allUsersAllGroupsConfig = Utils.ValidAllUsersAllGroupsConfig();
            msg = await provider.ValidationMessagesAsync(allUsersAllGroupsConfig);
            Assert.Null(msg);
        }

        #endregion
    }
}