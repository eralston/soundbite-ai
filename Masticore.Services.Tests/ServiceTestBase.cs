using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Masticore.Services.Tests
{
    /// <summary>
    /// Base class for building tests for masticore services.
    /// </summary>
    public abstract class ServiceTestBase<TInfrastructure, TDbContext> : TestBase
        where TDbContext : DbContext
        where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
    {
        #region Properties

        protected TestBuilder<TInfrastructure, TDbContext> Builder { get; } = new TestBuilder<TInfrastructure, TDbContext>();

        /// <summary>
        /// Gets default org route for the current db
        /// </summary>
        protected string OrgRoute
        {
            get
            {
                IIdentityDb db = Builder.Infrastructure.Value.IdentityDbAsync().Result;
                return db.Organizations.Find(1).Route;
            }
        }

        /// <summary>
        /// Gets default team route for the current db
        /// </summary>
        protected string GroupRoute
        {
            get
            {
                IIdentityDb db = Builder.Infrastructure.Value.IdentityDbAsync().Result;
                return db.Groups.Find(1).Route;
            }
        }

        #endregion

        #region Constructor

        public ServiceTestBase()
        {
            Builder.ImageService.Use(i => new MockImageService());
            Builder.NotificationService.Use((i) => new MockNotifications());
            Builder.MemberService.Use(i =>
                new MemberService(
                    i.SecurityContext.Value,
                    i.NotificationService.Value,
                    i.ImageService.Value,
                    i.Infrastructure.Value,
                    i.Mapper.Value,
                    i.Logger<MemberService>()
                )
            );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Asserts that the IPersist, IUniversal, and IArchive properties match expectations for an initialized object
        /// </summary>
        /// <param name="obj"></param>
        public void AssertEntity(ResourceEntityBase entity)
        {
            Assert.NotNull(entity);
            Assert.NotEqual(0, entity.Id);
            Assert.NotEqual(default, entity.CreatedUtc);
            Assert.NotEqual(default, entity.UpdatedUtc);
        }

        #endregion        
    }
}
