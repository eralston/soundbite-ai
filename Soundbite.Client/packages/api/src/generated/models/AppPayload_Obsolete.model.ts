/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { IAppPayload } from './IAppPayload.model';
import { OrganizationExtended } from './OrganizationExtended.model';
import { OrganizationWithPermissions_Obsolete } from './OrganizationWithPermissions_Obsolete.model';
import { SpaConfig } from './SpaConfig.model';
import { User } from './User.model';

/** 
* An obsolete version of
* DO NOT USE; please check the documentation for its replacement
* @deprecated
*/
export interface AppPayload_Obsolete extends IAppPayload {
  /***
   * Gets or sets SPA configuration settings to send to the client.
   */
  config: SpaConfig;

  /***
   * Gets or sets a reference to the organization the user is logging into.  This contains 
   *             additional information about the current organization that is not present in the list
   *             of organizations stored in .
   */
  organization: OrganizationWithPermissions_Obsolete;

  /***
   * Gets a list of organization to which the user belongs (across all tenants).
   */
  organizations: OrganizationExtended[];

  /***
   * Gets or sets the refresh token that can be used to request a new API token.  This value 
   *             is populated in the  in response to receiving an HTTP-only 
   *             cookie containing a valid refresh token.
   */
  refreshToken: string;

  /***
   * Gets or sets an authorization token that can be used to authenticate against the API.
   *             This value is populated in the  in response to receiving an
   *             HTTP-only cookie containing a valid refresh token.
   */
  token: string;

  /***
   * Gets or sets a reference to the user who is logged into the system.
   */
  user: User;
}
