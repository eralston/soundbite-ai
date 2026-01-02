using Microsoft.EntityFrameworkCore;
using System;

namespace Masticore.Entity
{
    /// <summary>
    /// Helper class for easing the handling of <see cref="EventEntityBase{TTarget}"/> classes 
    /// </summary>
    public static class EventUtils
    {
        /// <summary>
        /// Creates an event object for the given <see cref="EventEntityBase{TTarget}"/> targetting the given <see cref="EntityBase"/> with the given ID
        /// </summary>
        /// <remarks>
        /// This works well when the target exists in the database and you do not want the overhead of querying it again
        /// </remarks>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="targetId"></param>
        /// <param name="eventType"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static TEvent CreateEvent<TEvent, TTarget>(int targetId, EventType eventType, int? createdById = null)
            where TEvent : EventEntityBase<TTarget>, new()
            where TTarget : EntityBase
        {
            if (targetId == 0)
            {
                throw new ArgumentNullException($"{nameof(targetId)} is zero, incidating this entity was not yet created in the database");
            }

            TEvent ret = EntityBase.Create<TEvent>(createdById);
            ret.TargetId = targetId;
            ret.EventType = eventType;
            return ret;
        }

        /// <summary>
        /// Creates an event object for the given <see cref="EventEntityBase{TTarget}"/> targetting the given <see cref="EntityBase"/>
        /// </summary>
        /// <remarks>
        /// The caller still to add the object to the <see cref="DbContext"/> and save changes
        /// </remarks>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="target"></param>
        /// <param name="eventType"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static TEvent CreateEvent<TEvent, TTarget>(TTarget target, EventType eventType, int? createdById = null)
            where TEvent : EventEntityBase<TTarget>, new()
            where TTarget : EntityBase
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            TEvent ret = EntityBase.Create<TEvent>(createdById);
            ret.Target = target;
            ret.EventType = eventType;
            return ret;
        }

        /// <summary>
        /// Creates an event object for the given <see cref="EventEntityBase{TTarget}"/> targetting the given <see cref="EntityBase"/>, automatically adding it to the given <see cref="DbContext"/>
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <remarks>
        /// The caller still needs to persist changes by calling <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken"/> or similar
        /// </remarks>
        /// <param name="db"></param>
        /// <param name="target"></param>
        /// <param name="eventType"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static TEvent CreateEvent<TEvent, TTarget>(this DbContext db, TTarget target, EventType eventType, int? createdById = null)
            where TEvent : EventEntityBase<TTarget>, new()
            where TTarget : EntityBase
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            TEvent ret = CreateEvent<TEvent, TTarget>(target, eventType, createdById);
            db.Set<TEvent>().Add(ret);
            return ret;
        }
    }
}
