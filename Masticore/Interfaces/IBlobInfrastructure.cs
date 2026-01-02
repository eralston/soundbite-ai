using Masticore.Storage;
using System.Threading.Tasks;

namespace Masticore
{
    /// <summary>
    /// Factory for <see cref="IBlobs"/> objects
    /// </summary>
    public interface IBlobInfrastructure : IInfrastructure
    {
        /// <summary>
        /// Async get
        /// </summary>
        /// <returns></returns>
        Task<IBlobs> BlobsAsync();
    }
}
