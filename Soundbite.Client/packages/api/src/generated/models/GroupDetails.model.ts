/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { MemberRole } from '../enums';
import { Group } from './Group.model';
import { GroupFields } from './GroupFields.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';

/** 
* Automatically generated model for Masticore.Models.GroupDetails
*/
export interface GroupDetails extends Group, GroupFields, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  description: string;
  imageSrc: string;
  isAccepting: boolean;
  memberRole: MemberRole;
  name: string;
  route: string;
  updatedUtc: string;
}
