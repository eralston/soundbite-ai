using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class CurrentUserDbRbacTests
    {
        #region Properties

        protected TestBuilder<MockSeedInfrastructure, MockIdentityDb> Builder { get; } = new TestBuilder<MockSeedInfrastructure, MockIdentityDb>();

        public ILogger<CurrentUserDbRbac> Logger { get; } = NullLogger<CurrentUserDbRbac>.Instance;

        public MockSeedInfrastructure Infrastructure { get; protected set; } = new MockSeedInfrastructure();

        #endregion

        #region Constructor

        public CurrentUserDbRbacTests()
        {
            Builder.UseNullLogger = true;
        }

        #endregion

        #region Methods - Utility

        private static ISecurityContext Security(IIdentityDb db, string userRoute)
        {
            UserEntity user = db.Users.Where(u => u.Route == userRoute).SingleOrDefault();
            user.AssertFound($"Could not find user by route '{userRoute}'");
            ISecurityContext security = new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id);
            return security;
        }

        #endregion

        [Fact]
        public async Task GodUserHasAllRoles()
        {
            // ARRANGE
            MockIdentityDb db = await Infrastructure.DbAsync();
            ISecurityContext security = Security(db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Logger, Infrastructure, security);

            // ACT & ASSERT
            await dbRbac.AssertCurrentUserInRole(UserRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(UserRole.User);
            await dbRbac.AssertCurrentUserInRole(UserRole.God);
        }

        [Fact]
        public async Task RegularUserIsJustUser()
        {
            // ARRANGE
            MockIdentityDb db = await Infrastructure.DbAsync();
            ISecurityContext security = Security(db, IdentityDbSeed.UserRegular.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Logger, Infrastructure, security);

            // ACT & ASSERT
            await dbRbac.AssertCurrentUserInRole(UserRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(UserRole.User);
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(UserRole.God);
            });
        }

        [Fact]
        public async Task GodPersonHasAllRoles()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            IRbac dbRbac = Builder.Rbac.Value;

            // ACT & ASSERT

            // Can access as every role
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Guest);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Person);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Admin);

            // Can even access orgs where they don't have an association
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Guest);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Person);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Admin);

            // Cannot access non-existent resource
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(ResourceExtensions.NewRoute(), PersonRole.Unknown);
            });
        }

        [Fact]
        public async Task RegularPersonIsJustPerson()
        {
            // ARRANGE
            MockIdentityDb db = await Infrastructure.DbAsync();
            ISecurityContext security = Security(db, IdentityDbSeed.UserRegular.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Logger, Infrastructure, security);

            // ACT & ASSERT

            // Can access up to Admin
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Guest);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Person);
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, PersonRole.Admin);
            });

            // Cannot access non-associated at any level
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Unknown);
            });
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Guest);
            });
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Person);
            });
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, PersonRole.Admin);
            });
        }

        [Fact]
        public async Task GodIsMemberToAll()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            IRbac dbRbac = Builder.Rbac.Value;

            // ACT & ASSERT
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route, MemberRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route, MemberRole.Member);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route, MemberRole.Owner);

            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, MemberRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, MemberRole.Member);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, MemberRole.Owner);

            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, IdentityDbSeed.GroupOutside.Route, MemberRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, IdentityDbSeed.GroupOutside.Route, MemberRole.Member);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, IdentityDbSeed.GroupOutside.Route, MemberRole.Owner);

            // Cannot access non-existent resource
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(ResourceExtensions.NewRoute(), IdentityDbSeed.GroupOrgAdminOwned.Route, MemberRole.Unknown);
            });
        }

        [Fact]
        public async Task RegularMemberIsJustMember()
        {
            // ARRANGE
            await Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserRegular.Route);
            IRbac dbRbac = Builder.Rbac.Value;

            // ACT & ASSERT

            // Is a Member, but not admin
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route, MemberRole.Unknown);
            await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route, MemberRole.Member);
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route, MemberRole.Owner);
            });

            // Not a member
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, MemberRole.Unknown);
            });
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, MemberRole.Member);
            });
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, MemberRole.Owner);
            });

            // Neither a person nor member
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, IdentityDbSeed.GroupOutside.Route, MemberRole.Unknown);
            });
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, IdentityDbSeed.GroupOutside.Route, MemberRole.Member);
            });
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await dbRbac.AssertCurrentUserInRole(IdentityDbSeed.OrgOutside.Route, IdentityDbSeed.GroupOutside.Route, MemberRole.Owner);
            });
        }

        [Fact]
        public async Task GodUserReadRoles()
        {
            // ARRANGE
            MockIdentityDb db = await Infrastructure.DbAsync();
            ISecurityContext security = Security(db, IdentityDbSeed.UserGod.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Logger, Infrastructure, security);

            // ACT & ASSERT

            // User

            UserRole userRole = await dbRbac.CurrentUserRole();
            Assert.Equal(UserRole.God, userRole);

            // Person - God users basically bypass checking if orgs exist and just come back as having access to everything

            PersonRole personRole = await dbRbac.CurrentPersonRole(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(PersonRole.Admin, personRole);

            PersonRole outsidePersonRole = await dbRbac.CurrentPersonRole(IdentityDbSeed.OrgOutside.Route);
            Assert.Equal(PersonRole.Admin, outsidePersonRole);

            PersonRole randomOrgPersonRole = await dbRbac.CurrentPersonRole(ResourceExtensions.NewUniversalId());
            Assert.Equal(PersonRole.Admin, randomOrgPersonRole);

            // Member - God users basically bypass checking if orgs and groups exist and just come back as having access to everything

            MemberRole memberRole = await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(MemberRole.Owner, memberRole);

            MemberRole outsideMemberRole = await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOutside.Route);
            Assert.Equal(MemberRole.Owner, outsideMemberRole);

            MemberRole randomOrgMemberRole = await dbRbac.CurrentMemberRole(ResourceExtensions.NewUniversalId(), IdentityDbSeed.GroupOutside.Route);
            Assert.Equal(MemberRole.Owner, randomOrgMemberRole);

            MemberRole randomGroupMemberRole = await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, ResourceExtensions.NewUniversalId());
            Assert.Equal(MemberRole.Owner, randomGroupMemberRole);

            MemberRole randomBothMemberRole = await dbRbac.CurrentMemberRole(ResourceExtensions.NewUniversalId(), ResourceExtensions.NewUniversalId());
            Assert.Equal(MemberRole.Owner, randomBothMemberRole);
        }

        [Fact]
        public async Task RegularUserReadRoles()
        {
            // ARRANGE
            MockIdentityDb db = await Infrastructure.DbAsync();
            ISecurityContext security = Security(db, IdentityDbSeed.UserRegular.Route);
            IRbac dbRbac = new CurrentUserDbRbac(Logger, Infrastructure, security);

            // ACT & ASSERT

            // User

            UserRole userRole = await dbRbac.CurrentUserRole();
            Assert.Equal(UserRole.User, userRole);

            // Person

            PersonRole personRole = await dbRbac.CurrentPersonRole(IdentityDbSeed.OrgPrimary.Route);
            Assert.Equal(PersonRole.Person, personRole);

            PersonRole outsidePersonRole = await dbRbac.CurrentPersonRole(IdentityDbSeed.OrgOutside.Route);
            Assert.Equal(PersonRole.Unknown, outsidePersonRole);

            PersonRole randomOrgPersonRole = await dbRbac.CurrentPersonRole(ResourceExtensions.NewUniversalId());
            Assert.Equal(PersonRole.Unknown, randomOrgPersonRole);

            // Member

            MemberRole memberRole = await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            Assert.Equal(MemberRole.Member, memberRole);

            MemberRole outsideMemberRole = await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOutside.Route);
            Assert.Equal(MemberRole.Unknown, outsideMemberRole);

            MemberRole randomOrgMemberRole = await dbRbac.CurrentMemberRole(ResourceExtensions.NewUniversalId(), IdentityDbSeed.GroupOutside.Route);
            Assert.Equal(MemberRole.Unknown, randomOrgMemberRole);

            MemberRole randomGroupMemberRole = await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, ResourceExtensions.NewUniversalId());
            Assert.Equal(MemberRole.Unknown, randomGroupMemberRole);

            MemberRole randomBothMemberRole = await dbRbac.CurrentMemberRole(ResourceExtensions.NewUniversalId(), ResourceExtensions.NewUniversalId());
            Assert.Equal(MemberRole.Unknown, randomBothMemberRole);
        }

        [Fact]
        public async Task UnknownUserReadRoles()
        {
            // ARRANGE
            MockIdentityDb db = await Infrastructure.DbAsync();
            ISecurityContext security = new MockSecurityContext(true, true);
            IRbac dbRbac = new CurrentUserDbRbac(Logger, Infrastructure, security);

            // ACT & ASSERT

            // User

            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await dbRbac.CurrentUserRole();
            });

            // Person

            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await dbRbac.CurrentPersonRole(IdentityDbSeed.OrgPrimary.Route);
            });

            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await dbRbac.CurrentPersonRole(ResourceExtensions.NewRoute());
            });

            // Member

            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            });

            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await dbRbac.CurrentMemberRole(IdentityDbSeed.OrgPrimary.Route, ResourceExtensions.NewRoute());
            });

            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await dbRbac.CurrentMemberRole(ResourceExtensions.NewRoute(), ResourceExtensions.NewRoute());
            });
        }
    }
}
