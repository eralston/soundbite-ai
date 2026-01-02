using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// <see cref="IMemberSyncStore"/> over Entity Framework that is a child is composed into <see cref="OrgSyncEfStore"/>
    /// </summary>
    public class MemberSyncStore : CompositeSyncStore, IMemberSyncStore
    {
        #region Properties

        /// <summary>
        /// Mapping from compound UniversalId for a <see cref="IGroupFields"/> to a set of universalIds for a <see cref="IUserFields"/>
        /// </summary>
        protected Dictionary<string, HashSet<string>> GroupsToAddWithUserEmails { get; } = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Mapping from <see cref="IGroupFields"/>.UniversalId to a set of <see cref="IUserFields.Email"/> values that need removed from that group
        /// </summary>
        protected Dictionary<string, HashSet<string>> MembersToRemoveByEmail { get; } = new Dictionary<string, HashSet<string>>();

        protected PeopleSyncStore PeopleStore { get; }

        protected GroupSyncStore GroupStore { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor that relies on a parent store and a people store
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="peopleStore"></param>
        public MemberSyncStore(CompositeSyncStore parent, PeopleSyncStore peopleStore, GroupSyncStore groupStore) : base(parent)
        {
            Validator.ArgNotNull(nameof(peopleStore), peopleStore);
            Validator.ArgNotNull(nameof(groupStore), groupStore);

            PeopleStore = peopleStore;
            GroupStore = groupStore;
        }

        private HashSet<string> UsersInGroup(IGroupFields group, MemberSyncOpType memberSyncType)
        {
            if (group is null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            string groupKey = group.CombineGroupKey(memberSyncType);
            GroupsToAddWithUserEmails.TryGetValue(groupKey, out HashSet<string> usersInGroup);

            // If we don't have a dictionary yet for this group, then add it
            if (usersInGroup is null)
            {
                usersInGroup = new HashSet<string>();
                GroupsToAddWithUserEmails[groupKey] = usersInGroup;
            }

            return usersInGroup;
        }

        /// <summary>
        /// Syncs a single member into the org based on the arguments
        /// </summary>
        /// <remarks>Precondition: There must be a person record in the org for the given user</remarks>
        /// <param name="db"></param>
        /// <param name="groupUid"></param>
        /// <param name="userEmail"></param>
        /// <param name="opType"></param>
        /// <returns></returns>
        public async Task SyncMember(ISyncDb db, string groupUid, string userEmail, MemberSyncOpType? opType)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(groupUid))
            {
                throw new ArgumentException($"'{nameof(groupUid)}' cannot be null or empty.", nameof(groupUid));
            }

            if (opType is null)
            {
                throw new ArgumentNullException(nameof(opType));
            }
            // If we need to ignore; should never happen
            if (opType == MemberSyncOpType.SyncNoMembers || opType == null)
            {
                return;
            }

            if (userEmail == null)
            {
                Logger.LogWarning($"Cannot log user in group '{groupUid}' with null email");
                return;
            }

            MemberEntity member = await db.MemberWhereEmail(Config.OrgRoute, groupUid, userEmail, true);

            // Create if syncing all
            if (member == null && opType == MemberSyncOpType.SyncAllMembers)
            {
                member = await CreateMember(db, groupUid, userEmail);
            }

            // Update in both cases
            if (member != null &&
                (opType == MemberSyncOpType.SyncExistingMembers || opType == MemberSyncOpType.SyncAllMembers))
            {
                member.Timestamp();
                if (member.Restore())
                {
                    Result.MembersAdded++;
                }
            }
        }

        private async Task<MemberEntity> CreateMember(ISyncDb db, string groupUid, string userEmail)
        {
            LogDebug($"Created Member for '{groupUid}'");
            MemberEntity member = db.Members.CreateResource(null);
            member.Person = await PeopleStore.PersonForEmail(userEmail);
            member.Person.AssertFound();
            member.Group = await GroupStore.GroupForUid(groupUid);
            member.Group.AssertFound();
            member.MemberRole = MemberRole.Member;
            member.Invite(); // Does NOT send message
            Result.MembersAdded++;
            return member;
        }

        public async Task SyncMembers(ISyncDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            LogDebug($"Syncing Members");
            int count = 0;
            foreach (string groupKey in GroupsToAddWithUserEmails.Keys)
            {
                SyncStoreUtils.SplitGroupKey(groupKey, out string groupUid, out MemberSyncOpType? opType);
                HashSet<string> userEmails = GroupsToAddWithUserEmails[groupKey];
                foreach (string userEmail in userEmails)
                {
                    ++count;
                    await SyncMember(db, groupUid, userEmail, opType);
                }

                if (count % 1028 == 0)
                {
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteMember(ISyncDb db, string groupUid, string userEmail)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (string.IsNullOrEmpty(groupUid))
            {
                throw new ArgumentException($"'{nameof(groupUid)}' cannot be null or empty.", nameof(groupUid));
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentException($"'{nameof(userEmail)}' cannot be null or empty.", nameof(userEmail));
            }

            MemberEntity existingMember = await db.MemberWhereEmail(Config.OrgRoute, groupUid, userEmail);
            if (existingMember != null)
            {
                if (existingMember.DeletedUtc == null)
                {
                    Result.MembersRemoved++;
                }

                existingMember.SoftDelete();
            }
        }

        public async Task DeleteMembers(ISyncDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            LogDebug($"Deleting Members");
            foreach (string groupUid in MembersToRemoveByEmail.Keys)
            {
                HashSet<string> userEmails = MembersToRemoveByEmail[groupUid];
                foreach (string userEmail in userEmails)
                {
                    await DeleteMember(db, groupUid, userEmail);
                }
            }
        }

        #endregion

        #region IMemberSyncStore

        public override async Task EndSync()
        {
            await base.EndSync();
            ISyncDb db = await GetDb();
            await SyncMembers(db);
            await DeleteMembers(db);
        }

        public async Task AddMember(IGroupFields group, IUserFieldsWithAliases user, MemberSyncOpType memberSyncType)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (group is null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                throw new ArgumentException($"'{nameof(user.Email)}' cannot be null or empty.", nameof(user.Email));
            }

            if (string.IsNullOrEmpty(group.UniversalId))
            {
                throw new ArgumentException($"'{nameof(group.UniversalId)}' cannot be null or empty.", nameof(group.UniversalId));
            }

            if (memberSyncType == MemberSyncOpType.SyncNoMembers)
            {
                return;
            }

            LogDebug($"Add member '{user.Email.RedactEmail()}' to group '{group.UniversalId}'");

            // Pulls out the list of Users in this group
            HashSet<string> usersInGroup = UsersInGroup(group, memberSyncType);

            // Add this user as a unique entry for the given users
            await PeopleStore.AddPerson(user, memberSyncType.ToOpType());
            await GroupStore.AddGroup(group, memberSyncType.ToOpType());
            usersInGroup.Add(user.Email.ToEmail());
        }

        public Task RemoveMember(string groupUniversalId, string userEmail)
        {
            if (userEmail == null)
            {
                Logger.LogWarning($"Cannot log user in group '{groupUniversalId}' with null email");
                return Task.CompletedTask;
            }

            if (string.IsNullOrEmpty(groupUniversalId))
            {
                throw new ArgumentException($"'{nameof(groupUniversalId)}' cannot be null or empty.", nameof(groupUniversalId));
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentException($"'{nameof(userEmail)}' cannot be null or empty.", nameof(userEmail));
            }

            LogDebug($"Remove member '{userEmail.RedactEmail()}' from group '{groupUniversalId}'");

            MembersToRemoveByEmail.TryGetValue(groupUniversalId, out HashSet<string> removeEmails);
            if (removeEmails is null)
            {
                removeEmails = new HashSet<string>();
                MembersToRemoveByEmail[groupUniversalId] = removeEmails;
            }

            removeEmails.Add(userEmail);

            return Task.CompletedTask;
        }

        #endregion
    }
}
