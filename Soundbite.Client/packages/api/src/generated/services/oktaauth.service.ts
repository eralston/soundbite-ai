/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { AppPayload, OrgOktaSettingsWithOrgRoute } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.OktaAuthController
 **/
class OktaAuthServiceClass {

  /** 
  * Automatically generated API call
  */
  async getSettingsByEmail(email: string, options?: IHttpRequestOptions
): Promise<OrgOktaSettingsWithOrgRoute> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/oktaAuth/settingsByEmail?email=${encodeURIComponent(email)}`;
    const response = await HttpService.get<OrgOktaSettingsWithOrgRoute>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async getSettingsByRoute(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrgOktaSettingsWithOrgRoute> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/oktaAuth/settingsByRoute?orgRoute=${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.get<OrgOktaSettingsWithOrgRoute>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async login(orgRoute: string, options?: IHttpRequestOptions
): Promise<AppPayload> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/oktaAuth/login?orgRoute=${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.post<AppPayload>(url, null, options);
    return response;
  }
}

export const OktaAuthService = new OktaAuthServiceClass();
