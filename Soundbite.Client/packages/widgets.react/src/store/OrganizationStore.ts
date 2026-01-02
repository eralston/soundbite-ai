import { action, makeObservable, observable } from "mobx";

import {
  Group,
  GroupsService,
  OrganizationExtended,
  OrganizationsService,
  OrganizationWithSettings,
} from "@soundbite/api";

import { IOrganizationStore } from "../interfaces/IOrganizationStore";
import { ConcurrentCache } from "../modules/ConcurrentCache";
import { GlobalTheme } from "../styles";

/**
 * MobX store class containing states and actions for organizations.
 */
class OrganizationStoreClass implements IOrganizationStore {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      // Observable
      currentOrgRoute: observable,
      currentOrg: observable,
      myOrgs: observable,
      groups: observable,
      myGroups: observable,
      // Action
      readMyOrgsAsync: action,
      readOrgAsync: action,
      onGroupRemoved: action,
    });
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  private lastOrgRoute?: string;

  myOrgs?: OrganizationExtended[] = undefined; // Array containing all of the organization with which the user is associated
  currentOrg?: OrganizationWithSettings = undefined; // Reference to the current organization context under which the application is operating
  currentOrgRoute?: string = undefined;
  groups?: Group[] = undefined;
  myGroups?: Group[] = undefined;
  myTargetGroups?: Group[] = undefined;

  //////////[ Cache Members ]///////////////////////////////////////////////////////////////////////

  orgCache: ConcurrentCache<OrganizationWithSettings> =
    new ConcurrentCache<OrganizationWithSettings>();

  /**
   * Creates a key used for caching operations
   * @param orgRoute - route of the organization with which the session is associated
   */
  private getCacheKey(orgRoute: string): string {
    return `${orgRoute}`;
  }

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  reset(): void {
    this.orgCache.clear();
    this.lastOrgRoute = undefined;
    this.myOrgs = undefined;
    this.currentOrg = undefined;
    this.groups = undefined;
    this.myGroups = undefined;
  }

  async refresh(): Promise<void> {
    const promises: Promise<any>[] = [];

    promises.push(this.readMyOrgsAsync(false));

    if (this.lastOrgRoute != null) {
      const orgRoute = this.lastOrgRoute;
      promises.push(this.readOrgAsync(orgRoute, false));
      promises.push(this.readMyGroupsAsync(orgRoute, false));
      promises.push(this.readGroupsAsync(orgRoute, false));
    }

    await Promise.all(promises);
  }

  /**
   * Populates the myOrgs property of the store.
   * @param refresh - flag indicating whether to force a refresh of cached data
   * @returns a promise indicating success or failure of the operation
   */
  async readMyOrgsAsync(
    allowCache: boolean = true
  ): Promise<OrganizationExtended[]> {
    if (this.myOrgs != null && allowCache) {
      return this.myOrgs;
    }

    this.myOrgs = undefined;
    const ret = await OrganizationsService.readAllAsync();
    this.myOrgs = ret;

    return ret;
  }

  /**
   * Gets and organization by organization route.
   * @param orgRoute - route identifying the organization to retrieve.
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  async readOrgAsync(
    orgRoute: string,
    allowCache: boolean = true
  ): Promise<OrganizationWithSettings> {
    const cacheKey = this.getCacheKey(orgRoute);
    const ret = await this.orgCache.getAsync(
      cacheKey,
      () => OrganizationsService.readAsync(orgRoute),
      allowCache
    );
    if (ret == null) {
      throw new Error(`Could not find organization '${orgRoute}'`);
    }

    GlobalTheme.set(ret.settings.theme, ret.details.route, true);
    return ret;
  }

  /**
   * Retrieves the specified organization and sets it as the current organization
   * @param orgRoute - route identifying the organization to set as the current organization
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  async setCurrentOrganization(
    orgRoute: string,
    allowCache: boolean = true
  ): Promise<OrganizationWithSettings> {
    console.log("setCurrentOrg");
    if (
      this.lastOrgRoute === orgRoute &&
      this.currentOrg?.details.route === orgRoute &&
      allowCache
    ) {
      return this.currentOrg;
    }

    this.currentOrg = undefined;
    const org = await this.readOrgAsync(orgRoute, allowCache);
    this.reset();
    this.currentOrg = org;
    this.lastOrgRoute = orgRoute;
    GlobalTheme.set(org.settings.theme, org.details.route, true);
    return org;
  }

  /**
   * Responsible for updating any organization in state/cache when a group is removed.
   * @param orgRoute - route of the organization with which the group is associated
   * @param groupRoute - route of the group that was deleted
   */
  onGroupRemoved(orgRoute: string, groupRoute: string): void {
    // If we're not current only that org, then abort
    if (this.lastOrgRoute !== orgRoute) {
      return;
    }

    let didFilter = false;
    // Optionally pull from groups
    if (this.groups != null) {
      didFilter = true;
      this.groups = [...this.groups.filter((g) => g.route !== groupRoute)];
    }

    // Optionally pull from my groups
    if (this.myGroups != null) {
      didFilter = true;
      this.myGroups = [...this.myGroups.filter((g) => g.route !== groupRoute)];
    }

    // If we removed anything, then we need to re-trigger
    if (didFilter) {
      const temp = this.currentOrg;
      this.currentOrg = undefined;
      this.currentOrg = temp;
    }
  }

  async onGroupChanged(group: Group): Promise<void> {
    if (this.lastOrgRoute != null) {
      const orgRoute = this.lastOrgRoute;
      const groupsPromise = this.readGroupsAsync(orgRoute, false);
      const myGroupsPromise = this.readMyGroupsAsync(orgRoute, false);
      await Promise.all([groupsPromise, myGroupsPromise]);
    }
  }

  async readGroupsAsync(
    orgRoute: string,
    allowCache: boolean = true
  ): Promise<Group[]> {
    if (this.lastOrgRoute === orgRoute && this.groups != null && allowCache) {
      return this.groups;
    }

    this.lastOrgRoute = undefined;
    this.groups = undefined;
    const ret = await GroupsService.readAllGroupsAsync(orgRoute);
    this.lastOrgRoute = orgRoute;
    this.groups = ret;
    return ret;
  }

  async readMyGroupsAsync(
    orgRoute: string,
    allowCache: boolean = true
  ): Promise<Group[]> {
    if (this.lastOrgRoute === orgRoute && this.myGroups != null && allowCache) {
      return this.myGroups;
    }

    this.lastOrgRoute = undefined;
    this.myGroups = undefined;
    const ret = await GroupsService.readMyGroupsAsync(orgRoute);
    this.lastOrgRoute = orgRoute;
    this.myGroups = ret;
    return ret;
  }

  async readMyTargetGroupsAsync(
    orgRoute: string,
    allowCache: boolean = true
  ): Promise<Group[]> {
    if (
      this.lastOrgRoute === orgRoute &&
      this.myTargetGroups != null &&
      allowCache
    ) {
      return this.myTargetGroups;
    }

    this.lastOrgRoute = undefined;
    this.myTargetGroups = undefined;
    const ret = await GroupsService.readMyTargetGroupsAsync(orgRoute);
    this.lastOrgRoute = orgRoute;
    this.myTargetGroups = ret;
    return ret;
  }
}

// Export a singleton instance of the store
export const OrganizationStore = new OrganizationStoreClass();
