using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class UserTests : ResourceTestBase<UserEntity, MockIdentityDb, MockSeedInfrastructure>
    {
        [Fact]
        public async Task Create()
        {
            await DoCreate();
        }

        protected override void SetCreated(UserEntity entity, MockIdentityDb db)
        {
            entity.GivenName = "Alice";
            entity.FamilyName = "Smith";
            entity.Email = "alice@smith.com";
            entity.Phone = "123-456-7890";
            entity.Title = "VP";
            entity.InviteUtc = Time.UtcNow;
            entity.InviteAcceptUtc = Time.UtcNow;
            entity.AllowNews = true;
            entity.AllowMarketing = true;
            entity.AllowEmail = true;
            entity.AllowSms = true;

            entity.MergedToUser = db.Users.Find(1);
        }

        protected override void AssertCreated(UserEntity entity)
        {
            Assert.NotNull(entity.GivenName);
            Assert.NotNull(entity.FamilyName);
            Assert.NotNull(entity.Email);
            Assert.NotNull(entity.Phone);
            Assert.NotNull(entity.Title);
            Assert.NotNull(entity.InviteUtc);
            Assert.NotNull(entity.InviteAcceptUtc);
            Assert.True(entity.AllowNews);
            Assert.True(entity.AllowMarketing);
            Assert.True(entity.AllowEmail);
            Assert.True(entity.AllowSms);
        }

        [Fact]
        public async Task ReadToArray()
        {
            await DoReadToArray();
        }

        [Fact]
        public async Task ReadFind()
        {
            await DoReadFind();
        }

        [Fact]
        public async Task Update()
        {
            await DoUpdate();
        }

        private readonly string updatedTitle = "VP";

        protected override void SetUpdated(UserEntity entity, MockIdentityDb db)
        {
            entity.Title = updatedTitle;
        }

        protected override void AssertUpdated(UserEntity originalEntity, UserEntity updatedEntity)
        {
            Assert.Equal(updatedTitle, updatedEntity.Title);
        }

        [Fact]
        public async Task Delete()
        {
            await CheckDeleteConstrained();
        }

        [Fact]
        public async Task Create_NoEmail()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            UserEntity user = db.Users.CreateResource();

            // ACT & ASSERT
            await Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
        }

        [Fact]
        public async Task Create_BadEmail()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            UserEntity user = new UserEntity();

            // ACT & ASSERT
            Assert.Throws<ArgumentException>(() => user.Email = "bob");
        }
    }
}
