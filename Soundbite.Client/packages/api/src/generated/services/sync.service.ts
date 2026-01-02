/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { IndexPageRequest, IndexPageResponse, OrgSyncConfig, OrgSyncResult, SyncTarget, SyncValidation, TokenPageRequest, TokenPageResponse } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.SyncController
 **/
class SyncServiceClass {

  /** 
  * Automatically generated API call
  */
  async validateAsync(orgRoute: string, orgConfig?: OrgSyncConfig, options?: IHttpRequestOptions
): Promise<SyncValidation> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/validate`;
    const response = await HttpService.post<SyncValidation>(url, orgConfig ?? null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readConfigAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<SyncValidation> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/config`;
    const response = await HttpService.get<SyncValidation>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateConfigAsync(orgRoute: string, orgConfig?: OrgSyncConfig, options?: IHttpRequestOptions
): Promise<SyncValidation> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/config`;
    const response = await HttpService.post<SyncValidation>(url, orgConfig ?? null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async syncAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrgSyncResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync`;
    const response = await HttpService.post<OrgSyncResult>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readResults(orgRoute: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<OrgSyncResult>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/results?&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<OrgSyncResult>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readUsersAsync(orgRoute: string, page?: TokenPageRequest, options?: IHttpRequestOptions
): Promise<TokenPageResponse<SyncTarget>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/users/page?&filter=${encodeURIComponent(page?.filter ?? "")}&skipToken=${encodeURIComponent(page?.skipToken ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<TokenPageResponse<SyncTarget>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readUserAsync(orgRoute: string, userId: string, options?: IHttpRequestOptions
): Promise<SyncTarget> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/users/${encodeURIComponent(userId)}`;
    const response = await HttpService.get<SyncTarget>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readGroupsAsync(orgRoute: string, page?: TokenPageRequest, options?: IHttpRequestOptions
): Promise<TokenPageResponse<SyncTarget>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/groups/page?&filter=${encodeURIComponent(page?.filter ?? "")}&skipToken=${encodeURIComponent(page?.skipToken ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<TokenPageResponse<SyncTarget>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readGroupAsync(orgRoute: string, groupId: string, options?: IHttpRequestOptions
): Promise<SyncTarget> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/sync/groups/${encodeURIComponent(groupId)}`;
    const response = await HttpService.get<SyncTarget>(url, options);
    return response;
  }
}

export const SyncService = new SyncServiceClass();
