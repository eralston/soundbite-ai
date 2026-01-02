/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { ContentResult, SbClipManifestResult } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.MediaStreamingController
 **/
class MediaStreamingServiceClass {

  /** 
  * Automatically generated API call
  */
  async sbClipManifest(orgRoute: string, clipRoute: string, options?: IHttpRequestOptions
): Promise<SbClipManifestResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/mediaproxy/sbhosted/manifest?orgRoute=${encodeURIComponent(orgRoute)}&clipRoute=${encodeURIComponent(clipRoute)}`;
    const response = await HttpService.get<SbClipManifestResult>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async manifest(sourceUrl: string, token: string, options?: IHttpRequestOptions
): Promise<ContentResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/mediaproxy/manifest?sourceUrl=${encodeURIComponent(sourceUrl)}&token=${encodeURIComponent(token)}`;
    const response = await HttpService.get<ContentResult>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async manifestPart(sourceUrl: string, token: string, options?: IHttpRequestOptions
): Promise<ContentResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/mediaproxy/manifestpart?sourceUrl=${encodeURIComponent(sourceUrl)}&token=${encodeURIComponent(token)}`;
    const response = await HttpService.get<ContentResult>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readVariantPlaylist(orgRoute: string, sessionRoute: string, promptRoute: string, clipRoute: string, options?: IHttpRequestOptions
): Promise<ContentResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/media/${encodeURIComponent(orgRoute)}/${encodeURIComponent(sessionRoute)}/${encodeURIComponent(promptRoute)}/${encodeURIComponent(clipRoute)}/playlist.m3u8`;
    const response = await HttpService.get<ContentResult>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readVariantPlaylistByClip(clipRoute: string, options?: IHttpRequestOptions
): Promise<ContentResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/media/clip/${encodeURIComponent(clipRoute)}/playlist.m3u8`;
    const response = await HttpService.get<ContentResult>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async readLevelPlaylist(orgRoute: string, sessionRoute: string, promptRoute: string, clipRoute: string, level: string, options?: IHttpRequestOptions
): Promise<ContentResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/media/${encodeURIComponent(orgRoute)}/${encodeURIComponent(sessionRoute)}/${encodeURIComponent(promptRoute)}/${encodeURIComponent(clipRoute)}/${encodeURIComponent(level)}/playlist.m3u8`;
    const response = await HttpService.get<ContentResult>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  * DO NOT USE; please check the documentation for its replacement
  * @deprecated
  */
  async readLevelPlaylistByClip(clipRoute: string, level: string, options?: IHttpRequestOptions
): Promise<ContentResult> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/media/clip/${encodeURIComponent(clipRoute)}/${encodeURIComponent(level)}/playlist.m3u8`;
    const response = await HttpService.get<ContentResult>(url, options);
    return response;
  }
}

export const MediaStreamingService = new MediaStreamingServiceClass();
