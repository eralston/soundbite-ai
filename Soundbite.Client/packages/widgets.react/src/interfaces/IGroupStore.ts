import { NewGroup, Group, GroupDetails } from "@soundbite/api";

/**
 * Interface defininig the contract for a group store implementation.
 */
export interface IGroupStore {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  /**
   * Stores a reference to the current group context under which the application is operating.
   */
  currentGroup?: GroupDetails;

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /** Clear the data for this store */
  reset(): void;

  /** Reloads the contents of this store based on its current state */
  refresh(): Promise<void>;

  /**
   * Creates a new group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param newGroup - information about the group to create.
   */
  createGroupAsync(orgRoute: string, newGroup: NewGroup): Promise<GroupDetails>;

  /**
   * Deletes the specified group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param groupRoute - route of the group to delete.
   */
  deleteGroupAsync(orgRoute: string, groupRoute: string): Promise<void>;

  /**
   * Gets the specified group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param groupRoute - route of the group to get
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  readGroupAsync(
    orgRoute: string,
    groupRoute: string,
    allowCache?: boolean
  ): Promise<GroupDetails>;

  /**
   * Updates the specified group.
   * @param orgRoute - route of the organization with which the group is associated.
   * @param groupRoute - route of the group to get
   * @param patch - HTTP patch instructions identifying the group properties to update.
   */
  updateGroupAsync(
    orgRoute: string,
    groupRoute: string,
    patch: any
  ): Promise<GroupDetails>;
}
