#if DEBUG
using AutoMapper;
using Masticore.Entity;
using Masticore.Models;
using Masticore.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.AzFunc.Scheduler
{
    /// <summary>
    /// Called periodically to process sessions
    /// </summary>
    public class EmailTestFunc
    {
        private ISbNotificationService SbNotifications { get; }
        private INotificationService Notifications { get; }
        private ISbInfrastructure Infrastructure { get; }
        private IMapper Mapper { get; }

        // NOTE: Change this to send emails
        private readonly bool IsEnabled = false;

        public EmailTestFunc(INotificationService notifications, ISbNotificationService sbNotifications, ISbInfrastructure infrastructure, IMapper mapper)
        {
            Notifications = notifications;
            SbNotifications = sbNotifications;
            Infrastructure = infrastructure;
            Mapper = mapper;
        }

        private const string SessionTiming = "* * * * 1";
        private const bool RunOnStartup = false;

        [FunctionName(nameof(EmailTestFunc))]
        public async Task Run([TimerTrigger(SessionTiming, RunOnStartup = RunOnStartup)] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            if (!IsEnabled)
            {
                return;
            }

            // REMINDER This is running against local DB, so make sure you have decent data for these assumptions
            SbDb db = await Infrastructure.DbAsync();
            UserEntity userEnt = db.Users.First();
            User user = Mapper.MapSafe<User>(userEnt);
            OrganizationEntity orgEnt = db.Organizations.First();
            Organization org = Mapper.MapSafe<Organization>(orgEnt);
            SessionEntity sessionEnt = db.Sessions.First();

            await Notifications.UserInviteAsync(user, user);
            // REMINDER This is running against local DB, so make sure your local org has correct notification setttings (EG, e-mail is enabled)
            await SbNotifications.SessionReminderAsync(user, org, sessionEnt);
        }
    }
}
#endif