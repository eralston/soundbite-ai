using Masticore.Resources;
using Masticore.Security;
using System.Collections.Generic;
using System.Linq;

namespace Masticore.Entity.Tests
{
    public static class MockData
    {
        public const string DefaultEmail = "test@test.com";
        public const string DefaultOrgRoute = "orgRoute";
        public const string DefaultGroupRoute = "groupRoute";

        public static TEntity SoftDeleteNow<TEntity>(this TEntity delete) where TEntity : IResource
        {
            delete.SoftDelete();
            return delete;
        }

        public static OrganizationEntity MockOrg(this IIdentityDb db, string orgRoute = DefaultOrgRoute)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(orgRoute))
            {
                throw new System.ArgumentException($"'{nameof(orgRoute)}' cannot be null or empty.", nameof(orgRoute));
            }

            OrganizationEntity org = ResourceEntityBase.CreateMock<OrganizationEntity>();
            org.NewRoute();
            org.Tenant = db.Tenants.First();
            org.Route = orgRoute;
            org.CreatedById = 1;
            org.NewUniversalId();
            db.Organizations.Add(org);
            return org;
        }

        public static UserEntity MockUser(this IIdentityDb db, string email = null, string universalId = null)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            UserEntity user = ResourceEntityBase.CreateMock<UserEntity>();
            user.UniversalId = universalId ?? ResourceExtensions.NewUniversalId();
            user.Email = email ?? $"email_{user.UniversalId}@fake.com";
            user.CreatedById = 1;
            user.UserRole = UserRole.User;
            db.Users.Add(user);
            return user;
        }

        public static UserEntity MockUser(this IIdentityDb db, IClaims claims)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            if (claims is null)
            {
                throw new System.ArgumentNullException(nameof(claims));
            }

            UserEntity user = db.Users.Where(u => u.UniversalId == claims.UniversalId).FirstOrDefault();
            if (user == null)
            {
                user = ResourceEntityBase.CreateMock<UserEntity>();
                user.Email = claims.Email;
                user.UniversalId = claims.UniversalId;
                user.CreatedById = 1;
                user.UserRole = UserRole.User;
                db.Users.Add(user);
            }
            return user;
        }

        public static PersonEntity MockPerson(this IIdentityDb db, OrganizationEntity org, UserEntity user = null)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new System.ArgumentNullException(nameof(org));
            }

            PersonEntity person = ResourceEntityBase.CreateMock<PersonEntity>();
            person.NewRoute();
            person.Organization = org;
            person.User = user ?? MockUser(db);
            person.CreatedById = 1;
            person.PersonRole = PersonRole.Person;
            db.People.Add(person);
            return person;
        }

        public static PersonEntity MockPerson(this IIdentityDb db, OrganizationEntity org, IClaims claims)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new System.ArgumentNullException(nameof(org));
            }

            if (claims is null)
            {
                throw new System.ArgumentNullException(nameof(claims));
            }

            UserEntity user = db.MockUser(claims);
            user.GivenName = claims.FullName;
            return MockPerson(db, org, user);
        }

        public static GroupEntity MockGroup(this IIdentityDb db, OrganizationEntity org)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            if (org is null)
            {
                throw new System.ArgumentNullException(nameof(org));
            }

            GroupEntity grp = ResourceEntityBase.CreateMock<GroupEntity>();
            grp.NewRoute();
            grp.NewUniversalId();
            grp.Organization = org;
            grp.CreatedById = 1;
            db.Groups.Add(grp);
            return grp;
        }

        public static MemberEntity MockMember(this GroupEntity grp, PersonEntity person)
        {
            if (grp is null)
            {
                throw new System.ArgumentNullException(nameof(grp));
            }

            if (person is null)
            {
                throw new System.ArgumentNullException(nameof(person));
            }

            if (grp.Members == null)
            {
                grp.Members = new List<MemberEntity>();
            }

            MemberEntity member = ResourceEntityBase.CreateMock<MemberEntity>();
            member.Group = grp;
            member.Person = person;
            member.CreatedById = 1;
            grp.Members.Add(member);
            return member;
        }
    }
}
