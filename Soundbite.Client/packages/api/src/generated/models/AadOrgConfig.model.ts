/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { CollectionSyncMode } from '../enums';
import { GroupSyncConfig } from './GroupSyncConfig.model';
import { SyncStrategyConfigBase } from './SyncStrategyConfigBase.model';
import { UserSyncConfig } from './UserSyncConfig.model';

/** 
* Automatically generated model for Masticore.DirectorySync.Aad.AadOrgConfig
*/
export interface AadOrgConfig extends SyncStrategyConfigBase {
  appId?: string;
  groups?: GroupSyncConfig[];
  groupsMode?: CollectionSyncMode;
  importPhoneNumbers?: boolean;
  secretKey?: string;
  tenantId?: string;
  users?: UserSyncConfig[];
  usersMode?: CollectionSyncMode;
}
