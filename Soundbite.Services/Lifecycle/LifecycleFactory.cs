using Microsoft.Extensions.DependencyInjection;
using System;

namespace Soundbite.Services.Lifecycle
{
    /// <summary>
    /// Simple factory for registering and returning ILifecycleFactory objects
    /// </summary>
    public class LifecycleFactory : ILifecycleFactory
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly AnnouncementLifecycleStrategy _announcementStrategy;

        #endregion

        #region Construtor(s)

        /// <summary>
        /// Instantiates a new <see cref="LifecycleFactory"/> instance.
        /// </summary>
        /// <param name="serviceProvider">DI service provider reference.</param>
        public LifecycleFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Constructor created to ensure backwards compatibility with existing tests.
        /// </summary>
        /// <param name="announcement">AnnouncementLifecycleStrategy instance.</param>        
        public LifecycleFactory(AnnouncementLifecycleStrategy announcement)
        {
            _announcementStrategy = announcement;
        }

        #endregion

        #region ILifecycleFactory Implementation

        /// <inheritdoc />
        public ILifecycleStrategy StrategyForSession(SessionType sessionType)
        {
            switch (sessionType)
            {
                case SessionType.Announcement:
                    if (_announcementStrategy != null)
                    {
                        return _announcementStrategy;
                    }
                    return _serviceProvider.GetService<AnnouncementLifecycleStrategy>();
                default:
                    throw new NotImplementedException($"Session type '{sessionType}' does not have an ILifecycleStrategy implementation.");
            }
        }

        #endregion
    }
}