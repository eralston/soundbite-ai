/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { OrganizationExtended } from './OrganizationExtended.model';
import { Person } from './Person.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';

/** 
* Automatically generated model for Masticore.Models.OrganizationDetails
*/
export interface OrganizationDetails extends OrganizationExtended, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  description: string;
  imageSrc: string;
  isAccepting: boolean;
  me?: Person;
  name: string;
  route: string;
  updatedUtc: string;
}
