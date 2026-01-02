using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// <see cref="IGroupSyncStore"/> over Entity Framework that is a child is composed into <see cref="OrgSyncStore"/>
    /// </summary>
    public class GroupSyncStore : CompositeSyncStore, IGroupSyncStore
    {
        #region Properties

        /// <summary>
        /// Mapping from UniversalId+OpType to <see cref="IGroupFields"/> object
        /// </summary>
        protected Dictionary<string, IGroupFields> GroupsByOpKey { get; } = new Dictionary<string, IGroupFields>();

        /// <summary>
        /// A list of group universal IDs to remove from the system
        /// </summary>
        protected HashSet<string> GroupsToRemoveByUid { get; } = new HashSet<string>();

        protected Dictionary<string, GroupEntity> GroupsByUid { get; } = new Dictionary<string, GroupEntity>();

        #endregion

        #region Methods

        /// <summary>
        /// Constructor that composes this object around the given <see cref="CompositeSyncStore"/> parent
        /// </summary>
        /// <param name="parent"></param>
        public GroupSyncStore(CompositeSyncStore parent) : base(parent)
        {
        }

        public async Task<GroupEntity> GroupForUid(string groupUniversalId)
        {
            ISyncDb db = await GetDb();
            return await GroupForUid(groupUniversalId, db);
        }

        protected async Task<GroupEntity> GroupForUid(string groupUniversalId, ISyncDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(groupUniversalId))
            {
                throw new ArgumentException($"'{nameof(groupUniversalId)}' cannot be null or empty.", nameof(groupUniversalId));
            }

            // If we have it, send it back
            GroupsByUid.TryGetValue(groupUniversalId, out GroupEntity group);
            if (group != null)
            {
                return group;
            }

            // If we can find it, then hold onto it
            group = await db.GroupWhereUid(Config.OrgRoute, groupUniversalId);
            if (group != null)
            {
                GroupsByUid[groupUniversalId] = group;
            }

            return group;
        }

        private async Task SyncGroup(ISyncDb db, IGroupFields groupFields, SyncOpType? opType)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (groupFields is null)
            {
                throw new ArgumentNullException(nameof(groupFields));
            }

            // If it's an ignore op, then skip; this should never happen
            if (opType == SyncOpType.Ignore)
            {
                return;
            }

            // If it's a remove, then redirect to the remove method
            if (opType == SyncOpType.Remove)
            {
                await RemoveGroup(groupFields.UniversalId);
                return;
            }

            GroupEntity grpEnt = await GroupForUid(groupFields.UniversalId, db);

            // If we're adding or removing, then ensure it exists
            if (grpEnt == null && opType == SyncOpType.AddOrUpdate)
            {
                grpEnt = await CreateGroup(db, groupFields);
            }

            // If we're adding or update, then update fields
            if (grpEnt != null && (opType == SyncOpType.AddOrUpdate || opType == SyncOpType.UpdateOnly))
            {
                UpdateGroup(groupFields, grpEnt);
            }

            // If we're adding, then make sure it's alive
            if (grpEnt != null && opType == SyncOpType.AddOrUpdate)
            {
                LogDebug($"Restoring Group Entity for {groupFields.UniversalId}");
                if (grpEnt.Restore())
                {
                    Result.GroupsAdded++;
                }
            }
        }

        private void UpdateGroup(IGroupFields groupFields, GroupEntity grpEnt)
        {
            LogDebug($"Updating Group Entity for {groupFields.UniversalId}");
            // TODO: Multi-directory orgs could end up with groups with the same name. What to do then?
            grpEnt.Name = groupFields.Name;
            grpEnt.Description = groupFields.Description.Truncate(Constants.Models.Groups.GroupNameMaxLength);
            grpEnt.Timestamp();
            Result.GroupsUpdated++;
        }

        private async Task<GroupEntity> CreateGroup(ISyncDb db, IGroupFields groupFields)
        {
            GroupEntity grpEnt;
            LogDebug($"Creating Group Entity for {groupFields.UniversalId}");
            grpEnt = db.Groups.CreateResource(null);
            grpEnt.Organization = await GetOrg();
            // Groups are subjective to the org
            grpEnt.UniversalId = groupFields.UniversalId;
            GroupsByUid[groupFields.UniversalId] = grpEnt;
            Result.GroupsAdded++;
            return grpEnt;
        }

        private async Task SyncGroups(ISyncDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            LogDebug($"Syncing Groups");
            foreach (string key in GroupsByOpKey.Keys)
            {
                IGroupFields grpFields = GroupsByOpKey[key];
                SyncOpType? opType = SyncStoreUtils.SplitOpType(key);
                await SyncGroup(db, grpFields, opType);
            }
        }

        private async Task DeleteGroups(ISyncDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            LogDebug($"Deleting Groups");
            foreach (string grpUid in GroupsToRemoveByUid)
            {
                GroupEntity grp = await GroupForUid(grpUid, db);
                if (grp != null)
                {
                    if (grp.DeletedUtc == null)
                    {
                        Result.GroupsRemoved++;
                    }

                    grp.SoftDelete();
                }
            }
        }

        #endregion

        #region IGroupSyncStore

        public Task AddGroup(IGroupFields groupFields, SyncOpType opType)
        {
            if (groupFields is null)
            {
                throw new ArgumentNullException(nameof(groupFields));
            }

            if (string.IsNullOrEmpty(groupFields.UniversalId))
            {
                throw new ArgumentException($"'{nameof(groupFields.UniversalId)}' cannot be null or empty.", nameof(groupFields.UniversalId));
            }

            LogDebug($"Adding group '{groupFields.UniversalId}'");

            string key = groupFields.CombineUniversalKey(opType);
            GroupsByOpKey[key] = groupFields;

            return Task.CompletedTask;
        }

        public Task RemoveGroup(string groupUniversalId)
        {
            if (string.IsNullOrEmpty(groupUniversalId))
            {
                throw new ArgumentException($"'{nameof(groupUniversalId)}' cannot be null or empty.", nameof(groupUniversalId));
            }

            LogDebug($"Removing group '{groupUniversalId}'");
            GroupsToRemoveByUid.Add(groupUniversalId);
            return Task.CompletedTask;
        }

        public override async Task EndSync()
        {
            await base.EndSync();
            ISyncDb db = await GetDb();
            await SyncGroups(db);
            await DeleteGroups(db);
        }

        #endregion
    }
}
