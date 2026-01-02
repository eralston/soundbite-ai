using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Static Partial Class containing database extension methods for EnvSettings.
    /// </summary>
    public static partial class DbExtensions
    {
        /// <summary>
        /// Saves an EnvSettting
        /// </summary>
        /// <param name="db">Reference to the identity database.</param>
        /// <param name="setting">Reference to the environmental setting to save.</param>
        /// <param name="commitDbChanges">Flag indicating whether or not to commit database changes immediately.</param>
        /// <returns>a reference to the EnvSetting instance with appropriate updates from the save.</returns>
        public static async Task<EnvSetting> SaveEnvSetting(this IdentityDb db, EnvSetting setting)
        {
            Validator.NotNull(db, nameof(db));
            Validator.NotNull(setting, nameof(setting));

            // Attempt to locate the record first by ID and if not found then by name/groupkey
            EnvSetting record = await db.EnvSettings.FirstOrDefaultAsync(i =>
                i.ID == setting.ID || (i.Name == setting.Name && i.GroupKey == setting.GroupKey));

            if (record != null)
            {
                record.Description = setting.Description;
                record.GroupKey = setting.GroupKey;
                record.IsSecure = setting.IsSecure;
                record.Name = setting.Name;
                record.Value = setting.Value;
                db.EnvSettings.Update(record);
            }
            else
            {
                record = setting;
                db.EnvSettings.Add(record);
            }

            await db.SaveChangesAsync();

            return await ReadEnvSettingByName(db, setting.Name, setting.GroupKey);
        }

        /// <summary>
        /// Saves an EnvSettting
        /// </summary>
        /// <param name="db">Reference to the identity database.</param>
        /// <param name="setting">Reference to the environmental setting to save.</param>
        /// <returns>a reference to the EnvSetting instance with appropriate updates from the save.</returns>
        public static async Task HardDeleteEnvSetting(this IdentityDb db, string name, string groupKey)
        {
            Validator.NotNull(db, nameof(db));
            Validator.NotNull(name, nameof(name));
            EnvSetting setting = await ReadEnvSettingByName(db, name, groupKey);
            if (setting != null)
            {
                db.EnvSettings.Remove(setting);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Responsible for reading a single environment setting by name and group key.  When the 
        /// <paramref name="groupKey"/> argument is not supplied an environment setting with the
        /// specified <paramref name="name"/> will be sought with a null group key which are 
        /// generally reserved for values used across all applications and environments.
        /// </summary>
        /// <param name="db">Reference to the identity database.</param>
        /// <param name="name">Name of the parameter being sought.</param>
        /// <param name="groupKey">Name of the group key associated with the setting.</param>
        /// <param name="throwIfNotFound">Flag specifying whether or not to throw an error if the setting is not found (default is true).</param>
        /// <returns>a <see cref="EnvSetting"/> populated with data from the located environment setting or <c>null</c> if the setting is not found.</returns>
        public static async Task<EnvSetting> ReadEnvSettingByName(this IIdentityDb db, string name, string groupKey = null, bool throwIfNotFound = true)
        {
            Validator.NotNull(db, nameof(db));
            Validator.NotNullOrEmpty(name, nameof(name));

            bool hasGroupKey = !string.IsNullOrEmpty(groupKey);
            IQueryable<EnvSetting> query = db.EnvSettings.Where(i => i.Name == name);

            // Only apply the group key constraint if it is explicitly defined
            query = !hasGroupKey ? query : query.Where(i => i.GroupKey == groupKey);

            // Acquire query results
            IList<EnvSetting> results = await query.ToListAsync();
            EnvSetting result = null;

            // Determine whether there are multiple results
            if (results.Count > 1)
            {
                if (hasGroupKey)
                {
                    // When multiples are found and the group key was specified then something is wrong
                    throw new Exception($"Multiple Environmental Settings named '{name}' with group key '{groupKey}' were found.");
                }
                else
                {
                    // When no group key was specified then look for a null group key
                    result = results.FirstOrDefault(i => i.GroupKey == null);
                }
            }
            else if (results.Count > 0)
            {
                // Only one result so take it regardless
                result = results[0];
            }

            // Only throw exception if requested and not found
            if (throwIfNotFound && result == null)
            {
                throw new Exception(hasGroupKey
                    ? $"Environmental Settings named '{name}' with group key '{groupKey}' was not found."
                    : $"Environmental Settings named '{name}' was not found.");
            }

            return result;
        }

        /// <summary>
        /// Responsible for reading a single environment setting by name and group key.  When the 
        /// <paramref name="groupKey"/> argument is not supplied an environment setting with the
        /// specified <paramref name="name"/> will be sought with a null group key which are 
        /// generally reserved for values used across all applications and environments.
        /// </summary>
        /// <param name="db">Reference to the identity database.</param>
        /// <param name="name">Name of the parameter being sought.</param>
        /// <param name="groupKey">Name of the group key associated with the setting.</param>
        /// <returns>a <see cref="EnvSetting"/> populated with data from the located environment setting or <c>null</c> if the setting is not found.</returns>
        public static async Task<IList<EnvSetting>> ReadEnvSettingsByGroupKey(this IIdentityDb db, string groupKey)
        {
            Validator.NotNull(db, nameof(db));
            IQueryable<EnvSetting> query = !string.IsNullOrEmpty(groupKey)
                ? db.EnvSettings.Where(i => i.GroupKey == groupKey)
                : db.EnvSettings.Where(i => i.GroupKey == null || i.GroupKey == "");
            return await query.ToListAsync();
        }

        //TODO: Implement a GetByGroupKeys that can retrieve / sort env variables from multiple group keys in a single call.
        //TODO: when done with above, make sure to implement it in teh SbConfigProvider
    }
}
