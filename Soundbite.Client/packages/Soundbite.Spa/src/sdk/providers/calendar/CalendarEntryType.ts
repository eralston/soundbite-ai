import { ProviderType } from "@soundbite/api";

/**
 * Defines the contract required for interacting with a third-party calendar provider.  Each
 * calendar provider must have its own ICalendarProvider implementation.
 */
export interface ICalendarProvider {
  /**
   * Gets or sets the provider type
   */
  readonly providerType: ProviderType;

  /**
   * Responsible for retrieving an existing token from the provider if it exists, or initiating the
   * process for logging the user into the provider to retrieve a token.
   */
  getTokenAsync(): Promise<void>;

  /**
   * Responsible for logging the user out of the authentication provider.
   */
  logoutAsync(): Promise<void>;
}
