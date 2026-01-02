namespace Soundbite.Services.Lifecycle
{
    /// <summary>
    /// Interfaces for an object that manages ILifecycleStrategy instances
    /// </summary>
    public interface ILifecycleFactory
    {
        ILifecycleStrategy StrategyForSession(SessionType sessionType);
    }
}
