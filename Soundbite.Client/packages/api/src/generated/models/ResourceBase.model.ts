/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Record } from './Record.model';
import { Resource } from './Resource.model';

/** 
* Automatically generated model for Masticore.Models.ResourceBase
*/
export interface ResourceBase extends Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  route: string;
  updatedUtc: string;
}
