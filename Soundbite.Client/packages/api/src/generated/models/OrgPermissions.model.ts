/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { MemberRole, PersonRole } from '../enums';

/** 
* Automatically generated model for Soundbite.OrgPermissions
*/
export interface OrgPermissions {
  minRoleForAudience: PersonRole;
  minRoleForOrgInvite: PersonRole;
  minRoleForTeamInvite: MemberRole;
  minRoleToCreatePublic: PersonRole;
  minRoleToCreateSession: PersonRole;
  minRoleToCreateTeam: PersonRole;
  minRoleToCreateTeamPublic: MemberRole;
  minRoleToCreateTeamSession: MemberRole;
}
