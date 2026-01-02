using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class OrganizationServiceTests : ServiceTestBase<MockSeedInfrastructure, MockIdentityDb>
    {
        #region Constructor

        public OrganizationServiceTests()
        {
            Builder.UseNullLogger = true;
            Builder.ImageService.Use(i => new MockImageService());
            Builder.OrganizationService.Use(i => new OrganizationService(
                i.SecurityContext.Value,
                i.Infrastructure.Value,
                i.ImageService.Value,
                i.Mapper.Value,
                i.Logger<OrganizationService>(),
                i.Rbac.Value)
            );
        }

        #endregion

        #region ReadDetailsAsync

        [Fact]
        public async Task ReadDetailsAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            IOrganizationService organizationService = new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);

            // ACT
            OrganizationDetails org = await organizationService.ReadAsync(OrgRoute);

            // ASSERT
            Assert.NotNull(org);
            Assert.NotNull(org.Me);
            Assert.NotNull(org.Me.User);
            Assert.NotEqual(PersonRole.Unknown, org.Me.PersonRole);
        }

        [Fact]
        public async Task ReadDetailsAsync_UnknownOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            IOrganizationService organizationService = new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);

            // ACT & ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () => await organizationService.ReadAsync(ResourceExtensions.NewUniversalId()));
        }

        #endregion

        #region ReadAllAsync

        [Fact]
        public async Task ReadAllAsync()
        {
            // ARRANGE
            //TODO: when the "GOD" user is used here it fail. Not sure if that is an issue. 
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            IOrganizationService organizationService = new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);

            // ACT
            IEnumerable<OrganizationExtended> orgs = await organizationService.ReadAllAsync_Obsolete(false);

            // ASSERT - Collection
            Assert.NotEmpty(orgs);
            Assert.Single(orgs);

            // ASSERT - First Org
            OrganizationExtended org = orgs.First();
            // IResource
            AssertResource(org);
            // IOrganizationFields
            Assert.NotNull(org.Name);
            Assert.NotNull(org.Description);
            //IOrganization
            Assert.NotNull(org.ImageSrc);
        }

        [Fact]
        public async Task ReadAllAsync_UnkownClaims()
        {
            // ARRANGE
            Builder.SecurityContext.Use(new MockSecurityContext(true));
            IPersonService people = new MockPersonService(Builder.SecurityContext.Value, Builder.DbContext.Value);

            // ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                IOrganizationService organizationService = Builder.OrganizationService.Value;
                IEnumerable<OrganizationExtended> orgs = await organizationService.ReadAllAsync_Obsolete(false);
            });
        }

        #endregion

        #region CreateAsync

        private async Task<OrganizationExtended> CreateAsync(TestUserContext userContext)
        {
            await Builder.SetCurrentUserContext(userContext);

            // ARRANGE
            OrganizationService orgService = new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);
            OrganizationExtended org = new OrganizationExtended()
            {
                Name = "Org",
                Description = "Org description"
            };

            // ACT
            OrganizationExtended result = await orgService.CreateAsync(org);

            return result;
        }

        [Fact]
        public async Task CreateAsync_HasPermission_ButAnonymous()
        {
            // ARRANGE + ACT + ASSERT
            Exception ex = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await CreateAsync(TestUserContext.Anonymous);
            });

            Assert.Equal("Tenant is not associated with the user.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_HasPermission()
        {
            // ARRANGE + ACT
            OrganizationExtended result = await CreateAsync(TestUserContext.GodUser);

            // ASSERT
            Assert.NotNull(result);
            Assert.NotNull(result.Route);
            Assert.NotNull(result.UniversalId);
        }

        [Fact]
        public async Task CreateAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await CreateAsync(TestUserContext.OrgAdmin);
            });
        }

        #endregion

        #region DeleteAsync

        private async Task DeleteAsync(bool isAnonymous)
        {
            await Builder.SetCurrentUserContext(isAnonymous ? TestUserContext.Anonymous : TestUserContext.GodUser);

            // ARRANGE
            IOrganizationService orgService = Builder.OrganizationService.Value;
            OrganizationEntity org = Builder.DbContext.Value.Organizations.First();

            // ACT
            await orgService.DeleteAsync(org.Route);

            // Assert
            OrganizationEntity updated = await Builder.DbContext.Value.Organizations.Where(i => i.Route == org.Route).FirstOrDefaultAsync();
            Assert.NotNull(updated);
            Assert.True(updated.IsDeleted());
        }

        [Fact]
        public async Task DeleteAsync_HasPermission_ButAnonymous()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await DeleteAsync(true);
            });
        }

        [Fact]
        public async Task DeleteAsync_HasPermission()
        {
            // ARRANGE + ACT
            await DeleteAsync(false);
        }

        [Fact]
        public async Task DeleteAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await DeleteAsync(true);
            });
        }

        [Fact]
        public async Task DeleteAsync_OutsidePerson()
        {
            // ARRANGE & ACT & ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                MockIdentityDb db = Builder.DbContext.Value;
                await Builder.SetCurrentUserContextByRoute(IdentityDbSeed.UserOutside.Route);
                OrganizationService orgService = new OrganizationService(
                    Builder.SecurityContext.Value,
                    Builder.Infrastructure.Value,
                    Builder.ImageService.Value,
                    Builder.Mapper.Value,
                    Builder.Logger<OrganizationService>(),
                    Builder.Rbac.Value);
                await orgService.DeleteAsync(IdentityDbSeed.OrgPrimary.Route);
            });
        }

        #endregion        

        #region SaveAsync

        private async Task SaveAsync(TestUserContext userContext, bool exists)
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(userContext);
            string updateString = "2342344";
            IIdentityDb db = Builder.IdentityDb.Value;

            OrganizationService orgService = new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);

            Organization org = exists ? MockMapper.Instance.Map<Organization>(await db.Organizations.FirstAsync()) : new Organization()
            {
                Name = "Test Organization" + updateString,
                Description = "Test Organization Description" + updateString
            };

            if (exists)
            {
                org.Name += updateString;
                org.Description += updateString;
            }

            // ACT
            await orgService.SaveAsync(org);

            // Assert
            OrganizationEntity updated = await db.Organizations.Where(i => i.Route == org.Route).FirstOrDefaultAsync();
            Assert.NotNull(updated);
            Assert.NotEmpty(updated.Route);
            Assert.EndsWith(updateString, updated.Name);
            Assert.EndsWith(updateString, updated.Description);
        }

        #region SaveAsync (New)

        [Fact]
        public async Task SaveAsync_New_GodUser()
        {
            // ARRANGE + ACT
            await SaveAsync(TestUserContext.GodUser, false);
        }

        [Fact]
        public async Task SaveAsync_New_OrgAdmin()
        {
            // ARRANGE + ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await SaveAsync(TestUserContext.OrgAdmin, false);
            });
        }

        [Fact]
        public async Task SaveAsync_New_OrgMember()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await SaveAsync(TestUserContext.OrgMember, false);
            });
        }

        [Fact]
        public async Task SaveAsync_New_Anonymous()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await SaveAsync(TestUserContext.Anonymous, false);
            });
        }

        #endregion

        #region SaveAsync (Update)

        [Fact]
        public async Task SaveAsync_Update_GodUser()
        {
            // ARRANGE + ACT
            await SaveAsync(TestUserContext.GodUser, true);
        }

        [Fact]
        public async Task SaveAsync_Update_OrgAdmin()
        {
            // ARRANGE + ACT
            await SaveAsync(TestUserContext.OrgAdmin, true);
        }

        [Fact]
        public async Task SaveAsync_Update_OrgMember()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await SaveAsync(TestUserContext.OrgMember, true);
            });
        }

        [Fact]
        public async Task SaveAsync_Update_Anonymous()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await SaveAsync(TestUserContext.Anonymous, true);
            });
        }

        #endregion

        #endregion

        #region PatchAsync

        private async Task PatchAsync(TestUserContext userContext)
        {
            await Builder.SetCurrentUserContext(userContext);
            IIdentityDb db = Builder.DbContext.Value;
            IOrganizationService orgService = new OrganizationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.ImageService.Value,
                Builder.Mapper.Value,
                Builder.Logger<OrganizationService>(),
                Builder.Rbac.Value);

            OrganizationEntity org = await db.Organizations.FirstAsync();
            JsonPatchDocument patchDoc = new JsonPatchDocument();
            string updateString = "9283498";
            patchDoc.Replace($"/{nameof(org.Name)}", org.Name + updateString);
            patchDoc.Replace($"/{nameof(org.Description)}", org.Description + updateString);

            // ACT
            await orgService.PatchAsync(org.Route, patchDoc);

            // Assert
            OrganizationEntity updated = await db.Organizations.Where(i => i.Route == org.Route).FirstOrDefaultAsync();
            Assert.NotNull(updated);
            Assert.NotEmpty(updated.Route);
            Assert.EndsWith(updateString, updated.Name);
            Assert.EndsWith(updateString, updated.Description);
        }

        [Fact]
        public async Task PatchAsync_Anonymous()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await PatchAsync(TestUserContext.Anonymous);
            });
        }

        [Fact]
        public async Task PatchAsync_HasPermission()
        {
            // ARRANGE + ACT
            await PatchAsync(TestUserContext.GodUser);
        }

        [Fact]
        public async Task PatchAsync_NoPermission()
        {
            // ARRANGE + ACT + ASSERT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await PatchAsync(TestUserContext.OrgMember);
            });
        }

        #endregion                
    }

}