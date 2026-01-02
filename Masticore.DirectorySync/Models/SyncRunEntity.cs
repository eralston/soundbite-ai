using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// The potential levels at which a <see cref="SyncRunEntity"/> may capture history
    /// </summary>
    public enum SyncScope
    {
        Organization = 0,
        Region = 1
    }

    /// <summary>
    /// Entity that persists the key fields of <see cref="SyncResultBase"/> objects on behalf of an <see cref="OrganizationEntity"/>
    /// </summary>
    public class SyncRunEntity : Entity.ResourceEntityBase, IUniversal, IOrgSyncFields
    {
        // IUniversal

        /// <summary>
        /// Gets or sets the UUID for this run
        /// </summary>
        [Required]
        public string UniversalId { get; set; }

        /// <see cref="IOrgSyncFields"/>

        /// <summary>
        /// Gets or sets the sanitized <see cref="OrgSyncConfig.SyncConfigJson"/> for the run
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Column(TypeName = "ntext")]
        public string SyncConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the type of <see cref="IOrgSyncStrategy"/> used during the run
        /// </summary>
        public string SyncType { get; set; }

        // Fields

        /// <summary>
        /// Indicates the scope of this sync run
        /// </summary>
        public SyncScope Scope { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating success/failure for this run
        /// </summary>
        public bool IsFailed { get; set; }

        /// <summary>
        /// Gets or sets the memory difference in bytes between runs
        /// </summary>
        public long DeltaBytes { get; set; }

        /// <summary>
        /// Gets or sets the synchronization process duration in seconds.
        /// </summary>
        public double DeltaTime { get; set; }

        /// <summary>
        /// Gets or sets the number of actions (EG, requests to the network or CRUDs on the store) taken by this run
        /// </summary>
        public int ActionCount { get; set; }

        /// <summary>
        /// Gets or sets the JSON string containing serialized directory sync settings.
        /// </summary>
        [Required]
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [Column(TypeName = "ntext")]
        public string ResultJson { get; set; }

        // Relationships

        // To-One - Optional on parent self (should be self org sync, parent region sync)
        public int? ParentId { get; set; }
        public virtual SyncRunEntity Parent { get; set; }

        // To-One
        public int OrganizationId { get; set; }
        public virtual OrganizationEntity Organization { get; set; }
    }
}
