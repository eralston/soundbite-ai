using System;
using System.Collections.Generic;
using System.Linq;

namespace Masticore.Models
{
    /// <summary>
    /// Concrete class of <see cref="PageRequestBase"/> that implement index-based skip
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [CodeGenModel]
    public class IndexPageResponse<TEntity> : PageResponseBase<TEntity>
    {
        #region Metadata

        /// <summary>
        /// The minimum skip value for TEntity
        /// </summary>
        public int MinSkip { get; set; } = IndexPageRequest.DefaultMinSkip;

        /// <summary>
        /// The maximum skip value for TEntity
        /// </summary>
        public int MaxSkip { get; set; } = IndexPageRequest.DefaultMaxSkip;

        /// <summary>
        /// Max total number of items the API will ever send back' can inform future requests
        /// </summary>
        public int MaxTotalResults { get; set; } = IndexPageRequest.DefaultMaxTotalResults;

        #endregion

        #region Data

        /// <summary>
        /// The parameters driving the original request; if the request as originally null, then this should reflect some kind of safe defaults
        /// </summary>
        public IndexPageRequest Request { get; set; }

        /// <summary>
        /// Optional total count for all items in the collection
        /// </summary>
        /// <remarks>Only loaded if the <see cref="IndexPageRequest.IncludesCounts"/> is true</remarks>
        public int? TotalCount { get; set; }

        /// <summary>
        /// Determines the ceiling of the number of pages in the collection, assuming they all use the same <see cref="PageRequestBase.Take"/> value
        /// </summary>
        /// <remarks>Only loaded if the <see cref="IndexPageRequest.IncludesCounts"/> is true</remarks>
        public int? TotalPageCount { get; set; }

        #endregion

        #region Server-Side Methods

        /// <summary>
        /// Sets values for this response based on the given <see cref="IndexPageQuery"/>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IndexPageResponse<TEntity> BasedOn(IndexPageQuery query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            MaxTake = query.MaxTake;
            MaxTotalResults = query.MaxTotalResults;
            return this;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public IndexPageResponse(IndexPageRequest request = null, IEnumerable<TEntity> result = null)
        {
            Request = request ?? new IndexPageRequest
            {
                Skip = request?.Skip ?? IndexPageRequest.DefaultMinSkip,
                Take = request?.Take ?? IndexPageRequest.DefaultMaxTake,
                IncludesCounts = request?.IncludesCounts ?? false,
                Filter = request?.Filter,
            };
            Result = result ?? Enumerable.Empty<TEntity>();
        }

        /// <summary>
        /// Converts this page response from old to new classes using the given map function
        /// </summary>
        /// <typeparam name="TNewType"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public IndexPageResponse<TNewType> Map<TNewType>(Func<TEntity, TNewType> mapper)
        {
            IndexPageResponse<TNewType> ret = new IndexPageResponse<TNewType>()
            {
                // Metadata
                MinSkip = MinSkip,
                MaxSkip = MaxSkip,
                MinTake = MinTake,
                MaxTake = MaxTake,
                MaxTotalResults = MaxTotalResults,

                // Data with converted results
                Request = Request,
                TotalCount = TotalCount,
                TotalPageCount = TotalPageCount,
                Result = Result.Select(item => mapper(item)).ToArray(),
            };
            return ret;
        }

        /// <summary>
        /// Creates a new <see cref="IndexPageRequest"/> for the next page in this response
        /// </summary>
        /// <returns></returns>
        public IndexPageRequest NextPage()
        {
            IndexPageRequest newRequest = new IndexPageRequest
            {
                Filter = Request?.Filter,
                IncludesCounts = Request?.IncludesCounts,
                Skip = Request?.Skip,
                Take = Request?.Take
            };

            newRequest.NextPage(MaxTotalResults, MaxTake, MinSkip);
            return newRequest;
        }

        /// <summary>
        /// True if the given page likely has more results; otherwise false
        /// </summary>
        /// <returns></returns>
        public override bool HasMorePages()
        {
            if (Request.Skip >= MaxTotalResults)
            {
                return false;
            }

            // If we have the counts, then we can determine for a fact how many pages and which one this would be
            if (TotalCount.HasValue)
            {
                return TotalCount > Request.Skip + Request.Take;
            }

            // Otherwise, guess based on the number of items meeting the max requested
            return Request.Take == Result.Count();
        }

        #endregion
    }
}
