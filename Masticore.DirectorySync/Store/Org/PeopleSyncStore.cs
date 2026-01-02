using Masticore.Entity;
using Masticore.Models;
using Masticore.Resources;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// <see cref="IPeopleSyncStore"/> over Entity Framework that is a child is composed into <see cref="OrgSyncStore"/>
    /// </summary>
    public class PeopleSyncStore : CompositeSyncStore, IPeopleSyncStore
    {
        #region Properties

        /// <summary>
        /// Mapping from UniversalId+OpType to new <see cref="IUserFieldsWithAliases"/> object
        /// </summary>
        protected Dictionary<string, IUserFieldsWithAliases> UsersByOpKey { get; } = new Dictionary<string, IUserFieldsWithAliases>();
        /// <summary>
        /// Set of e-mails to remove in this process
        /// </summary>
        protected HashSet<string> UsersToRemoveByEmail { get; } = new HashSet<string>();

        protected Dictionary<string, UserEntity> UsersByEmail { get; } = new Dictionary<string, UserEntity>();
        protected Dictionary<string, PersonEntity> PeopleByEmail { get; } = new Dictionary<string, PersonEntity>();

        /// <summary>
        /// If true, then the sync will import phone numbers from the directory; off by default
        /// </summary>
        public bool? ImportPhoneNumbers { get; set; } = false;

        #endregion

        #region Methods
        public PeopleSyncStore(CompositeSyncStore parent) : base(parent)
        {
        }

        public async Task<PersonEntity> PersonForEmail(string userEmail)
        {
            ISyncDb db = await GetDb();
            return await PersonForEmail(userEmail, db);
        }

        /// <summary>
        /// Queries for the <see cref="PersonEntity"/> related to the given e-mail, checking the cache first
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        protected async Task<PersonEntity> PersonForEmail(string userEmail, ISyncDb db)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentException($"'{nameof(userEmail)}' cannot be null or empty.", nameof(userEmail));
            }

            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            // If we have it, then send it back
            userEmail = userEmail.ToEmail();
            PeopleByEmail.TryGetValue(userEmail, out PersonEntity person);
            if (person != null)
            {
                return person;
            }

            // If we found it, then hold onto it
            person = await db.PersonWhereEmail(Config.OrgRoute, userEmail, true, false, ProviderType);
            if (person != null)
            {
                PeopleByEmail[userEmail] = person;
            }

            return person;
        }

        /// <summary>
        /// Soft deletes the user record for the given user email address
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        private async Task DeletePerson(ISyncDb db, string userEmail)
        {
            PersonEntity person = await PersonForEmail(userEmail, db);

            if (person != null)
            {
                if (person.DeletedUtc == null)
                {
                    Result.PeopleRemoved++;
                }

                person.SoftDelete();
            }
        }

        /// <summary>
        /// Soft deletes those listed in <see cref="UsersToRemoveByEmail"/>
        /// </summary>
        /// <returns></returns>
        private async Task DeletePeople(ISyncDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (UsersToRemoveByEmail.Count == 0)
            {
                return;
            }

            foreach (string userEmail in UsersToRemoveByEmail)
            {
                await DeletePerson(db, userEmail);
            }
        }

        /// <summary>
        /// Responsible for setting the UPN for teams messaging. 
        /// </summary>
        /// <param name="userInfo">Incoming user information from sync.</param>
        /// <param name="userEntity">User entity (may be new or existing) on which to set the UPN.</param>
        private void SetUpnForTeamsMessaging(IUserFieldsWithAliases userInfo, UserEntity userEntity)
        {
            string upn = userInfo?.Upn?.Trim();
            bool upnAvailable = !string.IsNullOrEmpty(upn);

            if (upnAvailable)
            {
                string redactedEmail = userInfo.Email.RedactEmail();
                string redactedUpn = upn.RedactEmail();
                Logger.LogInformation("Found UPN {0} for user with email {1}", redactedUpn, redactedEmail);

                UserSettings userSettings = JsonUtils.FromLowerCamelJson<UserSettings>(userEntity.ConfigJson, false);
                userSettings ??= new UserSettings();
                userSettings.MsTeams ??= new UserMsTeamsSettings();
                userSettings.MsTeams.AltId = upn;
                userEntity.ConfigJson = JsonUtils.ToLowerCamelJson(userSettings);
            }
        }

        private UserEntity CreateUser(ISyncDb db, IUserFieldsWithAliases user)
        {
            LogDebug($"Creating User Entity for '{user.Email.RedactEmail()}'");
            UserEntity userEnt = db.Users.CreateResource(null);
            string email = user.Email.ToEmail();
            userEnt.Email = email;
            userEnt.Invite(); // Does NOT send message
            userEnt.UserRole = UserRole.User;
            userEnt.AllowEmail = true;
            userEnt.AllowMarketing = true;
            userEnt.AllowNews = true;
            userEnt.AllowSms = true;

            SetUpnForTeamsMessaging(user, userEnt);

            Result.UsersAdded++;
            UsersByEmail[email] = userEnt;
            return userEnt;
        }

        /// <summary>
        /// Creates a new <see cref="PersonEntity"/> in the given <see cref="ISyncDb"/> connected to the given <see cref="UserEntity"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userEntitiy"></param>
        /// <returns></returns>
        private async Task<PersonEntity> CreatePerson(ISyncDb db, UserEntity userEntitiy, IUserFieldsWithAliases user)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (userEntitiy is null)
            {
                throw new ArgumentNullException(nameof(userEntitiy));
            }

            if (string.IsNullOrEmpty(userEntitiy.Email))
            {
                throw new ArgumentException($"'{nameof(userEntitiy.Email)}' cannot be null or empty.", nameof(userEntitiy.Email));
            }

            LogDebug($"Creating Person Entity for '{userEntitiy.Email.RedactEmail()}'");

            PersonEntity person = db.People.CreateResource(null);
            person.User = userEntitiy;
            person.Organization = await GetOrg();
            person.PersonRole = PersonRole.Person;
            person.Invite(); // Does NOT send messages
            PeopleByEmail[userEntitiy.Email] = person;

            // Include mapping by alias email
            if (user.AliasEmails != null && user.AliasEmails.Length > 0)
            {
                foreach (string aliasEmail in user.AliasEmails)
                {
                    if (string.IsNullOrEmpty(aliasEmail))
                    {
                        continue;
                    }
                    PeopleByEmail[aliasEmail] = person;
                }
            }

            Result.PeopleAdded++;
            return person;
        }


        /// <summary>
        /// Syncs the given person using their full fields and the given operation
        /// </summary>
        /// <param name="user"></param>
        /// <param name="opType"></param>
        /// <returns></returns>
        private async Task SyncPerson(ISyncDb db, IUserFieldsWithAliases user, SyncOpType? opType)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // If we need to ignore; should never happen
            if (opType == SyncOpType.Ignore || opType == null)
            {
                return;
            }

            // If we need to remove them, then skip
            if (opType == SyncOpType.Remove)
            {
                // Queue for removal later
                await RemovePerson(user.Email);
                return;
            }

            if (user.Email == null)
            {
                Logger.LogWarning($"Cannot sync user with null email with ID '{user.UniversalId} in org '{Config.OrgRoute}'");
                return;
            }

            // Find a user in the system, even if they're deleted
            UsersByEmail.TryGetValue(user.Email, out UserEntity userEnt);
            if (userEnt == null)
            {
                userEnt = await db.UserWhereEmail(user.Email, ProviderType, true);
                if (userEnt != null)
                {
                    UsersByEmail[user.Email] = userEnt;
                }
            }

            // If we need to create it
            if (userEnt == null && opType == SyncOpType.AddOrUpdate)
            {
                userEnt = CreateUser(db, user);
            }

            // If we need to update it
            if (userEnt != null &&
                    (opType == SyncOpType.AddOrUpdate ||
                    opType == SyncOpType.UpdateOnly))
            {
                UpdateUser(user, userEnt);

                await AddUserAliases(db, user, userEnt);
            }

            // If we're adding or updating, then we connect the user to the org
            if (userEnt != null && opType == SyncOpType.AddOrUpdate)
            {
                LogDebug($"Restoring User Entity for '{user.Email.RedactEmail()}'");
                if (userEnt.Restore())
                {
                    Result.UsersAdded++;
                }

                PersonEntity perEnt = await PersonForEmail(userEnt.Email, db);
                // Create if missing
                perEnt ??= await CreatePerson(db, userEnt, user);
                if (perEnt.Restore())
                {
                    Result.PeopleAdded++;
                }
            }
        }

        /// <summary>
        /// Create or restore alternate email addresses for this directory
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        /// <param name="userEnt"></param>
        /// <returns></returns>
        private async Task AddUserAliases(ISyncDb db, IUserFieldsWithAliases user, UserEntity userEnt)
        {
            UserIdentityEntity[] identityEntities = await db.UserIdentitiesWhereUserId(userEnt.Id);
            Dictionary<string, UserIdentityEntity> identities = identityEntities.ToDictionary(p => $"{p.Identifier}|{p.ProviderType}");

            // alias e-mails is optional
            if (user.AliasEmails == null || user.AliasEmails.Length == 0)
            {
                return;
            }

            foreach (string aliasEmail in user.AliasEmails)
            {
                if (string.IsNullOrEmpty(aliasEmail))
                {
                    continue;
                }

                UsersByEmail[aliasEmail] = userEnt;
                UserIdentityEntity identity = null;

                // We can only affect emails associated with this provider
                string key = $"{aliasEmail}|{ProviderType}";
                if (identities.TryGetValue(key, out identity))
                {
                    // Already exists, so just make sure it's live
                    identity.Restore();
                }
                else
                {
                    // We need to consider if this alias email + provider is already in the system
                    // If so, then this user is claiming it for themselves since it's the latest owner of the alias
                    identity = await db.UserIdentityForIdentifier(aliasEmail, ProviderType);
                    if (identity != null)
                    {
                        identity.User = userEnt;
                        identity.Restore();
                    }
                    else
                    {
                        // If we don't already have one, then we create a whole new record
                        identity = db.UserIdentities.CreateResource(null);
                        identity.Identifier = aliasEmail;
                        identity.User = userEnt;
                        identity.ProviderType = ProviderType;
                    }
                }
            }
        }

        private void UpdateUser(IUserFieldsWithAliases user, UserEntity userEnt)
        {
            LogDebug($"Updating User Entity for '{user.Email.RedactEmail()}'");

            // Mutable fields
            userEnt.GivenName = user.GivenName;
            userEnt.FamilyName = user.FamilyName;
            userEnt.Title = user.Title;

            // Fixed by the sync provider
            userEnt.ProviderType = ProviderType;

            SetUpnForTeamsMessaging(user, userEnt);

            if (ImportPhoneNumbers.HasValue && ImportPhoneNumbers.Value == true)
            {
                userEnt.Phone = user.Phone;
            }

            userEnt.Timestamp();
            Result.UsersUpdated++;
        }

        private async Task SyncPeople(ISyncDb db)
        {
            if (db is null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (UsersByOpKey.Count == 0)
            {
                return;
            }

            int count = 0;
            foreach (string key in UsersByOpKey.Keys)
            {
                IUserFieldsWithAliases user = UsersByOpKey[key];
                SyncOpType? opType = SyncStoreUtils.SplitOpType(key);
                await SyncPerson(db, user, opType);
                ++count;
                if (count % 1028 == 0)
                {
                    await db.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region IPeopleSyncStore

        public Task AddPerson(IUserFieldsWithAliases userFields, SyncOpType syncType)
        {
            if (userFields is null)
            {
                throw new ArgumentNullException(nameof(userFields));
            }

            if (string.IsNullOrEmpty(userFields.Email))
            {
                throw new ArgumentException($"'{nameof(userFields.Email)}' cannot be null or empty.", nameof(userFields.Email));
            }

            if (userFields.Email == null)
            {
                Logger.LogWarning($"Cannot add person with null email with ID '{userFields.UniversalId} in org '{Config.OrgRoute}'");
                return Task.CompletedTask;
            }

            LogDebug($"Adding person '{userFields.Email.RedactEmail()}'");

            string userKey = userFields.CombineUniversalKey(syncType);
            UsersByOpKey[userKey] = userFields;
            return Task.CompletedTask;
        }

        public Task RemovePerson(string userEmail)
        {
            if (userEmail == null)
            {
                Logger.LogWarning($"Cannot remove user with null email in org '{Config.OrgRoute}'");
                return Task.CompletedTask;
            }

            LogDebug($"Removing person '{userEmail.RedactEmail()}'");

            UsersToRemoveByEmail.Add(userEmail.ToEmail());
            return Task.CompletedTask;
        }

        public override async Task<Guid> StartSync(OrgSyncConfig orgConfig, OrgSyncResult orgResult)
        {
            Guid ret = await base.StartSync(orgConfig, orgResult);
            if (orgConfig != null && !string.IsNullOrWhiteSpace(orgConfig.SyncConfigJson))
            {
                SyncStrategyConfigReader baseConfig = orgConfig.GetSyncConfig<SyncStrategyConfigReader>(false, false, true);
                ImportPhoneNumbers = baseConfig?.ImportPhoneNumbers ?? false;
            }

            return ret;
        }

        public override async Task EndSync()
        {
            await base.EndSync();

            ISyncDb db = await GetDb();
            await SyncPeople(db);
            await DeletePeople(db);
        }

        #endregion
    }
}
