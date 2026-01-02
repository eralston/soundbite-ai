/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Group } from './Group.model';
import { GroupFields } from './GroupFields.model';
import { NewMember } from './NewMember.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';

/** 
* Automatically generated model for Masticore.Resources.NewGroup
*/
export interface NewGroup extends Group, GroupFields, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  description: string;
  imageSrc: string;
  isAccepting: boolean;
  members: NewMember[];
  name: string;
  route: string;
  updatedUtc: string;
}
