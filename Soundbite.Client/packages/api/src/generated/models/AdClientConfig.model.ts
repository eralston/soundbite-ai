/***************************************************************************************************
 * Auto-Generated File - do not modify because all changes will be lost
 **************************************************************************************************/

/** 
* Captures the client-side configuration necessary for MSAL.js powered client auth strategies
* https://github.com/AzureAD/microsoft-authentication-library-for-js
*/
export interface AdClientConfig {
  /***
   * Gets or sets the list of AAD scopes that the client-side app will use
   */
  adScopes: string[];

  /***
   * Gets or sets the list of scopes the server-side app will use
   */
  apiScopes: string[];

  /***
   * Gets or sets the API app's location on the net
   */
  apiUri: string;

  /***
   * Gets or sets the Client ID for the app in AAD
   */
  clientId: string;

  /***
   * Gets or sets the URI that will receive the logout redirect
   */
  postLogoutRedirectUri: string;

  /***
   * Gets or sets the URI that will receive the login redirect
   */
  redirectUri: string;
}
