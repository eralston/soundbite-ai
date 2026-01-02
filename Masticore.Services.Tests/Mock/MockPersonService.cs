using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.Services.Tests
{
    public class MockPersonService : IPersonService
    {
        protected bool IsFreshUser { get; }
        protected MockIdentityDb Db { get; }
        protected ISecurityContext SecurityContext { get; }

        public MockPersonService(ISecurityContext securityContext, MockIdentityDb db, bool isFreshUser = false)
        {
            SecurityContext = securityContext;
            IsFreshUser = isFreshUser;
            Db = db;
        }

        private User CreateMockUser(IClaims claims)
        {
            string shortId = ResourceExtensions.NewRoute();
            if (!IsFreshUser)
            {
                Entity.UserEntity devUser = Db.Find<Entity.UserEntity>(1);
                shortId = devUser.Route;
            }

            User user = new User()
            {
                Route = shortId,
                GivenName = "Local",
                FamilyName = "Developer",
                Email = "Developer@soundbite.ai",
                Phone = "123-456-7890",
                Title = "Mr. Manager",
                ImageSrc = "/photo.jpg",
                CreatedUtc = Time.UtcNow,
                UpdatedUtc = Time.UtcNow,
            };
            return user;
        }

        public Task<User> UpsertMyUserWhereUid(IClaims claims)
        {
            return Task.FromResult(CreateMockUser(claims));
        }

        private Person CreateMockPerson(IClaims claims)
        {
            Person person = new Person()
            {
                User = new User()
                {
                    GivenName = "Local",
                    FamilyName = "Developer",
                    Email = "Developer@soundbite.ai",
                    Phone = "123-456-7890",
                    Title = "Mr. Manager",
                    ImageSrc = "/photo.jpg",
                },
                PersonRole = PersonRole.Admin,
                CreatedUtc = Time.UtcNow,
                UpdatedUtc = Time.UtcNow,
            };
            return person;
        }

        public Task<Person> ReadMeAsync(string orgRoute)
        {
            return Task.FromResult(CreateMockPerson(SecurityContext));
        }

        public Task<Person> ReadAsync(string orgRoute, string personId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string orgRoute, string personId)
        {
            throw new NotImplementedException();
        }

        public Task<Person> UpdateAsync(string orgRoute, string personId, Person person)
        {
            throw new NotImplementedException();
        }

        public Task<InviteResult[]> InviteAsync(string orgRoute, Invite[] invite)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Person>> ReadAllAsync_Obsolete(string orgRoute)
        {
            throw new NotImplementedException();
        }

        public Task<IndexPageResponse<Person>> ReadAllAsync(string orgRoute, IndexPageRequest request = null, PersonRole? personRole = null)
        {
            throw new NotImplementedException();
        }
    }
}
