using System.Collections.Generic;

namespace Masticore.Models
{
    /// <summary>
    /// The output for a paged request, most conveniently loaded based on a <see cref="IndexPageRequest"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [CodeGenModel(Name = "PageResponseBase")]
    public abstract class PageResponseBase<TEntity>
    {
        /// <summary>
        /// Max take size; can inform further requests
        /// </summary>
        public int MinTake { get; set; } = PageRequestBase.DefaultMinTake;

        /// <summary>
        /// Max take size; can inform further requests
        /// </summary>
        public int MaxTake { get; set; } = PageRequestBase.DefaultMaxTake;

        /// <summary>
        /// The current set of results starting at <see cref="IndexPageRequest.Skip"/> and including <see cref="PageRequestBase.Take"/>
        /// </summary>
        public IEnumerable<TEntity> Result { get; set; }

        /// <summary>
        /// True if the given page likely has more results; otherwise false
        /// </summary>
        /// <returns></returns>
        public abstract bool HasMorePages();
    }
}
