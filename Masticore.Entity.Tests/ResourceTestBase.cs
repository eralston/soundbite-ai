using Masticore.Resources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// Base class for a unit test class that check CRUD+Archive functionality
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TInfrastructure"></typeparam>
    public abstract class ResourceTestBase<TResource, TDbContext, TInfrastructure> : EntityTestBase<TInfrastructure, TDbContext>
        where TResource : ResourceEntityBase, new()
        where TDbContext : DbContext
        where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
    {
        /// <summary>
        /// Creates a new instance of TResource in the TDbContext
        /// </summary>
        /// <returns></returns>
        protected virtual async Task DoCreate()
        {
            // ARRANGE
            TDbContext db = Builder.DbContext.Value;

            TResource resource = new TResource
            {
                CreatedById = 1
            };
            resource.Timestamp();
            resource.NewRoute();
            SetCreated(resource, db);
            db.Set<TResource>().Add(resource);

            // ACT
            await db.SaveChangesAsync();

            // ASSERT
            Assert.NotEqual(0, resource.CreatedById);
            AssertResource(resource);
            AssertCreated(resource);
        }

        /// <summary>
        /// Child classes should override this method if they want to set additional fields on create
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SetCreated(TResource entity, TDbContext db)
        {
            // By default, do nothing
        }

        /// <summary>
        /// Child classes should override this method if they want to do additional processing on create
        /// EG, they want to check if IMapper.Map works for this type
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void AssertCreated(TResource entity)
        {
            // By default, do nothing
        }

        /// <summary>
        /// Reads all instance of the given TResource from the collection
        /// </summary>
        /// <returns></returns>
        protected Task DoReadToArray()
        {
            // ARRANGE
            TDbContext db = Builder.DbContext.Value;

            // ACT
            TResource[] resources = db.Set<TResource>().ToArray();

            // ASSERT
            Assert.NotEmpty(resources);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Implements reading the first TResource in the collection
        /// </summary>
        /// <returns></returns>
        protected Task DoReadFind()
        {
            // ARRANGE
            TDbContext db = Builder.DbContext.Value;

            // ACT
            TResource resource = db.Set<TResource>().Find(1);

            // ASSERT
            Assert.NotNull(resource);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Implements Update
        /// </summary>
        /// <returns></returns>
        protected async Task DoUpdate()
        {
            // ARRANGE
            TDbContext db = Builder.DbContext.Value;

            // ACT
            // Pull out the entity, modify it, and save
            TResource resource = db.Set<TResource>().Find(1);
            Assert.NotNull(resource);
            SetUpdated(resource, db);
            DateTime stamp = resource.Timestamp();
            await db.SaveChangesAsync();

            TResource updatedResource = db.Set<TResource>().Find(1);

            // ASSERT
            Assert.NotNull(updatedResource);
            Assert.Equal(resource.Id, updatedResource.Id);
            Assert.Equal(stamp, updatedResource.UpdatedUtc);
            Assert.NotEqual(stamp, updatedResource.CreatedUtc);
            AssertUpdated(resource, updatedResource);
        }

        /// <summary>
        /// Child classes should override this to do additional changes to the entity during the update unit test
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SetUpdated(TResource entity, TDbContext db)
        {
            // By default, do nothing
        }

        /// <summary>
        /// Child classes should override this to do additional assert statements based on what was changed in UpdateEntity
        /// </summary>
        /// <param name="updatedEntity"></param>
        protected virtual void AssertUpdated(TResource originalEntity, TResource updatedEntity)
        {
            //  Assert.Equal(newValue, updatedEntity.Name);
        }

        /// <summary>
        /// Performs a soft delete on the first instance of the Resource in the TDbContext
        /// </summary>
        /// <returns></returns>
        public async Task DoSoftDelete()
        {
            // ARRANGE
            TDbContext db = Builder.DbContext.Value;

            // ACT
            TResource resource = db.Set<TResource>().Find(1);
            Assert.NotNull(resource);
            Assert.Equal(default, resource.DeletedUtc);

            DateTime stamp = resource.SoftDelete();
            await db.SaveChangesAsync();
            TResource deletedSource = db.Set<TResource>().Find(1);

            // ASSERT
            Assert.NotNull(deletedSource);
            Assert.Equal(stamp, deletedSource.DeletedUtc);
        }

        /// <summary>
        /// Ensure trying to remove the given TResource causes a DbUpdateException
        /// </summary>
        /// <returns></returns>
        protected Task CheckDeleteConstrained()
        {
            // ARRANGE
            TDbContext db = Builder.DbContext.Value;

            // ACT
            TResource resource = db.Set<TResource>().Find(1);
            db.Set<TResource>().Remove(resource);

            // ASSERT
            // Should throw exception due to foreign key constraint
            Assert.Throws<DbUpdateException>(() => db.SaveChanges());

            return Task.CompletedTask;
        }

        /// <summary>
        /// Implements deleting the first instance of the current resource in the collection
        /// </summary>
        /// <returns></returns>
        protected async Task DoDelete()
        {
            // ARRANGE
            TDbContext db = Builder.DbContext.Value;

            // ACT
            TResource resource = db.Set<TResource>().Find(1);
            db.Set<TResource>().Remove(resource);
            await db.SaveChangesAsync();
            TResource deleteResource = db.Set<TResource>().Find(1);

            // ASSERT
            Assert.Null(deleteResource);
        }
    }
}
