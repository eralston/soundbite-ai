/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { IndexPageResponse, Invite, InviteResult, Person, ReadAllPeopleIndexPageRequest } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.PeopleController
 **/
class PeopleServiceClass {

  /** 
  * Automatically generated API call
  */
  async readMeAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<Person> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/me`;
    const response = await HttpService.get<Person>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async inviteAsync(orgRoute: string, invites: Invite[], options?: IHttpRequestOptions
): Promise<InviteResult[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/People/Invite`;
    const response = await HttpService.post<InviteResult[]>(url, invites, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateAsync(orgRoute: string, personRoute: string, personFields: Person, options?: IHttpRequestOptions
): Promise<Person> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/People/${encodeURIComponent(personRoute)}`;
    const response = await HttpService.patch<Person>(url, personFields, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAllAsync(orgRoute: string, pageRequest?: ReadAllPeopleIndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<Person>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/People/Page?&filter=${encodeURIComponent(pageRequest?.filter ?? "")}&includesCounts=${encodeURIComponent(pageRequest?.includesCounts ?? "")}&personRole=${encodeURIComponent(pageRequest?.personRole ?? "")}&skip=${encodeURIComponent(pageRequest?.skip ?? "")}&take=${encodeURIComponent(pageRequest?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<Person>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async delete(orgRoute: string, personRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/People/${encodeURIComponent(personRoute)}`;
    await HttpService.delete(url, options);
  }
}

export const PeopleService = new PeopleServiceClass();
