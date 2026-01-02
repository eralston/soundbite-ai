/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { SeriesDetails, SeriesPreview } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.SeriesController
 **/
class SeriesServiceClass {

  /** 
  * Automatically generated API call
  */
  async readAsync(orgRoute: string, seriesRoute: string, options?: IHttpRequestOptions
): Promise<SeriesDetails> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Series/${encodeURIComponent(seriesRoute)}`;
    const response = await HttpService.get<SeriesDetails>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAllAsync(orgRoute: string, options?: IHttpRequestOptions
): Promise<SeriesPreview[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Series`;
    const response = await HttpService.get<SeriesPreview[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readAllGroupAsync(orgRoute: string, groupRoute: string, options?: IHttpRequestOptions
): Promise<SeriesPreview[]> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/Organizations/${encodeURIComponent(orgRoute)}/Groups/${encodeURIComponent(groupRoute)}/Series`;
    const response = await HttpService.get<SeriesPreview[]>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async deleteAsync(orgRoute: string, seriesRoute: string, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/organizations/${encodeURIComponent(orgRoute)}/Series/${encodeURIComponent(seriesRoute)}`;
    await HttpService.delete(url, options);
  }
}

export const SeriesService = new SeriesServiceClass();
