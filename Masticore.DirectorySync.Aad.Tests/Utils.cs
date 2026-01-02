using Masticore.Ad;
using Masticore.Entity.Tests;
using Masticore.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Masticore.DirectorySync.Aad.Tests
{
    /// <summary>
    /// Utility class for static methods setting up or anazlying test data
    /// </summary>
    public static class Utils
    {
        #region Static Methods

        public static void SetupOrgConfigAndResult(AadOrgConfig aadOrgConfig, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult)
        {
            orgConfig = SetupAadOrgConfig(aadOrgConfig);
            orgResult = new OrgSyncResult(orgConfig.OrgRoute);
        }

        public static OrgSyncConfig SetupAadOrgConfig(AadOrgConfig aadOrgConfig)
        {
            return new OrgSyncConfig
            {
                OrgRoute = IdentityDbSeed.OrgPrimary?.Route ?? "VALID ROUTE",
                SyncType = AadOrgSyncStrategyBase.Name,
                SyncConfigJson = aadOrgConfig.ToLowerCamelJson(false)
            };
        }

        public static void SetupAadConfigAndResult(bool allGroups, bool allUsers, UserSyncConfig[] users, GroupSyncConfig[] groups, out OrgSyncConfig orgConfig, out OrgSyncResult orgResult)
        {
            AadOrgConfig aadOrgConfig = SetupAadConfig(allGroups, allUsers, users, groups);

            SetupOrgConfigAndResult(aadOrgConfig, out orgConfig, out orgResult);
        }

        public static CollectionSyncMode SyncMode(bool all, object[] selected)
        {
            if (all)
            {
                return CollectionSyncMode.All;
            }

            if (selected == null || selected.Length == 0)
            {
                return CollectionSyncMode.Some;
            }
            else
            {
                return CollectionSyncMode.None;
            }
        }

        public static AadOrgConfig SetupAadConfig(bool allGroups, bool allUsers, UserSyncConfig[] users, GroupSyncConfig[] groups)
        {
            // Aad config with just one configured user
            return new AadOrgConfig
            {
                TenantId = nameof(AadOrgConfig.TenantId),
                Users = users,
                Groups = groups,
                GroupsMode = SyncMode(allGroups, groups),
                UsersMode = SyncMode(allUsers, users)
            };
        }

        public static GraphGroup CreateGroup(string groupId, params GraphUser[] members)
        {
            GraphMemberDelta[] memberDeltas = members.Select(m => new GraphMemberDelta { UniversalId = m.UniversalId, Removed = m.Removed }).ToArray();
            return new GraphGroup
            {
                UniversalId = groupId,
                Name = $"Test Group {groupId}",
                Description = $"Test Group Description {groupId}",
                Members = memberDeltas
            };
        }

        public static KeyValuePair<string, GraphUser[]> CreateMember(string groupId, params GraphUser[] members)
        {
            return KeyValuePair.Create(groupId, members);
        }

        public static Dictionary<string, GraphUser[]> CreateMembers(params KeyValuePair<string, GraphUser[]>[] entries)
        {
            return new Dictionary<string, GraphUser[]>(entries);
        }

        public static OrgSyncConfig ValidAllUsersAllGroupsConfig()
        {
            AadOrgConfig aadConfig = SetupAadConfig(true, true, null, null);
            OrgSyncConfig orgConfig = SetupAadOrgConfig(aadConfig);
            return orgConfig;
        }

        public static OrgSyncConfig ValidGroupsConfig()
        {
            GroupSyncConfig[] groups = new GroupSyncConfig[] {
                new GroupSyncConfig {GroupId = "e2d83e41-a6fb-4ede-a086-627d65fa2b9c", MemberSyncType = MemberSyncOpType.SyncAllMembers, RenameTo="Alice"},
                new GroupSyncConfig {GroupId = "221a90a6-373b-4536-ae08-bb37635cea78", MemberSyncType = MemberSyncOpType.SyncExistingMembers}
            };
            AadOrgConfig aadConfig = SetupAadConfig(true, true, null, groups);
            OrgSyncConfig orgConfig = SetupAadOrgConfig(aadConfig);
            return orgConfig;
        }

        public static OrgSyncConfig ValidUserConfig()
        {
            UserSyncConfig[] users = new UserSyncConfig[] {
                new UserSyncConfig {Id = "b2be56fb-0b86-44c6-b0ab-8a00e90c61be"}
            };
            AadOrgConfig aadConfig = SetupAadConfig(false, false, users, null);
            OrgSyncConfig orgConfig = SetupAadOrgConfig(aadConfig);
            return orgConfig;
        }

        public static OrgSyncConfig MissingJsonConfig()
        {
            OrgSyncConfig nullAad = ValidOrgConfig(null);
            nullAad.SyncConfigJson = null;
            return nullAad;
        }

        public static OrgSyncConfig MissingTenantIdConfig()
        {
            AadOrgConfig nullTenantAad = ValidAadConfig();
            nullTenantAad.TenantId = null;
            OrgSyncConfig nullTenant = ValidOrgConfig(nullTenantAad);
            return nullTenant;
        }

        public static OrgSyncConfig WrongTypeConfig()
        {
            OrgSyncConfig wrongStrategy = ValidOrgConfig(ValidAadConfig());
            wrongStrategy.SyncType = "WRONG!";
            return wrongStrategy;
        }

        public static OrgSyncConfig InvalidJsonConfig()
        {
            return ValidOrgConfigWithNestedJson("HELLO WORLD!");
        }

        public static OrgSyncConfig NoTypeConfig()
        {
            OrgSyncConfig nullProvider = ValidOrgConfig(ValidAadConfig());
            nullProvider.SyncType = null;
            return nullProvider;
        }

        public static AadOrgConfig ValidAadConfig()
        {
            return new AadOrgConfig
            {
                TenantId = "TenantId",
                Groups = new GroupSyncConfig[] { new GroupSyncConfig { GroupId = "FirstGroup" } },
                Users = new UserSyncConfig[] { new UserSyncConfig { Id = "FirstUser" } }
            };
        }

        public static OrgSyncConfig ValidOrgConfig(AadOrgConfig aadConfig)
        {
            return new OrgSyncConfig
            {
                OrgRoute = "SQ4yhWkP",
                SyncType = AadOrgSyncStrategy.Name,
                SyncConfigJson = JsonConvert.SerializeObject(aadConfig)
            };
        }

        public static OrgSyncConfig ValidOrgConfigWithNestedJson(string aadConfig)
        {
            return new OrgSyncConfig
            {
                OrgRoute = "SQ4yhWkP",
                SyncType = AadOrgSyncStrategy.Name,
                SyncConfigJson = aadConfig
            };
        }

        public static void AssertAllUsers(MockOrgStore orgStore)
        {
            // OrgStore
            Assert.True(orgStore.IsStarted);
            Assert.True(orgStore.IsEnded);
            Assert.Empty(orgStore.AddGroupIds);
            Assert.Empty(orgStore.RemoveGroupIds);
            Assert.Empty(orgStore.AddMemberEmails);
            Assert.Empty(orgStore.RemoveMemberEmails);
            Assert.Equal(3, orgStore.AddPersonEmails.Count);
            Assert.Empty(orgStore.RemovePersonEmails);
        }

        public static void ArrangeAllUsers(out OrgSyncConfig orgConfig, out OrgSyncResult orgResult, out MockGraphClient graphClient)
        {
            // One custom user
            string firstUserId = nameof(firstUserId);
            string secondUserId = nameof(secondUserId);
            string thirdUserId = nameof(thirdUserId);
            GraphUser userInFirstGroup = new GraphUser { Email = $"{firstUserId}@geocities.com", UniversalId = firstUserId, Name = $"Name:{firstUserId}" };
            GraphUser userInSecondGroup = new GraphUser { Email = $"{secondUserId}@geocities.com", UniversalId = secondUserId, Name = $"Name:{secondUserId}" };
            GraphUser userInBoth = new GraphUser { Email = $"{thirdUserId}@geocities.com", UniversalId = thirdUserId, Name = $"Name:{thirdUserId}" };

            GroupSyncConfig[] groups = null;
            bool allGroups = false;
            bool allUsers = true;
            UserSyncConfig[] users = null;

            SetupAadConfigAndResult(allGroups, allUsers, users, groups, out orgConfig, out orgResult);

            // Load up a graph client that has the user in the config
            graphClient = new MockGraphClient
            {
                Users = new GraphUser[]
                {
                    userInFirstGroup,
                    userInSecondGroup,
                    userInBoth
                }
            };
        }

        #endregion
    }
}
