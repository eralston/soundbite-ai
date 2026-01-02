using Masticore;
using Masticore.Models;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Expands <see cref="IndexPageRequest"/> to includes additional filter fields
    /// </summary>
    [CodeGenModel]
    public class ReadAllPeopleIndexPageRequest : IndexPageRequest
    {
        /// <summary>
        /// The exact person role for desired query results
        /// </summary>
        public PersonRole? PersonRole { get; set; }
    }

    /// <summary>
    /// Controller with endpoints for interacting with people (i.e. members of an organization).
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class PeopleController : ControllerBase
    {
        #region Properties

        private IPersonService PersonService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PeopleController"/> class.
        /// </summary>
        /// <param name="people">The people.</param>
        public PeopleController(IPersonService people)
        {
            PersonService = people;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Gets the current user's person record for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <returns>a person record for the current user if found, otherwise throws an HTTP 404 not found exception.</returns>
        [Route("/Organizations/{orgRoute}/me")]
        [HttpGet]
        public async Task<Person> ReadMeAsync(string orgRoute)
        {
            return await PersonService.ReadMeAsync(orgRoute);
        }

        /// <summary>
        /// Invites one or more user's to an organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which to invite users.</param>
        /// <param name="invites">One or more invitations identifying the users to invite to the organization.</param>
        /// <returns>a list of <see cref="InviteResult"/> instances with user and person route information identifying the person record linking the user to the organization.</returns>
        [Route("/Organizations/{orgRoute}/People/Invite")]
        [HttpPost]
        public async Task<InviteResult[]> InviteAsync(string orgRoute, [FromBody] Invite[] invites)
        {
            return await PersonService.InviteAsync(orgRoute, invites);
        }

        //TODO: The following method uses the HTTP Patch verb but isn't a patch -- probably should chagne to POST but I don't want to break antyhing right now

        /// <summary>
        /// Updates the person record associating a user to an organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the user is associated.</param>
        /// <param name="personRoute">Route of the person record to update.</param>
        /// <param name="personFields">Person record containing updates to store.</param>
        /// <returns>a Person instance containing update information (if found), otherwise throws an HTTP 404 not found exception</returns>
        [Route("/Organizations/{orgRoute}/People/{personRoute}")]
        [HttpPatch]
        public async Task<Person> UpdateAsync(string orgRoute, string personRoute, [FromBody] Person personFields)
        {
            return await PersonService.UpdateAsync(orgRoute, personRoute, personFields);
        }

        /// <summary>
        /// Query for the org's list of people in a paged manner
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="pageRequest">Determine by picking apart the querystring into associated paging parameters</param>
        /// <returns></returns>
        [Route("/Organizations/{orgRoute}/People/Page")]
        [HttpGet]
        public async Task<IndexPageResponse<Person>> ReadAllAsync(string orgRoute, [FromQuery][CodeGenField(IsNullable = true)] ReadAllPeopleIndexPageRequest pageRequest)
        {
            return await PersonService.ReadAllAsync(orgRoute, pageRequest, pageRequest.PersonRole);
        }

        /// <summary>
        /// Performs a "soft" delete of the person record associating a user to an organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization associated with the person record.</param>        
        /// <param name="personRoute">Route of the person record to "soft" delete.</param>
        [Route("/Organizations/{orgRoute}/People/{personRoute}")]
        [HttpDelete]
        public async Task Delete(string orgRoute, string personRoute)
        {
            await PersonService.DeleteAsync(orgRoute, personRoute);
        }

        #endregion
    }
}
