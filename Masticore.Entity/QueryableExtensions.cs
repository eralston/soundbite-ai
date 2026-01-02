using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masticore.Entity
{
    /// <summary>
    /// Helpers for <see cref="IQueryable{T}"/>' in EntityFrameworkCore 3
    /// </summary>
    public static class QueryableExtensions
    {
        private static object ReadNonPublicField(this object obj, string privateField)
        {
            return obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        }

        private static T ReadNonPublicField<T>(this object obj, string privateField)
        {
            return (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        }

        /// <summary>
        /// Generates the analogous SQL code for the given <see cref="IQueryable{TEntity}"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string ToSql<TEntity>(this IQueryable<TEntity> query)
        {
            using IEnumerator<TEntity> enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            object relationalCommandCache = enumerator.ReadNonPublicField("_relationalCommandCache");
            SelectExpression selectExpression = relationalCommandCache.ReadNonPublicField<SelectExpression>("_selectExpression");
            IQuerySqlGeneratorFactory factory = relationalCommandCache.ReadNonPublicField<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

            QuerySqlGenerator sqlGenerator = factory.Create();
            IRelationalCommand command = sqlGenerator.GetCommand(selectExpression);

            string sql = command.CommandText;
            return sql;
        }
    }
}