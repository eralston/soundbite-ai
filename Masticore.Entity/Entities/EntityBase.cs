using Masticore.Resources;
using Masticore.Security;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// Abstract base class for an EF Core <see cref="IEntity"/>
    /// </summary>
    public abstract class EntityBase : IEntity
    {
        #region Static Methods

        /// <summary>
        /// Creates a new object with filled in ID and life-cycle dates
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TEntity CreateMock<TEntity>(int id = 0) where TEntity : EntityBase, new()
        {
            TEntity entity = new TEntity { Id = id };
            entity.Timestamp();
            return entity;
        }

        /// <summary>
        /// Creates an <see cref="EntityBase"/> object of the given time with the given value for <see cref="CreatedById"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static TEntity Create<TEntity>(int? createdById = null) where TEntity : EntityBase, new()
        {
            if (createdById.HasValue && createdById.Value == 0)
            {
                throw new ArgumentNullException($"Cannot create {typeof(TEntity).Name} with {nameof(createdById)} of zero; to represent records NOT created by a user, try setting it to null");
            }

            TEntity entity = new TEntity { CreatedById = createdById };
            entity.Timestamp();
            return entity;
        }

        /// <summary>
        /// Creates an entity, assigning the given Creator
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static TEntity Create<TEntity>(UserEntity creator) where TEntity : EntityBase, new()
        {
            TEntity entity = new TEntity { CreatedBy = creator };
            entity.Timestamp();
            return entity;
        }

        #endregion

        #region IEntity

        /// <summary>
        /// Sequential number. Internal ID only that should be used to join data, but never as part of an exposed API or SDK for the platform
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets the UTC based date/time when the data item was created.  Any modifications to this
        /// value are discarded by the application.
        /// </summary>
        [JsonProperty]
        [Display(Name = "Created Date")]
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Gets the UTC based date/time when the data item was last updated.  Any modifications to 
        /// this value are discarded by the application.
        /// </summary>        
        [JsonProperty]
        [Display(Name = "Updated Date")]
        public DateTime UpdatedUtc { get; set; }

        /// <summary>
        /// Date the object was soft deleted; otherwise, null
        /// </summary>
        [Display(Name = "Deleted Date")]
        public DateTime? DeletedUtc { get; set; }

        /// <summary>
        /// Optional primary key for the <see cref="UserEntity"/> that created this entity; if this is null, then this record was made by the system without correlating user interaction
        /// </summary>
        public int? CreatedById { get; set; }

        #endregion

        /// <summary>
        /// Gets the <see cref="UserEntity"/> associated with the CreatedById of this Entity/>
        /// </summary>
        /// <remarks>If this is null, then this record was created as a side-effect of system operations (EG, background tasks) and not via user interaction</remarks>
        public virtual UserEntity CreatedBy { get; set; }

        /// <summary>
        /// Applies common "create" property values to a resource when it is first created.
        /// </summary>
        /// <param name="securityContext">Security context under which the item is being created.</param>
        public void SetCreatedFields(UserEntity createdBy)
        {
            UpdatedUtc = Time.UtcNow;
            CreatedUtc = Time.UtcNow;
            CreatedBy = createdBy;
        }

        /// <summary>
        /// Applys common "update" property values to a resource when it is updated.        
        /// </summary>
        /// <param name="securityContext">Security context under which the item is being updated.</param>
        public virtual void SetUpdatedFields(ISecurityContext securityContext)
        {
            //TODO: figure out if we need a last updated by field
            UpdatedUtc = Time.UtcNow;
        }
    }
}
