using Masticore.Jobs;
using Masticore.Services;
using Microsoft.Extensions.Logging;
using Soundbite.Services;
using System;
using System.Threading.Tasks;

namespace Soundbite.Messaging.Jobs
{
    public interface ISbNotificationJobWorker : IJobWorker { }

    public class SbNotificationJobWorker : JobWorker<SbNotificationJob>, ISbNotificationJobWorker
    {
        ISbInfrastructure Infrastructure { get; }
        ILogger<SbNotificationJobWorker> Logger { get; }
        IUserService Users { get; }
        IOrganizationService Organizations { get; }
        IGroupService Groups { get; }
        ICombinedNotificationService Notifications { get; }

        public SbNotificationJobWorker(
            ISbInfrastructure infrastructure,
            ILogger<SbNotificationJobWorker> logger,
            IUserService userService,
            IOrganizationService orgService,
            IGroupService groupService,
            ICombinedNotificationService notifications)
        {
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Users = userService ?? throw new ArgumentNullException(nameof(userService));
            Organizations = orgService ?? throw new ArgumentNullException(nameof(orgService));
            Groups = groupService ?? throw new ArgumentNullException(nameof(groupService));
            Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        }

        public override Task<SbNotificationJob> Prepare(SbNotificationJob job)
        {
            job.Infrastructure = Infrastructure;
            job.Logger = Logger;
            job.Users = Users;
            job.Organizations = Organizations;
            job.Groups = Groups;
            job.Notifications = Notifications;

            return base.Prepare(job);
        }
    }
}
