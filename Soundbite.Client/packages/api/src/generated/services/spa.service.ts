/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { AppPayload, AppPayload_Obsolete, TokenInfo } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.SpaController
 **/
class SpaServiceClass {

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async spaInit(orgRoute?: string, options?: IHttpRequestOptions
): Promise<AppPayload_Obsolete> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/spa/init?orgRoute=${encodeURIComponent(orgRoute ?? "")}`;
    const response = await HttpService.get<AppPayload_Obsolete>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async load(orgRoute?: string, options?: IHttpRequestOptions
): Promise<AppPayload> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/spa/load?orgRoute=${encodeURIComponent(orgRoute ?? "")}`;
    const response = await HttpService.get<AppPayload>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async getToken(orgRoute: string, options?: IHttpRequestOptions
): Promise<TokenInfo> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/getToken/${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.get<TokenInfo>(url, options);
    return response;
  }

  /** 
  * Returns a default value to indicate we're alive
  */
  async ping(options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/`;
    const response = await HttpService.get<string>(url, options);
    return response;
  }

  /** 
  * Returns a default value to indicate we're alive
  */
  async version(options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/version`;
    const response = await HttpService.get<string>(url, options);
    return response;
  }
}

export const SpaService = new SpaServiceClass();
