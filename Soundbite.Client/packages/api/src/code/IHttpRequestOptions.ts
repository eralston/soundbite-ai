/***
 * Interface containing the various options allowed on HTTP calls.
 **/
export interface IHttpRequestOptions {
  /**
   * Gets or sets a flag specifying whether the current token should be appended to the request.
   * This value is assumed to be true unless it is explicitly set to false.
   */
  includeToken?: boolean;

  /**
   * Gets or sets the name of the content-type header to send in the HTTP request.
   * This value is assumed to be application/json unless it is explicitly set to a value.
   * In the event that the content-type header should not be sent, set to an empty string.
   */
  contentType?: string;

  /**
   * Gets or sets a "dictionary" object whose property names and respective values represent HTTP
   * request header names and values.
   */
  headers?: any;

  /**
   * Optional callback to receive progress updates from the work done by the
   * */
  onProgress?: (percentage: number) => void;
}
