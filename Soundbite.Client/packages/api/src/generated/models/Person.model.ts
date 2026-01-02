/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { PersonRole } from '../enums';
import { InviteLifecycle } from './InviteLifecycle.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';
import { User } from './User.model';

/** 
* Automatically generated model for Masticore.Models.Person
*/
export interface Person extends ResourceBase, InviteLifecycle, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  inviteAcceptUtc?: string;
  inviteUtc?: string;
  personRole: PersonRole;
  route: string;
  updatedUtc: string;
  user: User;
}
