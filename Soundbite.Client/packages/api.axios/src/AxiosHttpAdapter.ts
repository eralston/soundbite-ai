import Axios, { AxiosRequestConfig, AxiosResponse } from "axios";
import {
  IHttpAdapter,
  IHttpRequestOptions,
  SoundbiteApiConfig,
} from "@soundbite/api";

/**
 * IHttpAdapter implementation for the Axios library.
 */
export class AxiosHttpAdapter implements IHttpAdapter {
  /**
   * Issues an HTTP request with the DELETE verb to the specified url.
   * @param url - URL to which the request is sent.
   * @param options - HTTP request options used to customize the request.
   */
  async delete<T>(url: string, options?: IHttpRequestOptions): Promise<T> {
    const config = await this.getConfig(options);
    const response = await Axios.delete<T>(url, config);
    return response?.data;
  }

  /**
   * Issues an HTTP request with the GET verb to the specified url.
   * @param url - URL to which the request is sent.
   * @param options - HTTP request options used to customize the request.
   */
  async get<T>(url: string, options?: IHttpRequestOptions): Promise<T> {
    const config = await this.getConfig(options);
    const response = await Axios.get<T>(url, config);
    return response?.data;
  }

  /**
   * Issues an HTTP request with the PATCH verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  async patch<T>(
    url: string,
    data: unknown,
    options?: IHttpRequestOptions
  ): Promise<T> {
    const config = await this.getConfig(options);
    const response = await Axios.patch<T>(url, data, config);
    return response?.data;
  }

  /**
   * Issues an HTTP request with the POST verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  async post<T>(
    url: string,
    data: unknown,
    options?: IHttpRequestOptions
  ): Promise<T> {
    const config = await this.getConfig(options);
    const response = await Axios.post<T>(url, data, config);
    return response?.data;
  }

  /**
   * Issues an HTTP request with the PUT verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  async put<T>(
    url: string,
    data: unknown,
    options?: IHttpRequestOptions
  ): Promise<T> {
    const config = await this.getConfig(options);
    const response = await Axios.put<T>(url, data, config);
    return response?.data;
  }

  private static onProgressHandler(
    progressEvent: ProgressEvent,
    options?: IHttpRequestOptions
  ) {
    if (options?.onProgress == null) {
      return;
    }
    let percentage = (progressEvent.loaded / progressEvent.total) * 100;
    if (percentage > 100) {
      percentage = 100;
    }
    options.onProgress(percentage);
  }

  /**
   * Responsible for building an Axios request configuration containing the bearer token
   * @param options - HTTP request options used to customize the request.
   */
  private async getConfig(
    options?: IHttpRequestOptions
  ): Promise<AxiosRequestConfig | undefined> {
    let hasConfig: boolean = !!options?.headers;

    // Setup the result and merge headers from the options parameter into the configuration object
    let result: AxiosRequestConfig = {
      headers: options?.headers ? { ...options.headers } : {},
    } as AxiosRequestConfig;

    if (options?.includeToken !== false) {
      const token = await SoundbiteApiConfig.getToken();
      if (token) {
        result.headers.Authorization = `Bearer ${token}`;
        hasConfig = true;
      }
    }

    if (options?.contentType) {
      if (options.contentType !== "") {
        result.headers["Content-Type"] = options.contentType;
        hasConfig = true;
      }
    } else {
      result.headers["Content-Type"] = "application/json";
      hasConfig = true;
    }

    if (options?.onProgress != null) {
      result.onUploadProgress = (progress: ProgressEvent) =>
        AxiosHttpAdapter.onProgressHandler(progress, options);
      result.onDownloadProgress = (progress: ProgressEvent) =>
        AxiosHttpAdapter.onProgressHandler(progress, options);
    }

    return hasConfig ? result : undefined;
  }
}
