using Azure.Storage;
using System;

namespace Masticore.Storage
{
    /// <summary>
    /// Extensions affecting types in Azure.Storage SDK
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        /// Converts the given connectionstring to a StorageSharedKeyCredential
        /// This is useful for requesting SAS tokens
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static StorageSharedKeyCredential ToStorageSharedKeyCredential(this string connectionString)
        {
            if (connectionString is null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            string[] parts = connectionString.Split(';');
            string accountName = null;
            string accountKey = null;
            foreach (string part in parts)
            {
                string[] keyValue = part.Split(new[] { '=' }, 2);
                if (keyValue[0].Equals("AccountName", StringComparison.CurrentCultureIgnoreCase))
                {
                    accountName = keyValue[1];
                }

                if (keyValue[0].Equals("AccountKey", StringComparison.CurrentCultureIgnoreCase))
                {
                    accountKey = keyValue[1];
                }
            }

            if (accountName is null)
            {
                throw new ArgumentNullException(nameof(accountName));
            }

            if (accountKey is null)
            {
                throw new ArgumentNullException(nameof(accountKey));
            }

            StorageSharedKeyCredential sharedKey = new StorageSharedKeyCredential(accountName, accountKey);
            return sharedKey;
        }
    }
}
