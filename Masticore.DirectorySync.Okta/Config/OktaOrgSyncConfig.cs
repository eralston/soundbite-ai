using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Masticore.DirectorySync.Okta
{
    /// <summary>
    /// Defines the organization-level configuration settings for the Interact directory provider.    
    /// </summary>
    [CodeGenModel]
    public class OktaOrgSyncConfig : SyncStrategyConfigBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the OKTA domain to use for API calls.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the API token used for access to the OKTA API.
        /// </summary>
        public string ApiToken { get; set; }

        /// <summary>
        /// Gets or sets the starting date/time range for the last modified filter for the previous
        /// sync run. This is used to perform a "delta" against the directory so only objects 
        /// updated since the last sync are processed.
        /// </summary>
        public DateTime? ModifiedDateFilterStart { get; set; }

        /// <summary>
        /// Gets or sets the ending date/time range for the last modified filter for the previous
        /// sync run. This is used to perform a "delta" against the directory so only objects 
        /// updated since the last sync are processed. The end date is also important to ensure that
        /// resuming a partial run does not bring in "extra" items that could be skipped depending
        /// on where the in the process picks back up.
        /// </summary>
        public DateTime? ModifiedDateFilterEnd { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the current syncronization process was stopped
        /// and that only a partial result was obtained.  This alows the process to restart the
        /// sync where it stopped off.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public bool IsPartialSync { get; set; }

        /// <summary>
        /// Gets or sets the maximum duration (in milliseconds) to allow a job to run before
        /// aborting.  This was added to ensure the job ran within the timeframe allowed by azure
        /// functions. If the max run length is exceeded the job is "completed" as a partial run
        /// and resumed the next time the sync runs.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public int MaxRunLength { get; set; } = 1000 /*miliseconds*/ * 60 /*seconds*/ * 8 /*minutes*/;

        /// <summary>
        /// Gets or sets the maximum modified date encountered during the syncronizatino process.
        /// This value is destined for use as the <see cref="LastSync"/> value when a syncronization
        /// process completes, but needs to be stored to correctly resume a partial sync.
        /// </summary>
        // TODO: This CodeGen attribute removes it from the Typescript, but we should consider removing it from the response
        [CodeGenField(Ignore = true)]
        public DateTime? MaxModifiedDateFound { get; set; }

        /// <summary>
        /// Gets or sets the current skip token.  Skip tokens are issued by the OKTA API and used
        /// to navigate from one page of results to another.
        /// </summary>
        // TODO: This CodeGen attribute removes it from the Typescript, but we should consider removing it from the response
        [CodeGenField(Ignore = true)]
        public string NextPage { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the user sync process is complete. If this flag is
        /// true and the <see cref="IsPartialSync"/> flag is true, the sync process resumes the 
        /// partial group sync and bypasses the "completed" user sync.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public bool UserSyncComplete { get; set; }

        /// <summary>
        /// Gets a list of the IDs for the most recent items.  These can be used to resume a 
        /// synchronization run that was abandoned.
        /// </summary>
        [JsonIgnore]
        [CodeGenField(Ignore = true)]
        public Queue<string> MostRecentItems { get; set; }

        #endregion

        #region Overrides

        /// <inheritdoc />
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public override string SyncType => OktaOrgSyncStrategyBase.Name;

        #endregion
    }
}
