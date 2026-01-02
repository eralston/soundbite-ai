/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { PersonRole } from '../enums';
import { IndexPageRequest } from './IndexPageRequest.model';

/** 
* Expands  to includes additional filter fields
*/
export interface ReadAllPeopleIndexPageRequest extends IndexPageRequest {
  filter?: string;
  includesCounts?: boolean;
  /***
   * The exact person role for desired query results
   */
  personRole?: PersonRole;

  skip?: number;
  take?: number;
}
