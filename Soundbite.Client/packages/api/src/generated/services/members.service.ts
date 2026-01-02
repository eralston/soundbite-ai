/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { IndexPageRequest, IndexPageResponse, Invite, Member } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.MembersController
 **/
class MembersServiceClass {

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readAllAsync_Obsolete(orgRoute: string, groupRoute: string, options?: IHttpRequestOptions
): Promise<Member[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Members/`;
    const response = await HttpService.get<Member[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAllAsync(orgRoute: string, groupRoute: string, pageRequest?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<Member>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Members/Page?&filter=${encodeURIComponent(pageRequest?.filter ?? "")}&includesCounts=${encodeURIComponent(pageRequest?.includesCounts ?? "")}&skip=${encodeURIComponent(pageRequest?.skip ?? "")}&take=${encodeURIComponent(pageRequest?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<Member>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async inviteAsync(orgRoute: string, groupRoute: string, invites: Invite[], options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Members/`;
    await HttpService.post(url, invites, options);
  }

  /** 
  * Automatically generated API call
  */
  async updateAsync(orgRoute: string, groupRoute: string, memberRoute: string, member: Member, options?: IHttpRequestOptions
): Promise<Member> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Members/${encodeURIComponent(memberRoute)}`;
    const response = await HttpService.patch<Member>(url, member, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async deleteAsync(orgRoute: string, groupRoute: string, memberRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Members/${encodeURIComponent(memberRoute)}`;
    await HttpService.delete(url, options);
  }
}

export const MembersService = new MembersServiceClass();
