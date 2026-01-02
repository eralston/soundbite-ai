/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { IndexPageRequest, IndexPageResponse, NewSessionComment, SessionComment } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.SessionCommentsController
 **/
class SessionCommentsServiceClass {

  /** 
  * Automatically generated API call
  */
  async readAllSessionComments(orgRoute: string, sessionRoute: string, page?: IndexPageRequest, options?: IHttpRequestOptions
): Promise<IndexPageResponse<SessionComment>> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/comments?&filter=${encodeURIComponent(page?.filter ?? "")}&includesCounts=${encodeURIComponent(page?.includesCounts ?? "")}&skip=${encodeURIComponent(page?.skip ?? "")}&take=${encodeURIComponent(page?.take ?? "")}`;
    const response = await HttpService.get<IndexPageResponse<SessionComment>>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async createSessionComment(orgRoute: string, sessionRoute: string, newComment: NewSessionComment, options?: IHttpRequestOptions
): Promise<SessionComment> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/comments`;
    const response = await HttpService.post<SessionComment>(url, newComment, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateSessionComment(orgRoute: string, sessionRoute: string, commentRoute: string, newComment: NewSessionComment, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/comments/${encodeURIComponent(commentRoute)}`;
    await HttpService.put(url, newComment, options);
  }

  /** 
  * Automatically generated API call
  */
  async deleteSessionComment(orgRoute: string, sessionRoute: string, commentRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/comments/${encodeURIComponent(commentRoute)}`;
    await HttpService.delete(url, options);
  }
}

export const SessionCommentsService = new SessionCommentsServiceClass();
