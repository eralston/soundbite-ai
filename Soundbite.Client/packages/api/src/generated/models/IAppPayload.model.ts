/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { SpaConfig } from './SpaConfig.model';
import { User } from './User.model';

/** 
* Base interface for an app payload
*/
export interface IAppPayload {
  /***
   * Front-end app config
   */
  config: SpaConfig;

  /***
   * User's org refresh token
   */
  refreshToken: string;

  /***
   * User's org token
   */
  token: string;

  /***
   * Current user attibutes
   */
  user: User;
}
