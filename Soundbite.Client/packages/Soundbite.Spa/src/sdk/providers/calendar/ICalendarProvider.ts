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
   * Responsible for creating a calendar event for the user on the provider.
   */
  createEvent(data: any): Promise<string>;
}
