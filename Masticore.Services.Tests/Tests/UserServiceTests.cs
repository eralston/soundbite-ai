using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class UserServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="UserServiceTests"/> instance.
        /// </summary>
        public UserServiceTests()
        {
            MockGraph graph = new MockGraph();
            Builder.UserService.Use(i => new UserService(
                i.SecurityContext.Value,
                i.Rbac.Value,
                i.ImageService.Value,
                graph,
                i.NotificationService.Value,
                i.Infrastructure.Value,
                i.Mapper.Value,
                i.Logger<UserService>()));
        }

        #endregion

        protected async Task<IUserService> CreateUserService(bool isRandomUser = false)
        {
            await Builder.SetCurrentUserContext(isRandomUser ? TestUserContext.RandomUser : TestUserContext.OrgAdmin);
            IUserService users = Builder.UserService.Value;
            return users;
        }

        [Fact]
        public async Task ReadAsync()
        {
            // ARRANGE
            IUserService users = await CreateUserService();

            // ACT
            User user = await users.ReadAsync(IdentityDbSeed.UserOrgAdmin.Route);
            User userNoImage = await users.ReadAsync(IdentityDbSeed.UserOrgAdmin.Route, false);

            // ASSERT
            Assert.NotNull(user);
            Assert.Equal(IdentityDbSeed.UserOrgAdmin.Route, user.Route);
            Assert.NotNull(user.UniversalId);
            Assert.NotNull(user.ImageSrc);

            Assert.NotNull(userNoImage);
            Assert.Equal(IdentityDbSeed.UserOrgAdmin.Route, userNoImage.Route);
            Assert.NotNull(userNoImage.UniversalId);
            Assert.Null(userNoImage.ImageSrc);

            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await users.ReadAsync("Not A Real Route", true);
            });
        }

        [Fact]
        public async Task ReadMeAsync()
        {
            // ARRANGE
            IUserService users = await CreateUserService();

            // ACT
            User user = await users.ReadMeAsync();
            User userNoImage = await users.ReadMeAsync(false);

            // ASSERT
            Assert.NotNull(user);
            Assert.Equal(IdentityDbSeed.UserOrgAdmin.Route, user.Route);
            Assert.NotNull(user.UniversalId);
            Assert.NotNull(user.ImageSrc);

            Assert.NotNull(userNoImage);
            Assert.Equal(IdentityDbSeed.UserOrgAdmin.Route, userNoImage.Route);
            Assert.NotNull(userNoImage.UniversalId);
            Assert.Null(userNoImage.ImageSrc);
        }

        [Fact]
        public async Task ReadMeAsync_NoUser()
        {
            // ARRANGE
            IUserService users = await CreateUserService(true);

            // ACT
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await users.ReadMeAsync();
            });
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await users.ReadMeAsync(false);
            });
        }

        [Fact]
        public async Task ReadByOrgAndEmail()
        {
            // ARRANGE
            IUserService users = await CreateUserService();
            IdentityDb db = Builder.DbContext.Value;
            UserEntity userEnt = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();

            // ACT
            User user = await users.ReadByOrgRouteAndEmail(IdentityDbSeed.OrgPrimary.Route, userEnt.Email);
            User userNoImage = await users.ReadByOrgRouteAndEmail(IdentityDbSeed.OrgPrimary.Route, user.Email, false);

            // ASSERT
            Assert.NotNull(user);
            Assert.Equal(IdentityDbSeed.UserOrgAdmin.Route, user.Route);
            Assert.NotNull(user.UniversalId);
            Assert.NotNull(user.ImageSrc);

            Assert.NotNull(userNoImage);
            Assert.Equal(IdentityDbSeed.UserOrgAdmin.Route, userNoImage.Route);
            Assert.NotNull(userNoImage.UniversalId);
            Assert.Null(userNoImage.ImageSrc);

            Assert.Null(await users.ReadByOrgRouteAndEmail("Not A Real Route", user.Email, true));

            Assert.Null(await users.ReadByOrgRouteAndEmail(IdentityDbSeed.OrgPrimary.Route, "Not a real E-Mail", true));
        }

        [Fact]
        public async Task UpsertMeAsync()
        {
            // ARRANGE
            IUserService users = await CreateUserService();

            IdentityDb db = Builder.DbContext.Value;

            // Pull out the orgAdmin user
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            UserEntity orgAdminUser = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();
            string oldEmail = orgAdminUser.Email;
            string oldUid = orgAdminUser.UniversalId;
            string oldFirstName = orgAdminUser.GivenName;
            string oldFamilyName = orgAdminUser.FamilyName;

            // ACT - Update some example fields
            const string newFamilyName = "New Family Name";
            User user = await users.UpsertMeAsync(new User
            {
                // The upsert should actually ignore these fields
                UniversalId = "Does Not Match UID",
                Email = "Does Not Match Email",
                // But it should take other field changes
                FamilyName = newFamilyName,
            });

            // ASSERT

            Assert.NotNull(user);
            Assert.Equal(oldUid, user.UniversalId);
            Assert.Equal(oldEmail, user.Email);
            Assert.Equal(oldFirstName, user.GivenName);
            Assert.Equal(newFamilyName, user.FamilyName);

            // The user record should have the UID from the context and the e-mail from the database
            UserEntity emailUser = await db.UserWhereEmail(oldEmail);
            Assert.NotNull(emailUser);
            UserEntity uidUser = await db.UserWhereUid(oldUid);
            Assert.NotNull(uidUser);
            Assert.Same(emailUser, uidUser);

            // It should match both fields now
            Assert.Equal(oldUid, emailUser.UniversalId);
            Assert.Equal(oldEmail, emailUser.Email);
            Assert.Equal(oldFirstName, emailUser.GivenName);
            Assert.Equal(newFamilyName, emailUser.FamilyName);
        }

        [Fact]
        public async Task UpsertMeAsync_MergeUsers()
        {
            // ARRANGE
            IUserService users = await CreateUserService();

            IdentityDb db = Builder.DbContext.Value;

            // Manipulate the database to introduce a match conflict
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            UserEntity orgAdminUser = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();
            string matchEmail = orgAdminUser.Email;
            Assert.NotNull(matchEmail);
            string matchUid = orgAdminUser.UniversalId;
            Assert.NotNull(matchUid);
            orgAdminUser.Email = "other@email.com"; // UID not matching
            Builder.SecurityContext.Value.CurrentUser.Route = "Not Correct At All"; // Context route being null means we haven't matched to claims yet

            // Add a distinct user to the context that has a certain e-mail
            UserEntity misleadingUser = new UserEntity()
            {
                UniversalId = null, // UID not set
                Email = matchEmail, // E-mail matches
                Route = "Route 1"
            };
            db.Users.Add(misleadingUser);

            // Create redundant people entries point to the new misleading user
            PersonEntity[] orgAdminPeople = db.People.Where(u => u.User.Route == IdentityDbSeed.UserOrgAdmin.Route).Include(p => p.Organization).ToArray();
            foreach (PersonEntity person in orgAdminPeople)
            {
                db.People.Add(new PersonEntity
                {
                    Route = ResourceExtensions.NewRoute(),
                    User = misleadingUser,
                    Organization = person.Organization,
                });
            }

            db.SaveChanges();

            // We now have two competing UserEntity records such that one has a UID and the other has an e-mail

            // ACT - Call updating a user with UID from one user record and e-mail from another
            User user = await users.UpsertMeAsync(new User
            {
                // The upsert should actually ignore these fields
                UniversalId = matchUid,
                Email = matchEmail,
            });

            // ASSERT

            // The user we just got back has consistent attributes
            Assert.NotNull(user);
            Assert.Equal(matchUid, user.UniversalId);
            Assert.Equal(matchEmail, user.Email);

            // The user record should have the UID from the context and the e-mail from the database
            UserEntity emailUser = await db.UserWhereEmail(matchEmail);
            Assert.NotNull(emailUser);
            UserEntity uidUser = await db.UserWhereUid(matchUid);
            Assert.NotNull(uidUser);
            Assert.Same(emailUser, uidUser);

            // It should match both fields now
            Assert.Equal(matchUid, emailUser.UniversalId);
            Assert.Equal(matchEmail, emailUser.Email);
        }

        /// <summary>
        /// When a user has multiple <see cref="UserIdentityEntity"/> objects associated with it and we're logging in with one of them instead of the creds in their main <see cref=""/>
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpsertMeAsync_MultipleIdentities()
        {
            // ARRANGE

            IdentityDb db = Builder.DbContext.Value;

            // Manipulate the database to force a match by alt email
            Builder.SecurityContext.Value.CurrentUser = null;
            UserEntity orgAdminUser = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();
            string altEmail = orgAdminUser.Email;
            Assert.NotNull(altEmail);

            // Make the original email an alt
            UserIdentityEntity altForEmail = db.UserIdentities.CreateResource();
            altForEmail.Identifier = altEmail;
            altForEmail.User = orgAdminUser;

            // Ensure it must match by the alt email
            // Clear the UID
            string oldUid = orgAdminUser.UniversalId;
            string newUid = "67890";
            orgAdminUser.UniversalId = null; // UID not set
            orgAdminUser.Email = "other@email.com"; // e-mail not matching
            MockClaims claims = new MockClaims()
            {
                Email = altEmail,
                UniversalId = newUid
            };
            db.SaveChanges();

            IUserService users = Builder.UserService.Value;

            // ACT
            User myUser = await users.UpsertMeAsync(claims);

            // ASSERT

            // The user we just got back has consistent attributes
            Assert.NotNull(myUser);
            Assert.Equal(newUid, myUser.UniversalId);
            Assert.Equal(altEmail, myUser.Email);
        }
    }
}
