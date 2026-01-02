using Masticore.Models;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using System.Threading.Tasks;

namespace Soundbite.Services.Lifecycle
{
    /// <summary>
    /// Base class for building Lifecycle Strategy implementations.
    /// </summary>
    public abstract class LifecycleStrategyBase : ILifecycleStrategy
    {
        /// <inheritdoc />
        public ILogger Logger { get; set; }

        /// <inheritdoc />
        public virtual Task BeforeCreateSessionAsync(SbDb db, SessionEntity session) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task AfterCreateSessionAsync(SbDb db, SessionEntity session) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task BeforeCreateSeriesAsync(SbDb db, SeriesEntity series) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task AfterCreateSeriesAsync(SbDb db, SeriesEntity series) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task BeforeProcessSeriesAsync(SbDb db, SeriesEntity series) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task AfterProcessSeriesAsync(SbDb db, SeriesEntity series) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task BeforeCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, NewClip newClip) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task AfterCreateClipAsync(SbDb db, Organization org, SessionEntity session, ParticipantEntity participant, ClipEntity newClip) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task BeforeClipTranscribed(SbDb db, Organization org, SessionEntity session, ClipEntity clip) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task AfterClipTranscribed(SbDb db, Organization org, SessionEntity session, ClipEntity clip) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task BeforeClipMediaProcessing(SbDb db, Organization org, SessionEntity session, ClipEntity clip) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task AfterClipMediaProcessing(SbDb db, Organization org, SessionEntity session, ClipEntity clip) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task AfterClipOperationComplete(SbDb sbDb, string clipOpRoute) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task RemindAsync(SbDb db, Organization org, SessionEntity session) { return Task.CompletedTask; }

        /// <inheritdoc />
        public virtual Task PublishAsync(SbDb db, Organization org, SessionEntity session) { return Task.CompletedTask; }
    }
}
