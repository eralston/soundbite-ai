using Masticore.Models;
using Masticore.Services;
using Soundbite.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the service operations required to manage sessions.
    /// </summary>
    public interface ISessionService : IService
    {
        /// <summary>
        /// Acknowledges a public session for the specified user.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the public session belongs.</param>
        /// <param name="sessionRoute">Route of the public session being acknowledged.</param>
        /// <param name="userRoute">Route of the user acknowledging the public session.</param>
        /// <returns>a task indicating completion of the operation.</returns>
        Task AcknowledgePublicSession(string orgRoute, string sessionRoute, string userRoute);

        /// <summary>
        /// Creates a new session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="session">Session to create.</param>
        /// <returns>a ClientOpWrapper containing session details about the newly created session.</returns>
        Task<SessionDetails> CreateAsync(string orgRoute, NewSession session);

        /// <summary>
        /// Gets a list of all sessions in the specified organization to which the user has access, 
        /// including sessions outside of the user feed that are visible to the user due to 
        /// administrative rights.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose sessions should be retrieved.</param>
        /// <returns>a list of all sessions in the organization to which the user has access.</returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<Session>> ReadAllAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Gets a list of all sessions in the specified team to which the user has access, 
        /// including sessions outside of the user feed that are visible to the user due to 
        /// administrative rights.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the team belongs.</param>
        /// <param name="groupRoute">Route of the group whose sessions should be retrieved.</param>
        /// <returns>a list of all sessions in the specified team to which the user has access.</returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<Session>> ReadAllAsync_Obsolete(string orgRoute, string groupRoute);

        /// <summary>
        /// Gets a list containing the "feed" of sessions for the user in the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose feed should be retrieved.</param>
        /// <returns>a list containing the "feed" of sessions for the user in the specified organization.</returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<SessionPreview>> ReadFeedAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Gets a list containing the "feed" of sessions for the user in the specified team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the team.</param>
        /// <param name="groupRoute">Route of the team whose feed should be retrieved.</param>
        /// <returns>a list containing the "feed" of sessions for the user in the specified organization.</returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<SessionPreview>> ReadFeedAsync_Obsolete(string orgRoute, string groupRoute);

        /// <summary>
        /// Gets a list containing the public feed of sessions for the specified organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose feed should be retrieved.</param>
        /// <returns>a list containing the "feed" of sessions for the user in the specified organization.</returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<SessionPreview>> ReadPublicFeedAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Gets a list containing the public feed of sessions for the specified team.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the team.</param>
        /// <param name="groupRoute">Route of the team whose feed should be retrieved.</param>
        /// <returns>a list containing the "feed" of sessions for the user in the specified organization.</returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<SessionPreview>> ReadPublicFeedAsync_Obsolete(string orgRoute, string groupRoute);

        /// <summary>
        /// Gets the paged set of recently published session paged by the publish date descending
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<SessionPreview>> ReadRecentlyPublishedAsync_Obsolete(string orgRoute, int skip = 0, int take = 10);

        /// <summary>
        /// Retrieves the session details for the specified session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to retrieve.</param>
        /// <returns>session details for the requested session if found, otherwise throws an HTTP 404 not found exception.</returns>
        Task<SessionDetails> ReadAsync(string orgRoute, string sessionRoute);

        /// <summary>
        /// Read the reaction data for the given session
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        Task<ReactionSummary[]> ReadReactionsAsync(string orgRoute, string sessionRoute);

        /// <summary>
        /// Retrieves the session details for the specified publicly available session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to retrieve.</param>
        /// <returns>session details for the requested session if found, otherwise throws an HTTP 404 not found exception.</returns>
        Task<SessionDetails> ReadPublicAsync(string orgRoute, string sessionRoute);

        /// <summary>
        /// Retrieves a flag indicating whether the organization has any active public notifications.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to check for active public notifications.</param>
        /// <returns><c>true</c> if the organization has active public notifications, otherwise <c>false</c>.</returns>
        Task<bool> HasPublicNotifications(string orgRoute);

        /// <summary>
        /// Updates the specified session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="session">Updated session information to store.</param>
        /// <returns>an organization populated with updated values.</returns>
        Task<SessionDetails> UpdateAsync(string orgRoute, NewSession session);

        /// <summary>
        /// Updates the state of all participants in the session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session whose participants are being updated.</param>
        /// <param name="state">State value to assign to partipants.</param>
        /// <param name="role">Optional. When specified the state is only updated for particpants that match the specified role.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        Task UpdateStateAsync(string orgRoute, string sessionRoute, ParticipantState state, ParticipantRole role);

        /// <summary>
        /// Async update the <see cref="ParticipantReactionType"/> for the current user; must have access to the session
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="reaction"></param>
        /// <returns></returns>
        Task UpdateReactionAsync(string orgRoute, string sessionRoute, ParticipantReactionType reaction);

        /// <summary>
        /// Performs a "soft" delete of the session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization to which the session belongs.</param>
        /// <param name="sessionRoute">Route of the session to "soft" delete.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        Task DeleteAsync(string orgRoute, string sessionRoute);

        /// <summary>
        /// Sets the ReminderCalendarEventId property on a session.  This value is often identified
        /// on the client-side and needs to be sent back to Soundbite for storage.
        /// </summary>
        /// <param name="orgRoute">Route identifying the organization containing the session.</param>
        /// <param name="sessionRoute">Route identifying the session.</param>
        /// <param name="reminderCalendarEventId">Provider-specific calendar event ID to associate with the session.</param>
        /// <returns>a task indicating completion or failure of the operation.</returns>
        Task SetReminderCalendarEntryId(string orgRoute, string sessionRoute, string reminderCalendarEventId);

        /// <summary>
        /// Gets a paged resultset containing all of the participants of a session.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the session resides.</param>
        /// <param name="sessionRoute">Route of the session with which the group is associated.</param>
        /// <param name="includeRoles">Specifies which roles to include in the results. A null or empty array returns all roles.</param>
        /// <param name="page">Specifies which page of data to retrieve.</param>
        /// <returns>a page of participant information</returns>
        Task<IndexPageResponse<Participant>> ReadParticipants(
            string orgRoute,
            string sessionRoute,
            ParticipantRole[] includeRoles = null,
            IndexPageRequest page = null);

        /// <summary>
        /// Gets a paged resultset containing participants associated to the session through the specified participant group.
        /// </summary>
        /// <param name="orgRoute">Route of the organization in which the session resides.</param>
        /// <param name="sessionRoute">Route of the session with which the group is associated.</param>
        /// <param name="participantGroupRoute">Route of the participant group whose members are being sought.</param>
        /// <param name="includeRoles">Specifies which roles to include in the results. A null or empty array returns all roles.</param>
        /// <param name="page">Specifies which page of data to retrieve.</param>
        /// <returns>a page of participant information</returns>
        Task<IndexPageResponse<Participant>> ReadParticipants(
            string orgRoute,
            string sessionRoute,
            string participantGroupRoute,
            ParticipantRole[] includeRoles = null,
            IndexPageRequest page = null);
    }
}
