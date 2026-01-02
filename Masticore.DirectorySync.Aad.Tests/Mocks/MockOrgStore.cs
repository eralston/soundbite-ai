using Masticore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Aad.Tests
{
    /// <summary>
    /// Mock implementation of <see cref="IOrgSyncStore"/>
    /// </summary>
    public class MockOrgStore : IOrgSyncStore
    {
        #region Test Properties

        public bool IsStarted { get; protected set; } = false;
        public bool IsEnded { get; protected set; } = false;
        public bool IsConfigSaved { get; protected set; } = false;

        // Groups
        public List<string> AddGroupIds { get; } = new List<string>();
        public List<string> RemoveGroupIds { get; } = new List<string>();

        // Members
        public List<string> AddMemberEmails { get; } = new List<string>();
        public List<string> RemoveMemberEmails { get; } = new List<string>();

        // People
        public List<string> AddPersonEmails { get; } = new List<string>();
        public List<string> RemovePersonEmails { get; } = new List<string>();

        public ProviderType ProviderType { get; set; } = ProviderType.AAD;
        #endregion

        #region IOrgSyncStore

        public Task<Guid> StartSync(OrgSyncConfig _, OrgSyncResult __)
        {
            IsStarted = true;
            return Task.FromResult(Guid.NewGuid());
        }

        public Task EndSync()
        {
            IsEnded = true;
            return Task.CompletedTask;
        }

        // Members

        public Task AddMember(IGroupFields group, IUserFieldsWithAliases user, MemberSyncOpType memberSyncType)
        {
            AddMemberEmails.Add(user.Email);
            return Task.CompletedTask;
        }

        public Task RemoveMember(string groupUid, string userEmail)
        {
            RemoveMemberEmails.Add(userEmail);
            return Task.CompletedTask;
        }

        // Groups

        public Task AddGroup(IGroupFields groupFields, SyncOpType opType)
        {
            AddGroupIds.Add(groupFields.UniversalId);
            return Task.CompletedTask;
        }

        public Task RemoveGroup(string externalId)
        {
            RemoveGroupIds.Add(externalId);
            return Task.CompletedTask;
        }

        // People

        public Task AddPerson(IUserFieldsWithAliases userFields, SyncOpType opType)
        {
            AddPersonEmails.Add(userFields.Email);
            return Task.CompletedTask;
        }

        public Task RemovePerson(string userEmail)
        {
            RemovePersonEmails.Add(userEmail);
            return Task.CompletedTask;
        }

        // Config

        public Task SaveStrategyConfig<T>(T configObj)
        {
            IsConfigSaved = true;
            return Task.CompletedTask;
        }

        public Task SetCommandTimeout(int seconds)
        {
            // This does nothing in a mock
            return Task.CompletedTask;
        }

        public void SetProviderType(ProviderType providerType)
        {
            // Do nothing
        }

        #endregion
    }
}