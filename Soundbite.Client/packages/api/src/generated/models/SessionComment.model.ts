/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Person } from './Person.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Soundbite.Services.SessionComment
*/
export interface SessionComment extends ResourceBase, Record, Resource {
  content: string;
  createdUtc: string;
  deletedUtc?: string;
  person: Person;
  route: string;
  updatedUtc: string;
}
