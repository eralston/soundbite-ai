using Masticore.Resources;
using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// Abstract base class for persisting IResource objects
    /// </summary>
    public abstract class ResourceEntityBase : EntityBase, IResource
    {
        #region Utility Methods

        /// <summary>
        /// Creates a new object with filled in ID and life-cycle dates
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static new TResource CreateMock<TResource>(int id = 0) where TResource : ResourceEntityBase, new()
        {
            TResource resource = EntityBase.CreateMock<TResource>(id);
            resource.NewRoute();
            return resource;
        }

        /// <summary>
        /// Creates an entity, assigning the given Creator
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static new TResource Create<TResource>(UserEntity creator) where TResource : ResourceEntityBase, new()
        {
            TResource resource = EntityBase.Create<TResource>(creator);
            resource.NewRoute();
            return resource;
        }

        #endregion

        #region IResource

        /// <summary>
        /// Gets the route associated with the data item.  Routes are used like IDs to uniqely
        /// identify data items in the Soundbite platform.  This value is read-only and should
        /// not be updated.
        /// </summary>
        [Required]
        [StringLength(ResourceExtensions.RouteLength)]
        public string Route { get; set; }

        #endregion
    }
}
