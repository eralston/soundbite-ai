/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { OrgSyncState } from '../enums';
import { OrgSyncFields } from './OrgSyncFields.model';
import { SyncResultBase } from './SyncResultBase.model';

/** 
* Automatically generated model for Masticore.DirectorySync.OrgSyncResult
*/
export interface OrgSyncResult extends SyncResultBase, OrgSyncFields {
  actionCount: number;
  deltaBytes: number;
  deltaTime: number;
  endBytes: number;
  endTime: string;
  errorMessages: string[];
  groupNetworkRequests: number;
  groupsAdded: number;
  groupsRemoved: number;
  groupsUpdated: number;
  isFailed: boolean;
  membersAdded: number;
  membersRemoved: number;
  orgRoute: string;
  peopleAdded: number;
  peopleRemoved: number;
  startBytes: number;
  startTime: string;
  state?: OrgSyncState;
  syncType: string;
  universalId: string;
  userNetworkRequests: number;
  usersAdded: number;
  usersUpdated: number;
}
