using Masticore.Services;
using Soundbite.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the service operations required to manage series of sessions.
    /// </summary>
    public interface ISeriesService : IService
    {
        /// <summary>
        /// Creates a <see cref="Session"/> object
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="newSession"></param>
        /// <returns></returns>
        Task<SeriesDetails> CreateAsync(string orgRoute, NewSession newSession);

        /// <summary>
        /// Read a list of all series relevant to the current user in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<SeriesPreview>> ReadAllAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Read a list of all series relevant to the current user in the given group in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<SeriesPreview>> ReadAllAsync_Obsolete(string orgRoute, string groupRoute);

        /// <summary>
        /// Read details on the given series in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="seriesRoute"></param>
        /// <returns></returns>
        Task<SeriesDetails> ReadAsync(string orgRoute, string seriesRoute);

        /// <summary>
        /// Overwrite the given fields for the given series in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="seriesRoute"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        Task<SeriesDetails> UpdateAsync(string orgRoute, string seriesRoute, NewSession session);

        /// <summary>
        /// Soft delete the given series in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="seriesRoute"></param>
        /// <returns></returns>
        Task DeleteAsync(string orgRoute, string seriesRoute);
    }
}
