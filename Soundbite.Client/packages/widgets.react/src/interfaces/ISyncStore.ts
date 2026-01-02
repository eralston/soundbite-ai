import {
  OrgSyncConfig,
  OrgSyncResult,
  SyncTarget,
  SyncValidation,
} from "@soundbite/api";

/** An object that can interact with the Org Directory Sync System */
export interface ISyncStore {
  /** Gets the current OrgSyncResult (for the last run) */
  readonly result?: OrgSyncResult;
  /** Gets the list of last queried active users */
  readonly users?: SyncTarget[];
  /** Gets the list of last queried active groups */
  readonly groups?: SyncTarget[];
  /** Gets the list of last queried active groups */
  readonly validation?: SyncValidation;
  /** Gets the list of last queried sync settings */
  readonly config?: SyncValidation;
  /** Gets the JSON representing the config.santizedConfig object or undefined if not present  */
  readonly configJson?: string;

  // TODO: Consider moving the nextConfig operations into a context provider or other more specific MobX store

  /** Gets the object being built as the next config object */
  readonly nextConfig?: OrgSyncConfig;

  nextStrategyConfig<TStrategy>(): TStrategy | undefined;
  setNextConfig(orgConfig?: OrgSyncConfig): void;
  setNextStrategyConfig(strategyConfig?: object): OrgSyncConfig | undefined;
  validateNextConfig(orgRoute: string): Promise<SyncValidation>;
  updateToNextConfig(orgRoute: string): Promise<SyncValidation>;

  reset(): void;

  /**
   * Gets the current OrgSyncConfig object for the given org, if the current user has access
   * This will return undefined if sync is not currently configured for the org
   * @param orgRoute
   */
  readConfigAsync(orgRoute: string): Promise<SyncValidation>;

  updateConfigAsync(
    orgRoute: string,
    orgConfig?: OrgSyncConfig
  ): Promise<SyncValidation>;

  syncAsync(orgRoute: string): Promise<OrgSyncResult>;

  validateConfigAsync(
    orgRoute: string,
    orgConfig?: OrgSyncConfig
  ): Promise<SyncValidation>;
}
