/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { CollectionSyncMode } from '../enums';
import { GroupSyncConfig } from './GroupSyncConfig.model';
import { SyncStrategyConfigBase } from './SyncStrategyConfigBase.model';
import { UserSyncConfig } from './UserSyncConfig.model';

/** 
* Automatically generated model for Masticore.DirectorySync.Okta.OktaOrgSyncConfig
*/
export interface OktaOrgSyncConfig extends SyncStrategyConfigBase {
  apiToken: string;
  domain: string;
  groups?: GroupSyncConfig[];
  groupsMode?: CollectionSyncMode;
  importPhoneNumbers?: boolean;
  isPartialSync?: boolean;
  maxRunLength?: number;
  modifiedDateFilterEnd?: string;
  modifiedDateFilterStart?: string;
  users?: UserSyncConfig[];
  usersMode?: CollectionSyncMode;
  userSyncComplete?: boolean;
}
