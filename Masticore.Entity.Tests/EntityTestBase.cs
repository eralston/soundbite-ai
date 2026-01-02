using Masticore.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// Base class for building Masticore Entity tests.
    /// </summary>
    public abstract class EntityTestBase<TInfrastructure, TDbContext> : TestBase
        where TDbContext : DbContext
        where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
    {
        #region Properties

        protected TestBuilder<TInfrastructure, TDbContext> Builder { get; } = new TestBuilder<TInfrastructure, TDbContext>();

        #endregion

        #region Methods

        /// <summary>
        /// Asserts that the IPersist, IUniversal, and IArchive properties match expectations for an initialized object
        /// </summary>
        /// <param name="obj">Object to check.</param>
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
