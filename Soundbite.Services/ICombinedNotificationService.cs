using Masticore.Services;

namespace Soundbite.Services
{
    /// <summary>
    /// Interface that encompasses notifications services that serve as both an <see cref="INotificationService"/>
    /// and an <see cref="ISbNotificationService"/> implementation.
    /// </summary>
    public interface ICombinedNotificationService : INotificationService, ISbNotificationService
    {
    }
}