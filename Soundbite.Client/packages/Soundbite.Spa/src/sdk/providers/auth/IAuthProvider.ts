import { ProviderType } from "@soundbite/api";

export interface IAuthProvider {
  /**
   * Gets a value indicating the provider type
   */
  readonly providerType: ProviderType;

  /**
   * Responsible for handling the response returned from an OAuth redirect.
   */
  handleAuthRedirect(): Promise<void>;

  /**
   * Responsible for running any logic required to log the user in to the third-party provider.
   * @param forceReAuth - flag indicating that the user should be forced to login to the provider again.
   */
  loginAsync(forceReAuth: boolean): Promise<void>;

  /**
   * Responsible for logging the user out of the authentication provider.
   */
  logoutAsync(): Promise<void>;

  /**
   * Starts the flow to grant admin access for the current user and org
   * */
  grantAdminAsync(): Promise<void>;
}
