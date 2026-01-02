using Masticore.Models;
using System;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    /// <summary>
    /// Fake implementation of <see cref="ISessionCommentService"/>
    /// </summary>
    public class MockSessionCommentService : ISessionCommentService
    {
        /// <inheritdoc />
        public Task<SessionComment> CreateAsync(string orgRoute, string sessionRoute, NewSessionComment newComment)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IndexPageResponse<SessionComment>> ReadAllAsync(string orgRoute, string sessionRoute, IndexPageRequest pageRequest = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<SessionComment> UpdateAsync(string orgRoute, string sessionRoute, string commentRoute, NewSessionComment updatedComment)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteAsync(string orgRoute, string sessionRoute, string commentRoute)
        {
            throw new NotImplementedException();
        }
    }
}