namespace Masticore.Models
{
    /// <summary>
    /// A page request that uses skip via number index in the collection
    /// </summary>
    [CodeGenModel]
    public class IndexPageRequest : PageRequestBase
    {
        /// <summary>
        /// The minimum number to skip (zero)
        /// </summary>
        public const int DefaultMinSkip = 0;

        /// <summary>
        /// The maximum number for a skip command
        /// </summary>
        public const int DefaultMaxSkip = DefaultMaxTotalResults - DefaultMinTake;

        /// <summary>
        /// The default maximum number of items; specific types may want to override this
        /// </summary>
        /// <remarks>If one needs records beyond <see cref="DefaultMaxTotalResults"/> times <see cref="PageRequestBase.DefaultMaxTake"/> then use async batch processing</remarks>
        public const int DefaultMaxTotalResults = 1024;

        /// <summary>
        /// The amount to skip, starting with zero
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Whether or not to include <see cref="IndexPageResponse{TEntity}.TotalCount"/> and <see cref="IndexPageResponse{TEntity}.TotalPageCount"/>
        /// </summary>
        public bool? IncludesCounts { get; set; }

        /// <summary>
        /// Increments this request to the next page based on <see cref="Skip"/> and <see cref="PageRequestBase.Take"/>
        /// </summary>
        /// <returns></returns>
        public bool NextPage(int maxResults = DefaultMaxTotalResults, int maxTake = DefaultMaxTake, int minSkip = DefaultMinSkip)
        {
            int take = Take ?? maxTake;
            int nextSkip = (Skip ?? minSkip) + take;

            // If we're about to run off the end
            // Try shrinking the amount of take to get the last little bit; otherwise, we actually abort and do nothing
            if ((nextSkip + take) > maxResults)
            {
                take = maxResults - nextSkip;
                if (take <= 0)
                {
                    return false;
                }
                else
                {
                    Take = take;
                }
            }
            Skip = nextSkip;

            return Skip + Take <= maxResults;
        }
    }
}
