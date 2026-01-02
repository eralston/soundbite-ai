/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { Group, GroupDetails, NewGroup } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.GroupsController
 **/
class GroupsServiceClass {

  /** 
  * Automatically generated API call
  */
  async readAllGroupsAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<Group[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Groups`;
    const response = await HttpService.get<Group[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readMyGroupsAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<Group[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Me/Groups`;
    const response = await HttpService.get<Group[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readMyTargetGroupsAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<Group[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Me/Groups/Targets`;
    const response = await HttpService.get<Group[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async createAsync(orgRoute: string, newGroup: NewGroup, options?: IHttpRequestOptions
): Promise<GroupDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/`;
    const response = await HttpService.post<GroupDetails>(url, newGroup, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAsync(orgRoute: string, groupRoute: string, options?: IHttpRequestOptions
): Promise<GroupDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}`;
    const response = await HttpService.get<GroupDetails>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async deleteAsync(orgRoute: string, groupRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}`;
    await HttpService.delete(url, options);
  }

  /** 
  * Automatically generated API call
  */
  async patchAsync(orgRoute: string, groupRoute: string, changes: any, options?: IHttpRequestOptions
): Promise<GroupDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}`;
    const response = await HttpService.patch<GroupDetails>(url, changes, options);
    return response;
  }
}

export const GroupsService = new GroupsServiceClass();
