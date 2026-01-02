using Masticore.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Base class for building directory synchronization result classes.
    /// </summary>
    public abstract class SyncResultBase : IUniversal
    {
        #region Fields

        [JsonIgnore]
        public bool IsStarted { get; private set; } = false;
        [JsonIgnore]
        public bool IsEnded { get; private set; } = false;

        private readonly List<string> _errorMessages = new List<string>();
        private readonly List<Exception> _exceptions = new List<Exception>();

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="SyncResultBase"/> instance.
        /// </summary>
        public SyncResultBase(bool autoRun = true)
        {
            if (autoRun)
            {
                Start();
            }
        }

        #endregion

        #region Persistent Properties

        /// <summary>
        /// Gets or sets the total memory in bytes at the start
        /// </summary>
        public long EndBytes { get; set; }

        /// <summary>
        /// Gets or sets the total memory in bytes at the end
        /// </summary>
        public long StartBytes { get; set; }

        /// <summary>
        /// Gets or sets the memory difference in bytes between runs
        /// </summary>
        public long DeltaBytes { get; set; }

        /// <summary>
        /// Gets or sets the date/time when the sync process started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the date/time when the sync process completed.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the synchronization process duration in seconds.
        /// </summary>
        public double DeltaTime { get; set; }

        /// <summary>
        /// The total count for actions taken by this sync process.
        /// EG, the number of network requests and database CRUD actions
        /// </summary>
        public int ActionCount { get; set; }

        /// <summary>
        /// Gets or sets the UUID for this run
        /// </summary>
        public string UniversalId { get; set; }

        /// <summary>
        /// Gets a list of error messages that occured during the directory sync process.
        /// </summary>
        public string[] ErrorMessages
        {
            get => _errorMessages.ToArray();
            set => _errorMessages.AddRange(value);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a flag indicating whether or not a failure occured at any point during the entire
        /// organization directory sync process.
        /// </summary>
        public bool IsFailed => GetExceptions()?.Length > 0 || ErrorMessages?.Length > 0;

        [JsonIgnore]
        public bool IsSaved { get; set; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the total number of actions made by this result.
        /// This is used to populate <see cref="ActionCount"/>
        /// </summary>
        /// <returns></returns>
        protected abstract int GetActionCount();

        /// <summary>
        /// Gets a list of exceptions that occured during the directory sync process.
        /// </summary>
        public Exception[] GetExceptions()
        {
            return _exceptions.ToArray();
        }

        /// <summary>
        /// Starts the timer for this <see cref="SyncResultBase"/>
        /// </summary>
        public void Start()
        {
            IsStarted = true;
            EndBytes = GC.GetTotalMemory(false);
            StartTime = DateTime.UtcNow;
            this.NewUniversalId();
        }

        /// <summary>
        /// Ends the timer for this sync result
        /// </summary>
        /// <returns></returns>
        public double End()
        {
            IsEnded = true;
            StartBytes = GC.GetTotalMemory(false);
            DeltaBytes = StartBytes - EndBytes;
            EndTime = DateTime.UtcNow;
            DeltaTime = new TimeSpan(EndTime.Ticks - StartTime.Ticks).TotalSeconds;
            ActionCount = GetActionCount();
            return DeltaTime;
        }

        /// <summary>
        /// Adds an exception to the results.
        /// </summary>
        /// <param name="ex">Exception to capture.</param>
        public void AddError(Exception ex)
        {
            _exceptions.Add(ex);
        }

        /// <summary>
        /// Adds an error message to the results.
        /// </summary>
        /// <param name="message">Error message to capture.</param>
        public void AddError(string message)
        {
            _errorMessages.Add(message);
        }

        /// <summary>
        /// Adds an exception and its associated message
        /// </summary>
        /// <param name="ex">Exception to capture.</param>
        /// <param name="message">Error message to capture.</param>
        public void AddError(Exception ex, string message)
        {
            AddError(ex);
            AddError(message);
        }

        #endregion
    }
}
