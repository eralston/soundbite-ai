import { IHttpRequestOptions } from "./IHttpRequestOptions";

/***
 * Interface that defines the HTTP calls required by the Soundbite API.  This can be used to create
 * HTTP adapters for your HTTP library of choice.  Soundbite provides an out of the box Axios
 * implementation in the @soundbite/api-axios package.
 **/
export interface IHttpAdapter {
  /**
   * Issues an HTTP request with the DELETE verb to the specified url.
   * @param url - URL to which the request is sent.
   * @param options - HTTP request options used to customize the request.
   */
  delete: <T>(url: string, options?: IHttpRequestOptions) => Promise<T>;

  /**
   * Issues an HTTP request with the GET verb to the specified url.
   * @param url - URL to which the request is sent.
   * @param options - HTTP request options used to customize the request.
   */
  get: <T>(url: string, options?: IHttpRequestOptions) => Promise<T>;

  /**
   * Issues an HTTP request with the PATCH verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  patch: <T>(
    url: string,
    data: unknown,
    options?: IHttpRequestOptions
  ) => Promise<T>;

  /**
   * Issues an HTTP request with the POST verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  post: <T>(
    url: string,
    data: unknown,
    options?: IHttpRequestOptions
  ) => Promise<T>;

  /**
   * Issues an HTTP request with the PUT verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  put: <T>(
    url: string,
    data: unknown,
    options?: IHttpRequestOptions
  ) => Promise<T>;
}
