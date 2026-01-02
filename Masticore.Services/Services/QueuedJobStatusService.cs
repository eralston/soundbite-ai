using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// EF Implementation of <see cref="IGroupService"/>
    /// </summary>
    public class QueuedJobStatusService : IQueuedJobStatusService
    {
        #region Properties

        protected IIdentityInfrastructure Infrastructure { get; }

        #endregion

        #region Constructor

        public QueuedJobStatusService(IIdentityInfrastructure infrastructure)
        {
            Infrastructure = infrastructure;
        }

        #endregion

        #region IQueuedJobStatusService Implementation

        /// <inheritdoc />
        public async Task SetQueueInfo(string route, string messageId, string message)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            QueuedJobStatusEntity entity = await db.QueuedJobStatuses.Where(i => i.Route == route).FirstOrDefaultAsync();
            entity.AssertFound($"Cannot set queue information because the QueuedJobStatus entry (route:{route}) was not found.");
            entity.QueueMessageId = messageId;
            entity.QueueMessage = message;
            db.QueuedJobStatuses.Update(entity);
            await db.SaveChangesAsync();

        }

        /// <inheritdoc />
        public async Task<QueuedJobStatus> ReadByRouteAsync(string route)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            QueuedJobStatusEntity entity = await db.QueuedJobStatuses.Where(i => i.Route == route).FirstOrDefaultAsync();
            QueuedJobStatus result = entity?.ToModel();
            return result;
        }

        /// <inheritdoc />
        public async Task<QueuedJobStatus> SaveAsync(QueuedJobStatus item)
        {
            Validator.ArgNotNull(nameof(item), item, "Cannot save QueuedJobStatus because it is null.");
            bool isNew = false;
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            QueuedJobStatusEntity entity = string.IsNullOrEmpty(item.Route) ? null
                : await db.QueuedJobStatuses.Where(i => i.Route == item.Route).FirstOrDefaultAsync();

            // Create new entry for a new item
            if (entity == null)
            {
                isNew = true;
                entity = new QueuedJobStatusEntity();
                entity.SetCreatedFields(null);
                entity.NewRoute();
                db.QueuedJobStatuses.Add(entity);
            }

            // Make sure the item has a correlation ID
            item.CorrelationId = item.CorrelationId == Guid.Empty ? Guid.NewGuid() : item.CorrelationId;

            //Transfer Properties
            entity.Route = item.Route;
            entity.OrgRoute = item.OrgRoute;
            entity.Description = item.Description;
            entity.RequestDate = item.RequestDate;
            entity.ProcessingStarted = item.ProcessingStarted;
            entity.ProcessingComplete = item.ProcessingComplete;
            entity.JobType = item.JobType;
            entity.QueueMessageId = item.QueueMessageId;
            entity.QueueMessage = item.QueueMessage;
            entity.CorrelationId = item.CorrelationId;
            entity.JobStatus = item.JobStatus;
            entity.Details = item.Details;

            // Save the entity
            if (!isNew)
            {
                db.QueuedJobStatuses.Update(entity);
            }
            await db.SaveChangesAsync();
            QueuedJobStatus result = entity.ToModel();
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateStatusAsync(string route, JobStatusType status, string errorMessage = null)
        {
            IIdentityDb db = await Infrastructure.IdentityDbAsync();
            QueuedJobStatusEntity entity = await db.QueuedJobStatuses.Where(i => i.Route == route).FirstOrDefaultAsync();
            entity.AssertFound($"Cannot update status value because the QueuedJobStatus entry (route:{route}) was not found.");
            entity.JobStatus = status;
            entity.Details = errorMessage;

            switch (status)
            {
                case JobStatusType.Complete:
                case JobStatusType.Failed:
                    if (entity.ProcessingComplete == null)
                    {
                        entity.ProcessingComplete = DateTime.UtcNow;
                    }
                    break;
                case JobStatusType.Processing:
                    if (entity.ProcessingStarted == null)
                    {
                        entity.ProcessingStarted = DateTime.UtcNow;
                    }
                    break;
            }

            db.QueuedJobStatuses.Update(entity);
            await db.SaveChangesAsync();
        }

        #endregion

    }
}