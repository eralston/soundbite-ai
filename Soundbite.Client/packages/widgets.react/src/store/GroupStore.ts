import { action, observable, makeObservable } from "mobx";

import { GroupDetails, GroupsService, NewGroup } from "@soundbite/api";

import { OrganizationStore } from "./OrganizationStore";
import { IGroupStore } from "../interfaces/IGroupStore";

/**
 * MobX store class containing states and actions for groups.
 */
class GroupStoreClass implements IGroupStore {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      refresh: observable,
      currentGroup: observable,
      deleteGroupAsync: action,
      updateGroupAsync: action,
    });
  }

  //////////[ Cache Members ]////////////////////////////////////////////////////////////////////////

  groupDetailsCache: { [name: string]: GroupDetails } = {};

  /**
   * Responsible for clearing ALL cached data in the store.
   */
  clearCache(): void {
    this.groupDetailsCache = Object.create(null);
  }

  /**
   * Creates a key used for caching operations
   * @param orgRoute - route of the organization
   * @param groupRoute - route of the group
   */
  private getCacheKey(orgRoute: string, groupRoute: string): string {
    return `${orgRoute}-${groupRoute}`;
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  protected lastOrgRoute?: string = undefined;
  protected lastGroupRoute?: string = undefined;

  /**
   * Stores a reference to the current group context under which the application is operating.
   */
  currentGroup?: GroupDetails = undefined;

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  reset(): void {
    this.clearCache();
    this.lastOrgRoute = undefined;
    this.lastGroupRoute = undefined;
    this.currentGroup = undefined;
  }

  /** Reloads the data in this context */
  async refresh(): Promise<void> {
    this.clearCache();
    if (
      this.currentGroup == null ||
      this.lastOrgRoute == null ||
      this.lastGroupRoute == null
    ) {
      this.currentGroup = undefined;
      this.lastOrgRoute = undefined;
      this.lastGroupRoute = undefined;
      return;
    }

    await this.readGroupAsync(this.lastOrgRoute, this.lastGroupRoute, false);
  }

  /**
   * Creates a new group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param newGroup - information about the group to create.
   */
  async createGroupAsync(
    orgRoute: string,
    newGroup: NewGroup
  ): Promise<GroupDetails> {
    const group = await GroupsService.createAsync(orgRoute, newGroup);
    this.currentGroup = group;
    this.lastOrgRoute = orgRoute;
    this.lastGroupRoute = group.route;
    const refreshPromise = this.refresh();
    const orgChangePromise = OrganizationStore.onGroupChanged(group);
    await Promise.all([refreshPromise, orgChangePromise]);
    return group;
  }

  /**
   * Deletes the specified group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param groupRoute - route of the group to delete.
   */
  async deleteGroupAsync(orgRoute: string, groupRoute: string): Promise<void> {
    GroupsService.deleteAsync(orgRoute, groupRoute);
    delete this.groupDetailsCache[this.getCacheKey(orgRoute, groupRoute)];
    await OrganizationStore.onGroupRemoved(orgRoute, groupRoute);
  }

  /**
   * Gets the specified group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param groupRoute - route of the group to get
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  async readGroupAsync(
    orgRoute: string,
    groupRoute: string,
    allowCache: boolean = true
  ): Promise<GroupDetails> {
    const cacheKey = this.getCacheKey(orgRoute, groupRoute);
    let group: GroupDetails | null = allowCache
      ? this.groupDetailsCache[cacheKey]
      : null;
    if (!group) {
      group = await GroupsService.readAsync(orgRoute, groupRoute);
      if (group) {
        this.groupDetailsCache[cacheKey] = group;
      }
    }

    if (group) {
      this.lastOrgRoute = orgRoute;
      this.lastGroupRoute = groupRoute;
      this.currentGroup = group;
    }

    return group;
  }

  /**
   * Updates the specified group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param groupRoute - route of the group to get
   * @param patch - HTTP patch instructions identifying the group properties to update.
   */
  async updateGroupAsync(
    orgRoute: string,
    groupRoute: string,
    patch: any
  ): Promise<GroupDetails> {
    const group = await GroupsService.patchAsync(orgRoute, groupRoute, patch);
    await OrganizationStore.onGroupChanged(group);
    return group;
  }

  //////////[ Event Methods ]///////////////////////////////////////////////////////////////////////

  async onAnyMemberChanged(
    orgRoute: string,
    groupRoute: string
  ): Promise<void> {
    // If the updated member matches the current org/group, then we need to refresh
    // This ensures we update any showing lists of members, etc
    if (orgRoute !== this.lastOrgRoute || groupRoute !== this.lastGroupRoute) {
      return;
    }
    await this.refresh();
  }
}

// Export a singleton instance of the store
export const GroupStore = new GroupStoreClass();
