/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { Organization } from './Organization.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';

/** 
* Automatically generated model for Masticore.Models.OrganizationExtended
*/
export interface OrganizationExtended extends Organization, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  description: string;
  imageSrc: string;
  isAccepting: boolean;
  name: string;
  route: string;
  updatedUtc: string;
}
