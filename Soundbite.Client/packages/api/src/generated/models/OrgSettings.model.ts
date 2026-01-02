/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { IOrgSettings } from './IOrgSettings.model';
import { OrgAzureSettings } from './OrgAzureSettings.model';
import { OrgNotificationSettings } from './OrgNotificationSettings.model';
import { OrgOktaSettings } from './OrgOktaSettings.model';
import { OrgPermissions } from './OrgPermissions.model';
import { OrgSessionSettings } from './OrgSessionSettings.model';
import { Theme } from './Theme.model';

/** 
* Automatically generated model for Soundbite.OrgSettings
*/
export interface OrgSettings extends IOrgSettings {
  azure: OrgAzureSettings;
  notifications: OrgNotificationSettings;
  okta: OrgOktaSettings;
  permissions: OrgPermissions;
  sessions: OrgSessionSettings;
  theme: Theme;
}
