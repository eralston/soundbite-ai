using Masticore.Azure.MediaServices;

namespace Soundbite.Services.Tests
{
    public class SbMediaServiceTests : ServiceTestBase
    {

        #region Constructor

        public SbMediaServiceTests()
        {
            Builder.SbMediaService.Use(svc =>
            {
                return new SbMediaService(
                    svc.Logger<SbMediaService>(),
                    svc.Infrastructure.Value,
                    svc.SecurityContext.Value,
                    svc.SbTranscriptionService.Value,
                    svc.JobQueue.Value,
                    null,
                    svc.QueuedJobStatus.Value,
                    svc.MediaProcessing.Value,
                    svc.ClipFileService.Value,
                    svc.ClipOperationService.Value,
                    svc.LifeCycleFactory.Value,
                    null,
                    new AzureMediaService(),
                    svc.Mapper.Value,
                    svc.Rbac.Value,
                    svc.SbTranscriptionService.Value);
            });
        }
        #endregion
    }
}
