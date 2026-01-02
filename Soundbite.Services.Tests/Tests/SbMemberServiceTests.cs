using Masticore;
using Masticore.Azure.MediaServices;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services.Tests;
using Soundbite.Entity;
using Soundbite.Services.Tests.Mock;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class SbMemberServiceTests : ServiceTestBase
    {
        public SbMemberServiceTests()
        {
            //TODO: Expose AzMediaService/Config in builder
            AzureMediaServiceConfig config = new AzureMediaServiceConfig();
            Builder.SbOrganizationService.Use(i => new SbOrganizationService(
                i.OrganizationService.Value,
                i.SessionService.Value,
                i.Infrastructure.Value,
                i.Logger<SbOrganizationService>(),
                i.Rbac.Value,
                new MockEmailTemplates(),
                Builder.Mapper.Value,
                Builder.TokenDataService.Value,
                config, // TODO: make part of builder                
                new AzureMediaService()));
        }

        public SbMemberService CreateSbMemberService(ISecurityContext security = null)
        {
            // ARRANGE
            if (security != null)
            {
                Builder.SecurityContext.Use(security);
            }

            Builder.NotificationService.Use(new MockNotifications());

            SbMemberService ret = new SbMemberService(
                Builder.SecurityContext.Value,
                Builder.NotificationService.Value,
                Builder.ImageService.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                Builder.Logger<SbMemberService>());
            return ret;
        }

        [Fact]
        public async Task InviteAsync_MemberWithDefaultPerms()
        {
            // ARRANGE

            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity user = db.MockUser("member@test.com");
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            MemberEntity member = team.MockMember(person);
            // By default member should be able to invite
            member.MemberRole = MemberRole.Member;
            db.SaveChanges();

            DbSnapshot snap = new DbSnapshot(db)
                .Snap<UserEntity>()
                .Snap<PersonEntity>()
                .Snap<MemberEntity>();

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            SbMemberService members = CreateSbMemberService(securityContext);

            // ACT
            await members.InviteAsync(org.Route, team.Route, invites);

            // ASSERT
            snap.AssertAdd<UserEntity>(1);
            snap.AssertAdd<PersonEntity>(1);
            snap.AssertAdd<MemberEntity>(1);
        }

        [Fact]
        public async Task InviteAsync_OwnerWithDefaultPerms()
        {
            // ARRANGE

            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity user = db.MockUser("member@test.com");
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            MemberEntity member = team.MockMember(person);
            // By default member should be able to invite
            member.MemberRole = MemberRole.Owner;
            db.SaveChanges();

            DbSnapshot snap = new DbSnapshot(db)
                .Snap<UserEntity>()
                .Snap<PersonEntity>()
                .Snap<MemberEntity>();

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            SbMemberService members = CreateSbMemberService(securityContext);

            // ACT
            await members.InviteAsync(org.Route, team.Route, invites);

            // ASSERT
            snap.AssertAdd<UserEntity>(1);
            snap.AssertAdd<PersonEntity>(1);
            snap.AssertAdd<MemberEntity>(1);
        }


        [Fact]
        public async Task InviteAsync_MemberButOwnerOnlyPerms()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;

            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity user = db.MockUser("member@test.com");
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            MemberEntity member = team.MockMember(person);
            // The current user is a member
            member.MemberRole = MemberRole.Member;
            db.SaveChanges();

            // Required perms are now owner
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            ISbOrganizationService orgs = Builder.SbOrganizationService.Value;
            OrgPermissions settings = await orgs.ReadPermissions(org.Route);
            settings.MinRoleForTeamInvite = MemberRole.Owner;
            await orgs.UpdatePermissions(org.Route, settings);

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            SbMemberService members = CreateSbMemberService(securityContext);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await members.InviteAsync(org.Route, team.Route, invites)
            );
        }

        [Fact]
        public async Task InviteAsync_OwnerWithOwnerOnlyPerms()
        {
            // ARRANGE
            SbDb db = Builder.DbContext.Value;

            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity user = db.MockUser("member@test.com");
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            MemberEntity member = team.MockMember(person);
            member.MemberRole = MemberRole.Owner;
            db.SaveChanges();

            // Required perms are now owner
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            ISbOrganizationService orgs = Builder.SbOrganizationService.Value;
            OrgPermissions settings = await orgs.ReadPermissions(org.Route);
            settings.MinRoleForTeamInvite = MemberRole.Owner;
            await orgs.UpdatePermissions(org.Route, settings);

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            SbMemberService members = CreateSbMemberService(securityContext);

            DbSnapshot snap = new DbSnapshot(db)
              .Snap<UserEntity>()
              .Snap<PersonEntity>()
              .Snap<MemberEntity>();

            // ACT
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
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity user = db.MockUser("member@test.com");
            ISecurityContext securityContext = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            Builder.SecurityContext.Use(securityContext);
            PersonEntity person = db.MockPerson(org, user);
            GroupEntity team = db.MockGroup(org);
            MemberEntity member = team.MockMember(person);
            // Unknown has no rights to add anyone ever
            member.MemberRole = MemberRole.Unknown;
            db.SaveChanges();

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "test@test.com"}
            };
            SbMemberService members = CreateSbMemberService();

            // ACT

            await Assert.ThrowsAsync<NotFoundException>(async () => await members.InviteAsync(org.Route, team.Route, invites));
        }
    }
}