using Masticore.Resources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// Default seed for the IIdentityDb
    /// </summary>
    public class IdentityDbSeed
    {
        #region Fields

        /// <summary>
        /// Dictionary used for generating unique IDs for entities.
        /// </summary>
        private readonly IDictionary<Type, int> _idTracker = new Dictionary<Type, int>();

        #endregion

        #region Properties

        /// <summary>
        /// Stores a reference to the model builder
        /// </summary>
        protected ModelBuilder Builder { get; set; }

        #endregion

        #region References

        /// <summary>
        /// Gets or sets the number of User entries created during the seeding process. Use this 
        /// value for tests that check the number of users instead of using "magic" numbers.
        /// </summary>
        public static int SeededUserCount { get; set; }

        public static TenantEntity Tenant { get; set; }
        public static TenantEntity TenantOutside { get; set; }
        public static UserEntity UserOrgAdmin { get; set; }
        public static UserEntity UserGroupMember { get; set; }
        public static UserEntity UserOrgMemberNoGroupMembership { get; set; }
        public static UserEntity UserGod { get; set; }
        public static UserEntity UserRegular { get; set; }
        public static UserEntity UserOutside { get; set; }
        public static UserEntity UserNonOrgAdminGroupOwner { get; set; }
        public static PersonEntity PersonOrgAdmin { get; set; }
        public static PersonEntity PersonOutside { get; set; }
        public static PersonEntity PersonNoGroupMembership { get; set; }
        public static OrganizationEntity OrgPrimary { get; set; }
        public static OrganizationEntity OrgOutside { get; set; }
        public static GroupEntity GroupOrgAdminOwned { get; set; }
        public static GroupEntity GroupOrgAdminOwned2 { get; set; }

        /// <summary>
        /// Group that is owned by a normal org member and not an admin.
        /// </summary>
        public static GroupEntity GroupMemberOwned { get; set; }
        public static GroupEntity GroupOutside { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the next unique ID for the specified entity.
        /// </summary>
        /// <typeparam name="T">.NET type of the entity for which an ID is needed.</typeparam>
        /// <returns>a unique ID for the given entity type.</returns>
        protected int NextId<T>()
        {
            if (!_idTracker.ContainsKey(typeof(T)))
            {
                _idTracker[typeof(T)] = 0;
            }
            int result = _idTracker[typeof(T)];
            result++;
            _idTracker[typeof(T)] = result;
            return result;
        }

        #endregion

        #region Seed Methods

        /// <summary>
        /// Will seed objects into the in-memory representation of the TestDb
        /// </summary>
        /// <param name="builder"></param>
        public virtual void Seed(ModelBuilder builder)
        {
            Builder = builder;

            // Add current user
            UserOrgAdmin = SeedUser("[First]", "[Last]", "local@localhost".ToEmail(), UserRole.User);
            UserGod = SeedUser("[Zeus]", "[Of Olympus]", "ThunderboltAndLightning@Olympus.Net".ToEmail(), UserRole.God);

            // Seed Tenant
            Tenant = SeedTenant("Software");

            SeedMasticoreOrg();
            SeedOutsideOrg();
        }

        protected void SeedMasticoreOrg()
        {
            // Tenant & org
            OrgPrimary = SeedOrganization(Tenant, "Masticore", "Better than Contoso");

            // First User's Person Record
            PersonOrgAdmin = SeedPerson(OrgPrimary, UserOrgAdmin, PersonRole.Admin);

            // Other people
            PersonEntity person1, person2, person3;
            (UserRegular, person1) = SeedUserAndPerson(OrgPrimary, "Ada", "Lovelace", "Ada@Masticore".ToEmail());
            (UserNonOrgAdminGroupOwner, person2) = SeedUserAndPerson(OrgPrimary, "Alan", "Turing", "Alan@Masticore".ToEmail());
            (UserGroupMember, person3) = SeedUserAndPerson(OrgPrimary, "Margaret", "Hamilton", "Margaret@Masticore".ToEmail());
            (UserOrgMemberNoGroupMembership, PersonNoGroupMembership) = SeedUserAndPerson(OrgPrimary, "Dave", "NotInAGroup", "Dave@Masticore".ToEmail());

            // Groups
            GroupOrgAdminOwned = SeedGroup(OrgPrimary, "Red Team", PersonOrgAdmin, person1, person2);
            GroupOrgAdminOwned2 = SeedGroup(OrgPrimary, "Blue Team", PersonOrgAdmin, person2, person3);
            GroupMemberOwned = SeedGroup(OrgPrimary, "Purple Team", person2, person3);
        }

        protected void SeedOutsideOrg()
        {
            TenantOutside = SeedTenant("Invisible");
            OrgOutside = SeedOrganization(TenantOutside, "Invisible", "You should never see this org");
            (UserOutside, PersonOutside) = SeedUserAndPerson(OrgOutside, "Should", "Never See Me", "Invisible@Nowhere");
            GroupOutside = SeedGroup(OrgOutside, "Invisible Team", PersonOutside);
            SeedGroup(OrgOutside, "Team Invisible", PersonOutside);
        }

        protected OrganizationEntity SeedOrganization(
            TenantEntity tenant,
            string name,
            string desc)
        {
            OrganizationEntity org = CreateOrgEntity(tenant, name, desc);
            return AddOrganization(org);
        }

        protected OrganizationEntity CreateOrgEntity(TenantEntity tenant, string name, string desc)
        {
            OrganizationEntity org = ResourceEntityBase.CreateMock<OrganizationEntity>(NextId<OrganizationEntity>());
            org.NewRoute();
            org.Name = name;
            org.Description = desc;
            org.TenantId = tenant.Id;
            org.CreatedById = UserOrgAdmin.Id;
            org.NewUniversalId();
            return org;
        }

        protected OrganizationEntity AddOrganization(OrganizationEntity org)
        {
            Builder.Entity<OrganizationEntity>().HasData(org);
            return org;
        }

        protected TenantEntity SeedTenant(string name)
        {
            TenantEntity tenant = ResourceEntityBase.CreateMock<TenantEntity>(NextId<TenantEntity>());
            tenant.Name = name;
            tenant.NewRoute();
            tenant.CreatedById = UserOrgAdmin.Id;
            Builder.Entity<TenantEntity>().HasData(tenant);
            return tenant;
        }

        protected GroupEntity SeedGroup(OrganizationEntity org, string name, PersonEntity owner, params PersonEntity[] people)
        {
            GroupEntity grp = ResourceEntityBase.CreateMock<GroupEntity>(NextId<GroupEntity>());
            grp.Name = name;
            grp.Description = "Seeded Team";
            grp.OrganizationId = org.Id;
            grp.NewRoute();
            grp.CreatedById = UserOrgAdmin.Id;
            grp.NewUniversalId();
            AddGroup(Builder, grp);

            SeedMember(Builder, grp, owner, MemberRole.Owner);
            foreach (PersonEntity person in people)
            {
                SeedMember(Builder, grp, person, MemberRole.Member);
            }

            return grp;
        }

        private static GroupEntity AddGroup(ModelBuilder builder, GroupEntity grp)
        {
            builder.Entity<GroupEntity>().HasData(grp);
            return grp;
        }

        protected MemberEntity SeedMember(ModelBuilder builder, GroupEntity grp, PersonEntity person, MemberRole role)
        {
            MemberEntity member = ResourceEntityBase.CreateMock<MemberEntity>(NextId<MemberEntity>());
            member.MemberRole = role;
            member.PersonId = person.Id;
            member.GroupId = grp.Id;
            member.CreatedById = UserOrgAdmin.Id;
            return AddMember(builder, member);
        }

        protected MemberEntity AddMember(ModelBuilder builder, MemberEntity member)
        {
            builder.Entity<MemberEntity>().HasData(member);
            return member;
        }

        protected (UserEntity, PersonEntity) SeedUserAndPerson(OrganizationEntity org, string givenName, string familyName, string email, string userUniversalId = null)
        {
            UserEntity user = SeedUser(givenName, familyName, email, UserRole.User, userUniversalId);
            PersonEntity person = SeedPerson(org, user);
            return (user, person);
        }

        protected PersonEntity SeedPerson(OrganizationEntity org, UserEntity user, PersonRole role = PersonRole.Person)
        {
            PersonEntity person = ResourceEntityBase.CreateMock<PersonEntity>(NextId<PersonEntity>());
            person.UserId = user.Id;
            person.OrganizationId = org.Id;
            person.PersonRole = role;
            person.CreatedById = UserOrgAdmin.Id;
            return AddPerson(person);
        }

        protected PersonEntity AddPerson(PersonEntity person)
        {
            Builder.Entity<PersonEntity>().HasData(person);
            return person;
        }

        protected UserEntity SeedUser(string givenName, string familyName, string email, UserRole userRole = UserRole.User, string userUniversalId = null)
        {
            // A User in a directory
            UserEntity user = ResourceEntityBase.CreateMock<UserEntity>(NextId<UserEntity>());
            user.UserRole = userRole;
            user.Email = email;
            user.GivenName = givenName;
            user.FamilyName = familyName;
            user.Title = "Test User";
            user.AllowNews = true;
            user.AllowMarketing = true;
            user.AllowEmail = true;
            user.AllowSms = true;
            user.InviteUtc = Time.UtcNow;
            user.InviteAcceptUtc = Time.UtcNow;
            user.Phone = RandomPhoneNumber();
            user.UniversalId = userUniversalId ?? ResourceExtensions.NewUniversalId();
            user.CreatedById = UserOrgAdmin == null ? 1 : UserOrgAdmin.Id;
            user.ProviderType = ProviderType.AAD;
            AddEntity(user);
            SeededUserCount++;
            return user;
        }

        #endregion

        #region Utility Methods

        protected T AddEntity<T>(T user) where T : class
        {
            Builder.Entity<T>().HasData(user);
            return user;
        }

        protected string RandomPhoneNumber()
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            string s = "";
            for (int i = 0; i < 9; i++)
            {
                if (i == 0) { s += "("; }
                if (i == 3) { s += ")"; }
                if (i == 6) { s += "-"; }
                s += rnd.Next(0, 9).ToString();
            }
            return s;
        }

        #endregion
    }
}
