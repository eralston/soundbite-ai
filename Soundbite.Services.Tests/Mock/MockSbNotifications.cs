using Masticore.Models;
using Masticore.Services.Tests;
using Soundbite.Entity;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    /// <summary>
    /// Counts the number of notifications called
    /// </summary>
    public class MockSbNotifications : MockNotifications, ISbNotificationService
    {
        public void Reset() { Count = 0; }

        public Task<bool> SessionHostPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            Count++;
            return Task.FromResult(true);
        }

        public Task<bool> SessionPublishAsync(User receiver, Organization org, SessionEntity session)
        {
            Count++;
            return Task.FromResult(true);
        }

        public Task<bool> SessionReminderAsync(User receiver, Organization org, SessionEntity session)
        {
            Count++;
            return Task.FromResult(true);
        }
    }
}
