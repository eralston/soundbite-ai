using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Defines the contract for the service that manages QueuedJobStatus operations.
    /// </summary>
    public interface IQueuedJobStatusService : IService
    {
        /// <summary>
        /// Updates the specified the QueueJobStatus associated with the specified <paramref name="route"/>
        /// with the specified <paramref name="messageId"/> and <paramref name="message"/> information.
        /// </summary>
        /// <param name="route">Route of the <see cref="QueuedJobStatus"/> item to update.</param>
        /// <param name="messageId">Queue message ID associated with the job.</param>
        /// <param name="message">Queue message associated with the job.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task SetQueueInfo(string route, string messageId, string message);

        /// <summary>
        /// Retrieves a <see cref="QueuedJobStatus"/> by route.
        /// </summary>
        /// <param name="route">Route of the <see cref="QueuedJobStatus"/> to retrieve.</param>
        /// <returns>a reference to the requested <see cref="QueuedJobStatus"/> or <c>null</c> if the item is not found.</returns>
        Task<QueuedJobStatus> ReadByRouteAsync(string route);

        /// <summary>
        /// Saves the specified <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The <see cref="QueuedJobStatus"/> to save.</param>
        /// <returns>a <see cref="QueuedJobStatus"/> item that includes any updates from the save operation.</returns>
        Task<QueuedJobStatus> SaveAsync(QueuedJobStatus item);

        /// <summary>
        /// Updates the status of a <see cref="QueuedJobStatus"/> record.
        /// </summary>
        /// <param name="route">Route of the item.</param>
        /// <param name="status">Status to save to the item.</param>
        /// <param name="errorMessage">(Optional) error message to save to the item when the status is  <see cref="JobStatusType.Failed"/>.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task UpdateStatusAsync(string route, JobStatusType status, string errorMessage = null);
    }
}