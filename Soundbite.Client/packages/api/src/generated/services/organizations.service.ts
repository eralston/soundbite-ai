/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { Organization, OrganizationExtended, OrganizationWithSettings, OrgAzureSettings, OrgNotificationSettings, OrgPermissions, OrgSessionSettings, Theme } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.App.Controllers.OrganizationsController
 **/
class OrganizationsServiceClass {

  /** 
  * Gets a list of all the active organizations to which the current user has access.
  */
  async readAllAsync(options?: IHttpRequestOptions
): Promise<OrganizationExtended[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations`;
    const response = await HttpService.get<OrganizationExtended[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async createAsync(organization: Organization, options?: IHttpRequestOptions
): Promise<OrganizationExtended> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/`;
    const response = await HttpService.post<OrganizationExtended>(url, organization, options);
    return response;
  }

  /** 
  * Gets an array containing all of the active organizations in the system.
  */
  async readAllAsGodAsync(options?: IHttpRequestOptions
): Promise<OrganizationExtended[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/all`;
    const response = await HttpService.get<OrganizationExtended[]>(url, options);
    return response;
  }

  /** 
  * Gets a list of all the archived organizations to which the current user has access.
  */
  async readAllArchivedAsync(options?: IHttpRequestOptions
): Promise<OrganizationExtended[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/archived`;
    const response = await HttpService.get<OrganizationExtended[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrganizationWithSettings> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.get<OrganizationWithSettings>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async deleteAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}`;
    await HttpService.delete(url, options);
  }

  /** 
  * Automatically generated API call
  */
  async readNotificationSettingsAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrgNotificationSettings> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/notifications`;
    const response = await HttpService.get<OrgNotificationSettings>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readSessionSettingsAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrgSessionSettings> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/sessions`;
    const response = await HttpService.get<OrgSessionSettings>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateNotificationSettingsAsync(orgRoute: string, settings: OrgNotificationSettings, options?: IHttpRequestOptions
): Promise<OrgNotificationSettings> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/notifications`;
    const response = await HttpService.put<OrgNotificationSettings>(url, settings, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateSessionSettingsAsync(orgRoute: string, settings: OrgSessionSettings, options?: IHttpRequestOptions
): Promise<OrgSessionSettings> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/sessions`;
    const response = await HttpService.put<OrgSessionSettings>(url, settings, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAzureSettingsAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrgAzureSettings> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/azure`;
    const response = await HttpService.get<OrgAzureSettings>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateAzureSettingsAsync(orgRoute: string, settings: OrgAzureSettings, options?: IHttpRequestOptions
): Promise<OrgAzureSettings> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/azure`;
    const response = await HttpService.put<OrgAzureSettings>(url, settings, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readPermissionsAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrgPermissions> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/permissions`;
    const response = await HttpService.get<OrgPermissions>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updatePermissionsAsync(orgRoute: string, settings: OrgPermissions, options?: IHttpRequestOptions
): Promise<OrgPermissions> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/settings/permissions`;
    const response = await HttpService.put<OrgPermissions>(url, settings, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async restoreAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<OrganizationExtended> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/restore`;
    const response = await HttpService.post<OrganizationExtended>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async patchAsync(orgRoute: string, changes: any, options?: IHttpRequestOptions
): Promise<OrganizationExtended> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}`;
    const response = await HttpService.patch<OrganizationExtended>(url, changes, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async uploadImageAsync(orgRoute: string, formData: FormData, options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/image`;
    const response = await HttpService.post<string>(url, formData, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateThemeAsync(orgRoute: string, theme?: Theme, options?: IHttpRequestOptions
): Promise<Theme> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/theme`;
    const response = await HttpService.post<Theme>(url, theme ?? null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async setOrgToken(orgRoute: string, newToken: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/setOrgToken`;
    await HttpService.post(url, newToken, options);
  }

  /** 
  * Automatically generated API call
  */
  async syncOrgTokenWithAmsContentKeyPolicy(orgRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/syncOrgTokenWithAmsContentKeyPolicy`;
    await HttpService.put(url, null, options);
  }

  /** 
  * Automatically generated API call
  */
  async ensureAmsContentKeyPolicy(orgRoute: string, options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/EnsureAmsContentKeyPolicy`;
    const response = await HttpService.post<string>(url, null, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAmsStreamingUrl(orgRoute: string, options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/readAmsStreamingUrl`;
    const response = await HttpService.get<string>(url, options);
    return response;
  }
}

export const OrganizationsService = new OrganizationsServiceClass();
