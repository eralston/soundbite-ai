using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class MemberServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        #region Constructor

        public MemberServiceTests()
        {
            Builder.MemberService.Use(i => new MemberService(
                i.SecurityContext.Value,
                i.NotificationService.Value,
                i.ImageService.Value,
                i.Infrastructure.Value,
                i.Mapper.Value,
                i.Logger<MemberService>()));
        }

        #endregion

        [Fact]
        public async Task ReadAsync()
        {
            // ARRANGE            
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            IIdentityDb db = Builder.IdentityDb.Value;
            string devShortId = db.Members.Find(1).Route;

            // ACT
            IMemberService members = Builder.MemberService.Value;
            Member updatedMember = await members.ReadAsync(OrgRoute, GroupRoute, devShortId);

            // ASSERT
            AssertResource(updatedMember);
            Assert.NotEqual(MemberRole.Unknown, updatedMember.MemberRole);
            Assert.NotEqual(PersonRole.Unknown, updatedMember.Person.PersonRole);
        }

        [Fact]
        public async Task ReadAsync_UnknownClaims()
        {
            // ARRANGE            
            await Builder.SetCurrentUserContext(TestUserContext.RandomUser);
            MockIdentityDb db = Builder.DbContext.Value;
            string devShortId = db.Members.Find(1).Route;
            MockImageService images = new MockImageService();

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.ReadAsync(OrgRoute, GroupRoute, devShortId));
        }

        [Fact]
        public async Task ReadAsync_UnknownMember()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            string unknownShortId = ResourceExtensions.NewRoute();

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.ReadAsync(OrgRoute, GroupRoute, unknownShortId));
        }

        [Fact]
        [Obsolete]
        public async Task ReadAllAsync_Obsolete()
        {
            // ARRANGE
            MockImageService imageService = new MockImageService();
            Builder.ImageService.Use(imageService);
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            // ACT
            IMemberService members = Builder.MemberService.Value;
            IEnumerable<Member> allMembers = await members.ReadAllAsync_Obsolete(OrgRoute, GroupRoute);

            // ASSERT
            Assert.NotNull(allMembers);
            Assert.Equal(3, allMembers.Count());
            Assert.NotNull(allMembers.First().Route);
            Assert.NotNull(allMembers.First().Person.Route);
            Assert.NotNull(allMembers.First().Person.User.Route);
            Assert.NotNull(allMembers.First().Person.User.ImageSrc);
            Assert.Equal(3, imageService.UserImageCount);
        }

        [Fact]
        [Obsolete]
        public async Task ReadAllAsync_Obsolete_UnknownOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            IMemberService members = Builder.MemberService.Value;

            // ACT & ASSERT            
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.ReadAllAsync_Obsolete(ResourceExtensions.NewRoute(), GroupRoute));
        }

        [Fact]
        [Obsolete]
        public async Task ReadAllAsync_Obsolete_UnknownGroup()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.ReadAllAsync_Obsolete(OrgRoute, ResourceExtensions.NewRoute()));
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            IIdentityDb db = Builder.IdentityDb.Value;
            MemberEntity member = db.Members.Find(1);
            DateTime originalUpdatedUtc = member.UpdatedUtc;
            Member memberDto = new Member()
            {
                Route = member.Route,
                MemberRole = MemberRole.Member,
            };


            // ACT
            IMemberService members = Builder.MemberService.Value;
            Member updatedMember = await members.UpdateAsync(OrgRoute, GroupRoute, memberDto.Route, memberDto);

            // ASSERT
            AssertResource(updatedMember);
            Assert.Equal(MemberRole.Member, updatedMember.MemberRole);
            Assert.Equal(PersonRole.Admin, updatedMember.Person.PersonRole);
            Assert.Equal(member.CreatedUtc, updatedMember.CreatedUtc);
            Assert.NotEqual(member.UpdatedUtc, originalUpdatedUtc);
        }

        [Fact]
        public async Task UpdateAsync_UnknownMember()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            Member memberDto = new Member()
            {
                Route = ResourceExtensions.NewRoute(),
                MemberRole = MemberRole.Member
            };

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.UpdateAsync(OrgRoute, GroupRoute, memberDto.Route, memberDto));
        }

        [Fact]
        public async Task UpdateAsync_UnknownOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            Member memberDto = new Member()
            {
                Route = ResourceExtensions.NewRoute(),
                MemberRole = MemberRole.Member
            };

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.UpdateAsync("Unknown", GroupRoute, memberDto.Route, memberDto));
        }

        [Fact]
        public async Task UpdateAsync_UnknownTeam()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            IIdentityDb db = Builder.IdentityDb.Value;
            string devShortId = db.Members.Find(1).Route;
            Member memberDto = new Member()
            {
                Route = devShortId,
                MemberRole = MemberRole.Member
            };

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.UpdateAsync(OrgRoute, "Unknown", memberDto.Route, memberDto));
        }

        [Fact]
        public async Task DeleteAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            IIdentityDb db = Builder.IdentityDb.Value;
            MemberEntity member = db.Members.Find(1);
            Assert.NotNull(member);
            string devShortId = db.Members.Find(1).Route;

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await members.DeleteAsync(OrgRoute, GroupRoute, devShortId);

            // ASSERT
            member = db.Members.Find(1);
            Assert.NotNull(member.DeletedUtc);
        }

        [Fact]
        public async Task DeleteAsync_RegularPerson()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            MockIdentityDb db = Builder.DbContext.Value;
            UserEntity regularUser = db.Users.Where(u => u.Route == IdentityDbSeed.UserRegular.Route).Single();
            MemberEntity regularMember = db.Members.Where(m => m.Person.User.Route == regularUser.Route && m.Group.Route == IdentityDbSeed.GroupOrgAdminOwned.Route).Single();
            string memberRoute = regularMember.Route;

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.DeleteAsync(OrgRoute, GroupRoute, memberRoute));
        }

        [Fact]
        public async Task DeleteAsync_UnknownMember()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            string memberId = ResourceExtensions.NewRoute();

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.DeleteAsync(OrgRoute, GroupRoute, memberId));
        }

        [Fact]
        public async Task DeleteAsync_UnknownClaims()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.RandomUser);
            MockIdentityDb db = Builder.DbContext.Value;
            string devShortId = db.Members.Find(1).Route;

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.DeleteAsync(OrgRoute, GroupRoute, devShortId));
        }

        [Fact]
        public async Task InviteAsync_Admin()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            DbSnapshot snap = new DbSnapshot(Builder.DbContext.Value).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>();
            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(OrgRoute, GroupRoute, invites);

            // ASSERT
            snap.AssertAdd<UserEntity>(1);
            snap.AssertAdd<PersonEntity>(1);
            snap.AssertAdd<MemberEntity>(1);
        }

        [Fact]
        public async Task InviteAsync_Owner()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();

            // Make the admin user
            UserEntity owner = db.MockUser("admin@test.com");
            PersonEntity ownerPerson = db.MockPerson(org, owner);
            GroupEntity team = db.MockGroup(org);
            MemberEntity ownerMember = team.MockMember(ownerPerson);
            ownerMember.MemberRole = MemberRole.Owner;
            db.SaveChanges();
            await Builder.SetCurrentUserContext(owner);
            DbSnapshot snap = new DbSnapshot(db).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>();

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(org.Route, team.Route, invites);

            // ASSERT
            snap.AssertAdd<UserEntity>(1);
            snap.AssertAdd<PersonEntity>(1);
            snap.AssertAdd<MemberEntity>(1);
        }

        [Fact]
        public async Task InviteAsync_PersonWithWrongPerms()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity user = db.MockUser("member@test.com");
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            MemberEntity member = team.MockMember(person);
            member.MemberRole = MemberRole.Member;
            db.SaveChanges();
            await Builder.SetCurrentUserContext(user);
            DbSnapshot snap = new DbSnapshot(db).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>();

            ILogger<MemberService> logger = new ThrowIfErrorLogger<MemberService>();
            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            MockImageService images = new MockImageService();

            // ACT
            IMemberService members = Builder.MemberService.Value;

            await Assert.ThrowsAsync<NotFoundException>(async () => await members.InviteAsync(org.Route, team.Route, invites));
        }

        [Fact]
        public async Task InviteAsync_PersonWithRightPerms()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();

            // Make the admin user
            UserEntity user = db.MockUser("member@test.com");
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            MemberEntity member = team.MockMember(person);
            member.MemberRole = MemberRole.Owner;
            db.SaveChanges();
            await Builder.SetCurrentUserContext(user);
            DbSnapshot snap = new DbSnapshot(db).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>();

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };


            // ACT
            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(org.Route, team.Route, invites);

            // ASSERT
            snap.AssertAdd<UserEntity>(1);
            snap.AssertAdd<PersonEntity>(1);
            snap.AssertAdd<MemberEntity>(1);
        }

        [Fact]
        public async Task InviteAsync_ArchivedPerson()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();

            // Make the admin user
            UserEntity admin = db.MockUser("admin@test.com");
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(admin), admin.Id);
            Builder.SecurityContext.Use(securityContext);
            PersonEntity adminPerson = db.MockPerson(org, admin);
            adminPerson.PersonRole = PersonRole.Admin;
            // Make the regular user
            UserEntity user = db.MockUser("invitee@test.com");
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            db.SaveChanges();
            DbSnapshot snap = new DbSnapshot(db).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>();

            Invite[] invites = new Invite[]
            {
                new Invite { Token = user.Email}
            };

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(org.Route, team.Route, invites);

            // ASSERT
            snap.AssertAdd<UserEntity>(0);
            snap.AssertAdd<PersonEntity>(0);
            snap.AssertAdd<MemberEntity>(1);
        }

        [Fact]
        public async Task InviteAsync_ArchivedTeam()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity admin = db.MockUser("admin@test.com");
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(admin), admin.Id);
            Builder.SecurityContext.Use(securityContext);
            PersonEntity adminPerson = db.MockPerson(org, admin);
            adminPerson.PersonRole = PersonRole.Admin;
            // Make the regular user
            UserEntity user = db.MockUser("invitee@test.com");
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org).SoftDeleteNow();
            db.SaveChanges();

            ILogger<MemberService> logger = new ThrowIfErrorLogger<MemberService>();
            Invite[] invites = new Invite[]
            {
                new Invite { Token = user.Email}
            };
            MockImageService images = new MockImageService();

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.InviteAsync(org.Route, team.Route, invites));
        }

        [Fact]
        public async Task InviteAsync_ArchivedOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg().SoftDeleteNow();

            // Make the regular user
            UserEntity user = db.MockUser("invitee@test.com");
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            db.SaveChanges();

            ILogger<MemberService> logger = new ThrowIfErrorLogger<MemberService>();
            Invite[] invites = new Invite[]
            {
                new Invite { Token = user.Email}
            };

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.InviteAsync(org.Route, team.Route, invites));
        }

        [Fact]
        public async Task InviteAsync_ExistingMember()
        {
            // ARRANGE
            Builder.NotificationService.Use(Builder.NotificationService.Value);
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            IIdentityDb db = Builder.IdentityDb.Value;
            DbSnapshot dbSnap = new DbSnapshot(db as DbContext).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>();
            Invite[] invites = new Invite[]
            {
                new Invite { Token = "Ada@Masticore" }
            };

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(OrgRoute, GroupRoute, invites);

            // ASSERT
            Assert.Equal(1, Builder.GetMockNotificationService().Count);
            dbSnap.AssertAdd<UserEntity>(0);
            dbSnap.AssertAdd<PersonEntity>(0);
            dbSnap.AssertAdd<MemberEntity>(0);
        }

        [Fact]
        public async Task InviteAsync_UnknownClaims()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.RandomUser);
            ILogger<MemberService> logger = new ThrowIfErrorLogger<MemberService>();
            ISecurityContext securityContext = new MockSecurityContext(true);
            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            MockImageService images = new MockImageService();

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.InviteAsync(OrgRoute, GroupRoute, invites));
        }

        [Fact]
        public async Task InviteAsync_UnknownOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.ExternalOrgMember);
            ILogger<MemberService> logger = new ThrowIfErrorLogger<MemberService>();
            ISecurityContext securityContext = new MockSecurityContext(true);
            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            MockImageService images = new MockImageService();

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.InviteAsync("Unknown", GroupRoute, invites));
        }

        [Fact]
        public async Task InviteAsync_UnknownTeam()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            ILogger<MemberService> logger = new ThrowIfErrorLogger<MemberService>();
            ISecurityContext securityContext = new MockSecurityContext(true);
            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };

            // ACT & ASSERT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await members.InviteAsync(OrgRoute, "Unknown", invites));
        }

        [Fact]
        public async Task InviteAsync_RepeatEmail()
        {
            // TODO: Consider if this known behavior is actually a bug

            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            IIdentityDb db = Builder.IdentityDb.Value;
            List<Invite> invites = new List<Invite>
            {
                new Invite { Token = "test@test.com" },
                new Invite { Token = "test@test.com" }
            };

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await Assert.ThrowsAsync<DbUpdateException>(async () => await members.InviteAsync(OrgRoute, GroupRoute, invites.ToArray()));
        }

        [Fact]
        public async Task InviteAsync_MixedOldAndNew()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);

            IIdentityDb db = Builder.IdentityDb.Value;
            DbSnapshot dbSnap = new DbSnapshot(db as DbContext).Snap<UserEntity>().Snap<PersonEntity>().Snap<MemberEntity>();
            List<Invite> invites = new List<Invite>
            {
                new Invite { Token = "test@test.com" }
            };
            string[] newPersonRoutes = db.People.Where(p => p.Id == 4).Select(p => p.Route).ToArray();
            foreach (string route in newPersonRoutes)
            {
                invites.Add(new Invite { Token = route });
            }
            string existingPersonRoute = db.Members.Where(m => m.Group.Route == GroupRoute).Include(m => m.Person).FirstOrDefault().Person.Route;
            invites.Add(new Invite { Token = existingPersonRoute });

            // ACT
            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(OrgRoute, GroupRoute, invites.ToArray());

            // ASSERT
            Assert.Equal(3, Builder.GetMockNotificationService().Count);   // Everybody gets an email
            dbSnap.AssertAdd<UserEntity>(1);        // Only the email gets new user + person
            dbSnap.AssertAdd<PersonEntity>(1);
            dbSnap.AssertAdd<MemberEntity>(2);      // New records for people + email, but nothing for existing member
        }
    }
}
