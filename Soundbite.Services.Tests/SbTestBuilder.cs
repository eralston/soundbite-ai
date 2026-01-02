using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Providers;
using Microsoft.EntityFrameworkCore;
using Soundbite.Services;
using Soundbite.Services.Lifecycle;
using Soundbite.Services.Tests.Mock;

namespace Soundbite.Entity.Tests
{
    /// <summary>
    /// Creates mock objects in the Soundbite assemblies
    /// </summary>
    /// <typeparam name="TInfrastructure"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class SbTestBuilder<TInfrastructure, TDbContext>
        : Masticore.Entity.Tests.TestBuilder<TInfrastructure, TDbContext>
        where TDbContext : DbContext
        where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
    {
        #region Constructor

        public SbTestBuilder(TestUserContext userContext)
            : this()
        {
            SetCurrentUserContext(userContext).Wait();
        }

        public SbTestBuilder() : base()
        {
            ClipFileService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IClipFileService, TInfrastructure, TDbContext>(this, i => new MockClipFileService());
            ClipOperationService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IClipOperationService, TInfrastructure, TDbContext>(this, i => new MockClipOperationService());
            ClipService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IClipService, TInfrastructure, TDbContext>(this);
            LifeCycleFactory = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ILifecycleFactory, TInfrastructure, TDbContext>(this, i => new MockLifecycleFactory());
            ProviderFactory = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IProviderFactory, TInfrastructure, TDbContext>(this, i => new ProviderFactory());
            SbOrganizationService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISbOrganizationService, TInfrastructure, TDbContext>(this);
            SeriesService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISeriesService, TInfrastructure, TDbContext>(this, i => new MockSeriesService());
            SessionFeedService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISessionFeedService, TInfrastructure, TDbContext>(this);
            SessionService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISessionService, TInfrastructure, TDbContext>(this, i => new MockSessionService());
            SessionCommentService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISessionCommentService, TInfrastructure, TDbContext>(this, i => new MockSessionCommentService());
            SbMediaService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISbMediaService, TInfrastructure, TDbContext>(this, i => new MockSbMediaService());
            SbTranscriptionService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISbTranscriptionService, TInfrastructure, TDbContext>(this, i => new MockSbTranscriptionService());
            TranscriptFileService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ITranscriptFileService, TInfrastructure, TDbContext>(this, i => new MockTranscriptFileService());
            MediaStreamingService = new ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IMediaStreamingService, TInfrastructure, TDbContext>(this, i => new MockMediaStreamingService());
        }

        #endregion

        #region Properties

        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IClipFileService, TInfrastructure, TDbContext> ClipFileService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IClipOperationService, TInfrastructure, TDbContext> ClipOperationService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IClipService, TInfrastructure, TDbContext> ClipService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ILifecycleFactory, TInfrastructure, TDbContext> LifeCycleFactory { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IProviderFactory, TInfrastructure, TDbContext> ProviderFactory { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISbOrganizationService, TInfrastructure, TDbContext> SbOrganizationService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISeriesService, TInfrastructure, TDbContext> SeriesService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISessionFeedService, TInfrastructure, TDbContext> SessionFeedService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISessionService, TInfrastructure, TDbContext> SessionService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISessionCommentService, TInfrastructure, TDbContext> SessionCommentService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISbMediaService, TInfrastructure, TDbContext> SbMediaService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ISbTranscriptionService, TInfrastructure, TDbContext> SbTranscriptionService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, ITranscriptFileService, TInfrastructure, TDbContext> TranscriptFileService { get; }
        public ValueResolver<SbTestBuilder<TInfrastructure, TDbContext>, IMediaStreamingService, TInfrastructure, TDbContext> MediaStreamingService { get; }

        #endregion
    }
}