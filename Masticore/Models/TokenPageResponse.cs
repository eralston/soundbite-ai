using System;
using System.Collections.Generic;
using System.Linq;

namespace Masticore.Models
{
    /// <summary>
    /// Concrete implementation of <see cref="PageRequestBase"/> for endpoints that use skip tokens for paging
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [CodeGenModel]
    public class TokenPageResponse<TEntity> : PageResponseBase<TEntity>
    {
        /// <summary>
        /// The request that originated this response
        /// </summary>
        public TokenPageRequest Request { get; set; }

        /// <summary>
        /// Some endpoints implement paging by skip token (rather than skip number), in which case this token will represent the NEXT page in the sequence
        /// </summary>
        /// <remarks>If this is absent, then the endpoint either does not implement paging or it uses skip by number</remarks>
        [CodeGenField(IsNullable = true)]
        public string SkipToken { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        public TokenPageResponse(TokenPageRequest request = null, IEnumerable<TEntity> result = null)
        {
            Request = request ?? new TokenPageRequest
            {
                SkipToken = request.SkipToken,
                Take = request?.Take ?? IndexPageRequest.DefaultMaxTake,
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
        public TokenPageResponse<TNewType> Map<TNewType>(Func<TEntity, TNewType> mapper)
        {
            TokenPageResponse<TNewType> ret = new TokenPageResponse<TNewType>
            {
                // Metadata
                MinTake = MinTake,
                MaxTake = MaxTake,

                // Data with converted results
                Request = Request,
                Result = Result.Select(item => mapper(item))
            };
            return ret;
        }

        /// <summary>
        /// True if the given page likely has more results; otherwise false
        /// </summary>
        /// <returns></returns>
        public override bool HasMorePages()
        {
            bool result = !string.IsNullOrEmpty(SkipToken?.Trim());
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="IndexPageRequest"/> for the next page in this response
        /// </summary>
        /// <returns></returns>
        public TokenPageRequest NextPage()
        {
            TokenPageRequest newRequest = new TokenPageRequest
            {
                SkipToken = SkipToken,
                Take = Request.Take ?? MaxTake,
            };

            return newRequest;
        }
    }
}
