import { OrgOktaSettingsWithOrgRoute, ProviderType } from "@soundbite/api";

// Local storage key constants
const keys = {
  tokenKey: "sbt",
  refreshTokenKey: "sbrt",
  lastOktaSettingsKey: "los",
  lastSelectedOrgKey: "lo",
  authProviderTypeKey: "ap",
  failedClientOpsKey: "fco",
  returnToPage: "rtp",
};

/**
 * Manages storing and retrieving data from local storage and session storage. The point of this
 * class is to ensure a single point of reference for all data stored in the browser. There should
 * be no direct calls localStorage or sessionStorage anywhere outside of this class.
 */
class BrowserStorageServiceClass {
  clearNonAppLocalStorage(): void {
    const keysToKeep = Object.keys(keys).map((key) => (keys as any)[key]);
    const keysLocalStorage = Object.keys(localStorage);
    keysLocalStorage.forEach((key) => {
      if (keysToKeep.indexOf(key) < 0) {
        localStorage.removeItem(key);
      }
    });
  }

  get authProviderType(): ProviderType | null {
    return this.get<ProviderType>(keys.authProviderTypeKey);
  }

  set authProviderType(value: ProviderType | null) {
    this.set(keys.authProviderTypeKey, value);
  }

  get failedClientOps(): any[] {
    return this.get<any[]>(keys.failedClientOpsKey) || [];
  }

  set failedClientOps(value: any[]) {
    this.set(keys.failedClientOpsKey, value);
  }

  get lastOktaSettings(): OrgOktaSettingsWithOrgRoute | null {
    return this.get(keys.lastOktaSettingsKey);
  }

  set lastOktaSettings(value: OrgOktaSettingsWithOrgRoute | null) {
    this.set(keys.lastOktaSettingsKey, value);
  }

  get lastSelectedOrg(): string | null {
    return this.get(keys.lastSelectedOrgKey);
  }

  set lastSelectedOrg(value: string | null) {
    this.set(keys.lastSelectedOrgKey, value);
  }

  get refreshToken(): string | null {
    return this.get(keys.refreshTokenKey);
  }

  set refreshToken(value: string | null) {
    this.set(keys.refreshTokenKey, value);
  }

  get returnToPage(): string | null {
    return this.get(keys.returnToPage);
  }

  set returnToPage(value: string | null) {
    this.set(keys.returnToPage, value);
  }

  get token(): string | null {
    return this.get(keys.tokenKey);
  }

  set token(value: string | null) {
    this.set(keys.tokenKey, value);
  }

  /**
   * Gets a value from local storage
   * @param key - key of the item to retrieve
   */
  private get<T>(key: string): T | null {
    try {
      let result: any = localStorage.getItem(key) || null;
      if (result != null) {
        result = JSON.parse(result);
      }
      return result as T;
    } catch {
      return null;
    }
  }

  /**
   * Sets a value in local storage or deletes it
   * @param key
   * @param value
   */
  private set(key: string, value: any) {
    if (value) {
      localStorage.setItem(key, JSON.stringify(value));
    } else {
      localStorage.removeItem(key);
    }
  }
}

export const BrowserStorageService = new BrowserStorageServiceClass();
