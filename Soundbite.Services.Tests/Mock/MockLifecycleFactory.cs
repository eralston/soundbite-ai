using Masticore.Models;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using Soundbite.Services.Lifecycle;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockLifecycleStrategy : ILifecycleStrategy
    {
        public ILogger Logger { get; set; }
        public int PreSessionCount { get; private set; } = 0;
        public int PostSessionCount { get; private set; } = 0;
        public int PreSeriesCount { get; private set; } = 0;
        public int PostSeriesCount { get; private set; } = 0;
        public int PreClipCount { get; private set; } = 0;
        public int PostClipCount { get; private set; } = 0;
        public int ReminderCount { get; private set; } = 0;
        public int PublishCount { get; private set; } = 0;
        public int PreProcessCount { get; private set; } = 0;
        public int PostProcessCount { get; private set; } = 0;
        public int PreClipMediaProcessingCount { get; private set; } = 0;
        public int PostClipMediaProcessingCount { get; private set; } = 0;
        public int AfterClipOperationCompleteProcessingCount { get; private set; } = 0;
        public int PreClipTranscriptionCount { get; private set; } = 0;
        public int PostClipTranscriptionCount { get; private set; } = 0;

        public Task BeforeCreateSessionAsync(SbDb db, SessionEntity session)
        {
            PreSessionCount++;
            return Task.CompletedTask;
        }

        public Task AfterCreateSessionAsync(SbDb db, SessionEntity session)
        {
            PostSessionCount++;
            return Task.CompletedTask;
        }

        public Task BeforeCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, NewClip newClip)
        {
            PreClipCount++;
            return Task.CompletedTask;
        }

        public Task AfterCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, ClipEntity newClip)
        {
            PostClipCount++;
            return Task.CompletedTask;
        }

        public Task RemindAsync(SbDb db, Organization org, SessionEntity session)
        {
            ReminderCount++;
            return Task.CompletedTask;
        }

        public Task PublishAsync(SbDb db, Organization org, SessionEntity session)
        {
            PublishCount++;
            return Task.CompletedTask;
        }

        public Task BeforeCreateSeriesAsync(SbDb db, SeriesEntity series)
        {
            PreSeriesCount++;
            return Task.CompletedTask;
        }

        public Task AfterCreateSeriesAsync(SbDb db, SeriesEntity series)
        {
            PostSeriesCount++;
            return Task.CompletedTask;
        }

        public Task BeforeProcessSeriesAsync(SbDb db, SeriesEntity series)
        {
            ++PreProcessCount;
            return Task.CompletedTask;
        }

        public Task AfterProcessSeriesAsync(SbDb db, SeriesEntity series)
        {
            ++PostProcessCount;
            return Task.CompletedTask;
        }

        public Task BeforeClipTranscribed(SbDb db, Organization org, SessionEntity session, ClipEntity clip)
        {
            PreClipTranscriptionCount++;
            return Task.CompletedTask;
        }

        public Task AfterClipTranscribed(SbDb db, Organization org, SessionEntity session, ClipEntity clip)
        {
            PostClipTranscriptionCount++;
            return Task.CompletedTask;
        }

        public Task BeforeClipMediaProcessing(SbDb db, Organization org, SessionEntity session, ClipEntity clip)
        {
            PreClipMediaProcessingCount++;
            return Task.CompletedTask;
        }

        public Task AfterClipMediaProcessing(SbDb db, Organization org, SessionEntity session, ClipEntity clip)
        {
            PostClipMediaProcessingCount++;
            return Task.CompletedTask;
        }

        public Task AfterClipOperationComplete(SbDb sbDb, string clipOpRoute)
        {
            AfterClipOperationCompleteProcessingCount++;
            return Task.CompletedTask;
        }
    }

    public class MockLifecycleFactory : ILifecycleFactory
    {
        public ILifecycleStrategy Strategy { get; }

        public MockLifecycleStrategy MockStrategy => Strategy as MockLifecycleStrategy;

        public MockLifecycleFactory(ILifecycleStrategy strategy = null)
        {
            Strategy = strategy ?? new MockLifecycleStrategy();
        }

        public ILifecycleStrategy StrategyForSession(SessionType sessionType)
        {
            return Strategy;
        }
    }
}
