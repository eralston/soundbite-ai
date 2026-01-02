using Masticore.Services;
using Soundbite.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the service operations required to manage clip operations.
    /// </summary>
    public interface IClipOperationService : IService
    {
        /// <summary>
        /// Retrieves a ClipOperation by route.
        /// </summary>
        /// <param name="route">Route of the clip operation to retrieve.</param>
        /// <returns>a reference to the requested clip if found, otherwise <c>null</c>.</returns>
        Task<ClipOperation> ReadByRoute(string route);

        /// <summary>
        /// Retrieves all operations associated with the specified clip.
        /// </summary>
        /// <param name="clipRoute">Route of the clip whose operations are being sought.</param>
        /// <returns>a list of clip operations associated with the specified clip.</returns>
        Task<IList<ClipOperation>> ReadByClipRoute(string clipRoute);

        /// <summary>
        /// Retrieves an operation based on a third-party ID associated with the operation.
        /// </summary>
        /// <param name="externalId">Third-party ID associated with the operation.</param>
        /// <returns>a reference to the requested <see cref="ClipOperation"/> if found, otherwise <c>null</c>.</returns>
        Task<ClipOperation> ReadByExternalId(string externalId);

        /// <summary>
        /// Saves the clip operation.
        /// </summary>
        /// <param name="clipOp">Clip operation to save.</param>
        /// <returns>the ClipOperation passed into the method with updated values.</returns>
        Task<ClipOperation> SaveAsync(ClipOperation clipOp);

        /// <summary>
        /// Updates the state of the clip operation. If the state is marked as complete the method
        /// also set the <see cref="ClipOperation.CompletedDate"/> to the current UTC time.
        /// </summary>
        /// <param name="clipOpRoute">Route of the clip operation to update.</param>
        /// <param name="state">State to update on the clip operation.</param>
        /// <param name="errorDetails">Error details for the error state.  Value is disregarded unless <paramref name="state"/> value is <see cref="ClipOperationStateType.Error"/>.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task SetState(string clipOpRoute, ClipOperationStateType state, string errorDetails = null);
    }
}