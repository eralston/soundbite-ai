using Masticore.Models;
using System.Threading.Tasks;

namespace Masticore.Services.Tests
{
    /// <summary>
    /// Counts the number of notifications called
    /// </summary>
    public class MockNotifications : INotificationService
    {
        public int Count { get; set; } = 0;

        public Task<bool> UserInviteAsync(User receiver, User sender)
        {
            Count++;
            return Task.FromResult(true);
        }

        public Task<bool> PersonInviteAsync(User receiver, User sender, Organization org, bool isAppInvite)
        {
            Count++;
            return Task.FromResult(true);
        }

        public Task<bool> MemberInviteAsync(User receiver, User sender, Organization org, Group team, bool isAppInvite)
        {
            Count++;
            return Task.FromResult(true);
        }

        public Task<bool> WelcomeAsync(User joiner)
        {
            Count++;
            return Task.FromResult(true);
        }
    }
}
