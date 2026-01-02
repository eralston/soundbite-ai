using Masticore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Service for CRUD operations on <see cref="Member"/> objects
    /// </summary>
    public interface IMemberService : IService
    {
        /// <summary>
        /// Asynchronously creates invitations to all the specified invitees.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the group belongs.</param>
        /// <param name="groupRoute">Route of the group/team for which invitations are being sent.</param>
        /// <param name="invites">Array of invitees to which invitations should be sent.</param>
        /// <returns>An array containing ids that can be used to locate the membership records of the invitees.</returns>
        Task<InviteResult[]> InviteAsync(string orgRoute, string groupRoute, Invite[] invites);

        /// <summary>
        /// Asynchronously gets a member record based on org/group/member routes.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the group belongs.</param>
        /// <param name="groupRoute">Route of the group/team to which the member belongs.</param>
        /// <param name="memberRoute">Route of the member record to retrieve.</param>
        /// <returns>the requested member record or <c>null</c> if the member record cannot be found.</returns>
        Task<Member> ReadAsync(string orgRoute, string groupRoute, string memberRoute);

        /// <summary>   
        /// Asynchronously gets a page of members in the specified group.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the group belongs.</param>
        /// <param name="groupRoute">Route of the group/team to which the members belong.</param>
        /// <param name="page">Identifies the page of data to acquire.</param>
        /// <returns>the requested page of member information.</returns>
        Task<IndexPageResponse<Member>> ReadAllAsync(string orgRoute, string groupRoute, IndexPageRequest page = null);

        /// <summary>
        /// Async read all <see cref="Member"/> objects for the given group
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<Member>> ReadAllAsync_Obsolete(string orgRoute, string groupRoute);

        /// <summary>
        /// Asynchronously gets a member record based on org/group/user routes.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the group belongs.</param>
        /// <param name="groupRoute">Route of the group/team to which the member belongs.</param>
        /// <param name="userRoute">Route of the user whose membership record is being sought.</param>
        /// <returns>the requested member record or <c>null</c> if the member record cannot be found.</returns>
        Task<Member> ReadByUserRouteAsync(string orgRoute, string groupRoute, string userRoute);

        /// <summary>
        /// Asyncronously reads the <see cref="MemberRole"/> for the given user in the given group in the given org
        /// </summary>
        /// <remarks>
        /// <see cref="MemberRole.Unknown"/> indicates the user in unaffiliated. Other values may simply imply the user is a high-level user in the org or system.
        /// </remarks>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="userRoute"></param>
        /// <returns></returns>
        Task<MemberRole> ReadRoleAsync(string orgRoute, string groupRoute, string userRoute);

        /// <summary>
        /// Asynchronously updates the given member in the given group in the given org to match the given <see cref="Member"/> values
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the group belongs.</param>
        /// <param name="groupRoute">Route of the group/team to which the member belongs.</param>
        /// <param name="memberRoute">Route of the member record to update.</param>
        /// <param name="memberFields">Member containing updated information to apply to the data store.</param>
        /// <returns>an updated Member record with the changes applied</returns>
        Task<Member> UpdateAsync(string orgRoute, string groupRoute, string memberRoute, Member memberFields);

        /// <summary>
        /// Deletes the given member from the given group in the given org
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the group belongs.</param>
        /// <param name="groupRoute">Route of the group/team to which the member belongs.</param>
        /// <param name="memberRoute">Route of the member record to delete.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        Task DeleteAsync(string orgRoute, string groupRoute, string memberRoute);
    }
}