using Masticore.Models;
using Soundbite.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    /// <summary>
    /// Fake <see cref="ISessionService"/>
    /// </summary>
    public class MockSessionService : ISessionService
    {
        public Task AcknowledgePublicSession(string orgRoute, string sessionRoute, string userRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<SessionDetails> CreateAsync(string orgRoute, NewSession session)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(string orgRoute, string sessionRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> HasPublicNotifications(string orgRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Session>> ReadAllAsync_Obsolete(string orgRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Session>> ReadAllAsync_Obsolete(string orgRoute, string groupRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<SessionDetails> ReadAsync(string orgRoute, string sessionRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<SessionDetails> ReadPublicAsync(string orgRoute, string sessionRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<SessionPreview>> ReadFeedAsync_Obsolete(string orgRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<SessionPreview>> ReadFeedAsync_Obsolete(string orgRoute, string groupRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<SessionPreview>> ReadPublicFeedAsync_Obsolete(string orgRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<SessionPreview>> ReadPublicFeedAsync_Obsolete(string orgRoute, string groupRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<SessionPreview>> ReadRecentlyPublishedAsync_Obsolete(string orgRoute, int skip = 0, int take = 10)
        {
            throw new System.NotImplementedException();
        }

        public Task SetReminderCalendarEntryId(string orgRoute, string sessionRoute, string reminderCalendarEventId)
        {
            throw new System.NotImplementedException();
        }

        public Task<SessionDetails> UpdateAsync(string orgRoute, NewSession session)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateStateAsync(string orgRoute, string sessionRoute, ParticipantState state, ParticipantRole role)
        {
            throw new System.NotImplementedException();
        }

        public Task<IndexPageResponse<Participant>> ReadParticipants(string orgRoute, string sessionRoute, ParticipantRole[] includeRoles, IndexPageRequest page)
        {
            throw new System.NotImplementedException();
        }

        public Task<IndexPageResponse<Participant>> ReadParticipants(string orgRoute, string sessionRoute, string participantGroupRoute, ParticipantRole[] includeRoles, IndexPageRequest page)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateReactionAsync(string orgRoute, string sessionRoute, ParticipantReactionType reaction)
        {
            throw new System.NotImplementedException();
        }

        public Task<ReactionSummary[]> ReadReactionsAsync(string orgRoute, string sessionRoute)
        {
            throw new System.NotImplementedException();
        }
    }
}
