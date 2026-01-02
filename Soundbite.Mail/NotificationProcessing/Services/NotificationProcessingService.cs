using Masticore;
using Microsoft.EntityFrameworkCore;
using Soundbite.Entity;
using Soundbite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{


    public class NotificationProcessingService : INotificationProcessingService
    {
        ////////////////////////////////////////////////////////////////////////////////////////////
        // NOTE: All data access and data updates within this service must occur within a lock
        //       or else there is distinct potential of issues from wonkiness to disaster...
        ////////////////////////////////////////////////////////////////////////////////////////////

        #region Static Properties

        private static bool IsProcessing { get; set; } = false;

        #endregion

        #region Properties

        private LockService DbLock { get; }

        private ISbInfrastructure SbInfrastructure { get; set; }

        private IChannelProcessorFactory ChannelProcessorfactory { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="NotificationProcessingService"/> instance.
        /// </summary>
        public NotificationProcessingService(ISbInfrastructure sbInfrastructure, IChannelProcessorFactory channelProcessorfactory)
        {
            SbInfrastructure = sbInfrastructure;
            DbLock = new LockService(async () => await sbInfrastructure.DbAsync());
            ChannelProcessorfactory = channelProcessorfactory;
        }

        #endregion

        #region INotificationProcessingService Implementation

        /// <inheritdoc />
        public Task<bool> HasMessagesToSend(NotificationChannel channel)
        {
            bool result = DbLock.LockAsync(async () =>
            {
                SbDb db = await SbInfrastructure.DbAsync();
                return await db.SessionNotifications.AnyAsync(i => i.Session.DeletedUtc == null
                    && i.Session.Organization.DeletedUtc == null
                    && i.Session.Organization.Tenant.DeletedUtc == null
                    && i.Channel == channel
                    && (i.Status == NotificationStatus.Pending || i.Status == NotificationStatus.Retry));
            });
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<bool> ResetAbandonedItems(NotificationChannel channel, bool allChannels = false)
        {
            bool result = DbLock.LockAsync(async () =>
            {
                SbDb db = await SbInfrastructure.DbAsync();
                int counter = 0;

                // Assume that anything older than 3 minutes is abandoned
                DateTime abandonedItemCutoffTime = DateTime.UtcNow.AddMinutes(-3);

                IList<SessionNotificationEntity> lostItems = await db.SessionNotifications
                .Where(i =>
                    i.Session.DeletedUtc == null
                    && i.Session.Organization.DeletedUtc == null
                    && i.Session.Organization.Tenant.DeletedUtc == null
                    && (allChannels == true || i.Channel == channel)
                    && i.Status == NotificationStatus.Processing
                    && i.UpdatedUtc <= abandonedItemCutoffTime)
                .ToListAsync();

                foreach (SessionNotificationEntity lostItem in lostItems)
                {
                    counter++;
                    bool maxRetriesAttempted = false;
                    lostItem.StatusJson = JsonUtils.WithJson<List<SessionNotificationStatus>>(lostItem.StatusJson, i =>
                    {

                        // Determine whether the retry limit has been reached
                        maxRetriesAttempted = i.Count >= TeamsChannelProcessor.MaxTeamsNotificationRetries;

                        // Update the status accordingly so it is not stuck in limbo
                        lostItem.Status = maxRetriesAttempted
                            ? NotificationStatus.Failed
                            : NotificationStatus.Pending;

                        // Make sure to add a failure entry for logging and ensuring this does not repeat forever.
                        i.Add(new SessionNotificationStatus()
                        {
                            Info = maxRetriesAttempted
                             ? "Abandoned Item - Updated as failed because retry limit has been exceeded."
                             : "Abandoned Item - Updated for Retry"
                        });
                    });
                    db.SessionNotifications.Update(lostItem);

                    // Update in batches of 1000 to avoid massive final payload
                    if (counter > 1000)
                    {
                        await db.SaveChangesAsync();
                        counter = 0;
                    }
                }

                // Final save
                await db.SaveChangesAsync();

                return lostItems.Any();
            });
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<IEnumerable<int>> UniqueOrgIdsWithMessages(NotificationChannel channel, int maxCount, IEnumerable<int> exclude = null)
        {
            exclude = exclude ?? new int[] { };
            IEnumerable<int> result = DbLock.LockAsync(async () =>
            {
                SbDb db = await SbInfrastructure.DbAsync();
                return await db.SessionNotifications
                .Where(i => i.Session.DeletedUtc == null
                    && i.Session.Organization.DeletedUtc == null
                    && i.Session.Organization.Tenant.DeletedUtc == null
                    && i.Channel == channel
                    && !exclude.Contains(i.Session.OrganizationId)
                    && (i.Status == NotificationStatus.Pending || i.Status == NotificationStatus.Retry))
                .Select(i => i.Session.Organization.Id)
                .Distinct()
                .Take(maxCount)
                .ToListAsync();
            });
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<IEnumerable<SessionNotificationToProcess>> GetMessagesForProcessing(int orgId, NotificationChannel channel, int maxCount)
        {
            List<SessionNotificationToProcess> result = DbLock.LockAsync(async () =>
            {
                SbDb db = await SbInfrastructure.DbAsync();
                return await db.SessionNotifications
                .Where(i => i.Session.DeletedUtc == null
                    && i.Session.Organization.DeletedUtc == null
                    && i.Session.Organization.Tenant.DeletedUtc == null
                    && i.Session.OrganizationId == orgId
                    && i.Channel == channel
                    && (i.Status == NotificationStatus.Pending || i.Status == NotificationStatus.Retry))
                .OrderBy(i => i.SessionId)
                .ThenBy(i => i.Status) // this should put pending before retry (see enum values) which should force items to rerty to the back of the queue.
                .Select(i => new SessionNotificationToProcess()
                {
                    SessionId = i.SessionId,
                    SessionRoute = i.Session.Route,
                    SessionNotificationId = i.Id,
                    Title = i.Session.Name,
                    Email = i.User.Email,
                    UserConfigJson = i.User.ConfigJson,
                    Author = i.Session.CreatedBy.GivenName + " " + i.Session.CreatedBy.FamilyName,
                    OrgName = i.Session.Organization.Name,
                    StatusJson = i.StatusJson,
                    UserRoute = i.User.Route
                })
                .Take(maxCount)
                .ToListAsync();
            });
            return Task.FromResult<IEnumerable<SessionNotificationToProcess>>(result);
        }

        /// <inheritdoc />
        public async Task ProcessNotifications()
        {
            // NOTE: Currently we are only processing teams messages anyway, but if we ever end up
            // supporting additional channels for batching then we should probably have a method
            // that retrieves a list of all channels that need to be processed to avoid sending out
            // a single call for each channel...

            // Update any abandoned items before processing
            await ResetAbandonedItems(NotificationChannel.Teams);

            if (await HasMessagesToSend(NotificationChannel.Teams))
            {
                IChannelProcessor processor = ChannelProcessorfactory.GetChannelProcessor(NotificationChannel.Teams, this);
                await processor.ProcessMessages();
            }
        }

        #endregion
    }
}
