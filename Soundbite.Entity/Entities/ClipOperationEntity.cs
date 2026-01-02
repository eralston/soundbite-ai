using Masticore.Entity;
using Soundbite.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Soundbite.Entity
{
    /// <summary>
    /// Media files connected to sessions, enabling people to collaborate
    /// </summary>
    public class ClipOperationEntity : ResourceEntityBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the operation type associated with the clip.
        /// </summary>
        public ClipOperationType OperationType { get; set; }

        /// <summary>
        /// Gets or sets the state of the operation as it tracks to completion.
        /// </summary>
        public ClipOperationStateType OperationState { get; set; }

        /// <summary>
        /// Gets or sets a JSON string containing any operation configuration settings. Use the
        /// <see cref="ClipOperation{T}"/> to access configuration settings via a strongly-typed
        /// configuration object.
        /// </summary>
        [Column(TypeName = "ntext")]
        public virtual string OperationDataJson { get; set; }

        /// <summary>
        /// Gets or sets any error details associated with clip processing.  When the state denotes
        /// a failure this value should contain additional details on the error.
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Gets or sets the date/time when processing was marked as complete.  This value should
        /// not be set when an error occurs during processing.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets a code to help identify costs associated with the processing of the clip.
        /// </summary>
        public string BillingCode { get; set; }

        /// <summary>
        /// Gets or sets a value to help locate the clip operation based on data from an external process.
        /// </summary>
        public string ExternalId { get; set; }

        #endregion

        #region Entity Relationship Properties

        public virtual int ClipId { get; set; }

        public virtual ClipEntity Clip { get; set; }

        #endregion
    }
}