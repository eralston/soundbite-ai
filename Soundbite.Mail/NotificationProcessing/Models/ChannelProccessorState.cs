using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    internal class ChannelProccessorState
    {
        public bool Complete { get; set; }
        public int MaxWorkers { get; set; }
        public bool NoMoreOrgIdsToProcess { get; set; }
        public NotificationChannel Channel { get; set; }
        public ConcurrentDictionary<int, WorkerState> Workers { get; } = new ConcurrentDictionary<int, WorkerState>();
        public Task[] RunningWorkers => Workers.Where(i => i.Value.Status == "Processing").Select(i => i.Value.ProcessRef).ToArray();
        public int[] RunningOrgIds => Workers.Where(i => i.Value.Status == "Processing").Select(i => i.Value.OrgId).ToArray();
    }
}