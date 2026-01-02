using Masticore.Resources;
using Masticore.Security;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class TestDbExtensionsTests : ResourceTestBase<OrganizationEntity, MockIdentityDb, MockSeedInfrastructure>
    {
        [Fact]
        public async Task UserWhereUid()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            IClaims claims = new MockClaims();
            OrganizationEntity org = db.Organizations.Find(1);
            PersonEntity person = await db.PersonWhereUid(org.Route, claims.UniversalId);
            Assert.NotNull(person);
            Assert.Equal(PersonRole.Admin, person.PersonRole);

            // ACT
            UserEntity queryUser = await db.UserWhereUid(org.Route, claims.UniversalId, PersonRole.Admin);

            // ASSERT
            Assert.NotNull(queryUser);
        }

        [Fact]
        public async Task UserWhereUid_God()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            UserEntity user = db.Users.CreateResource();
            user.Email = "God@Heaven.org";
            user.UniversalId = ResourceExtensions.NewUniversalId();
            user.UserRole = UserRole.God;
            db.SaveChanges();

            IClaims claims = new MockUserClaims(user);
            OrganizationEntity org = db.Organizations.Find(1);
            // There is only a user record for the claims
            PersonEntity person = await db.PersonWhereUid(org.Route, claims.UniversalId);
            Assert.Null(person);

            // ACT
            UserEntity queryUser = await db.UserWhereUid(org.Route, claims.UniversalId, PersonRole.Admin);

            // ASSERT
            Assert.NotNull(queryUser);
        }

        [Fact]
        public async Task UserWhereUid_AdminCheckingAsGuest()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            IClaims claims = new MockClaims();
            OrganizationEntity org = db.Organizations.Find(1);
            PersonEntity person = await db.PersonWhereUid(org.Route, claims.UniversalId);
            Assert.NotNull(person);
            Assert.Equal(PersonRole.Admin, person.PersonRole);

            // ACT
            UserEntity queryUser = await db.UserWhereUid(org.Route, claims.UniversalId, PersonRole.Guest);

            // ASSERT
            Assert.NotNull(queryUser);
        }

        [Fact]
        public async Task UserWhereUid_UnknownClaims()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            IClaims claims = new MockClaims(true);
            OrganizationEntity org = db.Organizations.Find(1);
            PersonEntity person = await db.PersonWhereUid(org.Route, claims.UniversalId);
            Assert.Null(person);

            // ACT
            UserEntity queryUser = await db.UserWhereUid(org.Route, claims.UniversalId, PersonRole.Admin);

            // ASSERT
            Assert.Null(queryUser);
        }
    }
}
