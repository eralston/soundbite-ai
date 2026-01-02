/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { SessionCommentPolicy, SessionSecurityType, SessionType } from '../enums';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Soundbite.Models.Session
*/
export interface Session extends ResourceBase, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  limit: number;
  name: string;
  publish?: string;
  publishSent?: string;
  reminder?: string;
  reminderCalEventId: string;
  reminderSent?: string;
  route: string;
  sessionCommentPolicy: SessionCommentPolicy;
  sessionSecurity: SessionSecurityType;
  sessionType: SessionType;
  transcribe: boolean;
  updatedUtc: string;
}
