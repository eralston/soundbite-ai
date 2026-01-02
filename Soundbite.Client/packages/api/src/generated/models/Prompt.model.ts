/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Soundbite.Models.Prompt
*/
export interface Prompt extends ResourceBase, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  route: string;
  text: string;
  updatedUtc: string;
}
