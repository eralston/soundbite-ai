using Masticore.Jobs;
using System;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Jobs
{
    public interface ISyncJobWorker : IJobWorker { }

    /// <summary>
    /// <see cref="IJobWorker"/> for <see cref="SyncJob"/>
    /// </summary>
    public class SyncJobWorker : JobWorker<SyncJob>, ISyncJobWorker
    {
        IOrgSyncService OrgSyncService { get; }
        ISyncInfrastucture Infrastucture { get; }

        public SyncJobWorker(IOrgSyncService orgSyncService, ISyncInfrastucture infrastucture)
        {
            OrgSyncService = orgSyncService ?? throw new ArgumentNullException(nameof(orgSyncService));
            Infrastucture = infrastucture ?? throw new ArgumentNullException(nameof(infrastucture));
        }

        public override Task<SyncJob> Prepare(SyncJob job)
        {
            job.OrgSyncService = OrgSyncService;
            job.Infrastucture = Infrastucture;

            return base.Prepare(job);
        }
    }
}
