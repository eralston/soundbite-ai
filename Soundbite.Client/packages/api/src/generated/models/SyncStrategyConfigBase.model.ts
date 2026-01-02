/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { CollectionSyncMode } from '../enums';
import { GroupSyncConfig } from './GroupSyncConfig.model';
import { UserSyncConfig } from './UserSyncConfig.model';

/** 
* Automatically generated model for Masticore.DirectorySync.SyncStrategyConfigBase
*/
export interface SyncStrategyConfigBase {
  groups?: GroupSyncConfig[];
  groupsMode?: CollectionSyncMode;
  importPhoneNumbers?: boolean;
  syncType: string;
  users?: UserSyncConfig[];
  usersMode?: CollectionSyncMode;
}
