using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Exceptions;
using Masticore.Graph;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class PersonServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="PersonServiceTests"/> instance.
        /// </summary>
        public PersonServiceTests()
        {
            // Use a regular use by default if another user context is not specified
            Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserRegular.Route).Wait();

            Builder.UseNullLogger = true;

            Builder.PersonService.Use((i) => new PersonService(
                i.Rbac.Value,
                i.NotificationService.Value,
                i.Infrastructure.Value,
                i.Mapper.Value,
                i.ImageService.Value,
                i.Logger<PersonService>()));
        }

        #endregion

        #region InviteAsync

        [Fact]
        public async Task InviteAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            IIdentityDb db = Builder.IdentityDb.Value;
            DbSnapshot snapshot = new DbSnapshot(db as DbContext)
                .Snap<UserEntity>()
                .Snap<PersonEntity>();
            UserEntity existingUser = db.Users.Where(u => u.Route == IdentityDbSeed.UserOrgAdmin.Route).Single();
            UserEntity existingUserForRoute = db.Users.Where(u => u.Route == IdentityDbSeed.UserRegular.Route).Single();
            Invite[] invites = new Invite[]
            {
                new Invite { Token = "invitee@test.com" },              // New User and Person by e-mail
                new Invite { Token = existingUser.Email.ToUpper() },    // Existing User and Person by e-mail
                new Invite { Token = IdentityDbSeed.UserRegular.Route},  // Existing User and Person by route
                new Invite { Token = IdentityDbSeed.UserOutside.Route},  // Existing User, but new Person by route
            };

            UserEntity[] allUsers = Builder.IdentityDb.Value.Users.ToArray();

            // ACT
            IPersonService people = Builder.PersonService.Value;
            await people.InviteAsync(OrgRoute, invites);

            // ASSERT
            //TODO: Figure out a better way to handle the mock notification counts
            Assert.Equal(4, (Builder.NotificationService.Value as MockNotifications).Count);
            snapshot.AssertAdd<UserEntity>(1);
            snapshot.AssertAdd<PersonEntity>(2);
        }


        [Fact]
        public async Task InviteAsync_UnknownUserRoute()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Invite[] invites = new Invite[]
            {
                new Invite { Token = ResourceExtensions.NewRoute()}
            };

            // ACT & ASSERT
            IPersonService people = Builder.PersonService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await people.InviteAsync(OrgRoute, invites));
        }

        [Fact]
        public async Task InviteAsync_ExistingUser()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            int initialUserCount = db.Users.Count();
            int initialPersonCount = db.People.Count();
            UserEntity existingUser = db.Users.Where(u => u.Email != null).FirstOrDefault();
            Assert.NotNull(existingUser);
            Invite[] invites = new Invite[]
            {
                new Invite { Token = existingUser.Email }
            };

            // ACT
            IPersonService people = Builder.PersonService.Value;
            await people.InviteAsync(OrgRoute, invites);

            // ASSERT
            //TODO: Figure out a better way to handle the mock notification counts
            Assert.Equal(1, (Builder.NotificationService.Value as MockNotifications).Count);
            Assert.Equal(0, db.Users.Count() - initialUserCount);
            Assert.Equal(0, db.People.Count() - initialPersonCount);
        }

        [Fact]
        public async Task InviteAsync_ArchivedPerson()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == OrgRoute).Single();
            // Make the admin user
            UserEntity admin = db.MockUser("admin@test.com");
            PersonEntity adminPerson = db.MockPerson(org, admin);
            adminPerson.PersonRole = PersonRole.Admin;
            // Make the invitee user and archive
            string email = "invitee@test.com";
            UserEntity user = db.MockUser(email);
            db.MockPerson(org, user).SoftDeleteNow();
            db.SaveChanges();
            int initialUserCount = db.Users.Count();
            int initialPersonCount = db.People.Count();
            Invite[] invites = new Invite[]
            {
                new Invite { Token = email }
            };

            // ACT
            IPersonService people = Builder.PersonService.Value;
            await people.InviteAsync(org.Route, invites);

            // ASSERT
            //TODO: Figure out a better way to handle the mock notification counts
            Assert.Equal(1, (Builder.NotificationService.Value as MockNotifications).Count);
            Assert.Equal(0, db.Users.Count() - initialUserCount);
            Assert.Equal(0, db.People.Count() - initialPersonCount);
        }

        [Fact]
        public async Task InviteAsync_ArchivedOrg()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg().SoftDeleteNow();
            // Make the admin user
            UserEntity admin = db.MockUser("admin@test.com");
            Builder.SecurityContext.Use(new MockSecurityContext(MockMapper.Instance.Map<User>(admin), admin.Id));
            PersonEntity adminPerson = db.MockPerson(org, admin);
            adminPerson.PersonRole = PersonRole.Admin;
            db.SaveChanges();

            Invite[] invites = new Invite[]
            {
                new Invite { Token = "invitee@test.com" }
            };

            // ACT
            IPersonService people = Builder.PersonService.Value;
            await Assert.ThrowsAsync<SecurityException>(async () => await people.InviteAsync(org.Route, invites));
        }

        #endregion

        #region ReadMeAsync

        [Fact]
        public async Task ReadMeAsync()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            int initialUserCount = db.Users.Count();

            // ACT
            IPersonService people = Builder.PersonService.Value;
            Person person = await people.ReadMeAsync(OrgRoute);

            // ASSERT
            AssertResource(person);
            Assert.Equal(0, db.Users.Count() - initialUserCount);
        }

        [Fact]
        public async Task ReadMeAsync_UnknownUser()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.RandomUserRandomDir);

            // ACT
            IPersonService people = Builder.PersonService.Value;
            await Assert.ThrowsAsync<SecurityException>(async () => await people.ReadMeAsync(OrgRoute));
        }

        #endregion

        #region ReadAsync

        [Theory]
        [InlineData(TestUserContext.OrgAdmin, false)]
        [InlineData(TestUserContext.Anonymous, true)]
        public async Task ReadAsync(TestUserContext userContext, bool throwsSecEx)
        {
            // ARRANGE
            bool threwSecEx = false;
            await Builder.SetCurrentUserContext(userContext);
            MockIdentityDb SeedIdentityDb = Builder.DbContext.Value;
            PersonEntity devPerson = SeedIdentityDb.Find<PersonEntity>(1);
            Person person = null;

            // ACT
            IPersonService people = Builder.PersonService.Value;

            try
            {
                person = await people.ReadAsync(OrgRoute, devPerson.Route);
            }
            catch (SecurityException)
            {
                threwSecEx = true;
            }

            // ASSERT
            Assert.Equal(throwsSecEx, threwSecEx);
            if (!threwSecEx)
            {
                AssertResource(person);
                Assert.NotEqual(PersonRole.Unknown, person.PersonRole);
            }

        }

        [Fact]
        public async Task ReadAsync_UnknownPerson()
        {
            // ARRANGE
            string newShortId = ResourceExtensions.NewRoute();

            // ACT
            IPersonService people = Builder.PersonService.Value;
            await Assert.ThrowsAsync<NotFoundException>(async () => await people.ReadAsync(OrgRoute, newShortId));
        }

        [Fact]
        public async Task ReadAsync_UnknownClaims()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.RandomUserRandomDir);
            MockIdentityDb SeedIdentityDb = Builder.DbContext.Value;
            PersonEntity devPerson = SeedIdentityDb.Find<PersonEntity>(1);

            // ACT
            IPersonService people = Builder.PersonService.Value;
            await Assert.ThrowsAsync<SecurityException>(async () => await people.ReadAsync(OrgRoute, devPerson.Route));
        }

        [Fact]
        public async Task ReadAsync_ArchivedOrg()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg().SoftDeleteNow();

            // Make the admin user
            UserEntity user = db.MockUser("admin@test.com");
            Builder.SecurityContext.Use((i) => new MockSecurityContext(MockMapper.Instance.Map<User>(user), user.Id));

            PersonEntity person = db.MockPerson(org, user);
            person.PersonRole = PersonRole.Admin;
            db.SaveChanges();
            IGraph graph = new MockGraph();
            ILogger<PersonService> logger = new NullLogger<PersonService>();

            // ACT
            IPersonService people = Builder.PersonService.Value;

            await Assert.ThrowsAsync<SecurityException>(async () =>
                await people.ReadAsync(org.Route, person.Route)
            );
        }

        [Fact]
        public async Task ReadAsync_ArchivedPerson()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.MockOrg();
            // Make the admin user
            UserEntity user = db.MockUser("admin@test.com");
            db.SaveChanges();
            await Builder.SetCurrentUserContext(user);
            PersonEntity person = db.MockPerson(org, user).SoftDeleteNow();
            person.PersonRole = PersonRole.Admin;
            db.SaveChanges();


            // ACT
            IPersonService people = Builder.PersonService.Value;
            await Assert.ThrowsAsync<SecurityException>(async () => await people.ReadAsync(org.Route, person.Route));
        }

        #endregion

        #region ReadAllAsync

        [Fact]
        [Obsolete]
        public async Task ReadAllAsync_Obsolete()
        {
            // ARRANGE

            // ACT
            IPersonService people = Builder.PersonService.Value;
            IEnumerable<Person> orgPeople = await people.ReadAllAsync_Obsolete(OrgRoute);

            // ASSERT
            Assert.Equal(5, orgPeople.Count());
        }

        /// <summary>
        /// Very thoroughly tests paging in a way not all paging services necessarily need it, but this is the first batch of unit tests with paging enabled
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ReadAllAsync_Default()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).Single();

            // Count the number of people to make sure
            int startingPeopleCount = db.People.Count(p => p.DeletedUtc == null && p.Organization.Route == org.Route);
            int newPeople = 2 * PageRequestBase.DefaultMaxTake;

            // Make 3 pages total
            for (int i = 0; i < newPeople; i++)
            {
                db.MockPerson(org);
            }
            db.SaveChanges();

            // Ensure the parameters of the test haven't changed since first creation
            PersonEntity[] existingPeople = db.People.Where(p => p.DeletedUtc == null && p.Organization.Route == org.Route).ToArray();
            Assert.Equal(newPeople + startingPeopleCount, existingPeople.Length);

            // ACT
            IPersonService people = Builder.PersonService.Value;
            IndexPageResponse<Person> response;
            List<Person> allResults = new List<Person>();
            IndexPageRequest request = null;
            int numberOfRequests = 0;
            do
            {
                // Accumulate pages
                response = await people.ReadAllAsync(OrgRoute, request);
                allResults.AddRange(response.Result);
                request = response.Request;
                ++numberOfRequests;

                // ASSERT
                Assert.Null(response.TotalCount);
                Assert.Null(response.TotalPageCount);
                Assert.Equal(PersonService.MaxResultsReadAllAsync, response.MaxTotalResults);
                Assert.Equal(PageRequestBase.DefaultMaxTake, response.MaxTake);

            } while (response != null && response.HasMorePages() && request.NextPage());

            HashSet<string> existingPeopleRoutes = existingPeople.Select(p => p.Route).ToHashSet();
            HashSet<string> resultPeopleRoutes = allResults.Select(p => p.Route).ToHashSet();
            IEnumerable<string> intersection = existingPeopleRoutes.Intersect(resultPeopleRoutes);

            // ASSERT

            // Last result set
            int expectedLastPageItemCount = TestUtils.GetExpectedLastPageItemCount(existingPeople.Length, request.Take.Value);
            int expectedPageCount = TestUtils.GetExpectedPageCount(existingPeople.Length, request.Take.Value);
            Assert.Equal(expectedLastPageItemCount, response.Result.Count());

            // Comparing aggregate result
            Assert.Equal(expectedPageCount, numberOfRequests);
            Assert.Equal(existingPeopleRoutes.Count(), intersection.Count());
            Assert.Equal(existingPeople.Count(), allResults.Count());
        }

        /// <summary>
        /// Very thoroughly tests paging in a way not all paging services necessarily need it, but this is the first batch of unit tests with paging enabled
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ReadAllAsync_DefaultExactPageSize()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).Single();
            PersonEntity[] existingPeople = db.People.Where(p => p.DeletedUtc == null && p.Organization.Route == org.Route).ToArray();
            int existingCount = existingPeople.Length;
            int numberToCreate = (2 * PageRequestBase.DefaultMaxTake) - existingCount;
            // Make 3 pages total
            for (int i = 0; i < numberToCreate; i++)
            {
                db.MockPerson(org);
            }
            db.SaveChanges();

            // Ensure the parameters of the test haven't changed since first creation
            existingPeople = db.People.Where(p => p.DeletedUtc == null && p.Organization.Route == org.Route).ToArray();
            Assert.Equal(256, existingPeople.Length);

            // ACT
            IPersonService people = Builder.PersonService.Value;
            IndexPageResponse<Person> response;
            List<Person> allResults = new List<Person>();
            IndexPageRequest request = null;
            int numberOfRequests = 0;
            do
            {
                // Accumulate pages
                response = await people.ReadAllAsync(OrgRoute, request);
                allResults.AddRange(response.Result);
                request = response.Request;
                ++numberOfRequests;

                // ASSERT
                Assert.Null(response.TotalCount);
                Assert.Null(response.TotalPageCount);
                Assert.Equal(PersonService.MaxResultsReadAllAsync, response.MaxTotalResults);
                Assert.Equal(PageRequestBase.DefaultMaxTake, response.MaxTake);

            } while (response != null && response.HasMorePages() && request.NextPage());

            HashSet<string> existingPeopleRoutes = existingPeople.Select(p => p.Route).ToHashSet();
            HashSet<string> resultPeopleRoutes = allResults.Select(p => p.Route).ToHashSet();
            IEnumerable<string> intersection = existingPeopleRoutes.Intersect(resultPeopleRoutes);

            // ASSERT

            // Last result set
            Assert.Empty(response.Result);

            // Comparing aggregate result
            Assert.Equal(3, numberOfRequests); // One extra
            Assert.Equal(existingPeopleRoutes.Count(), intersection.Count());
            Assert.Equal(existingPeople.Count(), allResults.Count());
        }

        /// <summary>
        /// Very thoroughly tests paging in a way not all paging services necessarily need it, but this is the first batch of unit tests with paging enabled
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ReadAllAsync_DefaultExactPageSizeWithCount()
        {
            // This is exactly the same as ReadAllAsync_DefaultExactPageSize except it requests the count
            // This saves the caller one page, but it is less efficient overall

            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).Single();
            PersonEntity[] existingPeople = db.People.Where(p => p.DeletedUtc == null && p.Organization.Route == org.Route).ToArray();
            int existingCount = existingPeople.Length;
            int numberToCreate = (2 * PageRequestBase.DefaultMaxTake) - existingCount;
            // Make 3 pages total
            for (int i = 0; i < numberToCreate; i++)
            {
                db.MockPerson(org);
            }
            db.SaveChanges();

            // Ensure the parameters of the test haven't changed since first creation
            existingPeople = db.People.Where(p => p.DeletedUtc == null && p.Organization.Route == org.Route).ToArray();
            int existingPeopleCount = existingPeople.Length;
            Assert.Equal(256, existingPeopleCount);

            // ACT
            IPersonService people = Builder.PersonService.Value;
            IndexPageResponse<Person> response;
            List<Person> allResults = new List<Person>();
            IndexPageRequest request = new IndexPageRequest { IncludesCounts = true };
            int numberOfRequests = 0;
            do
            {
                // Accumulate pages
                response = await people.ReadAllAsync(OrgRoute, request);
                allResults.AddRange(response.Result);
                request = response.Request;
                ++numberOfRequests;

                // ASSERT
                Assert.Equal(existingPeopleCount, response.TotalCount);
                Assert.Equal(2, response.TotalPageCount);
                Assert.Equal(PersonService.MaxResultsReadAllAsync, response.MaxTotalResults);
                Assert.Equal(PageRequestBase.DefaultMaxTake, response.MaxTake);

            } while (response != null && response.HasMorePages() && request.NextPage());

            HashSet<string> existingPeopleRoutes = existingPeople.Select(p => p.Route).ToHashSet();
            HashSet<string> resultPeopleRoutes = allResults.Select(p => p.Route).ToHashSet();
            IEnumerable<string> intersection = existingPeopleRoutes.Intersect(resultPeopleRoutes);

            // ASSERT

            // Last result set
            Assert.Equal(IndexPageRequest.DefaultMaxTake, response.Result.Count());

            // Comparing aggregate result
            Assert.Equal(2, numberOfRequests); // Pulling count means we can end early
            Assert.Equal(existingPeopleRoutes.Count(), intersection.Count());
            Assert.Equal(existingPeople.Count(), allResults.Count());
        }

        /// <summary>
        /// Very thoroughly tests paging in a way not all paging services necessarily need it, but this is the first batch of unit tests with paging enabled
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ReadAllAsync_Filter()
        {
            // ARRANGE

            // ACT
            IPersonService people = Builder.PersonService.Value;
            IndexPageRequest request = new IndexPageRequest { Filter = "ada", IncludesCounts = true };
            IndexPageResponse<Person> response = await people.ReadAllAsync(OrgRoute, request);

            // ASSERT
            Assert.Single(response.Result);
            Assert.Single(response.Result.Where(p => p.User.Email.Contains("ada")));
            Assert.Equal(PersonService.MaxResultsReadAllAsync, response.MaxTotalResults);
            Assert.Equal(PageRequestBase.DefaultMaxTake, response.MaxTake);
            Assert.Equal(1, response.TotalCount);
            Assert.Equal(1, response.TotalPageCount);
        }

        [Fact]
        public async Task ReadAllAsync_PersonRole()
        {
            // ARRANGE

            // ACT
            IPersonService people = Builder.PersonService.Value;
            IndexPageResponse<Person> response = await people.ReadAllAsync(OrgRoute, null, PersonRole.Admin);

            // ASSERT
            Assert.True(response.Result.All(p => p.PersonRole == PersonRole.Admin));
            Assert.Single(response.Result);
            Assert.Equal(PersonService.MaxResultsReadAllAsync, response.MaxTotalResults);
            Assert.Equal(PageRequestBase.DefaultMaxTake, response.MaxTake);
            Assert.Null(response.TotalCount);
            Assert.Null(response.TotalPageCount);
        }

        [Fact]
        public async Task ReadAllAsync_LargerThanNormalMax()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            OrganizationEntity org = db.Organizations.Where(o => o.Route == IdentityDbSeed.OrgPrimary.Route).Single();
            PersonEntity[] existingPeople = db.People.Where(p => p.DeletedUtc == null && p.Organization.Route == org.Route).ToArray();
            int preCount = existingPeople.Length;

            // Make far more than the normal default maximum
            int numberToCreate = 2 * IndexPageRequest.DefaultMaxTotalResults;
            for (int i = 0; i < numberToCreate; i++)
            {
                db.MockPerson(org);
            }
            db.SaveChanges();

            // Ensure the parameters of the test haven't changed since first creation
            existingPeople = db.People.Where(p => p.DeletedUtc == null && p.Organization.Route == org.Route).ToArray();
            int newTotalPeople = existingPeople.Length;
            Assert.Equal(numberToCreate + preCount, newTotalPeople);

            // ACT
            IPersonService people = Builder.PersonService.Value;
            IndexPageResponse<Person> response;
            List<Person> allResults = new List<Person>();
            IndexPageRequest request = new IndexPageRequest { IncludesCounts = true, Take = 32 };
            int numberOfRequests = 0;
            do
            {
                // Accumulate pages
                response = await people.ReadAllAsync(OrgRoute, request);
                allResults.AddRange(response.Result);
                request = response.Request;
                ++numberOfRequests;

                // ASSERT
                Assert.Equal(newTotalPeople, response.TotalCount);
                Assert.Equal(65, response.TotalPageCount);
                Assert.Equal(int.MaxValue, response.MaxTotalResults);
                Assert.Equal(PageRequestBase.DefaultMaxTake, response.MaxTake);
                request = response.NextPage();

            } while (response != null && response.HasMorePages());

            HashSet<string> existingPeopleRoutes = existingPeople.Select(p => p.Route).ToHashSet();
            HashSet<string> resultPeopleRoutes = allResults.Select(p => p.Route).ToHashSet();
            IEnumerable<string> intersection = existingPeopleRoutes.Intersect(resultPeopleRoutes);

            // ASSERT

            // Last result set
            int expectedLastPageItemCount = TestUtils.GetExpectedLastPageItemCount(newTotalPeople, request.Take.Value);
            int expectedPageCount = TestUtils.GetExpectedPageCount(newTotalPeople, request.Take.Value);
            Assert.Equal(expectedLastPageItemCount, response.Result.Count());

            // Comparing aggregate result
            Assert.Equal(expectedPageCount, numberOfRequests);
            Assert.Equal(existingPeopleRoutes.Count(), intersection.Count());
            Assert.Equal(existingPeople.Count(), allResults.Count());
        }

        [Fact]
        public async Task ReadAllAsync_BadRequests()
        {
            // ARRANGE
            // ACT & ASSERT
            IPersonService people = Builder.PersonService.Value;

            await Assert.ThrowsAsync<BadRequestException>(async () =>
                    // Take is too small
                    await people.ReadAllAsync(OrgRoute, new IndexPageRequest { Take = 0 })
                );
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                    // Skip is too small
                    await people.ReadAllAsync(OrgRoute, new IndexPageRequest { Skip = -1 })
                );
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync()
        {
            // ARRANGE
            MockIdentityDb SeedIdentityDb = Builder.DbContext.Value;
            PersonEntity person = SeedIdentityDb.People.Where(p => p.Organization.Route == OrgRoute).ToArray()[2];
            Assert.Equal(PersonRole.Person, person.PersonRole);
            Person personFields = new Person
            {
                PersonRole = PersonRole.Admin,
            };

            // ACT
            IPersonService people = Builder.PersonService.Value;
            Person updatedPerson = await people.UpdateAsync(OrgRoute, person.Route, personFields);

            // ASSERT
            AssertResource(updatedPerson);
            Assert.Equal(PersonRole.Admin, updatedPerson.PersonRole);
        }

        #endregion

        #region DeleteAsync

        [Theory]
        [InlineData(TestUserContext.OrgAdmin, false)]
        [InlineData(TestUserContext.OrgMember, true)]
        [InlineData(TestUserContext.RandomUserRandomDir, true)]
        [InlineData(TestUserContext.Anonymous, true)]
        public async Task DeleteAsync(TestUserContext userContext, bool throwsSecEx)
        {
            // ARRANGE
            bool threwSecEx = false;
            await Builder.SetCurrentUserContext(userContext);
            MockIdentityDb db = Builder.IdentityDb.Value as MockIdentityDb;
            PersonEntity person = db.Find<PersonEntity>(2);

            // ACT
            try
            {
                IPersonService people = Builder.PersonService.Value;
                await people.DeleteAsync(OrgRoute, person.Route);
            }
            catch (SecurityException)
            {
                threwSecEx = true;
            }

            // ASSERT
            Assert.Equal(throwsSecEx, threwSecEx);
            if (!throwsSecEx)
            {
                person = db.Find<PersonEntity>(2);
                Assert.NotNull(person);
                Assert.NotNull(person.DeletedUtc);
            }
        }

        #endregion
    }
}
