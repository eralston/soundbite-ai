using Masticore;
using Masticore.Models;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with Members.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class MembersController : ControllerBase
    {
        #region Properties

        private IMemberService MemberService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MembersController"/> class.
        /// </summary>
        /// <param name="memberService">IMemberService DI reference.</param>
        public MembersController(IMemberService memberService)
        {
            MemberService = memberService;
        }

        #endregion

        #region EndPoints

        /// <summary>
        /// Async read all <see cref="Member"/> objects for the given group in the given org
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the group resides.</param>
        /// <param name="groupRoute">Route of the group whose members are being requested.</param>
        /// <returns>A list containing the members in the specified group.</returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Members/")]
        [HttpGet]
        [Obsolete]
        public async Task<IEnumerable<Member>> ReadAllAsync_Obsolete(string orgRoute, string groupRoute)
        {
            return await MemberService.ReadAllAsync_Obsolete(orgRoute, groupRoute);
        }

        /// <summary>
        /// Query for the org's list of people in a paged manner
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the group resides.</param>
        /// <param name="groupRoute">Route of the group whose members are being requested.</param>
        /// <param name="pageRequest">Determine by picking apart the querystring into associated paging parameters</param>
        /// <returns>A page of the list of members in the specified group.</returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Members/Page")]
        [HttpGet]
        public async Task<IndexPageResponse<Member>> ReadAllAsync(string orgRoute, string groupRoute, [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest pageRequest)
        {
            return await MemberService.ReadAllAsync(orgRoute, groupRoute, pageRequest);
        }

        /// <summary>
        /// Invites one or more users to a team, sending notifications to the new users that they have been added.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the team belongs.</param>
        /// <param name="groupRoute">Route of the team.</param>
        /// <param name="invites">One or more invitations identifying the people to invite to the team; each token is either a Person route OR an email address.</param>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Members/")]
        [HttpPost]
        public async Task InviteAsync(string orgRoute, string groupRoute, [FromBody] Invite[] invites)
        {
            await MemberService.InviteAsync(orgRoute, groupRoute, invites);
        }

        /// <summary>
        /// Updates a member
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the member exists.</param>
        /// <param name="groupRoute">Route of the team to which the member is associated.</param>
        /// <param name="memberRoute">Route of the member.</param>
        /// <param name="member">Member information to persist.</param>
        /// <returns>a member with updated member information</returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Members/{memberRoute}")]
        [HttpPatch]
        public async Task<Member> UpdateAsync(string orgRoute, string groupRoute, string memberRoute, [FromBody] Member member)
        {
            return await MemberService.UpdateAsync(orgRoute, groupRoute, memberRoute, member);
        }

        /// <summary>
        /// Performs a "soft" delete of the member removing them from the team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the team belongs.</param>
        /// <param name="groupRoute">Route of the team to which the member is associated.</param>
        /// <param name="memberRoute">Route of the member to remove.</param>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Members/{memberRoute}")]
        [HttpDelete]
        public async Task DeleteAsync(string orgRoute, string groupRoute, string memberRoute)
        {
            await MemberService.DeleteAsync(orgRoute, groupRoute, memberRoute);
        }

        #endregion
    }
}
