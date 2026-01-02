/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { MemberRole } from '../enums';
import { Group } from './Group.model';
import { InviteLifecycle } from './InviteLifecycle.model';
import { Person } from './Person.model';
import { Record } from './Record.model';
import { Resource } from './Resource.model';
import { ResourceBase } from './ResourceBase.model';

/** 
* Automatically generated model for Masticore.Models.Member
*/
export interface Member extends ResourceBase, InviteLifecycle, Record, Resource {
  createdUtc: string;
  deletedUtc?: string;
  group: Group;
  groupRoute: string;
  inviteAcceptUtc?: string;
  inviteUtc?: string;
  memberRole: MemberRole;
  person: Person;
  personRoute: string;
  route: string;
  updatedUtc: string;
}
