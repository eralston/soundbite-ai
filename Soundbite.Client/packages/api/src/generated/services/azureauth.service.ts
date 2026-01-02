/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { AppPayload, AppPayload_Obsolete, User } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.AzureAuthController
 **/
class AzureAuthServiceClass {

  /** 
  * Automatically generated API call
  */
  async loginAzureAD(externalUserInfo: User, orgRoute: string, options?: IHttpRequestOptions
): Promise<AppPayload> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/azureAuth/login?orgRoute=${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.post<AppPayload>(url, externalUserInfo, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async loginToOrgAzureAD(orgRoute: string, externalUserInfo: User, options?: IHttpRequestOptions
): Promise<AppPayload> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/azureAuth/login/${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.post<AppPayload>(url, externalUserInfo, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async loginTeams(options?: IHttpRequestOptions
): Promise<AppPayload> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/azureAuth/LoginTeams`;
    const response = await HttpService.post<AppPayload>(url, null, options);
    return response;
  }

  /** 
  * Responsible for managing the initial login process for Soundbite's Microsoft SharePoint app.
  * Action is responsible for azure token validation.
  */
  async loginSharePoint(options?: IHttpRequestOptions
): Promise<AppPayload> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/azureAuth/LoginSharePoint`;
    const response = await HttpService.post<AppPayload>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async loginToOrg(orgRoute: string, options?: IHttpRequestOptions
): Promise<AppPayload_Obsolete> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/azureAuth/loginToOrg/${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.get<AppPayload_Obsolete>(url, options);
    return response;
  }
}

export const AzureAuthService = new AzureAuthServiceClass();
