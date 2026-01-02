using System.IO;
using System.Threading.Tasks;

namespace Masticore.Graph
{
    /// <summary>
    /// Describes an object that wraps a graph implementation (EG, MS Graph)
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// Async gets the <see cref="Models.User"/> record for the current user
        /// </summary>
        /// <returns></returns>
        Task<Models.User> MyUserAsync();

        /// <summary>
        /// Gets the avatar for the current user
        /// </summary>
        /// <returns></returns>
        Task<Stream> MyPhotoAsync();
    }
}