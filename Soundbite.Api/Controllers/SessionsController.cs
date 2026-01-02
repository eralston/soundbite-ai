using Masticore;
using Masticore.Models;
using Masticore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Models;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with sessions.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [ApiController]
    [Authorize]
    public class SessionsController : ControllerBase
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="ISessionService"/> object for this controller; specialized to CRUD actions on <see cref="SessionDetails"/> and similar objects
        /// </summary>
        protected ISessionService SessionService { get; }

        /// <summary>
        /// Gets the <see cref="ISessionFeedService"/> object for this controller; specialized to user-contextual collections of <see cref="SessionPreview"/> and similar objects
        /// </summary>
        protected ISessionFeedService SessionFeedService { get; }

        /// <summary>
        /// Gets the <see cref="IOrganizationService"/> object for this controller; specialized in CRUD and collection actions on <see cref="OrganizationWithSettings"/> and similar objects
        /// </summary>
        protected IOrganizationService OrgService { get; }

        /// <summary>
        /// Gets the <see cref="IAiService"/> object for this controller
        /// </summary>
        public IAiService AiService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionsController"/> class.
        /// </summary>
        /// <param name="sessionService"> DI reference.</param>
        /// <param name="sessionFeedService">ISessionService DI reference.</param>
        /// <param name="orgService">IOrganizationService DI reference.</param>
        /// <param name="aiService">IAiService DI reference.</param>
        public SessionsController(
            ISessionService sessionService,
            ISessionFeedService sessionFeedService,
            IOrganizationService orgService,
            IAiService aiService)
        {
            SessionService = sessionService ?? throw new System.ArgumentNullException(nameof(sessionService));
            SessionFeedService = sessionFeedService ?? throw new System.ArgumentNullException(nameof(sessionFeedService));
            OrgService = orgService ?? throw new System.ArgumentNullException(nameof(orgService));
            AiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Responsible for parsing a comma delimited string into <see cref="ParticipantRole"/>
        /// values.  This utility method exists to help manage passing arrays through the querystring.
        /// </summary>
        /// <param name="delimitedString">String containing delimited values.</param>
        /// <param name="delimiter">(Optional) delimiter to break apart values.</param>
        /// <returns>an array of values from the parsed string or <c>null</c> if the string is <c>null</c> or empty.</returns>
        private ParticipantRole[] ParseRoles(string delimitedString, string delimiter = ",")
        {
            ParticipantRole[] result = null;
            if (!string.IsNullOrEmpty(delimitedString))
            {
                result = delimitedString
                    .Split(delimiter)
                    .Select(i => i.Trim())
                    .Where(i => !string.IsNullOrEmpty(i))
                    .Select<string, ParticipantRole?>(i =>
                    {
                        if (System.Enum.TryParse(i, out ParticipantRole value))
                        {
                            return value;
                        }
                        else
                        {
                            return null;
                        }
                    })
                    .Where(i => i != null)
                    .Select(i => i.Value)
                    .ToArray();
            }
            return result;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Creates a new session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="newSession">Session to create.</param>
        /// <returns>a ClientOpWrapper containing session details about the newly created session.</returns>
        [Route("/organizations/{orgRoute}/Sessions")]
        [HttpPost]
        [Obsolete("Use the version without ClientOpWrapper")]
        public async Task<ClientOpWrapper<SessionDetails>> CreateAsync_Obsolete(string orgRoute, [FromBody] NewSession newSession)
        {
            SessionDetails result = await SessionService.CreateAsync(orgRoute, newSession);

            return new ClientOpWrapper<SessionDetails>(result);
        }

        /// <summary>
        /// Async create a new session from the given description
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="newSession"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/New")]
        [HttpPost]
        public async Task<SessionDetails> CreateAsync(string orgRoute, [FromBody] NewSession newSession)
        {
            SessionDetails result = await SessionService.CreateAsync(orgRoute, newSession);
            return result;
        }

        /// <summary>
        /// Gets a list of all sessions in the specified organization to which the user has access, 
        /// including sessions outside of the user feed that are visible to the user due to 
        /// administrative rights.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose sessions should be retrieved.</param>
        /// <returns>a list of all sessions in the organization to which the user has access.</returns>
        [Route("/organizations/{orgRoute}/Sessions")]
        [HttpGet]
        [Obsolete("Need to implement paged version")]
        public async Task<IEnumerable<Session>> ReadAllAsync(string orgRoute)
        {
            return await SessionService.ReadAllAsync_Obsolete(orgRoute);
        }

        /// <summary>
        /// Gets a paging collection of <see cref="SessionPreview"/> relevant to the given org for the current user
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/Pending")]
        [HttpGet]
        public async Task<IndexPageResponse<SessionPreview>> ReadPendingAsync(string orgRoute, [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page)
        {
            return await SessionFeedService.ReadPendingAsync(orgRoute, page);
        }

        /// <summary>
        /// Gets a paging collection of <see cref="SessionPreview"/> objects relevant to the given org for the current user
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/Past")]
        [HttpGet]
        public async Task<IndexPageResponse<SessionPreview>> ReadPastAsync(string orgRoute, [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page)
        {
            return await SessionFeedService.ReadPastAsync(orgRoute, page);
        }

        /// <summary>
        /// Gets a list containing the "feed" of sessions for the current user in the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose feed should be retrieved.</param>
        /// <returns>a list containing the "feed" of sessions for the user in the specified organization.</returns>
        [Route("/organizations/{orgRoute}/Feed")]
        [HttpGet]
        [Obsolete("Need to implement paged version")]
        public async Task<IEnumerable<SessionPreview>> ReadFeedAsync(string orgRoute)
        {
            return await SessionService.ReadFeedAsync_Obsolete(orgRoute);
        }

        /// <summary>
        /// Gets a list containing the public feed of sessions for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose public feed should be retrieved.</param>
        /// <returns>a list containing the public feed of sessions in the specified organization.</returns>
        [AllowAnonymous]
        [Route("/organizations/{orgRoute}/Public/Feed")]
        [HttpGet]
        [Obsolete("Need to implement paged version")]
        public async Task<IEnumerable<SessionPreview>> ReadPublicFeedAsync(string orgRoute)
        {
            return await SessionService.ReadPublicFeedAsync_Obsolete(orgRoute);
        }

        /// <summary>
        /// Gets a list of all sessions in the specified team to which the user has access, 
        /// including sessions outside of the user feed that are visible to the user due to 
        /// administrative rights.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the team belongs.</param>
        /// <param name="groupRoute">Route of the team whose sessions should be retrieved.</param>
        /// <returns>a list of all sessions in the specified team to which the user has access.</returns>

        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Sessions")]
        [HttpGet]
        [Obsolete("Need to implement paged version")]
        public async Task<IEnumerable<Session>> ReadAllGroupSessionsAsync(string orgRoute, string groupRoute)
        {
            return await SessionService.ReadAllAsync_Obsolete(orgRoute, groupRoute);
        }

        /// <summary>
        /// Gets a paging collection of still waiting to be published <see cref="SessionPreview"/> objects relevant to the given group in the given org for the current user
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Groups/{groupRoute}/Sessions/Pending")]
        [HttpGet]
        public async Task<IndexPageResponse<SessionPreview>> ReadGroupPendingAsync(string orgRoute, string groupRoute, [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page)
        {
            return await SessionFeedService.ReadPendingAsync(orgRoute, groupRoute, page);
        }

        /// <summary>
        /// Reads a paging collection of already published <see cref="SessionPreview"/> objects relevant to the given group in the given org for the current user
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Groups/{groupRoute}/Sessions/Past")]
        [HttpGet]
        public async Task<IndexPageResponse<SessionPreview>> ReadGroupPastAsync(string orgRoute, string groupRoute, [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page)
        {
            return await SessionFeedService.ReadPublishedAsync(orgRoute, groupRoute, page);
        }

        /// <summary>
        /// Gets a list containing the "feed" of sessions for the user in the specified team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the team.</param>
        /// <param name="groupRoute">Route of the team whose feed should be retrieved.</param>
        /// <returns>a list containing the "feed" of sessions for the user in the specified organization.</returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Feed")]
        [HttpGet]
        [Obsolete("Need to implement paged version")]
        public async Task<IEnumerable<SessionPreview>> ReadGroupFeedAsync(string orgRoute, string groupRoute)
        {
            return await SessionService.ReadFeedAsync_Obsolete(orgRoute, groupRoute);
        }

        /// <summary>
        /// Gets a list containing the "feed" of sessions for the user in the specified team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the team.</param>
        /// <param name="groupRoute">Route of the team whose feed should be retrieved.</param>
        /// <returns>a list containing the "feed" of sessions for the user in the specified organization.</returns>
        [Route("/Organizations/{orgRoute}/Public/Groups/{groupRoute}/Feed")]
        [HttpGet]
        [Obsolete("Need to implement paged version")]
        public async Task<IEnumerable<SessionPreview>> ReadGroupPublicFeedAsync(string orgRoute, string groupRoute)
        {
            return await SessionService.ReadPublicFeedAsync_Obsolete(orgRoute, groupRoute);
        }

        /// <summary>
        /// Reads the recently published sessions for the given org; paged by most recent publishing date descending (newest on top)
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/Published")]
        [HttpGet]
        [Obsolete("Need to implement paged version")]
        public async Task<IEnumerable<SessionPreview>> ReadRecentlyPublishedAsync(string orgRoute, int skip = 0, int take = 10)
        {
            return await SessionService.ReadRecentlyPublishedAsync_Obsolete(orgRoute, skip, take);
        }

        /// <summary>
        /// Retrieves the session details for the specified session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to retrieve.</param>
        /// <returns>session details for the requested session if found, otherwise throws an HTTP 404 not found exception.</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}")]
        [HttpGet]
        public async Task<SessionDetails> ReadAsync(string orgRoute, string sessionRoute)
        {
            // Allow people to pass organization universal ID using "." in front of it.
            if (orgRoute?.StartsWith(".") == true)
            {
                orgRoute = await OrgService.GetRouteFromUniversalId(orgRoute[1..]);
            }
            return await SessionService.ReadAsync(orgRoute, sessionRoute);
        }

        /// <summary>
        /// Read the <see cref="ReactionSummary"/> objects for the given session in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/reactions")]
        [HttpGet]
        public async Task<ReactionSummary[]> ReadReactionsAsync(string orgRoute, string sessionRoute)
        {
            return await SessionService.ReadReactionsAsync(orgRoute, sessionRoute);
        }

        /// <summary>
        /// Generates a <see cref="SessionSummary"/> for the given session based on AI analysis of the session's transcript content
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        /// <remarks>This does require a transcript to work, so if it doesn't have one or it's not ready, this returns null</remarks>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/summary")]
        [HttpGet]
        public async Task<SessionSummary> SummaryAsync(string orgRoute, string sessionRoute, SessionSummaryType summaryType = SessionSummaryType.Paragraph)
        {
            return await AiService.SessionSummaryAsync(orgRoute, sessionRoute, summaryType);
        }

        /// <summary>
        /// Retrieves the session details for the specified session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to retrieve.</param>
        /// <returns>session details for the requested session if found, otherwise throws an HTTP 404 not found exception.</returns>
        [AllowAnonymous]
        [Route("/organizations/{orgRoute}/Public/Sessions/{sessionRoute}")]
        [HttpGet]
        public async Task<SessionDetails> ReadPublicAsync(string orgRoute, string sessionRoute)
        {
            // Allow people to pass organization universal ID using "." in front of it.            
            return await SessionService.ReadPublicAsync(orgRoute, sessionRoute);
        }

        /// <summary>
        /// Updates the specified session.  
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to update.</param>
        /// <param name="session">Updated session information to store.</param>
        /// <returns>a session detail instance populated with updated values.</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}")]
        [HttpPut]
        public async Task<SessionDetails> UpdateAsync(string orgRoute, string sessionRoute, [FromBody] NewSession session)
        {
            session.Route = sessionRoute;
            return await SessionService.UpdateAsync(orgRoute, session);
        }

        /// <summary>
        /// Updates the state of all participants in the session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session whose participants are being updated.</param>
        /// <param name="partUpdate">State and role data indicating the state to update participants and what role (if any) to limit the updates.</param>        
        /// <returns>an HTTP response indicating success or failure of the operation.</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/state")]
        [HttpPut]
        public async Task UpdateStateAsync(string orgRoute, string sessionRoute, [FromBody] ParticipantStateUpdate partUpdate)
        {
            await SessionService.UpdateStateAsync(orgRoute, sessionRoute, partUpdate.ParticipantState, partUpdate.ParticipantRole);
        }

        /// <summary>
        /// Updates the reaction for the given session for the current user
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="reactionType"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/reaction")]
        [HttpPut]
        public async Task UpdateReactionAsync(string orgRoute, string sessionRoute, [FromBody] ParticipantReactionType reactionType = ParticipantReactionType.None)
        {
            await SessionService.UpdateReactionAsync(orgRoute, sessionRoute, reactionType);
        }

        /// <summary>
        /// Anonymous PUT to apply the <see cref="ParticipantState.Consumed"/> state to the given session for the given user; doing nothing if not found
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="userRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/public/Sessions/{sessionRoute}/acknowledge")]
        [HttpPut]
        [AllowAnonymous]
        public async Task AcknowledgePublicSession(string orgRoute, string sessionRoute, [CodeGenField(IsNullable = true)][FromQuery] string userRoute = null)
        {
            if (!string.IsNullOrEmpty(userRoute))
            {
                await SessionService.AcknowledgePublicSession(orgRoute, sessionRoute, userRoute);
            }
        }

        /// <summary>
        /// Performs a "soft" delete of the session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to "soft" delete.</param>
        /// <returns>an HTTP response indicating success or failure of the operation.</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}")]
        [HttpDelete]
        public async Task DeleteAsync(string orgRoute, string sessionRoute)
        {
            await SessionService.DeleteAsync(orgRoute, sessionRoute);
        }

        /// <summary>
        /// Sets the ReminderCalendarEventId property on a session.  This value is often identified
        /// on the client-side and needs to be sent back to Soundbite for storage.
        /// </summary>
        /// <param name="orgRoute">Route identifying the organization containing the session.</param>
        /// <param name="sessionRoute">Route identifying the session.</param>
        /// <param name="reminderCalendarEventId">Provider-specific calendar event ID to associate with the session.</param>
        /// <returns>HTTP response indicating success or failure of the operation.</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/SetReminderCalendarEntryId")]
        [HttpPost]
        public async Task SetReminderCalendarEntryId(string orgRoute, string sessionRoute, [FromBody] string reminderCalendarEventId)
        {
            await SessionService.SetReminderCalendarEntryId(orgRoute, sessionRoute, reminderCalendarEventId);
        }

        /// <summary>
        /// Gets a paged resultset containing the members of a group associated with a session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the session resides.</param>
        /// <param name="sessionRoute">Route of the session with which the group is associated.</param>
        /// <param name="includeRoles">Specifies which roles to include in the results. A null or empty array returns all roles.</param>
        /// <param name="page">Specifies which page of data to retrieve.</param>
        /// <returns>a page of participant information</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/Participants/Page")]
        [HttpGet]
        public async Task<IndexPageResponse<Participant>> ReadParticipants(string orgRoute, string sessionRoute,
            [FromQuery][CodeGenField(IsNullable = true)] string includeRoles = null,
            [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page = null)
        {
            ParticipantRole[] includeRolesParsed = ParseRoles(includeRoles);
            IndexPageResponse<Participant> result = await SessionService.ReadParticipants(orgRoute, sessionRoute, includeRolesParsed, page);
            return result;
        }

        /// <summary>
        /// Gets a paged resultset containing the members of a group associated with a session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the session resides.</param>
        /// <param name="sessionRoute">Route of the session with which the group is associated.</param>
        /// <param name="groupRoute">Route of the group whose members are being sought.</param>
        /// <param name="includeRoles">Specifies which roles to include in the results. A null or empty array returns all roles.</param>
        /// <param name="page">Specifies which page of data to retrieve.</param>
        /// <returns>a page of group participant information</returns>
        [Route("/organizations/{orgRoute}/Sessions/{sessionRoute}/Participants/Groups/{groupRoute}/Page")]
        [HttpGet]
        public async Task<IndexPageResponse<Participant>> ReadPariticpantsInGroup(string orgRoute, string sessionRoute, string groupRoute,
            [FromQuery][CodeGenField(IsNullable = true)] string includeRoles,
            [FromQuery][CodeGenField(IsNullable = true)] IndexPageRequest page = null)
        {
            ParticipantRole[] includeRolesParsed = ParseRoles(includeRoles);
            IndexPageResponse<Participant> result = await SessionService.ReadParticipants(orgRoute, sessionRoute, groupRoute, includeRolesParsed, page);
            return result;
        }

        #endregion        
    }
}