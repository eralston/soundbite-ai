using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Tests;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.DirectorySync.Tests.Mocks
{
    public static class Extensions
    {
        public static void AssertUserForEmailDeleted(this ISyncDb db, string userEmail)
        {
            Assert.Null(db.Users.Where(u => u.Email == userEmail.ToEmail(true)).SingleOrDefault());
        }

        public static PersonEntity AssertPersonForEmail(this ISyncDb db, string orgRoute, IUserFields expectedUser, bool userActive = true, bool personActive = true, bool personHasPhone = true)
        {
            Assert.NotNull(orgRoute);
            Assert.NotNull(expectedUser);
            Assert.NotNull(expectedUser.Email);

            if (personHasPhone)
            {
                Assert.NotNull(expectedUser.Phone);
            }
            else
            {
                Assert.Null(expectedUser.Phone);
            }

            PersonEntity actualPerson = db.People
                .Where(p => p.Organization.Route == orgRoute && p.User.Email == expectedUser.Email.ToEmail(true))
                .Include(p => p.User)
                .SingleOrDefault();
            Assert.NotNull(actualPerson);
            Assert.NotNull(actualPerson.User);
            expectedUser.AssertUserEqual(actualPerson.User);

            // Check user active
            if (userActive)
            {
                Assert.Null(actualPerson.User.DeletedUtc);
            }
            else
            {
                Assert.NotNull(actualPerson.User.DeletedUtc);
            }

            // Check person active
            if (personActive)
            {
                Assert.Null(actualPerson.DeletedUtc);
            }
            else
            {
                Assert.NotNull(actualPerson.DeletedUtc);
            }

            return actualPerson;
        }

        public static void AssertPersonDeleted(this ISyncDb db, string orgRoute, string personRoute)
        {
            Assert.NotNull(orgRoute);
            Assert.NotNull(personRoute);

            PersonEntity actualPerson = db.People
                .Where(p => p.Organization.Route == orgRoute && p.Route == personRoute)
                .FirstOrDefault();
            Assert.NotNull(actualPerson);
            Assert.NotNull(actualPerson.DeletedUtc);
        }

        public static GroupEntity AssertGroupActive(this ISyncDb db, string orgRoute, IGroupFields grp)
        {
            Assert.NotNull(grp);
            Assert.NotNull(grp.Name);

            GroupEntity grpEnt = db.GroupForFields(orgRoute, grp);
            Assert.NotNull(grpEnt);
            Assert.Null(grpEnt.DeletedUtc);
            Assert.NotNull(grpEnt.UniversalId);
            Assert.Equal(grp.Name, grpEnt.Name);
            Assert.Equal(grp.Description, grpEnt.Description);
            return grpEnt;
        }

        public static void AssertGroupDeleted(this ISyncDb db, IGroupFields grp, string expectedOrgRoute)
        {
            Assert.NotNull(grp);
            Assert.NotNull(expectedOrgRoute);

            GroupEntity grpEnt = db.GroupForFields(expectedOrgRoute, grp);
            Assert.NotNull(grpEnt);
            Assert.NotNull(grpEnt.DeletedUtc);
            Assert.NotNull(grpEnt.UniversalId);
            Assert.Equal(grp.Name, grpEnt.Name);
            Assert.Equal(grp.Description, grpEnt.Description);
        }

        private static GroupEntity GroupForFields(this ISyncDb db, string orgRoute, IGroupFields grp)
        {
            Assert.NotNull(orgRoute);
            Assert.NotNull(grp);
            return db.Groups.Where(g => g.Name == grp.Name && g.Organization.Route == orgRoute).SingleOrDefault();
        }

        public static void AssertGroupMissing(this ISyncDb db, string grpUid)
        {
            Assert.NotNull(grpUid);

            GroupEntity grpEnt = db.Groups.Where(g => g.UniversalId == grpUid).SingleOrDefault();
            Assert.Null(grpEnt);
        }

        public static void AssertMemberActive(this ISyncDb db, string orgRoute, string groupUid, string userEmail)
        {
            MemberEntity newMember = db.Member(orgRoute, groupUid, userEmail);
            Assert.NotNull(newMember);
            Assert.Null(newMember.DeletedUtc);
            Assert.NotEqual(MemberRole.Unknown, newMember.MemberRole);
        }

        private static MemberEntity Member(this ISyncDb db, string orgRoute, string groupUid, string userEmail)
        {
            Assert.NotNull(groupUid);
            Assert.NotNull(userEmail);
            userEmail = userEmail.ToEmail();
            return db.Members.Where(m => m.Person.User.Email == userEmail && m.Group.UniversalId == groupUid && m.Group.Organization.Route == orgRoute).SingleOrDefault();
        }
        public static void AssertMemberMissing(this ISyncDb db, string orgRoute, string groupUid, string userEmail)
        {
            Assert.NotNull(groupUid);
            Assert.NotNull(userEmail);
            MemberEntity newMember = db.Member(orgRoute, groupUid, userEmail);
            Assert.Null(newMember);
        }

        public static void AssertMemberDeleted(this ISyncDb db, string orgRoute, string groupUid, string userEmail)
        {
            MemberEntity newMember = db.Member(orgRoute, groupUid, userEmail);
            Assert.NotNull(newMember);
            Assert.NotNull(newMember.DeletedUtc);
        }

        public static async Task<PersonEntity> ValidPerson(this ISyncDb db, string personRoute)
        {
            // Existing Person
            PersonEntity existingPerson = await db.PersonWhereRoute(IdentityDbSeed.OrgPrimary.Route, personRoute, false, true);
            Assert.NotNull(existingPerson);
            Assert.Null(existingPerson.DeletedUtc);
            Assert.NotNull(existingPerson.User);
            Assert.NotNull(existingPerson.User.UniversalId);
            Assert.Null(existingPerson.User.DeletedUtc);
            return existingPerson;
        }

        public static async Task<GroupEntity> ValidGroup(this ISyncDb db, string grpRoute)
        {
            Assert.NotNull(grpRoute);

            GroupEntity existingGroup = await db.GroupWhereRoute(IdentityDbSeed.OrgPrimary.Route, grpRoute);
            Assert.NotNull(existingGroup);
            Assert.Null(existingGroup.DeletedUtc);
            Assert.NotNull(existingGroup.UniversalId);
            return existingGroup;
        }
    }

}
