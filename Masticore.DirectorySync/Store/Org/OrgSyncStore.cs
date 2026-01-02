using Masticore.Entity;
using Masticore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// <see cref="IOrgSyncStore"/> over Entity Framework
    /// </summary>
    /// <remarks>This is a composite of <see cref="PeopleSyncStore"/>, <see cref="GroupSyncStore"/>, and <see cref="MemberSyncStore"/></remarks>
    public class OrgSyncStore : CompositeSyncStore, IOrgSyncStore
    {
        #region Properties

        // Child Store Composite Pattern

        private PeopleSyncStore _peopleStore = null;
        public PeopleSyncStore PeopleStore
        {
            get
            {
                if (_peopleStore == null)
                {
                    _peopleStore = new PeopleSyncStore(this)
                    {
                        ProviderType = ProviderType
                    };
                }
                return _peopleStore;
            }
        }

        private GroupSyncStore _groupStore = null;
        public GroupSyncStore GroupStore
        {
            get
            {
                if (_groupStore == null)
                {
                    _groupStore = new GroupSyncStore(this)
                    {
                        ProviderType = ProviderType
                    };
                }
                return _groupStore;
            }
        }

        private MemberSyncStore _memberStore = null;
        public MemberSyncStore MemberStore
        {
            get
            {
                if (_memberStore == null)
                {
                    _memberStore = new MemberSyncStore(this, PeopleStore, GroupStore)
                    {
                        ProviderType = ProviderType
                    };
                }
                return _memberStore;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor for <see cref="OrgSyncStore"/>
        /// </summary>
        /// <param name="logger"></param>
        public OrgSyncStore(ILogger<OrgSyncStore> logger, ISyncInfrastucture dbProvider) :
            base(logger, dbProvider)
        {
        }

        /// <summary>
        /// Set this store and its children to the given provider, marking all users created as synced from that type
        /// </summary>
        /// <param name="providerType"></param>
        public void SetProviderType(ProviderType providerType)
        {
            ProviderType = providerType;
            PeopleStore.ProviderType = providerType;
            GroupStore.ProviderType = providerType;
            MemberStore.ProviderType = providerType;
        }

        #endregion

        #region IOrgSyncStore

        /// <summary>
        /// Saves the provider
        /// This will only persist upon successfully calling <see cref="EndSync"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providerConfig"></param>
        /// <returns></returns>
        public async Task SaveStrategyConfig<T>(T providerConfig)
        {
            LogDebug("Saving provider config");
            OrganizationEntity org = await GetOrg();
            org.SyncConfigJson = providerConfig.ToLowerCamelJson();
        }

        // Sync

        /// <summary>
        /// Async starts the sync process
        /// </summary>
        /// <param name="orgConfig"></param>
        /// <param name="orgResult"></param>
        /// <returns></returns>
        public override async Task<Guid> StartSync(OrgSyncConfig orgConfig, OrgSyncResult orgResult)
        {
            await base.StartSync(orgConfig, orgResult);

            // Can we use the DB and reach the org for our current orgConfig and dbProvider?
            await GetOrg();

            await PeopleStore.StartSync(orgConfig, orgResult);
            await GroupStore.StartSync(orgConfig, orgResult);
            await MemberStore.StartSync(orgConfig, orgResult);

            return SyncRunId;
        }

        /// <summary>
        /// Async ends and commits the sync process
        /// </summary>
        /// <returns></returns>
        public override async Task EndSync()
        {
            await base.EndSync();

            // Users/People
            await PeopleStore.EndSync();
            await GroupStore.EndSync();
            await MemberStore.EndSync();

            ISyncDb db = await GetDb();
            await db.SaveChangesAsync();
        }

        // People

        public Task AddPerson(IUserFieldsWithAliases user, SyncOpType syncType)
        {
            return PeopleStore.AddPerson(user, syncType);
        }

        public Task RemovePerson(string userEmail)
        {
            return PeopleStore.RemovePerson(userEmail);
        }

        // Group

        public Task AddGroup(IGroupFields group, SyncOpType opType)
        {
            return GroupStore.AddGroup(group, opType);
        }

        public Task RemoveGroup(string groupUniversalId)
        {
            return GroupStore.RemoveGroup(groupUniversalId);
        }

        // Member

        public Task AddMember(IGroupFields group, IUserFieldsWithAliases user, MemberSyncOpType memberOpType)
        {
            return MemberStore.AddMember(group, user, memberOpType);
        }

        public Task RemoveMember(string groupUniversalId, string userEmail)
        {
            return MemberStore.RemoveMember(groupUniversalId, userEmail);
        }

        #endregion
    }
}
