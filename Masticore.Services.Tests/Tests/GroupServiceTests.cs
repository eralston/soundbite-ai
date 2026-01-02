using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class GroupServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        public GroupServiceTests()
        {
            Builder.SetCurrentUserContext(TestUserContext.OrgAdmin).Wait();
            Builder.UseNullLogger = true;
        }

        #region CreateAsync

        private async Task CreateAsync(TestUserContext userContext, bool includeMembers = true)
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(userContext);
            IIdentityDb db = Builder.IdentityDb.Value;
            DbSnapshot dbSnap = new DbSnapshot(db as DbContext).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>().Snap<GroupEntity>();
            PersonEntity person1 = db.People.Find(2);
            PersonEntity person2 = db.People.Find(3);
            string newMemberEmail = "CreateAsync@bob.com";


            List<NewMember> newMembers = new List<NewMember>();
            if (includeMembers)
            {
                newMembers.Add(new NewMember { MemberRole = MemberRole.Member, PersonToken = person1.Route });
                newMembers.Add(new NewMember { MemberRole = MemberRole.Member, PersonToken = person2.Route });
                newMembers.Add(new NewMember { MemberRole = MemberRole.Owner, PersonToken = person2.Route });
                newMembers.Add(new NewMember { MemberRole = MemberRole.Owner, PersonToken = newMemberEmail });
            }

            NewGroup newGroup = new NewGroup
            {
                Name = "Hello World",
                Description = "This is a description",
                Members = newMembers.ToArray(),
            };

            // ACT            
            IGroupService groups = new GroupService(
                Builder.Rbac.Value,
                Builder.NotificationService.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                new NullLogger<GroupService>());
            Group group = await groups.CreateAsync(OrgRoute, newGroup);

            // ASSERT
            AssertResource(group);
            dbSnap.AssertAdd<UserEntity>(1);
            dbSnap.AssertAdd<PersonEntity>(1);
            dbSnap.AssertAdd<MemberEntity>(3);
            dbSnap.AssertAdd<GroupEntity>(1);
            Assert.Equal(3, Builder.GetMockNotificationService().Count);
        }

        [Fact]
        public async Task CreateAsync_HasPermission()
        {
            await CreateAsync(TestUserContext.OrgAdmin);
        }

        [Fact]
        public async Task CreateAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await CreateAsync(TestUserContext.Anonymous);
            });
        }

        [Fact]
        public async Task CreateAsync_HasPermission_NoMembers()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await CreateAsync(TestUserContext.OrgAdmin, false);
            });
        }

        #endregion

        #region ReadAsync

        private async Task ReadAsync(bool hasPermission = true, string orgRoute = null, string groupRoute = null)
        {
            // ARRANGE
            if (orgRoute == null)
            {
                orgRoute = IdentityDbSeed.OrgPrimary.Route;
            }

            if (groupRoute == null)
            {
                groupRoute = IdentityDbSeed.GroupOrgAdminOwned.Route;
            }

            if (!hasPermission)
            {
                await Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserOutside.Route);
            }

            // ACT
            IGroupService groups = new GroupService(
                Builder.Rbac.Value,
                Builder.NotificationService.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                Builder.Logger<GroupService>());
            Group group = await groups.ReadAsync(orgRoute, groupRoute);

            // ASSERT
            AssertResource(group);
            Assert.NotNull(group.Name);
            Assert.NotNull(group.Description);
        }

        [Fact]
        public async Task ReadAsync_HasPermission()
        {
            // ARRANGE + ACT + ASSERT
            await ReadAsync();
        }

        [Fact]
        public async Task ReadAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadAsync(false);
            });
        }

        [Fact]
        public async Task ReadAsync_HasPermission_OutsideOrg()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadAsync(true, IdentityDbSeed.OrgOutside.Route, IdentityDbSeed.GroupOutside.Route);
            });
        }

        [Fact]
        public async Task ReadAsync_HasPermission_UnknownOrg()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadAsync(true, ResourceExtensions.NewRoute());
            });
        }

        [Fact]
        public async Task ReadAsync_HasPermission_OutsideGroup()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadAsync(true, IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOutside.Route);
            });
        }

        [Fact]
        public async Task ReadAsync_HasPermission_UnknownGroup()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadAsync(true, IdentityDbSeed.OrgPrimary.Route, ResourceExtensions.NewRoute());
            });
        }

        [Fact]
        public async Task ReadAsync_NonMember()
        {
            // ARRANGE

            // Downgrade current user
            PersonEntity curPerson = Builder.Infrastructure.Value.Db.People.Where(u => u.Route == IdentityDbSeed.PersonOrgAdmin.Route).Include(p => p.User).Single();
            curPerson.PersonRole = PersonRole.Person;
            curPerson.User.UserRole = UserRole.User;
            Builder.Infrastructure.Value.Db.SaveChanges();

            // ACT + ASSERT
            IGroupService groups = new GroupService(
                Builder.Rbac.Value,
                Builder.NotificationService.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                Builder.Logger<GroupService>());
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await groups.ReadAsync(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            });
        }

        #endregion

        #region ReadAllAsync

        private async Task<IEnumerable<Group>> ReadAllGroupsAsync(bool hasPermission = true)
        {
            // ARRANGE
            if (!hasPermission)
            {
                await Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserOutside.Route);
            }

            // ACT
            IGroupService groups = new GroupService(
               Builder.Rbac.Value,
               Builder.NotificationService.Value,
               Builder.Infrastructure.Value,
               Builder.Mapper.Value,
               Builder.Logger<GroupService>());
            IEnumerable<Group> group = await groups.ReadAllAsync_Obsolete(IdentityDbSeed.OrgPrimary.Route);

            return group;
        }

        [Fact]
        public async Task ReadAllGroupsAsync_HasPermission()
        {
            // ARRANGE + ACT
            IEnumerable<Group> groups = await ReadAllGroupsAsync();

            // ASSERT
            Assert.Equal(3, groups.Count());
        }

        [Fact]
        public async Task ReadAllGroupsAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadAllGroupsAsync(false);
            });
        }

        #endregion

        #region ReadMyAsync

        private async Task<IEnumerable<Group>> ReadMyGroupsAsync(bool hasPermission = true)
        {
            // ARRANGE
            if (!hasPermission)
            {
                await Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserOutside.Route);
            }

            // ACT
            IGroupService groups = new GroupService(
               Builder.Rbac.Value,
               Builder.NotificationService.Value,
               Builder.Infrastructure.Value,
               Builder.Mapper.Value,
               Builder.Logger<GroupService>());
            IEnumerable<Group> group = await groups.ReadMyAsync(IdentityDbSeed.OrgPrimary.Route);

            return group;
        }

        [Fact]
        public async Task ReadMyGroupsAsync_HasPermission()
        {
            // ARRANGE + ACT
            IEnumerable<Group> groups = await ReadMyGroupsAsync();

            // ASSERT
            Assert.Equal(2, groups.Count());
        }

        [Fact]
        public async Task ReadMyGroupsAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadMyGroupsAsync(false);
            });
        }

        private async Task<IEnumerable<Group>> ReadMyTargetGroupsAsync(bool hasPermission = true)
        {
            // ARRANGE
            if (!hasPermission)
            {
                await Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserOutside.Route);
            }

            // ACT
            IGroupService groups = new GroupService(
               Builder.Rbac.Value,
               Builder.NotificationService.Value,
               Builder.Infrastructure.Value,
               Builder.Mapper.Value,
               Builder.Logger<GroupService>());
            IEnumerable<Group> group = await groups.ReadMyTargetsAsync(IdentityDbSeed.OrgPrimary.Route);

            return group;
        }

        [Fact]
        public async Task ReadMyTargetGroupsAsync_HasPermission()
        {
            // ARRANGE + ACT
            IEnumerable<Group> groups = await ReadMyTargetGroupsAsync();

            // ASSERT
            Assert.Equal(3, groups.Count());
        }

        [Fact]
        public async Task ReadMyTargetGroupsAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await ReadMyTargetGroupsAsync(false);
            });
        }

        #endregion

        #region DeleteAsync

        private async Task DeleteAsync(TestUserContext userContext)
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(userContext);
            GroupEntity group = Builder.IdentityDb.Value.Groups.Find(1);
            OrganizationEntity org = await Builder.IdentityDb.Value.Organizations.FindAsync(group.OrganizationId);

            // ACT
            IGroupService groups = new GroupService(
               Builder.Rbac.Value,
               Builder.NotificationService.Value,
               Builder.Infrastructure.Value,
               Builder.Mapper.Value,
               Builder.Logger<GroupService>());
            await groups.DeleteAsync(org.Route, group.Route);

            // Assert
            GroupEntity updated = await Builder.IdentityDb.Value.Groups.FindAsync(1);
            Assert.NotNull(updated);
            Assert.True(updated.IsDeleted());
        }

        [Fact]
        public async Task DeleteAsync_HasPermission()
        {
            // ARRANGE + ACT
            await DeleteAsync(TestUserContext.OrgAdmin);
        }

        [Fact]
        public async Task DeleteAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await DeleteAsync(TestUserContext.Anonymous);
            });
        }

        #endregion

        #region PatchAsync

        private async Task PatchAsync(bool hasPermission = true)
        {
            // ARRANGE
            if (!hasPermission)
            {
                await Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserOutside.Route);
            }

            GroupEntity group = await Builder.IdentityDb.Value.Groups.FindAsync(1);
            OrganizationEntity org = await Builder.IdentityDb.Value.Organizations.FindAsync(group.OrganizationId);
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            string updateString = "9283498";
            patchDoc.Replace($"/{nameof(group.Name)}", group.Name + updateString);
            patchDoc.Replace($"/{nameof(group.Description)}", group.Description + updateString);

            // ACT
            IGroupService groups = new GroupService(
               Builder.Rbac.Value,
               Builder.NotificationService.Value,
               Builder.Infrastructure.Value,
               Builder.Mapper.Value,
               Builder.Logger<GroupService>());
            await groups.PatchAsync(org.Route, group.Route, patchDoc);

            // ASSERT
            GroupEntity updated = await Builder.IdentityDb.Value.Groups.FindAsync(1);
            Assert.NotNull(updated);
            Assert.NotEmpty(updated.Route);
            Assert.EndsWith(updateString, updated.Name);
            Assert.EndsWith(updateString, updated.Description);
        }

        [Fact]
        public async Task PatchAsync_HasPermission()
        {
            // ARRANGE + ACT
            await PatchAsync();
        }

        [Fact]
        public async Task PatchAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await PatchAsync(false);
            });
        }

        #endregion
    }
}
