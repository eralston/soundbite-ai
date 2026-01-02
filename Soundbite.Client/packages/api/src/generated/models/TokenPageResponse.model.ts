/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { PageResponseBase } from './PageResponseBase.model';
import { TokenPageRequest } from './TokenPageRequest.model';

/** 
* Automatically generated model for Masticore.Models.TokenPageResponse
*/
export interface TokenPageResponse<TEntity> extends PageResponseBase<TEntity> {
  maxTake: number;
  minTake: number;
  request: TokenPageRequest;
  result: TEntity[];
  skipToken?: string;
}
