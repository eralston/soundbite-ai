/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { OrgSyncConfig } from './OrgSyncConfig.model';

/** 
* DTO for sending back indication that the given config is valid and available
*/
export interface SyncValidation {
  /***
   * Flag for whether or not the given org has access
   */
  hasAccess: boolean;

  /***
   * Org for which we checked access
   */
  orgRoute: string;

  /***
   * A sanitized version of the config used for the check
   */
  sanitizedConfig?: OrgSyncConfig;

  /***
   * Null if the config is valid; otherwise, a string describing issues with the config
   */
  validationMessage?: string;
}
