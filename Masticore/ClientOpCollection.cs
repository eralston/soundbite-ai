using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Masticore
{
    /// <summary>
    /// Represents a request sent to the client to fulfil an operation that cannot be completed from
    /// the backend.  Normally these operations require user tokens from third-party providers.
    /// </summary>
    public class ClientOpCollection : Collection<ClientOp>
    {
        /// <summary>
        /// Adds the specified client operations to the collection.
        /// </summary>
        /// <param name="clientOps">Client operations to add to the collection.</param>
        public void Add(IEnumerable<ClientOp> clientOps)
        {
            clientOps?.ForEach(Add);
        }

        /// <summary>
        /// Adds the specified client operations to the collection.
        /// </summary>
        /// <param name="clientOpWrapper">Client operations to add to the collection.</param>
        public void Add<T>(ClientOpWrapper<T> clientOpWrapper)
        {
            clientOpWrapper?.ClientOps?.ToList().ForEach(Add);
        }

        /// <summary>
        /// Converts the collection into an array or <c>null</c> if there are no items in the collection.
        /// </summary>
        /// <returns>an array of the collection or <c>null</c> if there are no items in the collection.</returns>
        public ClientOp[] ToNullOrArray()
        {
            return this.Any() ? this.ToArray() : null;
        }
    }
}