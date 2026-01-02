/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ParticipantRole } from '../enums';
import { Group } from './Group.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Soundbite.Models.ParticipantGroup
*/
export interface ParticipantGroup extends ResourceBase, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  group: Group;
  groupRoute: string;
  participantRole: ParticipantRole;
  route: string;
  sessionRoute: string;
  updatedUtc: string;
}
