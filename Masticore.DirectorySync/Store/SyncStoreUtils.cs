using Masticore.Models;
using Masticore.Resources;
using System;

namespace Masticore.DirectorySync
{
    /// <summary>
    /// Helpers methods for <see cref="IOrgSyncStore"/> and <see cref="IRegionSyncStore"/> operations
    /// </summary>
    internal static class SyncStoreUtils
    {
        public static string CombineUniversalKey(this IUniversal uni, SyncOpType? opType = null)
        {
            if (opType.HasValue)
            {
                return $"{uni.UniversalId}|{opType}";
            }
            else
            {
                return uni.UniversalId;
            }
        }

        public static SyncOpType? SplitOpType(string keyWithOpType)
        {
            string[] parts = keyWithOpType.Split('|');
            if (parts.Length == 1)
            {
                return null;
            }

            SyncOpType type = Enum.Parse<SyncOpType>(parts[1]);
            return type;
        }

        public static SyncOpType ToOpType(this MemberSyncOpType memberSyncType)
        {
            return memberSyncType switch
            {
                MemberSyncOpType.SyncAllMembers => SyncOpType.AddOrUpdate,
                MemberSyncOpType.SyncExistingMembers => SyncOpType.UpdateOnly,
                MemberSyncOpType.SyncNoMembers => SyncOpType.Ignore,
                _ => SyncOpType.AddOrUpdate,
            };
        }

        public static string CombineGroupKey(this IGroupFields groupFields, MemberSyncOpType? memberSyncType)
        {
            if (memberSyncType.HasValue)
            {
                return $"{groupFields.UniversalId}|{memberSyncType}";
            }
            else
            {
                return groupFields.UniversalId;
            }
        }

        public static void SplitGroupKey(string groupKey, out string groupUniversalId, out MemberSyncOpType? memberSyncType)
        {
            string[] parts = groupKey.Split('|');
            groupUniversalId = parts[0];
            if (parts.Length > 1)
            {
                MemberSyncOpType type = Enum.Parse<MemberSyncOpType>(parts[1]);
                memberSyncType = type;
            }
            else
            {
                memberSyncType = null;
            }
        }
    }
}
