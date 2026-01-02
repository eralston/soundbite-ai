/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

import { HttpStatusCode } from '../enums';

/** 
* Converts an  during a request into helpful logs and response body format
*/
export interface ApiError {
  /***
   * A sentinel property in the JSON to indicate this is an error details object.
   */
  isApiError?: boolean;

  /***
   * A sentinel property in the JSON to indicate the message can be shown raw to the user
   */
  isUserSafe?: boolean;

  /***
   * Gets or sets the error message for this error
   */
  message: string;

  /***
   * Gets or sets the HTTP Method (EG, GET)
   */
  method: string;

  /***
   * The name for this error, enabling the client-side type to be compatible with JavaScript's native error; defaults to simply "Error"
   */
  name: string;

  /***
   * Gets or sets the path for this error's request
   */
  path: string;

  /***
   * Gets or sets the  for this error
   */
  statusCode: HttpStatusCode;
}
