import {
  OrgSyncConfig,
  OrgSyncResult,
  SyncTarget,
  SyncService,
  SyncValidation,
} from "@soundbite/api";
import { action, makeObservable, observable } from "mobx";
import { ISyncStore } from "../interfaces/ISyncStore";

/**
 * MobX store class containing states and actions for organizations.
 */
class SyncStoreClass implements ISyncStore {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      // Observables
      config: observable,
      result: observable,
      validation: observable,
      nextConfig: observable,
      // Actions
      reset: action,
      validateConfigAsync: action,
      readConfigAsync: action,
      updateConfigAsync: action,
      syncAsync: action,
      updateToNextConfig: action,
      validateNextConfig: action,
    });
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  config?: SyncValidation = undefined;
  result?: OrgSyncResult = undefined;
  validation?: SyncValidation = undefined;

  nextConfig?: OrgSyncConfig = undefined;
  nextStrategyConfig<TStrategy>(): TStrategy | undefined {
    if (this.nextConfig == null) return undefined;

    const json = this.nextConfig.syncConfigJson;
    if (json == null) return undefined;

    return JSON.parse(json);
  }

  //////////[ Methodss ]//////////////////////////////////////////////////////////

  protected sortTargets(result?: SyncTarget[]) {
    if (result == null || result.sort == null) return undefined;

    const originalLength = result.length;

    result = result
      .filter((t) => t.name != null)
      .sort((a: SyncTarget, b: SyncTarget) =>
        (a.name ?? "").localeCompare(b?.name ?? "")
      );

    const newLength = result.length;

    if (newLength !== originalLength)
      console.warn("Filtered out null names from sync targets");

    return result;
  }

  reset(): void {
    this.config = undefined;
    this.result = undefined;
    this.validation = undefined;
    this.nextConfig = undefined;
  }

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  setNextConfig(orgConfig?: OrgSyncConfig): void {
    this.nextConfig = { ...orgConfig };
  }

  setNextStrategyConfig(strategyConfig?: object): OrgSyncConfig | undefined {
    if (strategyConfig == null && this.nextConfig == null) {
      return undefined;
    }
    const json =
      strategyConfig != null ? JSON.stringify(strategyConfig) : undefined;
    if (this.nextConfig != null) this.nextConfig.syncConfigJson = undefined;
    const nextConfig = {
      ...this.nextConfig,
      syncConfigJson: json,
    };
    this.nextConfig = nextConfig;
    return nextConfig;
  }

  async updateToNextConfig(orgRoute: string): Promise<SyncValidation> {
    const val = await this.updateConfigAsync(orgRoute, this.nextConfig);
    return val;
  }

  async validateNextConfig(orgRoute: string): Promise<SyncValidation> {
    const val = await this.validateConfigAsync(orgRoute, this.nextConfig);
    return val;
  }

  /**
   * Async runs logic on the server to config orgConfig is valid
   * @param orgRoute
   * @param orgConfig If this is undefined, then it will check the current config on the server
   */
  async validateConfigAsync(
    orgRoute: string,
    orgConfig?: OrgSyncConfig
  ): Promise<SyncValidation> {
    this.validation = undefined;
    const response: SyncValidation = await SyncService.validateAsync(
      orgRoute,
      orgConfig
    );
    this.validation = response;
    return response;
  }

  /**
   * Reads the current config for the given org route
   * @param orgRoute
   * @param refresh Optional parameter that forces a refresh of the current org
   */
  async readConfigAsync(orgRoute: string): Promise<SyncValidation> {
    this.config = undefined;
    const response: SyncValidation = await SyncService.readConfigAsync(
      orgRoute
    );
    this.config = response;
    this.nextConfig = response.sanitizedConfig;
    return response;
  }

  /**
   * Updates the org sync config
   * If this errors, the current org sync config will not update
   * On success, this will update this.currentOrgSyncConfig
   * @param orgRoute
   * @param orgSyncConfig If you pass undefined, this will delete the config and disable directory sync for the org
   */
  async updateConfigAsync(
    orgRoute: string,
    orgSyncConfig?: OrgSyncConfig
  ): Promise<SyncValidation> {
    const response: SyncValidation = await SyncService.updateConfigAsync(
      orgRoute,
      orgSyncConfig
    );
    this.reset();
    this.config = response;
    this.nextConfig = response.sanitizedConfig;
    return response;
  }

  /**
   * Runs the sync process for the given org
   * On start, this will clear this.currentOrgSyncResult
   * On success, this will update this.currentOrgSyncResult
   * @param orgRoute
   */
  async syncAsync(orgRoute: string): Promise<OrgSyncResult> {
    this.result = undefined;
    const result: OrgSyncResult = await SyncService.syncAsync(orgRoute);
    this.result = result;
    return result;
  }
}

// Export a singleton instance of the store
export const OrgSyncStore = new SyncStoreClass();
