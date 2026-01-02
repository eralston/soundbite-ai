using Masticore.Exceptions;
using Masticore.Models;
using System;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Contains <see cref="IOrgSyncFields"/> extension methods.
    /// </summary>
    public static class IOrgSyncFieldsExtensions
    {
        /// <summary>
        /// Builds a typed synchronization configuration object from the organization sync settings.
        /// </summary>
        /// <typeparam name="T">.NET type of the synchronization configuration settings.</typeparam>
        /// <param name="orgSyncFields">Organization sync field from which the sync config object is being built.</param>
        /// <param name="throwOnMissingConfig">Flag indicating whether to throw an error if the JSON configuration is not present.</param>
        /// <param name="throwOnMismatchedConfig">Flag indicating whether to throw an error if the sync types don't match.</param>
        /// <returns>an instance of <typeparamref name="T"/> populated with the appropriate settings from the org sync settings or <c>null</c> if the configuration is missing.</returns>
        /// <exception cref="UserSafeException">Thrown when the configuration is missing the <paramref name="throwOnMissingConfig"/> flag is set to <c>true</c>.</exception>
        public static T GetSyncConfig<T>(this IOrgSyncFields orgSyncFields, bool throwOnMissingConfig = true, bool throwOnMismatchedConfig = true, bool returnObjectOnMismatch = false)
            where T : class, ISyncStrategyConfig
        {
            if (orgSyncFields == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(orgSyncFields.SyncType))
            {
                throw new UserSafeException(Constants.SyncConfigErrorMessages.MissingType);
            }

            if (string.IsNullOrEmpty(orgSyncFields.SyncConfigJson))
            {
                if (throwOnMissingConfig)
                {
                    throw new UserSafeException(Constants.SyncConfigErrorMessages.MissingConfig);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                T result;
                try
                {
                    result = JsonUtils.FromLowerCamelJson<T>(orgSyncFields.SyncConfigJson);
                }
                catch (Exception ex)
                {
                    throw new UserSafeException(Constants.SyncConfigErrorMessages.InvalidJson, ex);
                }

                if (result.SyncType != orgSyncFields.SyncType)
                {
                    if (throwOnMismatchedConfig)
                    {
                        throw new UserSafeException(Constants.SyncConfigErrorMessages.WrongType);
                    }
                    else if (returnObjectOnMismatch)
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }

                return result;
            }
        }
    }
}
