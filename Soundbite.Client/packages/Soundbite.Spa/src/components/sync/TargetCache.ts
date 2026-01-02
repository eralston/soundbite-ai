import {
  IndexPageRequest,
  PublicError,
  SyncService,
  SyncTarget,
  TokenPageResponse,
} from "@soundbite/api";

// Module-scoped variable means we cache for the entire session
// This assumes that IDs cannot be reused across cached target types
// This also means renames will NOT appear until the UI is reloaded
const targetsById: Map<string, SyncTarget> = new Map<string, SyncTarget>();

/** Provides a mechanism for managing a local cache of AadTarget objects, based on parameterized strategies for requesting them from the API and indicating they have loaded  */
export class TargetCache {
  protected currentTargets: SyncTarget[] = [];

  /**
   * Constructor that injects situationally needed behaviors
   * @param orgRoute
   * @param onLoaded
   * @param onRequest
   * @param defaultValues These may be real SyncTarget objects or objects with just an ID that need their name dynamically loaded
   */
  constructor(
    protected readonly orgRoute: string,
    protected readonly onLoaded: (targets: SyncTarget[]) => Promise<void>,
    protected readonly onRequest: (
      orgRoute: string,
      id: string
    ) => Promise<SyncTarget>,
    protected readonly onError: (id: string, err: Error) => void,
    protected readonly defaultValues?: SyncTarget[]
  ) {}

  protected async loadPage(
    page?: IndexPageRequest
  ): Promise<TokenPageResponse<SyncTarget>> {
    const ret: TokenPageResponse<SyncTarget> = await SyncService.readUsersAsync(
      this.orgRoute,
      page
    );
    if (ret != null && ret.result != null) {
      ret.result.forEach((u) => targetsById.set(u.id, u));
    }
    return ret;
  }

  protected async loadTarget(
    id: string,
    onLoaded?: (target: SyncTarget) => void
  ): Promise<SyncTarget> {
    // We need to both return what we have AND async load it for callback later
    try {
      const ret: SyncTarget = await this.onRequest(this.orgRoute, id);
      if (ret != null) {
        targetsById.set(ret.id, ret);
        if (onLoaded != null) {
          onLoaded(ret);
        }
      }
      return ret;
    } catch (err: unknown) {
      this.onError(id, err as Error);
      (err as any).innerError = undefined;
      PublicError.Log(err as Error);
      const ret: SyncTarget = {
        id,
        name: `Error loading target ${id}`,
      };
      if (onLoaded != null) {
        onLoaded(ret);
      }
      return ret;
    }
  }

  protected get(
    id: string,
    onLoaded?: (target: SyncTarget) => void
  ): SyncTarget {
    const entry = targetsById.get(id);
    if (entry == null) {
      this.loadTarget(id, onLoaded);
      return {
        id,
        name: "Loading...",
      };
    } else {
      return entry;
    }
  }

  async load(): Promise<void> {
    // We are trying to evaluate the default values that need their name dynamically loaded
    // This should be interopable with any object that provides an "id" field
    if (this.defaultValues == null) {
      return;
    }
    this.currentTargets = this.defaultValues.map((t: SyncTarget) => {
      if (t.name != null) {
        return t;
      } else {
        const target: SyncTarget = this.get(t.id, (newTarget: SyncTarget) => {
          if (this.currentTargets == null || this.currentTargets.length === 0) {
            return;
          }
          for (const existingTarget of this.currentTargets) {
            if (existingTarget.id === newTarget.id) {
              existingTarget.name = newTarget.name;
            }
          }
          this.onLoaded(this.currentTargets);
        });
        return target;
      }
    });

    this.onLoaded(this.currentTargets);
  }
}
