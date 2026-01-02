/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { OrganizationDetails } from './OrganizationDetails.model';
import { OrganizationWithPermissions } from './OrganizationWithPermissions.model';
import { OrgPermissions } from './OrgPermissions.model';
import { OrgSettings } from './OrgSettings.model';

/** 
* Automatically generated model for Soundbite.Models.OrganizationWithSettings
*/
export interface OrganizationWithSettings extends OrganizationWithPermissions {
  details: OrganizationDetails;
  hasPublicNotifications: boolean;
  /***
   * DO NOT USE; Please check the documentation for its replaced
   * @deprecated
   */
  permissions: OrgPermissions;

  settings: OrgSettings;
}
