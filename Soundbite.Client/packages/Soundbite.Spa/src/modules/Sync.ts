import {
  GroupSyncConfig,
  AadOrgConfig,
  UserSyncConfig,
  OrgSyncConfig,
  OktaOrgSyncConfig,
} from "@soundbite/api";

// Core types for the sync process
export enum SyncType {
  Disabled = "Disabled",
  AAD = "AAD", // Masticore.DirectorySync.Aad.AadOrgSyncStrategyBase.Name
  Okta = "OKTA", // Masticore.DirectorySync.Okta.OktaOrgSyncStrategyBase.Name
}

/**
 * Checks if the given two groupings of AadUserConfig match
 * @param left
 * @param right
 */
export const doUsersMatch = (
  left?: UserSyncConfig[],
  right?: UserSyncConfig[]
): boolean => {
  if (left == null && right == null) return true;

  if (left != null && right != null) {
    if (left.length !== right.length) return false;
    else {
      const leftOrderedIds = left
        .map((u) => u.id)
        .sort()
        .join();
      const rightOrderdIds = left
        .map((u) => u.id)
        .sort()
        .join();
      if (leftOrderedIds !== rightOrderdIds) return false;
      else return true;
    }
  } else {
    return false;
  }
};

/**
 * Checks if the given two groupings of AadGroupConfig match
 * @param left
 * @param right
 */
export const doGroupsMatch = (
  left?: GroupSyncConfig[],
  right?: GroupSyncConfig[]
): boolean => {
  if (left == null && right == null) return true;

  if (left != null && right != null) {
    if (left.length !== right.length) return false;
    else {
      const leftOrderedIds = left
        .map((g) => g.groupId)
        .sort()
        .join();
      const rightOrderdIds = left
        .map((g) => g.groupId)
        .sort()
        .join();
      if (leftOrderedIds !== rightOrderdIds) return false;
      else return true;
    }
  } else {
    // Nullness mismatch for left and right
    return false;
  }
};

/**
 * evaluates if the AAD strategies on the left and right configs match
 * Less explicit approaches, like JSON matching at the top-level, do no account variability in cosmetic-only fields
 * */
export const doAadStrategiesMatch = (
  leftJson?: string,
  rightJson?: string
): boolean => {
  if (leftJson == null && rightJson == null) return true;

  if (leftJson != null && rightJson != null) {
    const left = JSON.parse(leftJson as string) as AadOrgConfig;
    const right = JSON.parse(rightJson as string) as AadOrgConfig;

    // Misc
    if (left.importPhoneNumbers !== right.importPhoneNumbers) {
      return false;
    }

    // Connection
    if (left.tenantId !== right.tenantId) {
      return false;
    }
    if (left.appId !== right.appId) {
      return false;
    }
    if (left.secretKey !== right.secretKey) {
      return false;
    }
    // Groups
    else if (left.groupsMode !== right.groupsMode) {
      return false;
    } else if (!doGroupsMatch(left.groups, right.groups)) {
      return false;
    }
    // Users
    else if (left.usersMode !== right.usersMode) {
      return false;
    } else if (!doUsersMatch(left.users, right.users)) {
      return false;
    } else {
      return true;
    }
  } else {
    // Nullness mismatch for left and right
    return false;
  }
};

export const doOktaStrategiesMatch = (
  leftJson?: string,
  rightJson?: string
): boolean => {
  if (leftJson == null && rightJson == null) return true;

  if (leftJson != null && rightJson != null) {
    const left = JSON.parse(leftJson as string) as OktaOrgSyncConfig;
    const right = JSON.parse(rightJson as string) as OktaOrgSyncConfig;

    // Misc
    if (left.importPhoneNumbers !== right.importPhoneNumbers) {
      return false;
    }

    // Connection
    if (left.domain !== right.domain) {
      return false;
    }
    if (left.apiToken !== right.apiToken) {
      return false;
    }

    // Groups
    else if (left.groupsMode !== right.groupsMode) {
      return false;
    } else if (!doGroupsMatch(left.groups, right.groups)) {
      return false;
    }
    // Users
    else if (left.usersMode !== right.usersMode) {
      return false;
    } else if (!doUsersMatch(left.users, right.users)) {
      return false;
    } else {
      return true;
    }
  } else {
    // Nullness mismatch for left and right
    return false;
  }
};

/**
 * Checks if the given two configs match
 * @param left
 * @param right
 */
export const doConfigsMatch = (
  left?: OrgSyncConfig,
  right?: OrgSyncConfig
): boolean => {
  if (left == null && right == null) return true;

  if (left != null && right != null) {
    if (left.syncType !== right.syncType) return false;
    else {
      const leftJson = left.syncConfigJson;
      const rightJson = right.syncConfigJson;

      if (left.syncType !== right.syncType) {
        return false;
      }

      if (left.syncType === SyncType.AAD) {
        return doAadStrategiesMatch(leftJson, rightJson);
      } else if (left.syncType === SyncType.Okta) {
        return doOktaStrategiesMatch(leftJson, rightJson);
      } else {
        return true;
      }
      // Other sync types go here
    }
  } else {
    // Nullness mismatch for left and right
    return false;
  }
};
