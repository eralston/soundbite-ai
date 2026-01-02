using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    internal class WorkerState
    {
        public string Status { get; set; } = "Running";

        public Task ProcessRef { get; set; }

        public int OrgId { get; set; }

        public string OrgRoute { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the request for a batch of messages maxed out. When
        /// this occurs it implies there may be additional messages for processing.
        /// </summary>
        public bool MaxedOutMessageRetrieval { get; set; }

        public List<SessionNotificationToProcess> MessagesToProcess { get; set; }


        //NOTE: Teams properties that could be subclassed if this class is needed as a base calss
        //  for other procsses
        public string OrgGraphToken { get; set; }
        public DateTime OrgGraphTokenRecievedAt { get; set; }
        public int RequestRate { get; set; }
        public DateTime LastApiCall = DateTime.MinValue;


    }
}