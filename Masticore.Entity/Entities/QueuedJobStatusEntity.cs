using Masticore.Models;
using Masticore.Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// Maintains status information about a queued job.
    /// </summary>
    public class QueuedJobStatusEntity : ResourceEntityBase
    {
        #region Model Properties

        [StringLength(ResourceExtensions.RouteLength)]
        public string OrgRoute { get; set; }
        [MaxLength(512)]
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ProcessingStarted { get; set; }
        public DateTime? ProcessingComplete { get; set; }
        [MaxLength(256)]
        public string JobType { get; set; }
        [MaxLength(128)]
        public string QueueMessageId { get; set; }
        public string QueueMessage { get; set; }
        public Guid CorrelationId { get; set; }
        public JobStatusType JobStatus { get; set; }
        public string Details { get; set; }

        #endregion

        #region Entity Properties

        public int Id { get; set; }
        public int? OrganizationId { get; set; }

        #endregion

        #region Relationship Properties

        public OrganizationEntity Organization { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Populates the model used to represent the entity.
        /// </summary>
        /// <returns>a model populated with data from the entity.</returns>
        public QueuedJobStatus ToModel()
        {
            return new QueuedJobStatus()
            {
                CorrelationId = CorrelationId,
                Description = Description,
                Details = Details,
                JobStatus = JobStatus,
                JobType = JobType,
                OrgRoute = OrgRoute,
                ProcessingComplete = ProcessingComplete,
                ProcessingStarted = ProcessingStarted,
                QueueMessage = QueueMessage,
                QueueMessageId = QueueMessageId,
                RequestDate = RequestDate,
                Route = Route
            };
        }

        #endregion
    }
}
