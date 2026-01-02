using Masticore;
using Masticore.Models;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockEmailTemplates : IEmailTemplates
    {
        public int Count { get; protected set; }

        public Task<Body> MemberInviteAsync(MemberInviteModel model)
        {
            Count++;
            return Task.FromResult(new Body());
        }

        public Task<Body> PersonInviteAsync(PersonInviteModel model)
        {
            Count++;
            return Task.FromResult(new Body());
        }


        public Task<Body> SessionHostPublishAsync(SessionPublishModel model)
        {
            Count++;
            return Task.FromResult(new Body());
        }

        public Task<Body> SessionPublishAsync(SessionPublishModel model)
        {
            Count++;
            return Task.FromResult(new Body());
        }

        public Task<Body> SessionReminderAsync(SessionModel model)
        {
            Count++;
            return Task.FromResult(new Body());
        }

        public Task<Body> UserInviteAsync(UserInviteModel invite)
        {
            Count++;
            return Task.FromResult(new Body());
        }

        public Task<Body> WelcomeAsync(User user)
        {
            Count++;
            return Task.FromResult(new Body());
        }
    }
}
