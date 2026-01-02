/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Recurrence } from '../enums';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Soundbite.Models.Series
*/
export interface Series extends ResourceBase, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  name: string;
  recurrence: Recurrence;
  recurrenceData: string;
  route: string;
  updatedUtc: string;
}
