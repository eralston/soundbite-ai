using Masticore.Models;
using Masticore.Resources;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Service for groups
    /// </summary>
    public interface IGroupService : IService
    {
        /// <summary>
        /// Async create a group
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="newGroup"></param>
        /// <returns></returns>
        Task<GroupDetails> CreateAsync(string orgRoute, NewGroup newGroup);

        /// <summary>
        /// Async query for all groups in the given org; this is theoretically still limited by current user's roles
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<Group>> ReadAllAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Gets all of the <see cref="Group"/> object to which the current user belongs in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<IEnumerable<Group>> ReadMyAsync(string orgRoute);

        /// <summary>
        /// Gets all of the <see cref="Group"/> objects for which the current user can query
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="minRoleAction"></param>
        /// <returns></returns>
        Task<IEnumerable<Group>> ReadMyTargetsAsync(string orgRoute, Func<Task<MemberRole>> minRoleAction = null);

        /// <summary>
        /// Async reads the given group in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        Task<GroupDetails> ReadAsync(string orgRoute, string groupRoute);

        /// <summary>
        /// DO NOT USE!
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        [Obsolete("Deprecated for performance; use ReadAsync")]
        Task<GroupDetails_Obsolete> ReadAsync_Obsolete(string orgRoute, string groupRoute);

        /// <summary>
        /// Deletes the given group in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        Task DeleteAsync(string orgRoute, string groupRoute);

        /// <summary>
        /// Updates the given group in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="updates"></param>
        /// <returns></returns>
        Task<GroupDetails> PatchAsync(string orgRoute, string groupRoute, JsonPatchDocument updates);
    }
}