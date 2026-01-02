/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { ClientOpWrapper, ClipDetails } from '../models/index';
import { ClipState } from '../enums/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.ClipController
 **/
class ClipServiceClass {

  /** 
  * Automatically generated API call
  */
  async simulateAmsGridEvent(clipOpRoute: string, options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/SimulateAmsGridEvent/${encodeURIComponent(clipOpRoute)}`;
    const response = await HttpService.post<string>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async createAsync_Obsolete(orgRoute: string, sessionRoute: string, promptRoute: string, formData: FormData, options?: IHttpRequestOptions
): Promise<ClipDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/Prompts/${encodeURIComponent(promptRoute)}/Clips`;
    const response = await HttpService.post<ClientOpWrapper<ClipDetails>>(url, formData, options);
    await SoundbiteApiConfig.clientOpsHandler(response?.clientOps);
    return response.result;
  }

  /** 
  * Automatically generated API call
  */
  async createAsync(orgRoute: string, sessionRoute: string, promptRoute: string, formData: FormData, options?: IHttpRequestOptions
): Promise<ClipDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/Prompts/${encodeURIComponent(promptRoute)}/Clips/New`;
    const response = await HttpService.post<ClipDetails>(url, formData, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async stateAsync(orgRoute: string, sessionRoute: string, promptRoute: string, clipRoute: string, options?: IHttpRequestOptions
): Promise<ClipState> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/Prompts/${encodeURIComponent(promptRoute)}/Clips/${encodeURIComponent(clipRoute)}/state`;
    const response = await HttpService.get<ClipState>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async createAnnouncementClipAsync(orgRoute: string, sessionRoute: string, formData: FormData, options?: IHttpRequestOptions
): Promise<ClipDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/Prompts/Announcements`;
    const response = await HttpService.post<ClipDetails>(url, formData, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async getTranscriptUrl(orgRoute: string, sessionRoute: string, clipRoute: string, isPublic: boolean = false, options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Sessions/${encodeURIComponent(sessionRoute)}/Prompts/{promptRoute}/Clips/${encodeURIComponent(clipRoute)}/transcript?isPublic=${encodeURIComponent(isPublic)}`;
    const response = await HttpService.get<string>(url, options);
    return response;
  }
}

export const ClipService = new ClipServiceClass();
