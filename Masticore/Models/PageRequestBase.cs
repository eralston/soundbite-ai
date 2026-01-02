namespace Masticore.Models
{
    /// <summary>
    /// Defines the parameters for a paging request
    /// </summary>
    /// <remarks>
    /// This supports both a "skip and take by numbers" strategy and a "skip by continuation token and take by number" strategy, which may vary by endpoint
    /// </remarks>
    public abstract class PageRequestBase
    {
        /// <summary>
        /// The minimum number of "items" that can be taken at a time
        /// </summary>
        public const int DefaultMinTake = 1;

        /// <summary>
        /// The maximum number of "items" that can be taken at a time
        /// </summary>
        public const int DefaultMaxTake = 128;

        /// <summary>
        /// The amount to take, must be at least one
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// Search criteria for finding elements by search criteria
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string Filter { get; set; }
    }
}
