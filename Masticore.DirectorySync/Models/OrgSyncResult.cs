using Masticore.Models;
using Newtonsoft.Json;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// The steps in the lifecycle of an org sync
    /// </summary>
    public enum OrgSyncState
    {
        Unknown = 0,
        Pending = 10,
        Processing = 20,
        Done = 30,
        Cancelled = 40,
    }

    /// <summary>
    /// Synchronization results a single organization.    
    /// </summary>
    public class OrgSyncResult : SyncResultBase, IOrgSyncFields
    {
        #region Methods

        /// <summary>
        /// Instantiates a new <see cref="OrgSyncResult"/> instance for capturing a run
        /// </summary>
        /// <param name="orgConfig">Configuration for the target org</param>
        /// <param name="autoStart">Flag indicating if the run timer should automatically start</param>
        public OrgSyncResult(string orgRoute, bool autoStart = true) : base(autoStart)
        {
            if (orgRoute is null)
            {
                throw new System.ArgumentNullException(nameof(orgRoute));
            }

            OrgRoute = orgRoute;
        }

        /// <summary>
        /// Converts the given <see cref="SyncRunEntity"/> into its <see cref="OrgSyncResult"/> representation
        /// </summary>
        /// <param name="syncRun"></param>
        /// <returns></returns>
        public static OrgSyncResult FromRun(SyncRunEntity syncRun)
        {
            if (syncRun is null)
            {
                throw new System.ArgumentNullException(nameof(syncRun));
            }

            if (syncRun.ResultJson is null)
            {
                throw new System.ArgumentNullException(nameof(syncRun.ResultJson));
            }

            OrgSyncResult orgResult = JsonUtils.FromLowerCamelJson<OrgSyncResult>(syncRun.ResultJson);
            if (orgResult is null)
            {
                return null;
            }

            orgResult.SyncType = syncRun.SyncType;
            orgResult.SyncConfigJson = syncRun.SyncConfigJson;

            return orgResult;
        }

        /// <summary>
        /// Constructor that takes a <see cref="SyncRunEntity"/>
        /// </summary>
        /// <remarks>Requires a valid <see cref="SyncRunEntity.Organization"/> to be included</remarks>
        /// <param name="syncRun"></param>
        public OrgSyncResult(SyncRunEntity syncRun) :
            base(false)
        {
            if (syncRun is null)
            {
                throw new System.ArgumentNullException(nameof(syncRun));
            }

            OrgRoute = syncRun.Organization.Route;
        }

        /// <summary>
        /// Constructor when loading from JSON via EF
        /// </summary>
        public OrgSyncResult() { }

        /// <summary>
        /// Totals all of the count properties and returns them
        /// </summary>
        /// <returns></returns>
        protected override int GetActionCount()
        {
            return
                UserNetworkRequests +
                GroupNetworkRequests +
                UsersAdded +
                UsersUpdated +
                PeopleAdded +
                PeopleRemoved +
                GroupsAdded +
                GroupsUpdated +
                GroupsRemoved +
                MembersAdded +
                MembersRemoved;
        }

        #endregion

        #region Persistent Properties

        /// <summary>
        /// Gets or sets the route of the organization associated with the results.
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// The current state of the sync process
        /// </summary>
        public OrgSyncState? State { get; set; }

        /// <summary>
        /// Gets or sets the number of network requests issued to retrieve user data.
        /// </summary>
        public int UserNetworkRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of network requests issued to retrieve group data.
        /// </summary>
        public int GroupNetworkRequests { get; set; }

        /// <summary>
        /// Gets or sets the total number of user records that were created or restored during this run
        /// An added us is ALWAYS counted as a synced user as well
        /// </summary>
        public int UsersAdded { get; set; }

        /// <summary>
        /// Gets or sets the total number of users that were synced during the sync process.  
        /// This value includes users who were deleted during the sync.
        /// </summary>
        public int UsersUpdated { get; set; }

        /// <summary>
        /// Gets or sets the total number of people records that were created OR restored during this run
        /// </summary>
        public int PeopleAdded { get; set; }

        /// <summary>
        /// Get or sets the total number of users that were "removed" during the sync process.
        /// </summary>
        public int PeopleRemoved { get; set; }

        /// <summary>
        /// Gets or sets the count for groups added
        /// An added group is ALWAYS counted as a synced group
        /// </summary>
        public int GroupsAdded { get; set; }

        /// <summary>
        /// Gets or sets the total number of users that were synced during the sync process.  
        /// This value includes groups who were deleted during the sync.
        /// </summary>
        public int GroupsUpdated { get; set; }

        /// <summary>
        /// Get or sets the total number of groups that were soft deleted during the sync process.
        /// </summary>
        public int GroupsRemoved { get; set; }

        /// <summary>
        /// Gets or sets the total number of members that were synced during the sync process.  
        /// This value includes groups who were deleted during the sync.
        /// </summary>
        public int MembersAdded { get; set; }

        /// <summary>
        /// Get or sets the total number of members that were soft deleted during the sync process.
        /// </summary>
        public int MembersRemoved { get; set; }

        /// <see cref="IOrgSyncFields"/>

        /// <summary>
        /// Gets or sets the sanitized JSON to the underlying <see cref="IOrgSyncStrategy"/> config object
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string SyncConfigJson { get; set; }

        /// <summary>
        /// Gets or sets the 
        /// </summary>
        public string SyncType { get; set; }

        #endregion
    }
}
