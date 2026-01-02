/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpService } from '../../code/HttpService';
import { SoundbiteApiConfig } from '../../code/SoundbiteApiConfig';
import { IHttpRequestOptions } from '../../code/IHttpRequestOptions';
import { Invite, User, UserNotifications } from '../models/index';

/**
 * Automatically generated endpoint API for the Soundbite.Api.Controllers.UsersController
 **/
class UsersServiceClass {

  /** 
  * Automatically generated API call
  */
  async upsertMeAsync(userInfo: User, options?: IHttpRequestOptions
): Promise<User> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/me`;
    const response = await HttpService.post<User>(url, userInfo, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async updateNotificationAsync(notificationSettings: UserNotifications, options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/me/notifications/settings`;
    await HttpService.post(url, notificationSettings, options);
  }

  /** 
  * Uploads a new "avatar" image for the current user.  The URL returned in the ImageSrc
  * property contains a short-lived token used to access the image.
  */
  async upsertMyImageAsync(blob: Blob, options?: IHttpRequestOptions
): Promise<User> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/me/image`;
    const response = await HttpService.post<User>(url, blob, options);
    return response;
  }

  /** 
  * Retrieves user details for the current user.
  */
  async readMeAsync(options?: IHttpRequestOptions
): Promise<User> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/me`;
    const response = await HttpService.get<User>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async getRouteFromUniversalId(universalId: string, options?: IHttpRequestOptions
): Promise<string> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/users/routeFromUid/${encodeURIComponent(universalId)}`;
    const response = await HttpService.get<string>(url, options);
    return response;
  }

  /** 
  * Automatically generated API call
  */
  async inviteAsync(invites: Invite[], options?: IHttpRequestOptions
): Promise<void> {
    const url = SoundbiteApiConfig.ApiPrefixUrl + `/users`;
    await HttpService.post(url, invites, options);
  }
}

export const UsersService = new UsersServiceClass();
