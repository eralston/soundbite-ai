using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// A class for capturing and comparing database states
    /// </summary>
    public class DbSnapshot
    {
        protected class TypeSnapshot
        {
            public int Count { get; set; }
            public int LastId { get; set; }
        }

        private static string GetIdentifier<TEntity>() where TEntity : class
        {
            return typeof(TEntity).Name;
        }

        private DbContext Context { get; }
        private Dictionary<string, TypeSnapshot> TypeSnaps { get; } = new Dictionary<string, TypeSnapshot>();

        public DbSnapshot(DbContext context)
        {
            Context = context;
        }

        public DbSnapshot Snap<TEntity>() where TEntity : EntityBase
        {
            string identifier = GetIdentifier<TEntity>();

            TypeSnaps[identifier] = new TypeSnapshot
            {
                Count = CurrentCount<TEntity>(),
                LastId = CurrentLastId<TEntity>()
            };
            return this;
        }

        private int CurrentLastId<TEntity>() where TEntity : EntityBase
        {
            TEntity entity = Context.Set<TEntity>().OrderByDescending(e => e.Id).FirstOrDefault();
            if (entity is null)
            {
                return 0;
            }
            else
            {
                return entity.Id;
            }
        }

        public TEntity[] NewEntities<TEntity>() where TEntity : EntityBase
        {
            int lastId = TypeSnaps[GetIdentifier<TEntity>()].LastId;
            return Context.Set<TEntity>().Where(e => e.Id > lastId).ToArray();
        }

        private int OldCount<TEntity>() where TEntity : EntityBase
        {
            return TypeSnaps[GetIdentifier<TEntity>()].Count;
        }

        private int CurrentCount<TEntity>() where TEntity : EntityBase
        {
            return Context.Set<TEntity>().Count();
        }

        public DbSnapshot AssertAdd<TEntity>(int expectedDiff) where TEntity : EntityBase
        {
            int current = CurrentCount<TEntity>();
            int old = OldCount<TEntity>();
            Assert.Equal(expectedDiff, current - old);
            return this;
        }
    }
}
