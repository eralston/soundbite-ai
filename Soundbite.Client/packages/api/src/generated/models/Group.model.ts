/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { GroupFields } from './GroupFields.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Masticore.Models.Group
*/
export interface Group extends ResourceBase, GroupFields, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  description: string;
  imageSrc: string;
  isAccepting: boolean;
  name: string;
  route: string;
  updatedUtc: string;
}
