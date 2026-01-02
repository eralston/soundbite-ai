import { IHttpRequestOptions } from "./IHttpRequestOptions";
import { PublicError } from "./PublicError";
import { SoundbiteApiConfig } from "./SoundbiteApiConfig";

/**
 * Wrapper class for issuing HTTP calls for the application.  T
 */
class HttpServiceClass {
  /**
   * Issues an HTTP request with the DELETE verb to the specified url.
   * @param url - URL to which the request is sent.
   * @param options - HTTP request options used to customize the request.
   */
  delete<T>(url: string, options?: IHttpRequestOptions): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      SoundbiteApiConfig.httpAdapter
        .delete<T>(url, options)
        .then((result) => resolve(result))
        .catch((ex) => {
          const msg = ex && ex.message ? `: ${ex.message}` : ".";
          console.log(`[SB]-HTTP service DELETE failed calling ${url}`);
          reject(
            new PublicError(
              ex,
              "Network Request Failed",
              `A delete request to the Soundbite API failed ${msg}`
            )
          );
        });
    });
  }

  /**
   * Issues an HTTP request with the GET verb to the specified url.
   * @param url - URL to which the request is sent.
   * @param options - HTTP request options used to customize the request.
   */
  get<T>(url: string, options?: IHttpRequestOptions): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      SoundbiteApiConfig.httpAdapter
        .get<T>(url, options)
        .then((result) => resolve(result))
        .catch((ex) => {
          const msg = ex && ex.message ? `: ${ex.message}` : ".";
          console.log(`[SB]-HTTP service GET failed calling ${url}`);
          reject(
            new PublicError(
              ex,
              "Network Request Failed",
              `A get request to the Soundbite API failed ${msg}`
            )
          );
        });
    });
  }

  /**
   * Issues an HTTP request with the PATCH verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  patch<T>(
    url: string,
    data?: unknown,
    options?: IHttpRequestOptions
  ): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      SoundbiteApiConfig.httpAdapter
        .patch<T>(url, data, options)
        .then((result) => resolve(result))
        .catch((ex) => {
          const msg = ex && ex.message ? `: ${ex.message}` : ".";
          console.log(`[SB]-HTTP service PATCH failed calling ${url}`);
          if (data) {
            console.log("[SB]-", data);
          }
          reject(
            new PublicError(
              ex,
              "Network Request Failed",
              `A patch request to the Soundbite API failed ${msg}`
            )
          );
        });
    });
  }

  /**
   * Issues an HTTP request with the POST verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  post<T>(
    url: string,
    data?: unknown,
    options?: IHttpRequestOptions
  ): Promise<T> {
    // ASP.Net Core requires the text "null" to transmit an empty body
    data = data ?? "null";
    return new Promise<T>((resolve, reject) => {
      SoundbiteApiConfig.httpAdapter
        .post<T>(url, data, options)
        .then((result) => resolve(result))
        .catch((ex) => {
          const msg = ex && ex.message ? `: ${ex.message}` : ".";
          console.log(`[SB]-HTTP service POST failed calling ${url}`);
          if (data) {
            console.log("[SB]-", data);
          }
          reject(
            new PublicError(
              ex,
              "Network Request Failed",
              `A post request to the Soundbite API failed${msg}`
            )
          );
        });
    });
  }

  /**
   * Issues an HTTP request with the PUT verb to the specified url.
   * @param url - URL to which the request is sent.
   * @data data - data sent in the body of the request.
   * @param options - HTTP request options used to customize the request.
   */
  put<T>(
    url: string,
    data?: unknown,
    options?: IHttpRequestOptions
  ): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      SoundbiteApiConfig.httpAdapter
        .put<T>(url, data, options)
        .then((result) => resolve(result))
        .catch((ex) => {
          const msg = ex && ex.message ? `: ${ex.message}` : ".";
          console.log(`[SB]-HTTP service PUT failed calling ${url}`);
          if (data) {
            console.log("[SB]-", data);
          }
          reject(
            new PublicError(
              ex,
              "Network Request Failed",
              `A put request to the Soundbite API failed${msg}`
            )
          );
        });
    });
  }
}

export const HttpService = new HttpServiceClass();
