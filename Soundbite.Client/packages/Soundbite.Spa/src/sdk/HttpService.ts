import Axios, { AxiosRequestConfig } from "axios";
import SpaStore from "../store/Spa.Store";

/**
 * Wrapper class for issuing HTTP calls for the application
 */
class HttpServiceClass {
  /**
   * Issues an HTTP GET request
   * @param url - request URL
   * @param includeToken - flag indicating whether to include the Soundbite API token (default is true)
   */
  delete<T>(url: string, includeToken: boolean = true): Promise<T> {
    return Axios.delete<T>(url, this.getConfig(includeToken)).then(
      (response) => response.data
    );
  }

  /**
   * Issues an HTTP GET request
   * @param url - request URL
   * @param includeToken - flag indicating whether to include the Soundbite API token (default is true)
   */
  get<T>(url: string, includeToken: boolean = true): Promise<T> {
    return Axios.get<T>(url, this.getConfig(includeToken)).then(
      (response) => response.data
    );
  }

  /**
   * Issues an HTTP POST request
   * @param url - request URL
   * @param data - data to post
   * @param includeToken - flag indicating whether to include the Soundbite API token (default is true)
   */
  patch<T>(url: string, data?: any, includeToken: boolean = true): Promise<T> {
    return Axios.patch<T>(url, data, this.getConfig(includeToken)).then(
      (response) => response.data
    );
  }

  /**
   * Issues an HTTP POST request
   * @param url - request URL
   * @param data - data to post
   * @param includeToken - flag indicating whether to include the Soundbite API token (default is true)
   */
  post<T>(url: string, data?: any, includeToken: boolean = true): Promise<T> {
    return Axios.post<T>(url, data, this.getConfig(includeToken)).then(
      (response) => response.data
    );
  }

  /**
   * Issues an HTTP PUT request
   * @param url - request URL
   * @param data - data to post
   * @param includeToken - flag indicating whether to include the Soundbite API token (default is true)
   */
  put<T>(url: string, data?: any, includeToken: boolean = true): Promise<T> {
    return Axios.put<T>(url, data, this.getConfig(includeToken)).then(
      (response) => response.data
    );
  }

  /**
   * Responsible for building an Axios request configuration containing the bearer token
   * @param includeToken - flag indicating whether to include the Soundbite API token
   */
  private getConfig(includeToken: boolean): AxiosRequestConfig | undefined {
    // Determine whether the token should be included and whether it is available
    if (includeToken && SpaStore.token) {
      var options = {} as AxiosRequestConfig;
      options.headers = {
        Authorization: "Bearer " + SpaStore.token,
      };
      return options;
    } else {
      return undefined;
    }
  }
}

export const HttpService = new HttpServiceClass();
