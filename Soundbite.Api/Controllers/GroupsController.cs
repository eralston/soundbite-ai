using Masticore;
using Masticore.Models;
using Masticore.Resources;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with sessions.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        #region Fields

        private IGroupService Groups { get; }
        private ISbOrganizationService Orgs { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupService"></param>
        /// <param name="orgService"></param>
        public GroupsController(
            IGroupService groupService,
            ISbOrganizationService orgService)
        {
            Groups = groupService ?? throw new System.ArgumentNullException(nameof(groupService));
            Orgs = orgService ?? throw new System.ArgumentNullException(nameof(orgService));
        }

        #endregion

        #region EndPoints


        /// <summary>
        /// Async read all <see cref="Group"/> objects for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Groups")]
        [HttpGet]
        public async Task<IEnumerable<Group>> ReadAllGroupsAsync(string orgRoute)
        {
            return await Groups.ReadAllAsync_Obsolete(orgRoute);
        }

        /// <summary>
        /// Async read all <see cref="Group"/> objects for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Me/Groups")]
        [HttpGet]
        public async Task<IEnumerable<Group>> ReadMyGroupsAsync(string orgRoute)
        {
            return await Groups.ReadMyAsync(orgRoute);
        }

        /// <summary>
        /// Async read all <see cref="Group"/> objects for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Me/Groups/Targets")]
        [HttpGet]
        public async Task<IEnumerable<Group>> ReadMyTargetGroupsAsync(string orgRoute)
        {
            // We will dynamically determine the role necessary to create a session if a group, but only if needed
            Func<Task<MemberRole>> roleFunc = async () => { return (await Orgs.ReadPermissions(orgRoute, true)).MinRoleToCreateTeamSession; };
            return await Groups.ReadMyTargetsAsync(orgRoute, roleFunc);
        }

        /// <summary>
        /// Creates a new team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which to create the team.</param>
        /// <param name="newGroup">Team to create.</param>
        /// <returns>an IGroupDetail instance populated with the newly created team info.</returns>
        [Route("/Organizations/{orgRoute}/Groups/")]
        [HttpPost]
        public async Task<GroupDetails> CreateAsync(string orgRoute, [FromBody] NewGroup newGroup)
        {
            return await Groups.CreateAsync(orgRoute, newGroup);
        }

        /// <summary>
        /// Retrieves the specified team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the team resides.</param>
        /// <param name="groupRoute">Route of the team to retrieve.</param>
        /// <returns>the requested team if found, otherwise throws an HTTP 404 not found exception.</returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}")]
        [HttpGet]
        public async Task<GroupDetails> ReadAsync(string orgRoute, string groupRoute)
        {
            return await Groups.ReadAsync(orgRoute, groupRoute);
        }

        /// <summary>
        /// Performs a "soft" delete of the team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the team resides.</param>
        /// <param name="groupRoute">Route of the team to delete.</param>
        /// <returns>an HTTP response indicating success or failure of the operation.</returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}")]
        [HttpDelete]
        public async Task DeleteAsync(string orgRoute, string groupRoute)
        {
            await Groups.DeleteAsync(orgRoute, groupRoute);
        }

        /// <summary>
        /// Patches the specified team according to the patch instructions.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the team belongs.</param>
        /// <param name="groupRoute">Route of the team to update.</param>
        /// <param name="changes">Patch instructions identifying what changes to apply to the organization.</param>
        /// <returns>an ITeamDetails instance populated with the updated team information.</returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}")]
        [HttpPatch]
        public async Task<GroupDetails> PatchAsync(string orgRoute, string groupRoute, [FromBody] JsonPatchDocument changes)
        {
            //TODO: need to actually return a TeamDetail from this method
            await Groups.PatchAsync(orgRoute, groupRoute, changes);
            return null;
        }

        #endregion
    }
}