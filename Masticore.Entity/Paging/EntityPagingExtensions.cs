using Masticore.Exceptions;
using Masticore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Static methods to support paging LINQ queries, driven mostly by <see cref="IndexPageRequest"/> and <see cref="IndexPageResponse{TEntity}"/>
    /// </summary>
    public static class EntityPagingExtensions
    {
        /// <summary>
        /// An all-in-one async process for turning a <see cref="IQueryable{T}"/> and <see cref="IndexPageRequest"/> into a fully loaded <see cref="IndexPageResponse{TEntity}"/>
        /// </summary>
        /// <remarks>This does NOT enforce a max skip/take, for that please use <see cref="ReadPageAsync{TEntity}(IQueryable{TEntity}, IndexPageQuery, Expression{Func{TEntity, bool}}, bool)"/></remarks>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="allQuery"></param>
        /// <param name="request">If not provides, this will start at item <see cref="MinSkip"/> and provide <see cref="MaxTake"/> items; never returning counts</param>
        /// <param name="filterPreciate">The predicate to use for searching, but only applied if <see cref="IndexPageRequest.Filter"/> is offered so it is safe to reference <see cref="IndexPageRequest.Filter"/> w/o checking null</param>
        /// <param name="isReadOnly">Should we query with AsNoTracking? Default true</param>
        /// <returns>Response with complete context of the request and optional metadata about paging</returns>      
        /// <exception cref="BadRequestException">If <see cref="IndexPageRequest"/> asks for something out of range</exception>
        public static async Task<IndexPageResponse<TEntity>>
            ReadPageAsync<TEntity>(
                this IQueryable<TEntity> allQuery,
                IndexPageRequest request = null,
                Expression<Func<TEntity, bool>> filterPreciate = null,
                bool isReadOnly = true)
                where TEntity : class
        {
            if (allQuery is null)
            {
                throw new ArgumentNullException(nameof(allQuery));
            }

            // Request might be null, so we need to fill in values as needed

            bool includeTotals = request?.IncludesCounts ?? false;

            int skip = request?.Skip ?? IndexPageRequest.DefaultMinSkip;
            int take = request?.Take ?? IndexPageRequest.DefaultMaxTake;

            IndexPageResponse<TEntity> response = new IndexPageResponse<TEntity>
            {
                Request = new IndexPageRequest
                {
                    Skip = request?.Skip ?? IndexPageRequest.DefaultMinSkip,
                    Take = request?.Take ?? IndexPageRequest.DefaultMaxTake,
                    IncludesCounts = includeTotals,
                    Filter = request?.Filter,
                }
            };

            // Optionally filter for items using the given filter criteria, but only if request has a filter value
            // NOTE: This does NOT inject the filter criteria, it just looks if it is non-empty then goes with the logic of the filter predicate
            if (!string.IsNullOrEmpty(request?.Filter) && filterPreciate != null)
            {
                allQuery = allQuery.Where(filterPreciate);
            }

            // Query with skip/take and optional read-only
            IQueryable<TEntity> query = allQuery.Skip(skip).Take(take);
            if (isReadOnly)
            {
                query = query.AsNoTracking();
            }

            response.Result = await query.ToArrayAsync();

            // Optionally include the totals
            if (includeTotals)
            {
                int total = await allQuery.CountAsync();
                response.TotalCount = total;
                response.TotalPageCount = (int)Math.Ceiling(total / (decimal)take);
                if (response.TotalPageCount == 0)
                {
                    response.TotalPageCount = 1;
                }
            }

            return response;
        }

        /// <summary>
        /// Executes the given query against the given query parameters, returning a paged response
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="allQuery"></param>
        /// <param name="query"></param>
        /// <param name="filterPreciate"></param>
        /// <param name="isReadOnly"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<IndexPageResponse<TEntity>>
            ReadPageAsync<TEntity>(
                this IQueryable<TEntity> allQuery,
                IndexPageQuery query,
                Expression<Func<TEntity, bool>> filterPreciate = null,
                bool isReadOnly = true)
                where TEntity : class
        {
            if (allQuery is null)
            {
                throw new ArgumentNullException(nameof(allQuery));
            }

            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (query.PageRequest is null)
            {
                throw new ArgumentNullException(nameof(query.PageRequest));
            }

            query.AssertMinAndMax();

            IndexPageResponse<TEntity> ret = await ReadPageAsync(allQuery, query.PageRequest, filterPreciate, isReadOnly);

            ret.MinSkip = query.MinSkip;
            ret.MaxSkip = query.MaxSkip;
            ret.MinTake = query.MinTake;
            ret.MaxTake = query.MaxTake;
            ret.MaxTotalResults = query.MaxTotalResults;

            return ret;
        }

        /// <summary>
        /// Async read the given query for the 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="page"></param>
        /// <param name="query"></param>
        /// <param name="filterPreciate"></param>
        /// <param name="isReadOnly"></param>
        /// <returns></returns>
        public static async Task<IndexPageResponse<TEntity>> ReadPageResponseAsync<TEntity>(
            this IQueryable<TEntity> query,
            IndexPageRequest page,
            Expression<Func<TEntity, bool>> filterPreciate = null,
            bool isReadOnly = true)
            where TEntity : class
        {
            IndexPageQuery pageQuery = new IndexPageQuery(page);
            IndexPageResponse<TEntity> ret = await query.ReadPageAsync(pageQuery, filterPreciate, isReadOnly);
            ret.BasedOn(pageQuery);
            return ret;
        }

    }
}
