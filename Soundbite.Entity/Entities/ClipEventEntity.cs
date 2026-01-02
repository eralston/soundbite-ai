using Masticore.Entity;
using Microsoft.EntityFrameworkCore;
using Soundbite.Models;
using System.Threading.Tasks;

namespace Soundbite.Entity
{
    /// <summary>
    /// An entity for capturing <see cref="ClipEntity"/> events
    /// </summary>
    public class ClipEventEntity : EventEntityBase<ClipEntity>
    {
        /// <summary>
        /// Gets or sets the specific clip event type
        /// </summary>
        public ClipEventType ClipEventType { get; set; }

        /// <summary>
        /// The amount of time in the clip that this event is indicating, EG, the duration of listening for the clip
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// The play head position relevant to the event
        /// </summary>
        public int? Position { get; set; }
    }

    /// <summary>
    /// Helpers for <see cref="ClipEventEntity"/>
    /// </summary>
    public static class ClipEventEntityUtils
    {
        /// <summary>
        /// Creates a clip event in the given db for the given clip
        /// </summary>
        /// <remarks>
        /// The caller still needs to persist changes by calling <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken"/> or similar
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="clip"></param>
        /// <param name="eventType"></param>
        /// <param name="clipEventType"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static ClipEventEntity CreateClipEvent(this SbDb db, ClipEntity clip, EventType eventType, ClipEventType clipEventType, int createdById)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            if (clip is null)
            {
                throw new System.ArgumentNullException(nameof(clip));
            }

            ClipEventEntity ret = db.CreateEvent<ClipEventEntity, ClipEntity>(clip, eventType, createdById);
            ret.ClipEventType = clipEventType;
            return ret;
        }

        /// <summary>
        /// Async creates a <see cref="ClipEventEntity"/> for the clip at the given route
        /// </summary>
        /// <remarks>
        /// The caller still needs to persist changes on the <see cref="DbContext"/> by themselves
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="clipRoute"></param>
        /// <param name="eventType"></param>
        /// <param name="clipEventType"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static async Task<ClipEventEntity> CreateClipEvent(this SbDb db, string clipRoute, EventType eventType, ClipEventType clipEventType, int? createdById = null)
        {
            if (db is null)
            {
                throw new System.ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(clipRoute))
            {
                throw new System.ArgumentException($"'{nameof(clipRoute)}' cannot be null or empty.", nameof(clipRoute));
            }

            ClipEntity clip = await db.SingleLocalOrRemoteAsync<ClipEntity>(c => c.Route == clipRoute, c => c.Route == clipRoute);
            ClipEventEntity ret = db.CreateEvent<ClipEventEntity, ClipEntity>(clip, eventType, createdById);
            ret.ClipEventType = clipEventType;
            return ret;
        }
    }
}
