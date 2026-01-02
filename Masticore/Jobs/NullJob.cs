using Masticore.Queue;
using System.Threading.Tasks;

namespace Masticore.Jobs
{
    /// <summary>
    /// This <see cref="IJob"/> does nothing, but it is useful for unit testing or stand-ins
    /// </summary>
    public class NullJob : JobBase
    {
        /// <summary>
        /// Gets or sets stand-in generic data
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Does no real work
        /// </summary>
        /// <returns></returns>
        public override Task Process()
        {
            return Task.CompletedTask;
        }
    }
}
