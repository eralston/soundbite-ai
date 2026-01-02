using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Services;
using Masticore.Services.Tests;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests
{
    public abstract class ServiceTestBase : EntityTestBase<MockSbDbInfrastructure, SbDb>
    {
        #region Constructor

        public ServiceTestBase()
        {
            Builder.MemberService.Use((i) => new MemberService(
                i.SecurityContext.Value,
                i.NotificationService.Value,
                i.ImageService.Value,
                i.Infrastructure.Value,
                i.Mapper.Value, i.Logger<MemberService>()));

            Builder.OrganizationService.Use(i => new OrganizationService(
                i.SecurityContext.Value,
                i.Infrastructure.Value,
                i.ImageService.Value,
                i.Mapper.Value,
                i.Logger<OrganizationService>(),
                i.Rbac.Value));

            Builder.ImageService.Use((i) => new MockImageService());
        }

        #endregion

        #region Properties

        protected new SbTestBuilder<MockSbDbInfrastructure, SbDb> Builder { get; }
            = new SbTestBuilder<MockSbDbInfrastructure, SbDb>();

        #endregion

        protected async Task<SessionEntity> SessionEntityAsync(string sessionRoute = null)
        {
            SbDb db = Builder.DbContext.Value;
            SessionEntity orgEnty = await db.SessionAsync(sessionRoute ?? SbDbSeed.SessionRoute, true);
            return orgEnty;
        }

        protected async Task<GroupEntity> GroupEntityAsync(string orgRoute = null)
        {
            SbDb db = Builder.DbContext.Value;
            GroupEntity grpEnt = await db.GroupWhereRoute(orgRoute ?? IdentityDbSeed.GroupOrgAdminOwned.Route);
            return grpEnt;
        }

        protected async Task<SbDb> SaveDbAsync()
        {
            SbDb db = Builder.DbContext.Value;
            await db.SaveChangesAsync();
            return db;
        }

        protected async Task<Group> GroupAsync(string grpRoute = null)
        {
            GroupEntity grpEnt = await GroupEntityAsync(grpRoute);
            Group grp = Builder.Mapper.Value.Map<Group>(grpEnt);
            return grp;
        }

        protected async Task<OrganizationEntity> OrgEntityAsync(string orgRoute = null)
        {
            SbDb db = Builder.DbContext.Value;
            OrganizationEntity orgEnty = await db.OrgWhereRoute(orgRoute ?? IdentityDbSeed.OrgPrimary.Route);
            return orgEnty;
        }

        protected async Task<Organization> OrgAsync(string orgRoute = null)
        {
            OrganizationEntity orgEnty = await OrgEntityAsync(orgRoute);
            Organization org = Builder.Mapper.Value.Map<Organization>(orgEnty);
            return org;
        }

        protected async Task<UserEntity> UserEntityAsync(string userRoute = null)
        {
            SbDb db = Builder.DbContext.Value;
            UserEntity userEnt = await db.UserWhereRoute(userRoute ?? IdentityDbSeed.UserOrgAdmin.Route);
            return userEnt;
        }

        protected async Task<User> UserWhereRoute(string userRoute = null)
        {
            UserEntity userEnt = await UserEntityAsync(userRoute);
            User user = Builder.Mapper.Value.Map<User>(userEnt);
            return user;
        }
    }
}
