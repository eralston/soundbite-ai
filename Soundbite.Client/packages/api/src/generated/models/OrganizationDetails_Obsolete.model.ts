/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Group } from './Group.model';
import { OrganizationExtended } from './OrganizationExtended.model';
import { Person } from './Person.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';

/** 
* Automatically generated model for Masticore.Models.OrganizationDetails_Obsolete
* DO NOT USE; please check the documentation for its replacement
* @deprecated
*/
export interface OrganizationDetails_Obsolete extends OrganizationExtended, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  description: string;
  groups: Group[];
  imageSrc: string;
  isAccepting: boolean;
  me: Person;
  myGroups: Group[];
  name: string;
  route: string;
  updatedUtc: string;
}
