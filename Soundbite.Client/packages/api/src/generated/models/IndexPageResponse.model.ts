/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { IndexPageRequest } from './IndexPageRequest.model';
import { PageResponseBase } from './PageResponseBase.model';

/** 
* Automatically generated model for Masticore.Models.IndexPageResponse
*/
export interface IndexPageResponse<TEntity> extends PageResponseBase<TEntity> {
  maxSkip: number;
  maxTake: number;
  maxTotalResults: number;
  minSkip: number;
  minTake: number;
  request: IndexPageRequest;
  result: TEntity[];
  totalCount?: number;
  totalPageCount?: number;
}
