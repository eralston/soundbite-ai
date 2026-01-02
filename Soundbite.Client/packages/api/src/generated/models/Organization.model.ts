/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Masticore.Models.Organization
*/
export interface Organization extends ResourceBase, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  description: string;
  name: string;
  route: string;
  updatedUtc: string;
}
