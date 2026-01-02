using System;

namespace Masticore.Models
{
    /// <summary>
    /// Maintains status information about a queued job.
    /// </summary>
    public class QueuedJobStatus
    {
        /// <summary>
        /// Get or set route for this job
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Get or set route for the org connected to this job
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// Text summary of the job
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// DateTime for the when it was started
        /// </summary>
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// DateTime for when it was started
        /// </summary>
        public DateTime? ProcessingStarted { get; set; }

        /// <summary>
        /// DateTime for when it finished
        /// </summary>
        public DateTime? ProcessingComplete { get; set; }

        /// <summary>
        /// Marker for the genre of the job
        /// </summary>
        public string JobType { get; set; }

        /// <summary>
        /// Captures the ID in the queue
        /// </summary>
        public string QueueMessageId { get; set; }

        /// <summary>
        /// The message itself
        /// </summary>
        public string QueueMessage { get; set; }

        /// <summary>
        /// ID The queue gave the job
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Lifecycle state of the job
        /// </summary>
        public JobStatusType JobStatus { get; set; } = JobStatusType.None;

        /// <summary>
        /// Detailed message for result of the job, especially any exceptioms
        /// </summary>
        public string Details { get; set; }
    }
}
