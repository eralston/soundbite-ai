import { ProviderType } from "@soundbite/api";

import { getAadAuthProvider } from "./auth/AadAuthProvider";
import { IAuthProvider } from "./auth/IAuthProvider";
import { ICalendarProvider } from "./calendar/ICalendarProvider";
import { AadCalendarProvider } from "./calendar/AadCalendarProvider";

/**
 * Responsible for managing authentication providers supported by the application.
 */
class ProviderFactoryClass {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  private authProvider?: IAuthProvider;
  private calendarProvider?: ICalendarProvider;
  private getProviderType?: () => ProviderType | undefined;

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Runs any initialization code that needs to run when the application starts.
   */
  async initialize(getProviderType: () => ProviderType): Promise<void> {
    // Validate
    if (getProviderType === null)
      throw new Error("Argument getProviderType cannot be null.");

    this.getProviderType = getProviderType;

    // Determine whether there is a current provider and instantiate the associated implementation
    switch (this.getProviderType()) {
      case ProviderType.AAD:
        this.authProvider = getAadAuthProvider();
        break;
    }
  }

  /**
   * Retrieves the IAuthProvider instance for the specified provider type
   */
  getAuthProvider(): IAuthProvider | null {
    let result: IAuthProvider | null = null;
    const providerType = this.getProviderType
      ? this.getProviderType()
      : ProviderType.Unknown;

    if (this.authProvider && this.authProvider.providerType === providerType) {
      result = this.authProvider;
    } else {
      switch (providerType) {
        case ProviderType.AAD:
          this.authProvider = this.authProvider ?? getAadAuthProvider();
          result = this.authProvider;
          break;
      }
    }

    return result;
  }

  /**
   * Retrieves the ICalendarProvider instance for the specified provider type
   */
  getCalendarProvider(): ICalendarProvider {
    let result: ICalendarProvider | undefined | null = null;
    const providerType = this.getProviderType
      ? this.getProviderType()
      : ProviderType.Unknown;

    // Determine if we already have the right calendar provider
    if (
      this.calendarProvider &&
      this.calendarProvider.providerType === providerType
    ) {
      result = this.calendarProvider;
    } else {
      // Determine the appropriate provider to instantiate
      switch (providerType) {
        case ProviderType.AAD:
          this.calendarProvider =
            this.calendarProvider ?? new AadCalendarProvider();
          result = this.calendarProvider;
          break;
      }
    }

    if (!result) {
      throw new Error(
        `Failed to retrieve provider for specified provider type "${providerType}".`
      );
    }

    return result;
  }
}

export const ProviderFactory = new ProviderFactoryClass();
