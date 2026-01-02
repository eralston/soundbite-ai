using Masticore;
using Masticore.Models;
using System;

namespace Soundbite.Models
{
    /// <summary>
    /// Clip operation with a strongly-typed data property.
    /// </summary>
    /// <typeparam name="T">.NET type of the clip operation configuration settings.</typeparam>
    public class ClipOperation<T> : ClipOperation
        where T : class, new()
    {
        #region Fields

        private T _operationData;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a strongly-typed object containing clip operation configuration settings.
        /// </summary>
        public T OperationData
        {
            get
            {
                if (string.IsNullOrEmpty(base.OperationDataJson))
                {
                    _operationData = new T();
                }
                else
                {
                    _operationData = JsonUtils.FromLowerCamelJson<T>(base.OperationDataJson);
                }
                return _operationData;
            }
        }

        /// <inheritdoc />
        public override string OperationDataJson
        {
            get => _operationData == null
                    ? base.OperationDataJson
                    : _operationData.ToLowerCamelJson();
            set
            {
                _operationData = null;
                base.OperationDataJson = value;
            }
        }

        #endregion

        #region Constructor

        public ClipOperation() { }
        public ClipOperation(ClipOperation source)
        {
            CompletedDate = source.CompletedDate;
            CreatedUtc = source.CreatedUtc;
            DeletedUtc = source.DeletedUtc;
            ErrorDetails = source.ErrorDetails;
            ExternalId = source.ExternalId;
            OperationDataJson = source.OperationDataJson;
            OperationState = source.OperationState;
            OperationType = source.OperationType;
            Route = source.Route;
            UpdatedUtc = source.UpdatedUtc;
        }

        #endregion
    }

    /// <summary>
    /// A clip operation represents a process that runs against a clip.  Clip operations are often
    /// out-of-band processes that need to be tracked until completion.
    /// </summary>
    public class ClipOperation : ResourceBase
    {
        public ClipOperation<T> To<T>()
            where T : class, new()
        {
            return new ClipOperation<T>(this);
        }

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
        public virtual string OperationDataJson { get; set; }

        /// <summary>
        /// Gets or sets any error details associated with clip processing.  When the state denotes
        /// a failure this value should contain additional details on the error.
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Gets or sets a code to help identify costs associated with the processing of the clip.
        /// </summary>
        public string BillingCode { get; set; }

        /// <summary>
        /// Gets or sets a value to help locate the clip operation based on data from an external process.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the date/time when processing was marked as complete.  This value should
        /// not be set when an error occurs during processing.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets or sets the route of the clip with which the operation is associated.
        /// </summary>
        public string ClipRoute { get; set; }
    }
}