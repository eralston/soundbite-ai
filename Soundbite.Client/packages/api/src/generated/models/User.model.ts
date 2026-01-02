/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { ProviderType, UserRole } from '../enums';
import { IUserNotifications } from './IUserNotifications.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';
import { UserFields } from './UserFields.model';

/** 
* Automatically generated model for Masticore.Models.User
*/
export interface User extends ResourceBase, Record, Resource, UserFields, IUserNotifications {
  allowEmail: boolean;
  allowMarketing: boolean;
  allowNews: boolean;
  allowSms: boolean;
  calendarSettings: string;
  createdUtc: string;
  deletedUtc?: string;
  email: string;
  familyName: string;
  givenName: string;
  imageSrc: string;
  isAccepting: boolean;
  phone: string;
  providerType: ProviderType;
  route: string;
  title: string;
  updatedUtc: string;
  userRole: UserRole;
}
