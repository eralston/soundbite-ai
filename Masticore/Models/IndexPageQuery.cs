using Masticore.Exceptions;

namespace Masticore.Models
{
    /// <summary>
    /// The system's internal wrapper of <see cref="PageRequest"/> that allows decorating it with additional system-level info
    /// </summary>
    public class IndexPageQuery
    {
        /// <summary>
        /// Gets the optional page request for setting up this query
        /// </summary>
        public IndexPageRequest PageRequest { get; }

        /// <summary>
        /// The minimum number to skip (zero)
        /// </summary>
        public int MinSkip { get; set; } = IndexPageRequest.DefaultMinSkip;

        /// <summary>
        /// The maximum number this query may skip
        /// </summary>
        public int MaxSkip { get; set; } = IndexPageRequest.DefaultMaxTotalResults - PageRequestBase.DefaultMinTake;

        /// <summary>
        /// The minimum number to skip (zero)
        /// </summary>
        public int MinTake { get; set; } = PageRequestBase.DefaultMinTake;

        /// <summary>
        /// The maximum number this query may skip
        /// </summary>
        public int MaxTake { get; set; } = PageRequestBase.DefaultMaxTake;

        /// <summary>
        /// The maximum total items
        /// </summary>
        public int MaxTotalResults { get; protected set; } = IndexPageRequest.DefaultMaxTotalResults;

        /// <summary>
        /// Constructor that wraps up the page query
        /// </summary>
        /// <param name="pageRequest"></param>
        public IndexPageQuery(IndexPageRequest pageRequest = null)
        {
            PageRequest = pageRequest ?? new IndexPageRequest();
        }

        /// <summary>
        /// Sets the new value for <see cref="MaxTotalResults"/> and re-calculates the new <see cref="MaxSkip"/> based on it
        /// </summary>
        /// <param name="newMax"></param>
        public IndexPageQuery SetMaxResults(int newMax)
        {
            MaxTotalResults = newMax;
            MaxSkip = newMax - MinTake;
            return this;
        }

        /// <summary>
        /// Enforces maximum skip/take; falls back to the defaults in <see cref="PageRequest"/>
        /// </summary>
        /// <exception cref="BadRequestException"></exception>
        public void AssertMinAndMax()
        {
            int take = PageRequest?.Take ?? PageRequestBase.DefaultMaxTake;
            int skip = PageRequest?.Skip ?? IndexPageRequest.DefaultMinSkip;

            // Enforce min and max take
            if (take < MinTake)
            {
                throw new BadRequestException($"Cannot take {take}; out of range of minimum take {MinTake}");
            }
            else if (take > MaxTake)
            {
                throw new BadRequestException($"Cannot take {take}; out of range of maximum take {MaxTake}");
            }

            if (skip < MinSkip)
            {
                throw new BadRequestException($"Cannot skip {skip}; out of range of minimum skip {MinSkip}");
            }
            else if (skip > MaxSkip)
            {
                throw new BadRequestException($"Cannot skip {skip}; out of range of maximum skip {MaxSkip}");
            }

            // Enforce max paging through the collection
            if (skip + take > MaxTotalResults)
            {
                throw new BadRequestException($"Cannot skip to page {skip} with take {take}; page would be out of range for max total results {MaxTotalResults}");
            }
        }
    }
}
